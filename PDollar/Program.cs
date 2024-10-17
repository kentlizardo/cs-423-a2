using PDollarGestureRecognizer;

namespace PDollar
{
    internal class Program
    {
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
                case []:
                    Console.WriteLine(HelpText);
                    break;
                default:
                    Console.WriteLine("Invalid number or set of arguments. Use \"pdollar\" for help.");
                    break;
            }

            //string[] x =  File.ReadAllLines();
        }
    }
}
