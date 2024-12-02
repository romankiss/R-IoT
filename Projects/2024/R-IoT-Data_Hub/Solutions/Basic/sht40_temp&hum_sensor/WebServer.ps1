# Define the COM port and baud rate
$COMPort = "COM6"  # Change this to your COM port
$BaudRate = 115200    # Change this to your baud rate

# Create a SerialPort object
$serialPort = New-Object System.IO.Ports.SerialPort $COMPort, $BaudRate
$serialPort.ReadTimeout = 5000
$serialPort.Open()

$SerialPort.Write("AT" + "`r`n")
    $SerialPort.ReadLine()
    #Attention! moving-disconnecting the contact on the pins will force the device to reset - can be seen when there are no commands send, but the blue led on the adaper blinks. After each reset, there will be a "+READY" response generated - be aware of reading the newest line from the buffer(?)
    $SerialPort.Write("AT+RESET" + "`r`n")
    #after the reset command, there must be 2 reads, to get the full 2 responses, else later in the program the read will be reading the second newest, not the newest line from the buffer(?)
    $SerialPort.ReadLine()
    $SerialPort.ReadLine()

# Create a simple HTTP listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:8080/")
$listener.Start()

Write-Host "Listening for requests on http://localhost:8080/"

# Variable to store the latest data
$latestData = ""

# Function to read data from the serial port
function Read-SerialData {
    if ($serialPort.IsOpen -and $serialPort.BytesToRead -gt 0) {
        return $serialPort.ReadLine().Trim()
    }
    return ""
}

# Start a background job to read data every second
$job = Start-Job -ScriptBlock {
    param($serialPort, [ref]$latestData)
    while ($true) {
        $data = Read-SerialData
        Write-Host $($data)
        if ($data -ne "") {

            # Assuming $packet is the input string
            $packet = $data.TrimEnd("`r", "`n")  # Trim the carriage return and newline characters

            # Split the packet into two parts using '=' as the delimiter
            $rcv = $packet -split '=', 2

            # Check if the split resulted in exactly two parts
            if ($rcv.Length -ne 2) {
                return $null
            }

            # Split the second part into arguments using ',' as the delimiter
            $args = $rcv[1] -split ',', 5

            # Check if the split resulted in exactly five arguments
            if ($args.Length -ne 5) {
                return $null
            }

            # Continue processing with $args if needed


            $latestData.Value = $args[2]
            Write-Host $args[2]
        }
        Start-Sleep -Seconds 1
    }
} -ArgumentList $serialPort, [ref]$latestData

# Main loop to handle HTTP requests
try {
    while ($true) {
        # Wait for an incoming request
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response

        if ($request.Url.AbsolutePath -eq "/") {
            # Serve the HTML page
            $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Serial Data Display</title>

        <style>

        canvas {

            border: 1px solid black;

        }

    </style>
</head>
<body>
    <h1>Serial Data</h1>
    <div id="data"></div>
    <canvas id="graph" width="150" height="100"></canvas>



        <script>
        var hum = 50;
        var temp = 20;
        function fetchData() {

    fetch('/data')

        .then(response => response.json())

        .then(data => {

            console.log(data); // Log the entire data object to inspect its structure

            document.getElementById('data').innerText = data.data;

            const jsonObject = JSON.parse(data.data);
            hum = jsonObject.Hum; 
            temp = jsonObject.Temp;// This may return undefined if the structure is not as expected

        })

        .catch(error => console.error('Error fetching data:', error));

}

         // Fetch data every second







const canvas = document.getElementById('graph');
const ctx = canvas.getContext('2d');

const dataPoints = [];
const tempdataPoints = [];
const maxDataPoints = 100;
const graphWidth = canvas.width;
const graphHeight = canvas.height;

function drawGraph() {
    ctx.clearRect(0, 0, graphWidth, graphHeight); // Clear the canvas

    // Draw the axes
    ctx.beginPath();
    ctx.moveTo(0, graphHeight);
    ctx.lineTo(graphWidth, graphHeight);
    ctx.moveTo(0, 0);
    ctx.lineTo(0, graphHeight);
    ctx.strokeStyle = '#000';
    ctx.lineWidth = 5;
    ctx.stroke();

            // Draw the middle line

        ctx.beginPath();

        ctx.moveTo(0, graphHeight / 2); // Start at the left middle

        ctx.lineTo(graphWidth, graphHeight / 2); // Draw to the right middle

        ctx.strokeStyle = 'red'; // Color of the middle line

        ctx.lineWidth = 2; // Width of the middle line

        ctx.stroke();

    
    // Draw the data points
    ctx.beginPath();
    dataPoints.forEach((point, index) => {
        const x = (index / maxDataPoints) * graphWidth;
        const y = graphHeight - point; // Invert the y-axis
        ctx.lineTo(x, y);
    });
    ctx.strokeStyle = 'blue';
    ctx.lineWidth = 5;
    ctx.stroke();



        ctx.beginPath();
    tempdataPoints.forEach((point, index) => {
        const x = (index / maxDataPoints) * graphWidth;
        const y = graphHeight - point; // Invert the y-axis
        ctx.lineTo(x, y);
    });
    ctx.strokeStyle = 'yellow';
    ctx.lineWidth = 5;
    ctx.stroke();
    console.log("Frame updated");
}

function updateData() {
    // Generate a new random data point and add it to the dataPoints array
    const newPoint = hum; // Random height
    console.log("HUM EXTRACTED:"+hum);
    dataPoints.push(newPoint);

    // Keep only the latest maxDataPoints
    if (dataPoints.length > maxDataPoints) {
        dataPoints.shift();
    }


    tempdataPoints.push(temp);

    // Keep only the latest maxDataPoints
    if (tempdataPoints.length > maxDataPoints) {
        tempdataPoints.shift();
    }
}

function animate() {
    updateData();
    drawGraph();
    //requestAnimationFrame(animate); // Call animate again for the next frame
}

// Start the animation after its loaded (DOM)
function pageUpdater(){
fetchData();
animate();
}


setInterval(pageUpdater, 5000);

    </script>
</body>
</html>
"@
            $buffer = [System.Text.Encoding]::UTF8.GetBytes($html)
            $response.ContentLength64 = $buffer.Length
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
            $response.OutputStream.Close()
        } elseif ($request.Url.AbsolutePath -eq "/data") {
            # Return the latest serial data as JSON




            $data = Read-SerialData
            Write-Host $($data)
        

            # Assuming $packet is the input string
            $packet = $data.TrimEnd("`r", "`n")  # Trim the carriage return and newline characters

            # Split the packet into two parts using '=' as the delimiter
            $rcv = $packet -split '=', 2

           

            # Split the second part into arguments using ',' as the delimiter
            $args = $rcv[1] -split ',', 6 #the "," in the json causes to split the data payload into two coz it continues a "," (json)


            # Continue processing with $args if needed


            
            Write-Host $args[2] + "," + $args[3]





            $jsonResponse = @{ data = ($args[2] +"," + $args[3])} | ConvertTo-Json
            $buffer = [System.Text.Encoding]::UTF8.GetBytes($jsonResponse)
            $response.ContentType = "application/json"
            $response.ContentLength64 = $buffer.Length
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
            $response.OutputStream.Close()
        } else {
            # Handle 404 Not Found
            $response.StatusCode = 404
            $response.OutputStream.Close()
        }
    }
} finally {
    # Clean up
    Stop-Job $job
    $serialPort.Close()
    $listener.Stop()
}
