using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Simple3DGame.Core;
using Simple3DGame.Config;
using System;

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
                    builder.SetMinimumLevel(LogLevel.Information);
                })
                .AddSingleton<ConfigSettings>()
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger?.LogInformation("Starting Simple3DGame");

            var configSettings = serviceProvider.GetRequiredService<ConfigSettings>();
            var gameLogger = serviceProvider.GetRequiredService<ILogger<Game>>();

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
        }
    }
}
