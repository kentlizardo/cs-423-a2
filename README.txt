Kent Brian Lizardo
kliza2@uic.edu
pdollar <args>

# 1. Build

In order to build this project you must have Visual Studio Build Tools and the .NET SDK installed on your machine.

Make sure you are in the `/PDollar/` directory for the Make commands.
Makefile commands and descriptions
```
make - Builds the application executable into '/PDollar/build/'
make clean - Cleans build files and config.txt
```

Make sure you are in the `/PDollar/build/` directory.
```
pdollar
  Prints a help screen with list of commands
pdollar -t <gesturefile>
  Adds a gesture file to list of gesture templates.
pdollar -r
  Resets all templates
pdollar <eventstream>
  Reads in an eventstream file, printing any recognized gestures.

```

# AI Usage Statement
The only usage of AI in this project was to assist in generation of various Makefile(s) for MsBuild and Mono and the .NET CLI. The only work affected was the latest Makefile for the .NET build process.
No other usage of AI.
