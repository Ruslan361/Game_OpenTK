using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
}