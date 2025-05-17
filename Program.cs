using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Simple3DGame.Config;
using Simple3DGame.Core.Logging;
using System;
using System.IO;

namespace Simple3DGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- Configuration Setup ---
            var config = ConfigLoader.LoadConfig();
            if (config == null)
            {
                Console.WriteLine("Failed to load configuration. Exiting.");
                return;
            }

            // --- Dependency Injection Setup ---
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, config);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // --- Initialize Logging ---
            // Initialize the static ApplicationLogger with the factory from DI
            ApplicationLogger.Initialize(serviceProvider);
            var logger = ApplicationLogger.CreateLogger<Program>(); // Get logger for Program

            logger.LogInformation("Application starting...");

            // --- Game Window Settings ---
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(config.WindowWidth, config.WindowHeight),
                Title = config.WindowTitle,
                // Optional: Add other settings like VSync, StartVisible, StartFocused, etc.
            };

            // --- Run Game ---
            try
            {
                // Resolve Game instance from DI container
                using (var game = serviceProvider.GetRequiredService<Game>())
                {
                    // Pass nativeWindowSettings when creating Game or set them here if needed
                    // Note: Game constructor now takes settings from DI/Config
                    game.Run();
                }
                logger.LogInformation("Application finished running.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Unhandled exception during game execution.");
            }
            finally
            {
                // --- Cleanup ---
                logger.LogInformation("Cleaning up resources...");
                ApplicationLogger.Cleanup(); // Clean up logging factory
                serviceProvider.Dispose(); // Dispose DI container
                logger.LogInformation("Cleanup complete. Exiting.");
            }
        }

        private static void ConfigureServices(IServiceCollection services, ConfigSettings config)
        {
            // Add logging services
            services.AddLogging(builder =>
            {
                builder.ClearProviders(); // Optional: Remove default providers
                builder.AddConfiguration(ConfigLoader.LoadLoggingConfig()); // Load logging config
                builder.AddConsole();
                builder.AddDebug();
                // Add file logging provider (e.g., Serilog, NLog, or custom)
                builder.AddProvider(new FileLoggerProvider(config.LogFilePath)); // Assuming FileLoggerProvider exists
                builder.SetMinimumLevel(LogLevel.Trace); // Set global minimum level
            });

            // Register configuration instance
            services.AddSingleton(config);

            // Register Game specific settings (optional, could be part of ConfigSettings)
            services.AddSingleton(new GameWindowSettings
            {
                // Settings for GameWindow base class (e.g., UpdateFrequency)
                // RenderFrequency = 60.0,
                UpdateFrequency = 60.0
            });
            services.AddSingleton(new NativeWindowSettings
            {
                ClientSize = new Vector2i(config.WindowWidth, config.WindowHeight),
                Title = config.WindowTitle,
                // Flags = ContextFlags.ForwardCompatible, // Optional
            });

            // Register Game class
            // Game depends on GameWindowSettings, NativeWindowSettings, ILogger<Game>, ConfigSettings
            services.AddTransient<Game>(); // Use Transient or Singleton as appropriate

            // Register other services if needed (e.g., World, Systems, Resource Managers)
            // services.AddSingleton<World>(); // Example if World was managed by DI
            // services.AddTransient<RenderSystem>(); // Example
        }
    }

    // Dummy FileLoggerProvider for ConfigureServices - Replace with actual implementation
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFilePath;
        public FileLoggerProvider(string logFilePath)
        {
            _logFilePath = logFilePath;
            // Ensure log directory exists
            var logDir = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }
        public ILogger CreateLogger(string categoryName) => new FileLogger(_logFilePath, categoryName);
        public void Dispose() { /* Nothing to dispose in this simple version */ }
    }

    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;
        private static readonly object _lock = new object();

        public FileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        // Implement explicit interface to avoid nullability warning
        IDisposable ILogger.BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information; // Log Info and above

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var logRecord = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}";
            if (exception != null)
            {
                logRecord += $"\nException: {exception}";
            }

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_filePath, logRecord + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to log file: {ex.Message}");
                }
            }
        }
    }
}
