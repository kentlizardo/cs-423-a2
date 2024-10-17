
CS_PROJ = pdollar.csproj

all: $(CS_PROJ)
	dotnet build $(CS_PROJ) -c Release

clean:
	dotnet clean $(CS_PROJ)
	del config.txt

.PHONY: all clean
