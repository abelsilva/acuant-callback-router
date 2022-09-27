using AcuantCallbackRouter.Exceptions;
using Microsoft.Extensions.Logging.Console;

namespace AcuantCallbackRouter;

public static class AppSetup
{
    public static void SetupLogging(ConfigurationManager configuration, ILoggingBuilder logging)
    {
        logging.Configure(options =>
        {
            options.ActivityTrackingOptions = ActivityTrackingOptions.None;
        });
        logging.ClearProviders();
        logging.AddConfiguration(configuration.GetSection("Logging"));
        logging.AddSimpleConsole(cfg =>
        {
            cfg.IncludeScopes = true;
            cfg.SingleLine = true;
            cfg.ColorBehavior = LoggerColorBehavior.Disabled;
            cfg.UseUtcTimestamp = true;
            cfg.TimestampFormat = "[yyyy-MM-ddTHH:mm:ss.fffffffK] ";
        });
    }

    public static void SetupConfiguration(IWebHostEnvironment env, IConfigurationBuilder config)
    {
        Console.WriteLine("Starting with environment name: " + env.EnvironmentName);
        var directoryInfo = new DirectoryInfo(".");
        var files = directoryInfo.GetFiles("appsettings*");
        var configFileFound = false;
        foreach (var file in files)
        {
            if (file.Name == $"appsettings.{env.EnvironmentName}.json")
            {
                configFileFound = true;
                Console.WriteLine("Found settings file: " + file.Name);
            }
            else
                Console.WriteLine("Found settings file: " + file.Name + " (IGNORED)");
        }

        if (!configFileFound)
            throw new ConfigurationException(
                $"Settings file not! Expected file: appsettings.{env.EnvironmentName}.json");

        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        config.AddEnvironmentVariables();
    }
}