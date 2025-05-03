using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simple3DGame.Config;
namespace Simple3DGame.Core.Logging
{
    /// <summary>
    /// Application logger with Russian language messages
    /// </summary>
    public static class ApplicationLogger
    {
        private static ILoggerFactory? _loggerFactory;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            if (_loggerFactory == null)
            {
                Console.WriteLine("Error: ILoggerFactory not found in service provider.");
                // Fallback or throw exception
                _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            }
        }

        // Correct way to create logger using the factory
        public static ILogger<T> CreateLogger<T>()
        {
            if (_loggerFactory == null)
            {
                throw new InvalidOperationException("ApplicationLogger not initialized. Call Initialize first.");
            }
            return _loggerFactory.CreateLogger<T>();
        }

        // Add Cleanup method
        public static void Cleanup()
        {
            _loggerFactory?.Dispose();
            _loggerFactory = null;
            Console.WriteLine("ApplicationLogger cleaned up.");
        }

        /// <summary>
        /// Logs application started message
        /// </summary>
        public static void Started() => 
            _loggerFactory?.CreateLogger("Application").LogInformation(ConfigSettings.RuLogMessages.ApplicationStarted);

        /// <summary>
        /// Logs application stopped message
        /// </summary>
        public static void Stopped() => 
            _loggerFactory?.CreateLogger("Application").LogInformation(ConfigSettings.RuLogMessages.ApplicationStopped);
    }

}