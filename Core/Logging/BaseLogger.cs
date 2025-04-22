using System;
using Microsoft.Extensions.Logging;
namespace Simple3DGame.Core.Logging
{
    /// <summary>
    /// Base abstract logger class that encapsulates Russian logging methods
    /// </summary>
    public abstract class BaseLogger
    {
        protected readonly ILogger _logger;

        protected BaseLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Base logging methods

        /// <summary>
        /// Logs informational message
        /// </summary>
        protected void LogInfo(string message, params object[] args) => 
            _logger.LogInformation(message, args);

        /// <summary>
        /// Logs warning message
        /// </summary>
        protected void LogWarning(string message, params object[] args) => 
            _logger.LogWarning(message, args);

        /// <summary>
        /// Logs error message with exception
        /// </summary>
        protected void LogError(Exception exception, string message, params object[] args) => 
            _logger.LogError(exception, message, args);

        /// <summary>
        /// Logs error message without exception
        /// </summary>
        protected void LogError(string message, params object[] args) => 
            _logger.LogError(message, args);

        /// <summary>
        /// Logs debug message
        /// </summary>
        protected void LogDebug(string message, params object[] args) => 
            _logger.LogDebug(message, args);

        #endregion
    }
}