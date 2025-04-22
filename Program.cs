using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Simple3DGame.Core;
using Simple3DGame.Config;
using System;
using System.IO;
using Simple3DGame.Core.Logging;

namespace Simple3DGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => 
                {
                    builder.AddConsole();
                    
                    // Временная инициализация ConfigSettings для получения пути к логам
                    var tempLogger = new LoggerFactory().CreateLogger<ConfigSettings>();
                    var tempConfig = new ConfigSettings(tempLogger);
                    
                    // Добавление логирования в файл
                    builder.AddFile(tempConfig.LogFilePath);
                    builder.SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<ConfigSettings>()
                // Добавляем русскоязычные логгеры в контейнер
                .AddRuLoggers()
                .BuildServiceProvider();

            // Получаем логгер приложения
            var logger = serviceProvider.GetRequiredService<ApplicationLogger>();
            logger.Started();

            var configSettings = serviceProvider.GetRequiredService<ConfigSettings>();
            var gameLogger = serviceProvider.GetRequiredService<GameLogger>();

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Simple 3D Game",
                // This is needed to run on macos
                Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible,
            };

            // Use the Game class
            using (var game = new Game(GameWindowSettings.Default, nativeWindowSettings, gameLogger, configSettings))
            {
                game.Run();
            }
            
            logger.Stopped();
        }
    }
    
    // Расширение для добавления логирования в файл
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
        {
            // Убедимся, что директория существует
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            builder.AddProvider(new FileLoggerProvider(filePath));
            return builder;
        }
    }
    
    // Провайдер для логирования в файл
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        
        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_filePath);
        }
        
        public void Dispose() { }
    }
    
    // Класс для логирования в файл
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();
        
        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }
        
        public IDisposable BeginScope<TState>(TState state) => null;
        
        public bool IsEnabled(LogLevel logLevel) => true;
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
                
            var message = formatter(state, exception);
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";
            
            lock (_lock)
            {
                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
        }
    }
}
