
CS_PROJ = pdollar.csproj
OUTPUT = build
FLAGS = --output $(OUTPUT) -c Release

all: $(CS_PROJ)
	dotnet build $(CS_PROJ) $(FLAGS)

clean:
	dotnet clean $(CS_PROJ) $(FLAGS)

.PHONY: all clean
