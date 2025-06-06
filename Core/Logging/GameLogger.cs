using Microsoft.Extensions.Logging;
using OpenTK.Mathematics; // For Vector2i
using Simple3DGame.Config;
using System;

namespace Simple3DGame.Core.Logging
{
    /// <summary>
    /// Game logger with Russian language messages
    /// </summary>
    public class GameLogger : BaseLogger
    {
        private readonly new ILogger _logger;
        
        public GameLogger(ILogger logger) : base(logger) 
        { 
            _logger = logger;
        }

        /// <summary>
        /// Logs game initialized message
        /// </summary>
        public void Initialized(Vector2i size) => 
            _logger.LogInformation($"Игра инициализирована с размером: {size.X}x{size.Y}");

        /// <summary>
        /// Logs initialization start message
        /// </summary>
        public void InitializationStarted() => 
            LogInfo("Начало инициализации...");

        /// <summary>
        /// Logs OpenGL settings message
        /// </summary>
        public void OpenGLSettingsApplied() => 
            LogInfo("OpenGL настройки установлены");

        /// <summary>
        /// Logs camera initialized message
        /// </summary>
        public void CameraInitialized() => 
            LogInfo("Камера инициализирована");

        /// <summary>
        /// Logs game world creation message
        /// </summary>
        public void WorldCreating() => 
            LogInfo("Создание игрового мира...");

        /// <summary>
        /// Logs game world created message
        /// </summary>
        public void WorldCreated() => 
            LogInfo("Игровой мир создан");

        /// <summary>
        /// Logs game world loading message
        /// </summary>
        public void WorldLoading() => 
            LogInfo("Загрузка игрового мира...");

        /// <summary>
        /// Logs game world loaded message
        /// </summary>
        public void WorldLoaded() => 
            LogInfo("Игровой мир загружен");

        /// <summary>
        /// Logs OnLoad completed message
        /// </summary>
        public void OnLoadCompleted() => 
            LogInfo("OnLoad завершен успешно");

        /// <summary>
        /// Logs exit signal message
        /// </summary>
        public void ExitSignalReceived() => 
            LogInfo("Получен сигнал выхода");

        /// <summary>
        /// Logs window resize message
        /// </summary>
        public void WindowResized(int width, int height) => 
            LogInfo($"Изменен размер окна: {width}x{height}");

        /// <summary>
        /// Logs game completed message
        /// </summary>
        public void Completed(TimeSpan runtime) => 
            LogInfo($"Игра завершена. Общее время работы: {runtime}");

        /// <summary>
        /// Logs game initialization error
        /// </summary>
        public void InitializationError(Exception exception) => 
            LogError(exception, "Ошибка при инициализации игры");

        // Add missing specific methods
        public void GameLoading() => 
            _logger.LogInformation("Игра загружается...");

        public void GameLoadedSuccessfully() => 
            _logger.LogInformation("Игра успешно загружена.");

        public void GameUnloading() => 
            _logger.LogInformation("Игра выгружается...");

        public void GameUnloadedSuccessfully() => 
            _logger.LogInformation("Игра успешно выгружена.");

        // Expose generic logging methods
        public void LogInformation(string message) => 
            _logger.LogInformation(message);

        public void LogWarning(string message) => 
            _logger.LogWarning(message);

        public void LogError(string message, Exception? ex = null) => 
            _logger.LogError(ex, message);

        public void LogCritical(string message, Exception? ex = null) => 
            _logger.LogCritical(ex, message);
    }
}