# KoBeLU Core

## Build

To build a Windows executable run `build.ps1` in PowerShell. After a successfull build process the result can be found in KoBeLUAdmin/bin/ *<Debug | Release>* /.

The build tool is Cake (C# Make.)

## System architecture
The following diagram shows the current state of the planned system architecture.
In center it shows the three main components of the KoBeLU system. Each component is an own process being possible to host on a separated machine.
Additionally, Combu and a mySQL database must be available.