
<h2>Concept</h2>

The following screen snippet shows an integration concept based on the Azure services such as an Azure Event Grid (AEG) and Azure Function (AF):

<img width="640" alt="TTN2Azure" src="https://user-images.githubusercontent.com/30365471/218275872-442e536c-eada-400d-982e-43c099bd85fc.PNG">

As the above concept shows, the TTN server publishes the device messages on the AEG Custom Topic for distributing them using the loosely decoupled event driven PUB/SUB model to the destination targets.
This solution is very generic and easy for extension and filtering messages in the AEG model.

<h2>TTN Publisher</h2>

The TTN supports a Webhook integration for publishing an uplink messages. The following screen snippet shows a configuration of the Webhook as an AEG Publisher with a CloudEventSchema:


<img width="640" alt="TTN-WebhookToAzureEventGrid" src="https://user-images.githubusercontent.com/30365471/218276983-36786d1c-b9ae-421e-a3c9-ea5c40e75c1a.PNG">

<h2>Azure</h2>

<img width="464" alt="AzureDashboard" src="https://user-images.githubusercontent.com/30365471/218279181-a89f1c6b-46c8-4bdb-9bc7-08dfd1d8e501.png">




<h2>Azure IoT Central Application</h2>

<img width="464" alt="IoTCentralDashboard" src="https://user-images.githubusercontent.com/30365471/218279425-788fc415-283b-4a2e-af14-6f97c63d28fe.png">




<h4>Dashboard</h4>

<img width="464" alt="IoTCentralDashboard" src="https://user-images.githubusercontent.com/30365471/218278511-fab7e8a2-ef36-4626-86ce-e001362dda4d.png">

<h4>Device Info</h4>

<img width="464" alt="IoTCentralDeviceInfo" src="https://user-images.githubusercontent.com/30365471/218278455-3aad7d1c-8bce-4156-8f2e-ebd3599bf997.png">

<h4>Raw Data</h4>

<img width="640" alt="AzureDeviceInfo" src="https://user-images.githubusercontent.com/30365471/218278994-85e65e13-3510-44b6-af61-90d916eb6ed1.png">




