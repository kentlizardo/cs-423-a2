using PDollarGestureRecognizer;
using System.IO;

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
                case ["-t", string gesturePath]:  // pdollar -t <gesturefile>
                    AddTemplateToConfig(gesturePath);
                    break;
                case ["-r"]:  // pdollar -r
                    ResetConfig();
                    break;
                case [string eventstreamPath]:  // pdollar <eventstream>
                    var trainingSet = CreateTrainingSetFromConfig();
                    ReadEventStream(eventstreamPath, trainingSet);
                    break;
                case []:  // pdollar
                    Console.WriteLine(HelpText);
                    break;
                default:
                    Console.WriteLine("Invalid number or set of arguments. Use \"pdollar\" for help.");
                    break;
            }

            //string[] x =  File.ReadAllLines();
        }

        public static string[] GetTemplateListFromConfig()
        {
            string[] gestureList = File.ReadAllLines(ConfigPath);
            //  Skip any null or empty lines.
            gestureList = gestureList.Where(line => string.IsNullOrWhiteSpace(line)).ToArray();
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
            catch (Exception ex)
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
                    //  Skip any null or empty lines.
                    oldGestureList = oldGestureList.Where(line => string.IsNullOrWhiteSpace(line)).ToArray();
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
