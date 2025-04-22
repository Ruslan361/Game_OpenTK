using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Simple3DGame.Config;
using Simple3DGame.Models;
using Simple3DGame.Rendering;
using Simple3DGame.Core.Logging;

namespace Simple3DGame.Core
{
    public class World
    {
        private readonly WorldLogger _logger;
        private readonly ConfigSettings _config;
        private Shader _shader = null!;
        private Shader _lightShader = null!;
        
        private Models.ObjLoader.Model _model = null!;
        private Mesh _lightCube = null!;
        
        private Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private readonly Vector3[] _pointLightPositions =
        {
            new Vector3(0.7f, 0.2f, 2.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };
        
        // Add positions for cubes in the scene
        private readonly Vector3[] _cubePositions =
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(2.0f, 5.0f, -15.0f),
            new Vector3(-1.5f, -2.2f, -2.5f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3(2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f, 3.0f, -7.5f),
            new Vector3(1.3f, -2.0f, -2.5f),
            new Vector3(1.5f, 2.0f, -2.5f),
            new Vector3(1.5f, 0.2f, -1.5f),
            new Vector3(-1.3f, 1.0f, -1.5f)
        };

        public World(ILogger<World> logger, ConfigSettings config)
        {
            _logger = new WorldLogger(logger);
            _config = config;
            _logger.Initialized();
        }

        public void Load(int screenWidth, int screenHeight)
        {
            try
            {
                _logger.LoadingStarted();
                
                // No need to check directories - ConfigSettings takes care of this
                _logger.ShadersDirectory(_config.ShadersPath);
                _logger.ModelsDirectory(_config.ModelsPath);
                
                // Configure face culling
                // Option 1: Disable face culling completely (show all faces)
                GL.Disable(EnableCap.CullFace);
                
                // Option 2 (alternative): Enable face culling with specific settings
                // Uncomment these lines if you want to enable face culling with specific settings
                // GL.Enable(EnableCap.CullFace);
                // GL.CullFace(CullFaceMode.Back); // Don't render back faces
                // GL.FrontFace(FrontFaceDirection.Ccw); // Counter-clockwise winding defines front faces
                
                _logger.ShadersLoading();
                _shader = new Shader(_config.VertexShaderPath, _config.LightingFragmentShaderPath);
                _lightShader = new Shader(_config.VertexShaderPath, _config.FragmentShaderPath);
                
                // Load default textures for materials
                string texturePath = Path.Combine(_config.TexturesPath, "container2.png");
                string specularPath = Path.Combine(_config.TexturesPath, "container2_specular.png");
                
                // Copy texture files to the textures directory if they don't exist
                if (!File.Exists(texturePath))
                {
                    _logger.DiffuseTextureNotFound(texturePath);
                    
                    // Ensure the directory exists
                    Directory.CreateDirectory(_config.TexturesPath);
                    
                    // If we have the file in assets/textures, copy it to the target location
                    string sourceTexturePath = Path.Combine("assets", "textures", "container2.png");
                    if (File.Exists(sourceTexturePath))
                    {
                        File.Copy(sourceTexturePath, texturePath, true);
                        _logger.CopyingDefaultDiffuseTexture(sourceTexturePath, texturePath);
                    }
                }
                
                if (!File.Exists(specularPath))
                {
                    _logger.SpecularTextureNotFound(specularPath);
                    
                    // Ensure the directory exists
                    Directory.CreateDirectory(_config.TexturesPath);
                    
                    // If we have the file in assets/textures, copy it to the target location
                    string sourceSpecularPath = Path.Combine("assets", "textures", "container2_specular.png");
                    if (File.Exists(sourceSpecularPath))
                    {
                        File.Copy(sourceSpecularPath, specularPath, true);
                        _logger.CopyingSpecularTexture(sourceSpecularPath, specularPath);
                    }
                }
                
                // Load the model
                string modelPath = Path.Combine(_config.ModelsPath, _config.DefaultModelName);
                _logger.ModelLoading(modelPath);
                
                if (!File.Exists(modelPath))
                {
                    _logger.ModelNotFound(modelPath);
                    // Create default cube if model not found, passing the main shader
                    _model = ModelFactory.CreateDefaultCube(_shader);
                    
                    // Load default textures
                    _model.DiffuseMap = Texture.LoadFromFile(texturePath);
                    _model.SpecularMap = Texture.LoadFromFile(specularPath);
                }
                else
                {
                    _model = CjObjLoader.LoadModel(modelPath, _shader);
                }
                
                // Create light cube, passing the light shader
                _lightCube = ModelFactory.CreateCube(_lightShader);
                
                _logger.WorldLoadingSuccessful();
            }
            catch (Exception ex)
            {
                _logger.WorldResourcesLoadingError(ex);
                throw;
            }
        }

        public void Render(Camera camera)
        {
            // Render the model
            _shader.Use();
            
            // Set textures uniforms
            _shader.SetInt("material.diffuse", 0);  // Texture unit 0
            _shader.SetInt("material.specular", 1); // Texture unit 1
            
            // Set camera uniforms
            _shader.SetMatrix4("view", camera.GetViewMatrix());
            _shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            _shader.SetVector3("viewPos", camera.Position);
            
            // Set material properties
            _shader.SetFloat("material.shininess", 32.0f);
            
            // Directional light
            _shader.SetVector3("dirLight.direction", new Vector3(-0.2f, -1.0f, -0.3f));
            _shader.SetVector3("dirLight.ambient", new Vector3(0.05f, 0.05f, 0.05f));
            _shader.SetVector3("dirLight.diffuse", new Vector3(0.4f, 0.4f, 0.4f));
            _shader.SetVector3("dirLight.specular", new Vector3(0.5f, 0.5f, 0.5f));
            
            // Point lights
            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                _shader.SetVector3($"pointLights[{i}].position", _pointLightPositions[i]);
                _shader.SetVector3($"pointLights[{i}].ambient", new Vector3(0.05f, 0.05f, 0.05f));
                _shader.SetVector3($"pointLights[{i}].diffuse", new Vector3(0.8f, 0.8f, 0.8f));
                _shader.SetVector3($"pointLights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
                _shader.SetFloat($"pointLights[{i}].constant", 1.0f);
                _shader.SetFloat($"pointLights[{i}].linear", 0.09f);
                _shader.SetFloat($"pointLights[{i}].quadratic", 0.032f);
            }
            
            // Spotlight (flashlight from camera position)
            _shader.SetVector3("spotLight.position", camera.Position);
            _shader.SetVector3("spotLight.direction", camera.Front);
            _shader.SetVector3("spotLight.ambient", new Vector3(0.0f, 0.0f, 0.0f));
            _shader.SetVector3("spotLight.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
            _shader.SetVector3("spotLight.specular", new Vector3(1.0f, 1.0f, 1.0f));
            _shader.SetFloat("spotLight.constant", 1.0f);
            _shader.SetFloat("spotLight.linear", 0.09f);
            _shader.SetFloat("spotLight.quadratic", 0.032f);
            _shader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            _shader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(17.5f)));
            
            // Render the model instances at different positions
            for (int i = 0; i < _cubePositions.Length; i++)
            {
                Matrix4 model = Matrix4.CreateTranslation(_cubePositions[i]);
                float angle = 20.0f * i;
                model = model * Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                _shader.SetMatrix4("model", model);
                
                _model.Render();
            }
            
            // Render the light cubes for each point light
            _lightShader.Use();
            _lightShader.SetMatrix4("view", camera.GetViewMatrix());
            _lightShader.SetMatrix4("projection", camera.GetProjectionMatrix());
            
            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                Matrix4 lampMatrix = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(_pointLightPositions[i]);
                _lightShader.SetMatrix4("model", lampMatrix);
                _lightCube.Render();
            }
        }

        public void Update(float deltaTime, KeyboardState keyboardState, MouseState mouseState, Camera camera)
        {
            // Process keyboard input for camera movement
            if (keyboardState.IsKeyDown(Keys.W))
                camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            if (keyboardState.IsKeyDown(Keys.S))
                camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            if (keyboardState.IsKeyDown(Keys.A))
                camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            if (keyboardState.IsKeyDown(Keys.D))
                camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
            
            // Process mouse movement for camera rotation
            Vector2 mousePos = mouseState.Position;
            camera.ProcessMouseMovement(mouseState.Delta.X, -mouseState.Delta.Y);
        }
    }
}