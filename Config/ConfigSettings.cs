using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Simple3DGame.Config
{
    public class ConfigSettings
    {
        private readonly ILogger<ConfigSettings> _logger;

        public ConfigSettings(ILogger<ConfigSettings> logger)
        {
            _logger = logger;
            
            // Initialize paths immediately in constructor
            AssetsRootPath = "Assets";
            ModelsPath = Path.Combine(AssetsRootPath, "Models");
            ShadersPath = Path.Combine(AssetsRootPath, "Shaders");
            TexturesPath = Path.Combine(AssetsRootPath, "Textures");
            LogsPath = Path.Combine("Logs");

            VertexShaderPath = Path.Combine(ShadersPath, "shader.vert");
            FragmentShaderPath = Path.Combine(ShadersPath, "shader.frag");
            LightingFragmentShaderPath = Path.Combine(ShadersPath, "lighting.frag");
            LogFilePath = Path.Combine(LogsPath, $"game_log_{DateTime.Now:yyyy-MM-dd}.txt");
            
            InitializePaths();
        }

        // Asset paths
        public string AssetsRootPath { get; private set; }
        public string ModelsPath { get; private set; }
        public string ShadersPath { get; private set; }
        public string TexturesPath { get; private set; }
        public string LogsPath { get; private set; }
        public string LogFilePath { get; private set; }
        public string DefaultModelName { get; private set; } = "sample.obj";

        // Shader files
        public string VertexShaderPath { get; private set; }
        public string FragmentShaderPath { get; private set; }
        public string LightingFragmentShaderPath { get; private set; }

        // Русскоязычные сообщения логов
        public static class RuLogMessages
        {
            public const string WorldInitialized = "Мир инициализирован";
            public const string StartingWorldLoading = "Начинается загрузка мира";
            public const string UsingShaderDirectory = "Используется директория шейдеров: {0}";
            public const string UsingModelsDirectory = "Используется директория моделей: {0}";
            public const string LoadingShaders = "Загрузка шейдеров";
            public const string DefaultDiffuseTextureNotFound = "Стандартная диффузная текстура не найдена по пути {0}, создаем стандартную текстуру";
            public const string CopiedDefaultDiffuseTexture = "Скопирована стандартная диффузная текстура из {0} в {1}";
            public const string DefaultSpecularTextureNotFound = "Стандартная спекулярная текстура не найдена по пути {0}, создаем стандартную текстуру";
            public const string CopiedDefaultSpecularTexture = "Скопирована стандартная спекулярная текстура из {0} в {1}";
            public const string LoadingModel = "Загрузка модели: {0}";
            public const string ModelFileNotFound = "Файл модели не найден: {0}, генерируем стандартный куб";
            public const string WorldLoadedSuccessfully = "Мир успешно загружен";
            public const string ErrorLoadingWorld = "Ошибка при загрузке ресурсов мира: {0}";
            public const string AssetPathsInitialized = "Пути к ресурсам инициализированы: {0}";
            public const string CreatingDirectory = "Создаем директорию: {0}";
            public const string ApplicationStarted = "Приложение запущено";
            public const string ApplicationStopped = "Приложение остановлено";
        }

        private void InitializePaths()
        {
            // Create directories if they don't exist
            EnsureDirectoryExists(AssetsRootPath);
            EnsureDirectoryExists(ModelsPath);
            EnsureDirectoryExists(ShadersPath);
            EnsureDirectoryExists(TexturesPath);
            EnsureDirectoryExists(LogsPath);

            _logger.LogInformation(RuLogMessages.AssetPathsInitialized, AssetsRootPath);
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogInformation(RuLogMessages.CreatingDirectory, path);
                Directory.CreateDirectory(path);
            }
        }
    }
}