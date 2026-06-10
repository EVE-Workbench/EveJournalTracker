APP := UI.avalonia/UI.avalonia.csproj

.PHONY: run build release restore clean help

## run: start de Avalonia desktop-app
run:
	dotnet run --project $(APP)

## build: compileer de Avalonia-client (+ SharedLibrary)
build:
	dotnet build $(APP)

## release: bouw een Release-build
release:
	dotnet build $(APP) -c Release

## restore: haal de NuGet packages op
restore:
	dotnet restore $(APP)

## clean: verwijder bin/ en obj/ output
clean:
	dotnet clean $(APP)

## help: toon de beschikbare commando's
help:
	@echo Beschikbare commando's:
	@echo   make run      - start de app
	@echo   make build    - compileer de Avalonia-client
	@echo   make release  - Release-build
	@echo   make restore  - herstel NuGet packages
	@echo   make clean    - opschonen
