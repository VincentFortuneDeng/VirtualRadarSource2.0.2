@echo off

rem This should be executed as a part of the post-build step for the plugin.
rem The parameters are:
rem 1) $(SolutionDir)
rem 2) $(ProjectDir)
rem 3) $(ConfigurationName)
rem 3) $(TargetDir)
rem 4) $(TargetName)
rem 5) Name-Of-Plugin-SubFolder
rem Ensure that the XML manifest file is set to always copy to the output directory on a build

    set SLNDIR=%~1
    set PROJDIR=%~2
    set CONFIG=%~3
    set TARGETDIR=%~4
    set TARGETNAME=%~5
    set PLUGINFOLDER=%~6

    if "%PLUGINFOLDER%"=="" goto NOPARAM

    set SOURCEDLL=%TARGETDIR%\%TARGETNAME%.dll
    set SOURCEXML=%TARGETDIR%\%TARGETNAME%.xml
    set SOURCEWEB=%PROJDIR%\Web
    set PLUGINS=%SLNDIR%VirtualRadar\bin\x86\%CONFIG%\Plugins
    set DEST=%PLUGINS%\%PLUGINFOLDER%
    set WEB=%PLUGINS%\%PLUGINFOLDER%\Web

    if exist "%PLUGINS%" goto MKDEST
        md "%PLUGINS%"

:MKDEST
    if exist "%DEST%" goto CPPLUGIN
        md "%DEST%"

:CPPLUGIN
    copy "%SOURCEDLL%" "%DEST%"
    copy "%SOURCEXML%" "%DEST%"

    rem Delete and then copy the WEB folder into the plugin folder.
    rem If your plugin has no web content then comment out or remove this block
    if not exist "%WEB%\" goto WEBDELETED
        rmdir /s /q "%WEB%"
        if errorlevel 0 goto WEBDELETED
        echo FAILED: Could not remove the "%WEB%" folder, errorlevel is %ERRORLEVEL%
        goto :ENDBAD
    :WEBDELETED
    xcopy /EQYI "%SOURCEWEB%" "%WEB%"
    if not errorlevel 0 goto :WEBFAILED
        echo Copied web site output to "%WEB%"
        goto :END
    :WEBFAILED
        echo FAILED: The copy of the plugin web site content failed with errorlevel %ERRORLEVEL%
        goto :ENDBAD
    
    goto :END

:NOPARAM
    echo.
    echo Missing parameter - add this as a post-build step but remember to replace the
    echo "YOUR_PLUGIN_NAME_HERE" part with the name of your plugin:
    echo.
    echo $(ProjectDir)\_PostBuild.bat "$(SolutionDir)" "$(ProjectDir)" "$(ConfigurationName)" "$(TargetPath)" "YOUR_PLUGIN_NAME_HERE"
    echo.

:ENDBAD
    exit /b 1

:END
    exit /b 0