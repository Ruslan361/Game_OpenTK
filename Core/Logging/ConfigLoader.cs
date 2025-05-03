using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Simple3DGame.Config;

namespace Simple3DGame.Core.Logging
{
    /// <summary>
    /// Configuration logger with Russian language messages
    /// </summary>
    public class ConfigLogger : BaseLogger
    {
        public ConfigLogger(ILogger logger) : base(logger) { }

        /// <summary>
        /// Logs paths initialized message
        /// </summary>
        public void PathsInitialized(string path) => 
            LogInfo(ConfigSettings.RuLogMessages.AssetPathsInitialized, path);

        /// <summary>
        /// Logs directory creation message
        /// </summary>
        public void DirectoryCreating(string path) => 
            LogInfo(ConfigSettings.RuLogMessages.CreatingDirectory, path);
    }

    /// <summary>
    /// Handles loading of configuration from various sources
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Loads the application configuration settings
        /// </summary>
        public static ConfigSettings LoadConfig()
        {
            try
            {
                // Create a temporary logger factory for bootstrap configuration
                var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
                
                // Create logger for ConfigSettings
                var logger = loggerFactory.CreateLogger<ConfigSettings>();
                
                // Create configuration with required logger
                var config = new ConfigSettings(logger);
                
                // Set window settings
                config.WindowWidth = 1280;
                config.WindowHeight = 720;
                config.WindowTitle = "Simple 3D Game";

                // In a real app, you might load from a config file
                // var configBuilder = new ConfigurationBuilder()
                //    .SetBasePath(Directory.GetCurrentDirectory())
                //    .AddJsonFile("appsettings.json", optional: true)
                //    .Build();
                // 
                // configBuilder.GetSection("WindowSettings").Bind(config);

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                
                // Create a minimal configuration instead of returning null
                var loggerFactory = LoggerFactory.Create(builder => {
                    builder.AddConsole();
                });
                
                var logger = loggerFactory.CreateLogger<ConfigSettings>();
                return new ConfigSettings(logger);
            }
        }

        /// <summary>
        /// Loads the logging configuration
        /// </summary>
        public static IConfiguration LoadLoggingConfig()
        {
            try 
            {
                // In a real app, you would load from a config file
                // For now, return a minimal configuration
                return new ConfigurationBuilder().Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading logging configuration: {ex.Message}");
                return new ConfigurationBuilder().Build(); // Return empty config
            }
        }
    }
}