Kent Brian Lizardo
kliza2@uic.edu
pdollar <args>

# 1. Build

In order to build this project you must have Visual Studio Build Tools and the .NET SDK installed on your machine.

Makefile commands and descriptions
```
make - Builds the application executable into 
make clean - Cleans build files and ext_gestures.txt
```

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
The only usage of AI in this project was to assist in generation of various Makefile(s) for MsBuild and Mono and the .NET CLI. The only included one in the Makefile was the .NET build process.
No other usage of AI.
