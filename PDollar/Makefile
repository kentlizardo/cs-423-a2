
CS_PROJ = pdollar.csproj
OUTPUT = build

all: $(CS_PROJ)
	dotnet build $(CS_PROJ) --output $(OUTPUT) -c Release

clean:
	dotnet clean $(CS_PROJ)

.PHONY: all clean
