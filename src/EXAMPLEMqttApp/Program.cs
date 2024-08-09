using HomeAssistantAddOn.Core;

namespace EXAMPLEMqttApp;

public class Program
{
    public static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddHomeAssistantAddOnConfig();
            })
            .ConfigureLogging((context, loggingBuilder) =>
            {
                var config = context.Configuration.Get<CommonOptions>();
                loggingBuilder
                .AddFilter(string.Empty, config!.LogLevel)
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });

            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
                services.AddOptions<EXAMPLEOptions>().
                    Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.Bind(settings);
                    });

                services.AddHomeAssistantMqtt();
            })
            .Build();

        host.Run();
    }
}