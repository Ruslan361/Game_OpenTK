using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Simple3DGame.Core.Logging;

namespace Simple3DGame.Core
{
    /// <summary>
    /// Contains extension methods for registering logging services in the DI container
    /// </summary>
    public static class LoggingServiceExtensions
    {
        /// <summary>
        /// Adds GameLogger to the DI container
        /// </summary>
        public static IServiceCollection AddGameLogger(this IServiceCollection services)
        {
            // Register GameLogger<T> for each type
            services.AddSingleton(typeof(GameLogger<>));
            
            return services;
        }

        /// <summary>
        /// Adds Russian loggers to the DI container
        /// </summary>
        public static IServiceCollection AddRuLoggers(this IServiceCollection services)
        {
            // Register loggers by type
            services.AddTransient<ApplicationLogger>(provider => 
                new ApplicationLogger(provider.GetRequiredService<ILogger<Program>>()));
                
            services.AddTransient<WorldLogger>(provider => 
                new WorldLogger(provider.GetRequiredService<ILogger<World>>()));
                
            services.AddTransient<ConfigLogger>(provider => 
                new ConfigLogger(provider.GetRequiredService<ILogger<Config.ConfigSettings>>()));
                
            services.AddTransient<GameLogger>(provider => 
                new GameLogger(provider.GetRequiredService<ILogger<Game>>()));
            
            return services;
        }
    }

    /// <summary>
    /// Generic version of GameLogger, injecting ILogger<T> for typed logging
    /// </summary>
    public class GameLogger<T> : GameLogger
    {
        public GameLogger(ILogger<T> logger) : base(logger)
        {
        }
    }
}