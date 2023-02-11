
<h2>Concept</h2>

The following screen snippet shows an integration concept based on the Azure services such as an Azure Event Grid (AEG) and Azure Function (AF):

<img width="381" alt="TTN2Azure" src="https://user-images.githubusercontent.com/30365471/218275872-442e536c-eada-400d-982e-43c099bd85fc.PNG">

As the above concept shows, the TTN server publishes the device messages on the AEG Custom Topic for distributing them using the loosely decoupled event driven PUB/SUB model to the destination targets.
This solution is very generic and easy for extension and filtering messages in the AEG model.

<h2>TTN Publisher</h2>

The TTN supports a Webhook integration for publishing an uplink messages. The following screen snippet shows a configuration of the Webhook as an AEG Publisher with a CloudEventSchema:


<img width="452" alt="TTN-WebhookToAzureEventGrid" src="https://user-images.githubusercontent.com/30365471/218276983-36786d1c-b9ae-421e-a3c9-ea5c40e75c1a.PNG">
