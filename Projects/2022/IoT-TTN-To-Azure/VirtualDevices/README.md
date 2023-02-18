
<h2>Tools</h2>

<a href="https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-TTN-To-Azure/VirtualDevices/AzureIoTHubTester_1201_preview.zip">Azure IoT Hub Tester - 1.201-preview</a>

See more details about the design and implementation of the small tool tester for exploring Azure IoT Hub with virtual MQTT device <a href="https://www.codeproject.com/articles/1173356/Azure-IoT-Hub-Tester">here.</a>

for Android mobile version <a href="https://www.codeproject.com/Articles/5322753/Azure-IoT-Central-Tester">here.</a>

<img width="640" alt="AzureIoTHubTester" src="https://user-images.githubusercontent.com/30365471/219094987-7d7775fb-6534-4348-9b25-2302d8ff0495.PNG">


<h3>Provisioning device</h3>

<img width="451" alt="Connect" src="https://user-images.githubusercontent.com/30365471/219115730-02781382-eba7-4e46-acf8-08c500273a09.PNG">

The following line shows a format for inserting requested parameters into the above textbox:

<code><b>scopeId  deviceId  @PrimaryKey from SAS-IoT-Devices</b>  | modelId </code>

<h5>Example:</h5>
<code><b>0ne001234A5  device911  @Zh+ECoBEMgX/CHZxxxxxxxxxxxxxxxxxxA==</b>  | dtmi:com:example:TemperatureController;2 </code>


