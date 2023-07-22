using HomeAssistantAddOn.Mqtt;
using Microsoft.Extensions.Options;

namespace EXAMPLEMqttApp;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly MqttService _mqttService;
    private readonly IOptionsMonitor<EXAMPLEOptions> _optionsMonitor;

    public Worker(
        ILogger<Worker> logger
        , MqttService mqttService
        , IOptionsMonitor<EXAMPLEOptions> optionsMonitor
        )
    {
        _logger = logger;
        _mqttService = mqttService;
        _optionsMonitor = optionsMonitor;
    }

    private string ExampleDeviceId = "example";

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttService.StartAsync();

        //Initialize MQTT Device
        await PublishDeviceConfigsAsync();
        await PublishDeviceStatusAsync();

        SubscribeCommandTopic();

        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Timer Loop
        var timer = new PeriodicTimer(_optionsMonitor.CurrentValue.Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishDeviceStatusAsync();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttService.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    #region Configure Senser
    private async Task PublishDeviceConfigsAsync()
    {
        await PublishSensorConfigAsync(
            type: "serialnumber"
            , name: "Serial Number"
            , icon: "mdi:identifier");

        await PublishSensorConfigAsync(
            type: "example_battery_value"
            , name: "Battery"
            , device_class: "battery"
            , state_class: "measurement"
            , unit_of_measurement: "%");

        await PublishSensorConfigAsync(
            type: "example_apparent_power_value"
            , name: "Apparent Power"
            , device_class: "apparent_power"
            , state_class: "measurement"
            , unit_of_measurement: "VA");

        await PublishSensorConfigAsync(
            type: "timestamp"
            , name: "Last Update"
            , device_class: "timestamp"
            , value_template: "{% set ts = value_json.get('timestamp', {})  %} {% if ts %}\n  {{ (ts / 1000) | timestamp_local | as_datetime }}\n{% else %}\n  {{ this.state }}\n{% endif %}");

        await SendButtonConfigAsync("ExampleButton", "update");

    }

    private async Task PublishSensorConfigAsync(
        string type, string name
        , string? icon = null
        , string? device_class = null
        , string? state_class = null
        , string? unit_of_measurement = null, string? value_template = null)
    {
        var payload = new
        {
            icon,
            name,
            state_topic = $"homeassistant/sensor/{ExampleDeviceId}/state",
            unit_of_measurement,
            state_class,
            device_class,
            value_template = value_template ?? $"{{{{value_json.{type}}}}}",
            unique_id = $"{type}_{ExampleDeviceId}",
            object_id = $"{type}_{ExampleDeviceId}",
            device = new
            {
                identifiers = new[] { $"example_device_{ExampleDeviceId}" },
                name = "ExampleDevice",
            },
        };
        await _mqttService.PublishAsync($"homeassistant/sensor/{type}_{ExampleDeviceId}/config", payload, true);
    }

    private async Task SendButtonConfigAsync(string name, string device_class)
    {
        var payload = new
        {
            device_class,
            name,
            command_topic = $"homeassistant/button/{ExampleDeviceId}/cmd",
            payload_press = "do",
            unique_id = $"btn_{ExampleDeviceId}",
            object_id = $"btn_{ExampleDeviceId}",
            device = new
            {
                identifiers = new[] { $"example_device_{ExampleDeviceId}" },
                name = "ExampleDevice",
            },
        };
        await _mqttService.PublishAsync($"homeassistant/button/btn_{ExampleDeviceId}/config", payload, true);
    }
    #endregion

    #region Notifiy Senser Stauts

    public async Task PublishDeviceStatusAsync()
    {
        _logger.LogTrace("Trace:{TimeStamp}", DateTimeOffset.UtcNow);
        _logger.LogDebug("Debug:{TimeStamp}", DateTimeOffset.UtcNow);
        _logger.LogInformation("nformation:{TimeStamp}", DateTimeOffset.UtcNow);
        _logger.LogWarning("arning:{TimeStamp}", DateTimeOffset.UtcNow);
        _logger.LogError("Error:{TimeStamp}", DateTimeOffset.UtcNow);
        _logger.LogCritical("Critical:{TimeStamp}", DateTimeOffset.UtcNow);

        await _mqttService.PublishAsync($"homeassistant/sensor/{ExampleDeviceId}/state", new
        {
            serialnumber = ExampleDeviceId,
            example_battery_value = new Random().Next(0, 100),
            example_apparent_power_value = new Random().Next(100,2000),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }, false);
    }
    #endregion

    private void SubscribeCommandTopic()
    {
        _mqttService.Subscribe($"homeassistant/button/{ExampleDeviceId}/cmd", async (payload) =>
        {
            _logger.LogInformation("Receive:{payload}", payload);
            if (payload == "do")
            {
                await PublishDeviceStatusAsync();
            }
        });
    }


}