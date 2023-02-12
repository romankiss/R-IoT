
<h2>Concept</h2>

The following screen snippet shows an integration concept based on the Azure services such as an <b>Azure Event Grid (AEG)</b> and <b>Azure Function (AF)</b>:

<img width="640" alt="TTN2Azure" src="https://user-images.githubusercontent.com/30365471/218275872-442e536c-eada-400d-982e-43c099bd85fc.PNG">

As the above concept shows, the TTN server publishes the device messages on the AEG Custom Topic for distributing them using the loosely decoupled event driven PUB/SUB model to the destination targets.
This solution is very generic and easy for extension and filtering messages in the AEG model.

In prior of using the <b>TTN Publisher</b>, we have to create in the Azure some resources, such as AEG, AF and IoT Central Application.

<h2>Azure (<a href="https://portal.azure.com/#home">portal</a>)</h2>

<img width="464" alt="AzureDashboard" src="https://user-images.githubusercontent.com/30365471/218279181-a89f1c6b-46c8-4bdb-9bc7-08dfd1d8e501.png">

As a first azure resource, we can create an Azure IoT Central App for consuming a device telemetry data.
<h3>Azure IoT Central App (<a href="https://spsepn-iotc.azureiotcentral.com/">spsepn-iotc</a>)</h3>

<img width="464" alt="IoTCentralDashboard" src="https://user-images.githubusercontent.com/30365471/218279425-788fc415-283b-4a2e-af14-6f97c63d28fe.png">


<h3>AEG Custom Topic (topicTTN-1)</h3>

<img width="640" alt="CustomTopic" src="https://user-images.githubusercontent.com/30365471/218316629-1c8fe066-c2c4-4064-903a-c1c3d03ebf61.png">


Now, we can integrate the TTN Publisher with AEG Pub/Sub eventing service. Once we have created a Custom Topic endpoint, we can use this entry point for publishing an event to the AEG Pub/Sub model. 
The following step shows a plug-in place in the TTN Integration Webhook pane.

<code><h2>TTN Publisher</h2></code>

The TTN supports a Webhook integration for publishing an uplink messages. The following screen snippet shows a configuration of the Webhook as an AEG Publisher with a CloudEventSchema:

<img width="640" alt="TTN-WebhookToAzureEventGrid" src="https://user-images.githubusercontent.com/30365471/218276983-36786d1c-b9ae-421e-a3c9-ea5c40e75c1a.PNG">

In this point, we can subscribed on the custom topic topicTTN-1 some built-in (standard) target resources such as storage queue, service bus, etc. 
For our destination such as the Azure IoT Central App we have to create a custom subscriber which will handle all integration needs like is a device provisioning (registration + configuration), etc. For this purpose we can use a serverless component such as the <b>Azure HttpTrigger Function</b>.

<h3>Azure Function - IoT Subscriber</h3>
First of all, the Azure Fuction App is neccessary to create it, see the following screen snippet:

<img width="640" alt="AzureDeviceInfo" src="https://user-images.githubusercontent.com/30365471/218318422-29feccec-bba6-4d81-bc40-acff9af0d9c2.png">

Now, we can create a HttpTriggerTTNtoIoTC function deploying its code in the Azure Portal. The files can be found in the folder 
<a href="https://github.com/romankiss/R-IoT/tree/main/Projects/2022/IoT-TTN-To-Azure/IoT-Subscriber/HttpTriggerTTNtoIoTC">here.</a>


<code><h2>IoT Central App Subscription</h2></code>


<h3>Azure IoT Central Application</h3>

Our integration (subscribing) to the TTN is done, so we can see the device messages (Raw Data) in the IoT Central App:
<h4>Raw Data</h4>

<img width="640" alt="AzureDeviceInfo" src="https://user-images.githubusercontent.com/30365471/218278994-85e65e13-3510-44b6-af61-90d916eb6ed1.png">

<h4>Device Info</h4>

<img width="464" alt="IoTCentralDeviceInfo" src="https://user-images.githubusercontent.com/30365471/218278455-3aad7d1c-8bce-4156-8f2e-ebd3599bf997.png">


<h4>Dashboard</h4>

<img width="464" alt="IoTCentralDashboard" src="https://user-images.githubusercontent.com/30365471/218278511-fab7e8a2-ef36-4626-86ce-e001362dda4d.png">









