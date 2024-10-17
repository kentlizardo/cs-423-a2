using PDollarGestureRecognizer;
using System.IO;

// PDollar Tool
// Kent Brian Lizardo
// 
// An implementation of PDollar, see (http://depts.washington.edu/madlab/proj/dollar/pdollar.html)
// by Jacob O. Wobbrock, et. al. This application was written for the CS 423
// Natural User Interfaces course for the Fall 2024 semester at UIC.
//   The app is a command line tool used to interact with an eventstream file
// format and a gesturefile format following assignment specifications. Each
// form of command is run by executing the program with a set number of arguments.
// The tool runs "stateless" and uses the 'config.txt' file in order to store a list of
// gesture templates which will be processed each time.
// 
// File Formats:
//
// (gesturefile format)
// GestureName  <-- This is what the gesture will be recognized as when the point cloud
//              recognizer classifies it.
// BEGIN        <-- Indicates the start of a stroke.
// [x],[y]      <-- Adds a point to the current stroke. Any number of points can go
//              between BEGIN and END.
// END          <-- Indicates end of a stroke.
//
// (eventstream file format)
// MOUSEDOWN    <-- Indicates the start of a stroke for the current gesture.
// [x],[y]      <-- Adds a point to the current stroke in the current gesture.
// END          <-- Ends the stroke and adds it to the current gesture.
// RECOGNIZE    <-- Clears the current gesture data while also printing out
//              the recognizer's classification of it's data.


namespace PDollar
{
    internal class Program
    {
        const string ConfigPath = "config.txt";

        const string HelpText = """
            pdollar
              Prints a help screen with list of commands
            pdollar -t <gesturefile>
              Adds a gesture file to list of gesture templates.
            pdollar -r
              Resets all templates
            pdollar <eventstream>
              Reads in an eventstream file, printing any recognized gestures.
            """;

        static void Main(string[] args)
        {
            switch(args)
            {
                // pdollar -t <gesturefile>
                case ["-t", string gesturePath]:
                    AddTemplateToConfig(gesturePath);
                    break;
                // pdollar -r
                case ["-r"]:
                    ResetConfig();
                    Console.WriteLine("Reset gesture template list (in config.txt)");
                    break;
                // pdollar <eventstream>
                case [string eventstreamPath]:  
                    var trainingSet = CreateTrainingSetFromConfig();
                    ReadEventStream(eventstreamPath, trainingSet);
                    break;
                // pdollar
                case []:
                    Console.WriteLine(HelpText);
                    break;
                default:
                    Console.WriteLine("Invalid number or set of arguments. Use \"pdollar\" for help.");
                    break;
            }
        }

        public static string[] GetTemplateListFromConfig()
        {
            string[] gestureList = File.ReadAllLines(ConfigPath)
                .Where(line => !string.IsNullOrWhiteSpace(line))  //  Skip any null or empty lines.
                .ToArray();
            return gestureList;
        }
        public static bool TryGetTemplateListFromConfig(out string[] val)
        {
            val = Array.Empty<string>();
            try
            {
                val = GetTemplateListFromConfig();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void AddTemplateToConfig(string gesturePath)
        {
            string[] gestureList = { };
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string[] oldGestureList = File.ReadAllLines(ConfigPath);
                    oldGestureList = GetTemplateListFromConfig()
                        .Where(line => line != gesturePath)  // Ignore adding new path if already added.
                        .ToArray();

                    gestureList = oldGestureList;
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine($"Gesture list not found, creating a new one");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unrecognized error while reading gestures from {gesturePath}.\n Making a new gesture list: {e}");
                }
            }
            gestureList = gestureList.Append(gesturePath).ToArray();
            File.WriteAllLines(ConfigPath, gestureList);
            Console.WriteLine($"Added new gesture template path {gesturePath}.");
            Console.WriteLine($"Current list is as follows:");
            foreach (var path in gestureList)
            {
                Console.WriteLine($"  {path}");
            }
        }

        public static void ResetConfig()
        {
            string contents = "";
            File.WriteAllText(ConfigPath, contents);
        }

        public static Gesture[] CreateTrainingSetFromConfig()
        {
            List<Gesture> trainingSet = new List<Gesture>();
            foreach(var gesturePath in GetTemplateListFromConfig())
            {
                var gesture = ReadTemplate(gesturePath);
                if (gesture is not null)
                    trainingSet.Add(gesture);
            }
            return trainingSet.ToArray();
        }

        public static Gesture? ReadTemplate(string gesturePath)
        {
            try
            {
                using (StreamReader reader = File.OpenText(gesturePath)) {
                    string? line;

                    string gestureName;
                    if ((line = reader.ReadLine()) is not null)
                        gestureName = line.Trim();
                    else
                        throw new Exception($"The gesture template located at {gesturePath} must have a GestureName and valid stroke data.");

                    var lineNumber = 3;
                    var strokeIdx = 0;
                    var creatingStroke = false;
                    var points = new List<Point>();

                    while ((line = reader.ReadLine()) is not null)
                    {
                        line = line.Trim();
                        if (line == "BEGIN")
                        {
                            if(creatingStroke)
                                throw new Exception($"New stroke started when already created. Line {lineNumber}");
                            creatingStroke = true;
                            strokeIdx += 1;
                        } else if (line == "END") {
                            if (!creatingStroke)
                                throw new Exception($"Stroke ended without being created. Line {lineNumber}");
                            creatingStroke = false;
                        } else {
                            var parts = line.Split(',');
                            int xToken, yToken;
                            if (parts.Length >= 2 &&
                                (int.TryParse(parts[0], out xToken) && int.TryParse(parts[1], out yToken)))
                            {
                                points.Add(new Point(xToken, yToken, strokeIdx));
                            } 
                            else
                            {
                                throw new Exception($"Line {lineNumber}: \"{line}\" is not a valid point.");
                            }
                        }
                        lineNumber++;
                    }

                    return new Gesture(points.ToArray(), gestureName);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"The gesture template data located at {gesturePath} is invalid: {e}");
                return null;
            }
        }

        public static void ReadEventStream(string eventStreamPath, Gesture[] trainingSet)
        {
            try
            {
                using (StreamReader reader = File.OpenText(eventStreamPath))
                {
                    string? line;
                    var lineNumber = 3;
                    var points = new List<Point>();

                    var strokeIdx = 0;
                    var creatingStroke = false;

                    while ((line = reader.ReadLine()) is not null)
                    {
                        line = line.Trim();
                        if (line == "MOUSEDOWN")
                        {
                            if (creatingStroke)
                                throw new Exception($"New stroke started when already created. Line {lineNumber}");
                            creatingStroke = true;
                            strokeIdx += 1;
                        }
                        else if (line == "MOUSEUP")
                        {
                            if (!creatingStroke)
                                throw new Exception($"Stroke ended without being created. Line {lineNumber}");
                            creatingStroke = false;
                        }
                        else if (line == "RECOGNIZE")
                        {
                            var result = PointCloudRecognizer.Classify(new Gesture(points.ToArray()), trainingSet);
                            Console.WriteLine(result);
                            strokeIdx = 0;
                            points.Clear();
                        }
                        else  // Add new point to current stroke and gesture.
                        {
                            var parts = line.Split(',');
                            int xToken, yToken;
                            if (parts.Length >= 2 &&
                                (int.TryParse(parts[0], out xToken) && int.TryParse(parts[1], out yToken)))
                            {
                                points.Add(new Point(xToken, yToken, strokeIdx));
                            }
                            else
                            {
                                throw new Exception($"Line {lineNumber}: \"{line}\" is not a valid point.");
                            }
                        }
                        lineNumber++;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"The event stream file located at {eventStreamPath} is invalid: {e}");
            }
        }

    }
}
