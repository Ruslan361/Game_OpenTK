using System;
using Microsoft.Extensions.Logging;
using Simple3DGame.Config;
namespace Simple3DGame.Core.Logging
{
    /// <summary>
    /// Application logger with Russian language messages
    /// </summary>
    public class ApplicationLogger : BaseLogger
    {
        public ApplicationLogger(ILogger logger) : base(logger) { }

        /// <summary>
        /// Logs application started message
        /// </summary>
        public void Started() => 
            LogInfo(ConfigSettings.RuLogMessages.ApplicationStarted);

        /// <summary>
        /// Logs application stopped message
        /// </summary>
        public void Stopped() => 
            LogInfo(ConfigSettings.RuLogMessages.ApplicationStopped);
    }

}