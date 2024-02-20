
The [.Net nanoFramework](https://github.com/nanoframework) is to be a platform that enables the writing of managed code applications for constrained embedded devices. Developers can harness the familiar IDE Visual Studio and their .NET (C#) knowledge to quickly write applications without having to worry about the low level hardware intricacies of a micro-controller.

Getting started with .NET nanoFramework

Installation steps:

- Download [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/community/)

- Install [.NET nanoFramework Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=nanoframework.nanoframework-vs2022-extension)

- Install [.NET nanoFramework Firmware Flasher](https://www.nuget.org/packages/nanoff) using PowerShell

<br />
<br />

Note: The following are instructions for instalation of the .Net tool using PowerShell:
   1. Open PowerShell as an administrator.
   2. Run the following command to install the latest version of .NET:

     Install-Package -Name dotnet

<br />

Now, you can install nanoff (nano Firmware Flasher tool):

     dotnet tool install -g nanoff

<br />

Example of using a nanoff for download a firmware for AtomLite microcontroller:

     nanoff --target ESP32_PICO --serialport COM3 --update
<br />
Example of using a nanoff for showing COM ports:

     nanoff --listports
<br />
Example of using a nanoff for showing all targets (microcontrollers):

     nanoff --listtargets    

<br />
<br />
NuGet path not found fix:

   1. Navigate to Tools -> Options -> NuGet Package Manager -> Package Sources
![image](https://github.com/romankiss/R-IoT/assets/121643952/31bf6e2a-6c67-4e39-8422-c1f748aac670)
   2. Click on the + (PLUS) sign in the right corner
   3. Type: nuget.org in the name section
   4. Type: https://api.nuget.org/v3/index.json in the source section
   5. Check your newly added nuget.org in the package sources
   6. Update

This should fix your problem.

<br />
Samples:

 <ul>
    <li><a href="https://github.com/nanoframework/Samples/tree/main/samples/Blinky">Blink your first led</a></li>
    <li><a href="https://github.com/nanoframework/Samples">Generic Samples</a></li>
  <li><a href="https://github.com/nanoFramework/nanoFramework.IoT.Device">IoT.Device Samples</a></li>
  <br />
 </ul>
