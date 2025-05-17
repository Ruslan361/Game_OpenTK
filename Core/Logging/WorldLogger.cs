using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Simple3DGame.Core.Logging;
using Simple3DGame.Config;


namespace Simple3DGame.Core
{
    /// <summary>
    /// World logger with Russian language messages
    /// </summary>
    public class WorldLogger : BaseLogger
    {
        
        public WorldLogger(ILogger logger) : base(logger) { }

        /// <summary>
        /// Logs world initialized message
        /// </summary>
        public void Initialized() => 
            LogInfo(ConfigSettings.RuLogMessages.WorldInitialized);

        /// <summary>
        /// Logs world loading started message
        /// </summary>
        public void LoadingStarted() => 
            LogInfo(ConfigSettings.RuLogMessages.StartingWorldLoading);

        /// <summary>
        /// Logs shader directory path
        /// </summary>
        public void ShadersDirectory(string path) => 
            LogInfo(ConfigSettings.RuLogMessages.UsingShaderDirectory, path);

        /// <summary>
        /// Logs models directory path
        /// </summary>
        public void ModelsDirectory(string path) => 
            LogInfo(ConfigSettings.RuLogMessages.UsingModelsDirectory, path);

        /// <summary>
        /// Logs shader loading message
        /// </summary>
        public void ShadersLoading() => 
            LogInfo(ConfigSettings.RuLogMessages.LoadingShaders);

        /// <summary>
        /// Logs diffuse texture copied message
        /// </summary>
        public void DiffuseTextureCopied(string source, string destination) => 
            LogInfo(ConfigSettings.RuLogMessages.CopiedDefaultDiffuseTexture, source, destination);
            
        /// <summary>
        /// Alternative name for backward compatibility
        /// </summary>
        public void CopyingDefaultDiffuseTexture(string source, string destination) => 
            DiffuseTextureCopied(source, destination);

        /// <summary>
        /// Logs specular texture copied message
        /// </summary>
        public void SpecularTextureCopied(string source, string destination) => 
            LogInfo(ConfigSettings.RuLogMessages.CopiedDefaultSpecularTexture, source, destination);
            
        /// <summary>
        /// Alternative name for backward compatibility
        /// </summary>
        public void CopyingSpecularTexture(string source, string destination) => 
            SpecularTextureCopied(source, destination);

        /// <summary>
        /// Logs model loading message
        /// </summary>
        public void ModelLoading(string path) => 
            LogInfo(ConfigSettings.RuLogMessages.LoadingModel, path);

        /// <summary>
        /// Logs world successfully loaded message
        /// </summary>
        public void LoadedSuccessfully() => 
            LogInfo(ConfigSettings.RuLogMessages.WorldLoadedSuccessfully);
            
        /// <summary>
        /// Alternative name for backward compatibility
        /// </summary>
        public void WorldLoadingSuccessful() => 
            LoadedSuccessfully();

        /// <summary>
        /// Logs diffuse texture not found warning
        /// </summary>
        public void DiffuseTextureNotFound(string path) => 
            LogWarning(ConfigSettings.RuLogMessages.DefaultDiffuseTextureNotFound, path);

        /// <summary>
        /// Logs specular texture not found warning
        /// </summary>
        public void SpecularTextureNotFound(string path) => 
            LogWarning(ConfigSettings.RuLogMessages.DefaultSpecularTextureNotFound, path);

        /// <summary>
        /// Logs model file not found warning
        /// </summary>
        public void ModelNotFound(string path) => 
            LogWarning(ConfigSettings.RuLogMessages.ModelFileNotFound, path);

        /// <summary>
        /// Logs world resources loading error
        /// </summary>
        public void ResourcesLoadingError(Exception exception) => 
            LogError(exception, ConfigSettings.RuLogMessages.ErrorLoadingWorld, exception.Message);
            
        /// <summary>
        /// Alternative name for backward compatibility
        /// </summary>
        public void WorldResourcesLoadingError(Exception exception) => 
            ResourcesLoadingError(exception);

        // Expose specific logging methods needed by World.cs
        public void EntityCreated(int id) => 
            LogDebug($"Entity created: {id}");

        public void EntityDestroyed(int id) => 
            LogDebug($"Entity destroyed: {id}");

        public void ComponentAdded(int entityId, string componentType) => 
            LogTrace($"Component {componentType} added to entity {entityId}");

        public void ComponentRemoved(int entityId, string componentType) => 
            LogTrace($"Component {componentType} removed from entity {entityId}");

        public void SystemAdded(string systemType) => 
            LogInformation($"System added: {systemType}");

        public void SystemRemoved(string systemType) => 
            LogInformation($"System removed: {systemType}");

        // Expose generic logging methods
        public void LogInformation(string message) => 
            LogInfo(message);

        public void LogWarning(string message) => 
            LogWarning(message);

        public void LogError(string message, Exception? ex = null)
        {
            if (ex != null)
                LogError(ex, message);
            else
                LogError(message);
        }

        public void LogCritical(string message, Exception? ex = null)
        {
            if (ex != null)
                LogCritical(ex, message);
            else
                LogCritical(message);
        }
    }
}