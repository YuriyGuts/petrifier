@echo off

set PetriNetSamplesRoot=.
set TestAppPath=..\bin\Release\PetriNetRunner.exe

for %%i in (%PetriNetSamplesRoot%\*.pnml) do (
	echo --- Executing Petri Net: %%i...
	%TestAppPath% %%i
	echo --- Execution complete
	echo.
)

pause
