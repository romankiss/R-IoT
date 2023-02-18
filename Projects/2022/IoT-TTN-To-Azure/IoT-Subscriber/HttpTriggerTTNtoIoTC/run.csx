#r "Newtonsoft.Json"

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public static async Task<ActionResult> Run(JObject eventGridEvent, HttpRequest req, ILogger log)
{ 
    if (req.Method == HttpMethod.Options.ToString())
    {
        log.LogInformation("CloudEventSchema validation");
        req.HttpContext.Response.Headers.Add("Webhook-Allowed-Origin", req.Headers["WebHook-Request-Origin"].FirstOrDefault()?.Trim());
        return (ActionResult)new OkResult();
    }

    //log.LogInformation(eventGridEvent.ToString());

    // consumer of telemetry (iot central)
    uint sasTokenTTLInHrs = 1;
    string iotcScopeId = req.Headers["iotc-scopeId"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("AzureIoTC_scopeId");
    string iotcSasToken = req.Headers["iotc-sasToken"].FirstOrDefault() ?? Environment.GetEnvironmentVariable("AzureIoTC_sasToken");

    // mandatory properties from the TTN CloudEvent message
    string type = eventGridEvent["type"]?.Value<string>();
    var data = eventGridEvent["data"];
    string deviceId = data?["end_device_ids"]?["device_id"]?.Value<string>();
    var uplink_message = data?["uplink_message"];

    if (type == "lorawan" && !string.IsNullOrEmpty(deviceId) && Regex.IsMatch(deviceId, @"^[a-z0-9\-]+$") && uplink_message != null)
    {   
        // received by gateway
        var rx_metadata = uplink_message["rx_metadata"]?[0];             
        var enqueuedtime = data["received_at"]?.Value<DateTime>().ToString("o");
        // option: modelId in the decoded_payload
        var modelId = uplink_message["decoded_payload"]?["_modelId"]?.Value<string>();
        ((JObject)uplink_message["decoded_payload"])?.Remove("_modelId");
        // option: device group (device prefix)
        var deviceGroup = req.Headers["iotc-device-group"].FirstOrDefault();
        deviceId = $"{(deviceGroup == null ? "" : deviceGroup + "-")}{deviceId}";

        // lora telemetry data 
        var loraTelemetry = new
        {
            device_eui = data["end_device_ids"]?["dev_eui"]?.Value<string>(),
            gateway_id = rx_metadata?["gateway_ids"]?["gateway_id"]?.Value<string>(),
            rssi = rx_metadata?["rssi"]?.Value<Int32>(),
            fcnt = uplink_message["f_cnt"]?.Value<ulong>(),
            payload = BitConverter.ToString(Convert.FromBase64String(uplink_message["frm_payload"]?.Value<string>() ?? "")).Replace("-", ""),
            sensor = uplink_message["decoded_payload"]?.Value<JObject>(),
            deviceInfo = uplink_message["version_ids"]?.Value<JObject>(),
        };
        // remove null properties
        var jsontext = JsonConvert.SerializeObject(loraTelemetry, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        try
        {
            // send telemetry data to the IoT Central
            var info = await Connectivity.GetConnectionInfo(deviceId, modelId, iotcScopeId, iotcSasToken, log, sasTokenTTLInHrs);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", info.SasToken);
                client.DefaultRequestHeaders.Add("iothub-app-iothub-creation-time-utc", enqueuedtime);
                var response = await client.PostAsync(info.RequestUri, new StringContent(jsontext, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
            log.LogInformation($"POST: {info.RequestUri}\r\n{jsontext}");
        }
        catch (Exception ex)
        {
            log.LogError(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            Connectivity.RemoveDevice(iotcScopeId, deviceId);
            throw ex; // for retrying and deadlettering undeliverable message
        }
    }
    else
    {
        log.LogWarning($"Wrong event message:\r\n{eventGridEvent}");
    }
    return (ActionResult)new OkResult();
}

class ConnectivityInfo
{
    public string IoTHubName { get; set; }
    public string RequestUri { get; set; }
    public string SasToken { get; set; }
    public ulong SaSExpiry { get; set; }
    public string ModelId { get; set; }
    public string DeviceConnectionString { get; set; }
}


static class Connectivity
{
    static Dictionary<string, ConnectivityInfo> devices = new Dictionary<string, ConnectivityInfo>();

    public static async Task<ConnectivityInfo> GetConnectionInfo(string deviceId, string modelId, string iotcScopeId, string iotcSasToken, ILogger log, uint sasTokenTTLInHrs = 24, int retryCounter = 10, int pollingTimeInSeconds = 3)
    {
        string xdeviceId = $"_{iotcScopeId}_{deviceId}";
        if (devices.ContainsKey(xdeviceId))
        {
            if (!string.IsNullOrEmpty(modelId) && devices[xdeviceId].ModelId != modelId)
            {
                log.LogWarning($"Reprovissiong device with new model");
                devices.Remove(xdeviceId);
            }
            else
            {
                if (!SharedAccessSignatureBuilder.IsValidExpiry(devices[xdeviceId].SaSExpiry, 100))
                {
                    log.LogWarning($"Refreshing sasToken");
                    devices[xdeviceId].SasToken = SharedAccessSignatureBuilder.GetSASTokenFromConnectionString(devices[xdeviceId].DeviceConnectionString, sasTokenTTLInHrs);
                    devices[xdeviceId].SaSExpiry = ulong.Parse(SharedAccessSignatureBuilder.GetExpiry(sasTokenTTLInHrs));
                }
                return devices[xdeviceId];
            }
        }

        string deviceKey = SharedAccessSignatureBuilder.ComputeSignature(iotcSasToken, deviceId);
        string address = $"https://global.azure-devices-provisioning.net/{iotcScopeId}/registrations/{deviceId}/register?api-version=2021-06-01";
        string sas = SharedAccessSignatureBuilder.GetSASToken($"{iotcScopeId}/registrations/{deviceId}", deviceKey, "registration", 1);

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", sas);
            client.DefaultRequestHeaders.Add("accept", "application/json");
            string jsontext = string.IsNullOrEmpty(modelId) ? null : $"{{ \"modelId\":\"{modelId}\" }}";
            var response = await client.PutAsync(address, new StringContent(JsonConvert.SerializeObject(new { registrationId = deviceId, payload = jsontext }), Encoding.UTF8, "application/json"));

            var atype = new { errorCode = "", message = "", operationId = "", status = "", registrationState = new JObject() };
            do
            {
                dynamic operationStatus = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), atype);
                if (!string.IsNullOrEmpty(operationStatus.errorCode))
                {
                    throw new Exception($"{operationStatus.errorCode} - {operationStatus.message}");
                }
                response.EnsureSuccessStatusCode();
                if (operationStatus.status == "assigning")
                {
                    Task.Delay(TimeSpan.FromSeconds(pollingTimeInSeconds)).Wait();
                    address = $"https://global.azure-devices-provisioning.net/{iotcScopeId}/registrations/{deviceId}/operations/{operationStatus.operationId}?api-version=2021-06-01";
                    response = await client.GetAsync(address);
                }
                else if (operationStatus.status == "assigned")
                {
                    var cinfo = new ConnectivityInfo();
                    cinfo.ModelId = modelId;
                    cinfo.IoTHubName = operationStatus.registrationState.assignedHub;
                    cinfo.DeviceConnectionString = $"HostName={cinfo.IoTHubName};DeviceId={deviceId};SharedAccessKey={deviceKey}";
                    cinfo.RequestUri = $"https://{cinfo.IoTHubName}/devices/{deviceId}/messages/events?api-version=2021-04-12";
                    cinfo.SasToken = SharedAccessSignatureBuilder.GetSASToken($"{cinfo.IoTHubName}/{deviceId}", deviceKey, null, sasTokenTTLInHrs);
                    cinfo.SaSExpiry = ulong.Parse(SharedAccessSignatureBuilder.GetExpiry(sasTokenTTLInHrs));
                    devices.Add(xdeviceId, cinfo);
                    log.LogInformation($"DeviceConnectionString: {cinfo.DeviceConnectionString}");                        
                    return cinfo;
                }
                else
                {
                    throw new Exception($"{operationStatus.registrationState.status}: {operationStatus.registrationState.errorCode} - {operationStatus.registrationState.errorMessage}");
                }
            } while (--retryCounter > 0);

            throw new Exception("Registration device status retry timeout exprired, try again.");
        }
    }

    public static void RemoveDevice(string iotcScopeId, string deviceId)
    {
        string xdeviceId = $"_{iotcScopeId}_{deviceId}";
        if (devices.ContainsKey(xdeviceId))
            devices.Remove(xdeviceId);
    }
}

public sealed class SharedAccessSignatureBuilder
{
    public static string GetHostNameNamespaceFromConnectionString(string connectionString)
    {
        return GetPartsFromConnectionString(connectionString)["HostName"].Split('.').FirstOrDefault();
    }
    public static string GetSASTokenFromConnectionString(string connectionString, uint hours = 24)
    {
        var parts = GetPartsFromConnectionString(connectionString);
        if (parts.ContainsKey("HostName") && parts.ContainsKey("SharedAccessKey"))
            return GetSASToken(parts["HostName"], parts["SharedAccessKey"], parts.Keys.Contains("SharedAccessKeyName") ? parts["SharedAccessKeyName"] : null, hours);
        else
            return string.Empty;
    }
    public static string GetSASToken(string resourceUri, string key, string keyName = null, uint hours = 24)
    {
        try
        {
            var expiry = GetExpiry(hours);
            string stringToSign = System.Web.HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            var signature = SharedAccessSignatureBuilder.ComputeSignature(key, stringToSign);
            var sasToken = keyName == null ?
                String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}", System.Web.HttpUtility.UrlEncode(resourceUri), System.Web.HttpUtility.UrlEncode(signature), expiry) :
                String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", System.Web.HttpUtility.UrlEncode(resourceUri), System.Web.HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
        catch
        {
            return string.Empty;
        }
    }

    #region Helpers
    public static string ComputeSignature(string key, string stringToSign)
    {
        using (HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key)))
        {
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        }
    }

    public static Dictionary<string, string> GetPartsFromConnectionString(string connectionString)
    {
        return connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Split(new[] { '=' }, 2)).ToDictionary(x => x[0].Trim(), x => x[1].Trim(), StringComparer.OrdinalIgnoreCase);
    }

    // default expiring = 24 hours
    public static string GetExpiry(uint hours = 24)
    {
        TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return Convert.ToString((ulong)sinceEpoch.TotalSeconds + 3600 * hours);
    }

    public static DateTime GetDateTimeUtcFromExpiry(ulong expiry)
    {
        return (new DateTime(1970, 1, 1)).AddSeconds(expiry);
    }
    public static bool IsValidExpiry(ulong expiry, ulong toleranceInSeconds = 0)
    {
        return GetDateTimeUtcFromExpiry(expiry) - TimeSpan.FromSeconds(toleranceInSeconds) > DateTime.UtcNow;
    }
    #endregion
}
