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

            VertexShaderPath = Path.Combine(ShadersPath, "shader.vert");
            FragmentShaderPath = Path.Combine(ShadersPath, "shader.frag");
            LightingFragmentShaderPath = Path.Combine(ShadersPath, "lighting.frag");
            
            InitializePaths();
        }

        // Asset paths
        public string AssetsRootPath { get; private set; }
        public string ModelsPath { get; private set; }
        public string ShadersPath { get; private set; }
        public string TexturesPath { get; private set; }
        public string DefaultModelName { get; private set; } = "sample.obj";

        // Shader files
        public string VertexShaderPath { get; private set; }
        public string FragmentShaderPath { get; private set; }
        public string LightingFragmentShaderPath { get; private set; }

        private void InitializePaths()
        {
            // Create directories if they don't exist
            EnsureDirectoryExists(AssetsRootPath);
            EnsureDirectoryExists(ModelsPath);
            EnsureDirectoryExists(ShadersPath);
            EnsureDirectoryExists(TexturesPath);

            _logger.LogInformation($"Asset paths initialized: {AssetsRootPath}");
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogInformation($"Creating directory: {path}");
                Directory.CreateDirectory(path);
            }
        }
    }
}