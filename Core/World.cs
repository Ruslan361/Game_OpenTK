using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging; // Keep for logger instance type
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Simple3DGame.Config;
using Simple3DGame.Core.ECS;
using Simple3DGame.Core.ECS.Components;
using Simple3DGame.Core.ECS.Systems;
using Simple3DGame.Core.Logging;
using Simple3DGame.Models; // Use correct namespace for Model
using Simple3DGame.Rendering;
using System.IO;
using Simple3DGame.Core.Utils; // Added for MazeGenerator

namespace Simple3DGame.Core
{
    public class World : IDisposable
    {
        private readonly WorldLogger _logger; // Use the specific logger wrapper
        private readonly ConfigSettings _config;
        private Camera _camera; // Changed to non-readonly to allow modification for follow cam

        // ECS Core Data Structures
        private readonly List<Entity> _entities = new List<Entity>();
        // Store components as Dictionary<int, IComponent> for each type
        private readonly Dictionary<Type, Dictionary<int, IComponent>> _components = new Dictionary<Type, Dictionary<int, IComponent>>();
        private readonly List<ISystem> _systems = new List<ISystem>();
        private int _nextEntityId = 0;

        // Resource Cache (simple example)
        private Shader? _lightingShader;
        private Shader? _lightCubeShader;
        private Model? _sampleModel; // Use Model type directly
        private Mesh? _cubeMesh; // For light representation

        // Floor resources
        private Model? _floorModel;
        private Texture? _floorDiffuseTexture;
        private Texture? _floorSpecularTexture;
        
        // Skybox
        private Skybox? _skybox;

        // Maze Wall resources
        private Model? _wallModel;
        private Texture? _brickDiffuseTexture;
        private Texture? _brickSpecularTexture;

        // Player Entity
        private Entity _playerEntity;
        private Vector3 _cameraOffset = new Vector3(0, 1f, 3f); // Offset from player to camera. X=0, Y=up, Z=behind


        // --- Removed old fields ---
        // private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f); // Removed - Use components
        private readonly Vector3[] _pointLightPositions =
        {
            new Vector3( 0.7f,  0.2f,  2.0f),
            new Vector3( 2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f,  2.0f, -12.0f),
            new Vector3( 0.0f,  0.0f, -3.0f)
        };
        // private readonly Vector3[] _cubePositions = // This will be replaced by maze generation
        // {
        //     new Vector3( 0.0f,  0.0f,  0.0f),
        //     new Vector3( 2.0f,  5.0f, -15.0f),
        //     new Vector3(-1.5f, -2.2f, -2.5f),
        //     new Vector3(-3.8f, -2.0f, -12.3f),
        //     new Vector3( 2.4f, -0.4f, -3.5f),
        //     new Vector3(-1.7f,  3.0f, -7.5f),
        //     new Vector3( 1.3f, -2.0f, -2.5f),
        //     new Vector3( 1.5f,  2.0f, -2.5f),
        //     new Vector3( 1.5f,  0.2f, -1.5f),
        //     new Vector3(-1.3f,  1.0f, -1.5f)
        // };
        // --- End Removed fields ---


        public World(ILogger<World> logger, ConfigSettings config, Camera camera) // Accept Camera
        {
            _logger = new WorldLogger(logger); // Wrap the provided logger
            _config = config;
            _camera = camera; // Store camera
            _logger.Initialized();
        }

        // --- ECS Management Methods ---
        // Ensure public accessibility matches Entity struct
        public Entity CreateEntity()
        {
            var entity = new Entity(_nextEntityId++); // Use public constructor
            _entities.Add(entity);
            _logger.EntityCreated(entity.Id); // Use logger wrapper method
            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            // Remove all components associated with the entity
            foreach (var componentType in _components.Keys.ToList()) // Use ToList to avoid modification during iteration
            {
                 if (_components.TryGetValue(componentType, out var store))
                 {
                    store.Remove(entity.Id); // Use public Id
                 }
            }
             // Find the entity in the list and remove it
             int indexToRemove = _entities.FindIndex(e => e.Id == entity.Id); // Use public Id
             if (indexToRemove != -1)
             {
                 _entities.RemoveAt(indexToRemove);
                 _logger.EntityDestroyed(entity.Id); // Use logger wrapper method
             }
        }


        public void AddComponent<T>(Entity entity, T component) where T : IComponent
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
            {
                _components[type] = new Dictionary<int, IComponent>();
            }
            _components[type][entity.Id] = component; // Use public Id
            _logger.ComponentAdded(entity.Id, type.Name); // Use logger wrapper method
        }

        // GetComponent for classes (like RenderComponent)
        public T? GetComponent<T>(Entity entity) where T : class, IComponent
        {
             var type = typeof(T);
             if (_components.TryGetValue(type, out var store) && store.TryGetValue(entity.Id, out var component)) // Use public Id
             {
                 return component as T;
             }
             return null;
        }

         // TryGetComponent for structs (like TransformComponent, LightComponent)
         public bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent
         {
             var type = typeof(T);
             if (_components.TryGetValue(type, out var store) && store.TryGetValue(entity.Id, out var comp)) // Use public Id
             {
                 if (comp is T structComp)
                 {
                     component = structComp;
                     return true;
                 }
             }
             component = default;
             return false;
         }

        public bool HasComponent<T>(Entity entity) where T : IComponent
        {
            return _components.TryGetValue(typeof(T), out var store) && store.ContainsKey(entity.Id); // Use public Id
        }

        public void RemoveComponent<T>(Entity entity) where T : IComponent
        {
            if (_components.TryGetValue(typeof(T), out var store))
            {
                if(store.Remove(entity.Id)) // Use public Id
                {
                    _logger.ComponentRemoved(entity.Id, typeof(T).Name); // Use logger wrapper method
                }
            }
        }

        // Get all entities that have a specific set of components
        public IEnumerable<Entity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            if (componentTypes == null || componentTypes.Length == 0)
            {
                // Return a copy to prevent modification issues if the list changes during iteration
                return _entities.ToList();
            }

            // Find the component type with the fewest entities for optimization
            Type? minType = null;
            int minCount = int.MaxValue;
            foreach (var type in componentTypes)
            {
                if (_components.TryGetValue(type, out var store))
                {
                    if (store.Count < minCount)
                    {
                        minCount = store.Count;
                        minType = type;
                    }
                }
                else
                {
                    // If any component type has no entities, the result is empty
                    return Enumerable.Empty<Entity>();
                }
            }

            // Should always find a minType if componentTypes is not empty and all types exist
            if (minType == null) return Enumerable.Empty<Entity>();

            // Start with the IDs from the smallest component set
            var potentialIds = _components[minType].Keys.ToHashSet();

            // Filter this set by checking other required components
            foreach (var type in componentTypes)
            {
                if (type == minType) continue; // Already have these IDs

                if (_components.TryGetValue(type, out var store))
                {
                    // Remove IDs that don't have this component
                    potentialIds.IntersectWith(store.Keys);
                }
                else
                {
                    // Should not happen due to initial check, but defensively return empty
                    return Enumerable.Empty<Entity>();
                }
            }

            // Convert the final IDs back to Entity structs, ensuring they still exist in the main list
            // This is crucial if entities can be destroyed during the frame/iteration.
            // Using a HashSet for Contains check is efficient.
            return _entities.Where(e => potentialIds.Contains(e.Id)).ToList(); // Return a copy
        }


        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
            _logger.SystemAdded(system.GetType().Name); // Use logger wrapper method
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);
            _logger.SystemRemoved(system.GetType().Name); // Use logger wrapper method
        }
        // --- End ECS Management Methods ---


        public void Load(int screenWidth, int screenHeight) // screenWidth/Height might not be needed here anymore
        {
            try
            {
                // 1. Load Resources (Shaders, Models, Textures)
                _lightingShader = new Shader(_config.VertexShaderPath, _config.LightingFragmentShaderPath);
                _lightCubeShader = new Shader(_config.VertexShaderPath, _config.FragmentShaderPath); // Simple shader for light markers

                string modelPath = Path.Combine(_config.ModelsPath, _config.DefaultModelName);
                _sampleModel = CjObjLoader.LoadModel(modelPath, _lightingShader);
                _cubeMesh = ModelFactory.CreateCube(_lightCubeShader); // This cube mesh can be reused

                // Load Brick Textures for Walls
                string brickDiffusePath = Path.Combine(_config.TexturesPath, "brick-1.jpg");
                string brickSpecularPath = Path.Combine(_config.TexturesPath, "brick-1-2.jpg");
                _brickDiffuseTexture = Texture.LoadFromFile(brickDiffusePath);
                _brickSpecularTexture = Texture.LoadFromFile(brickSpecularPath);

                // Create Wall Model using _cubeMesh and brick textures
                if (_cubeMesh != null && _brickDiffuseTexture != null && _brickSpecularTexture != null)
                {
                    _wallModel = new Model(new List<Mesh> { _cubeMesh }); // Re-use the cube mesh geometry
                    _wallModel.DiffuseMap = _brickDiffuseTexture; // Assign single texture
                    _wallModel.SpecularMap = _brickSpecularTexture; // Assign single texture
                    _logger.LogInformation("Wall model created with brick textures.");
                }
                else
                {
                    _logger.LogError("Failed to load cube mesh or brick textures for wall model.");
                }


                Entity floorEntity = CreateFloor(); // Floor created first to define its size

                // Add components to the floor entity - исправлен масштаб, чтобы не масштабировать по Y
                AddComponent(floorEntity, new TransformComponent(
                    Vector3.Zero,
                    Quaternion.Identity,
                    new Vector3(1.0f, 1.0f, 1.0f))); // Убираем масштабирование, используя уже большой размер в вершинах

                // Увеличиваем shininess для более выраженного блеска
                AddComponent(floorEntity, new RenderComponent(_floorModel, _lightingShader, 64.0f));
                _logger.LogInformation("Floor entity configured with Transform and Render components.");
                // --- End Create Floor ---

                // Maze Generation and Player/Cube Setup Values
                int mazeGridWidth = 21; 
                int mazeGridHeight = 21; 
                float floorSize = 35.0f; 
                float cubeSize = 1.0f; 
                float cellWidthOnFloor = floorSize / mazeGridWidth;
                float cellHeightOnFloor = floorSize / mazeGridHeight;

                GenerateAndPlaceMaze(mazeGridWidth, mazeGridHeight, floorSize, cubeSize, cellWidthOnFloor, cellHeightOnFloor);


                // Create Player Entity
                _playerEntity = CreateEntity();
                // Player starts at the center of the maze's typical start (1,1) if maze is e.g. 11x11 or 21x21
                float playerStartXCell = 1; // Logical X cell for player start
                float playerStartZCell = 1; // Logical Z cell for player start
                float playerX = (playerStartXCell - mazeGridWidth / 2.0f + 0.5f) * cellWidthOnFloor;
                float playerZ = (playerStartZCell - mazeGridHeight / 2.0f + 0.5f) * cellHeightOnFloor;
                float playerModelHeight = 2f; // Assuming player model is 0.5 units high
                float playerY = -0.55f + playerModelHeight / 2.0f; // On floor

                Vector3 playerStartPosition = new Vector3(playerX, playerY, playerZ); 
                AddComponent(_playerEntity, new TransformComponent(playerStartPosition, Quaternion.Identity, new Vector3(playerModelHeight / 2f))); // Player model scale (e.g., 0.25f if height is 0.5f)
                if (_sampleModel != null && _lightingShader != null)
                {
                    AddComponent(_playerEntity, new RenderComponent(_sampleModel, _lightingShader, 32.0f));
                    _logger.LogInformation("Player entity created and configured.");
                }
                else
                {
                    _logger.LogError("Failed to load sample model or lighting shader for player.");
                }

                // Create Point Light Entities
                if (_cubeMesh != null && _lightCubeShader != null) // Check Mesh (class) for null
                {
                    // Create a simple model from the _cubeMesh for rendering light markers
                    var lightMarkerModel = new Model(new List<Mesh> { _cubeMesh }); 

                    foreach (var pos in _pointLightPositions)
                    {
                        var lightEntity = CreateEntity();
                        AddComponent(lightEntity, new TransformComponent(pos, Quaternion.Identity, new Vector3(0.2f)));
                        AddComponent(lightEntity, LightComponent.CreatePointLight(pos));
                        AddComponent(lightEntity, new RenderComponent(lightMarkerModel, _lightCubeShader)); // Use lightMarkerModel
                    }
                    _logger.LogInformation($"Created {_pointLightPositions.Length} point light entities."); // Use logger wrapper method
                }
                else
                {
                    _logger.LogError("Failed to load cube mesh or light cube shader. Cannot create light entities."); // Use logger wrapper method
                }


                // Create Directional Light Entity (Data only)
                var dirLightEntity = CreateEntity();
                AddComponent(dirLightEntity, LightComponent.CreateDirectionalLight(new Vector3(-0.2f, -1.0f, -0.3f)));
                _logger.LogInformation("Created directional light data entity."); // Use logger wrapper method
                                                                                  // No Transform or Render component needed if it's just data


                // Create Camera Spotlight Entity (Data only, position/direction handled by RenderSystem)
                var spotlightDataEntity = CreateEntity();
                // Add Transform (initial position, RenderSystem will use camera's)
                AddComponent(spotlightDataEntity, new TransformComponent(_camera.Position));
                // Add Light component
                AddComponent(spotlightDataEntity, LightComponent.CreateSpotLight(_camera.Position, _camera.Front));
                _logger.LogInformation("Created camera spotlight data entity."); // Use logger wrapper method

                // Создаем Skybox из отдельных изображений
                string[] skyboxFaces = new string[6];
                string texturesPath = _config.TexturesPath;
                
                // Порядок загрузки кубических текстур:
                // +X (правая), -X (левая), +Y (верх), -Y (низ), +Z (перед), -Z (зад)
                skyboxFaces[0] = Path.Combine(texturesPath, "vz_clear_ocean_right.bmp");
                skyboxFaces[1] = Path.Combine(texturesPath, "vz_clear_ocean_left.bmp");
                skyboxFaces[2] = Path.Combine(texturesPath, "vz_clear_ocean_up.bmp");
                skyboxFaces[3] = Path.Combine(texturesPath, "vz_clear_ocean_down.bmp");  // Используем доступную текстуру down
                skyboxFaces[4] = Path.Combine(texturesPath, "vz_clear_ocean_front.bmp");
                skyboxFaces[5] = Path.Combine(texturesPath, "vz_clear_ocean_back.bmp");   // Используем доступную текстуру back
                
                // Проверяем наличие всех необходимых файлов
                bool allFilesExist = true;
                foreach (string face in skyboxFaces)
                {
                    if (!File.Exists(face))
                    {
                        _logger.LogWarning($"Текстура скайбокса не найдена: {face}");
                        allFilesExist = false;
                    }
                }
                
                if (allFilesExist)
                {
                    // Теперь не используем параметр масштаба, так как вершины скайбокса уже имеют большой размер
                    _skybox = new Skybox(skyboxFaces);
                    _logger.LogInformation("Skybox создан с использованием отдельных текстур");
                }
                else
                {
                    _logger.LogWarning("Не удалось создать скайбокс - отсутствуют некоторые файлы текстур");
                }


                // 3. Add Systems
                AddSystem(new RenderSystem(_camera)); // Pass the camera to the RenderSystem

                _logger.WorldLoadingSuccessful(); // Use logger wrapper method
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Critical error during World loading.", ex); // Use logger wrapper method
                throw;
            }

            Entity CreateFloor()
            {
                var floorEntity = CreateEntity();

                // --- Create Floor ---
                _logger.LogInformation("Creating floor...");
                float floorY = -0.55f; // Position it slightly below origin-centered cubes
                float floorTextureTileFactor = 10.0f; // How many times to tile the texture
                float floorSize = 20.0f; // Total size of the floor plane (e.g., 20x20 units)

                // Изменен порядок вершин для правильной ориентации нормалей (по часовой стрелке)
                float[] floorVertices = {
                    // Positions                     // Normals           // Texture Coords
                    -floorSize, floorY, -floorSize,  0.0f, 1.0f, 0.0f,  0.0f, 0.0f,
                     floorSize, floorY, -floorSize,  0.0f, 1.0f, 0.0f,  floorTextureTileFactor, 0.0f,
                     floorSize, floorY,  floorSize,  0.0f, 1.0f, 0.0f,  floorTextureTileFactor, floorTextureTileFactor,
                    -floorSize, floorY,  floorSize,  0.0f, 1.0f, 0.0f,  0.0f, floorTextureTileFactor
                };

                // Порядок индексов изменен для правильного направления грани
                uint[] floorIndices = {
                    2, 1, 0,  // Первый треугольник: 0-1-2 (нижний левый -> нижний правый -> верхний правый)
                    3, 2, 0   // Второй треугольник: 0-2-3 (нижний левый -> верхний правый -> верхний левый)
                };

                var floorMesh = new Mesh(floorVertices, floorIndices);

                // Load floor textures
                string floorDiffusePath = Path.Combine(_config.TexturesPath, "wood-4.jpg");
                string floorSpecularPath = Path.Combine(_config.TexturesPath, "wood-4-1.jpg");

                _floorDiffuseTexture = Texture.LoadFromFile(floorDiffusePath);
                _floorSpecularTexture = Texture.LoadFromFile(floorSpecularPath);

                var floorMeshes = new List<Mesh> { floorMesh };
                var floorDiffuseMaps = new List<Texture> { _floorDiffuseTexture };
                var floorSpecularMaps = new List<Texture> { _floorSpecularTexture };

                _floorModel = new Model(floorMeshes);
                _floorModel.SpecularMap = floorSpecularMaps.FirstOrDefault();
                _floorModel.DiffuseMap = floorDiffuseMaps.FirstOrDefault();

                _logger.LogInformation("Floor model created.");
                return floorEntity;
            }
        }

        // Render method is removed - RenderSystem handles this in its Update
        // public void Render(Camera camera) { ... } // REMOVED

        public void Update(float deltaTime, KeyboardState keyboardState, MouseState mouseState) // Remove Camera parameter
        {
            // --- Camera Input Update ---
            // Keep camera updates here for now, driven by Game class input.
            // An InputSystem would be a better place for this.
            // UpdateCameraInput(deltaTime, keyboardState, mouseState, _camera); // Old free-look camera
            // --- End Camera Input Update ---

            // --- Player Movement (Example) ---
            if (TryGetComponent(_playerEntity, out TransformComponent playerTransform))
            {
                Vector3 moveDirection = Vector3.Zero;
                float playerSpeed = 2.0f * deltaTime;

                // Basic movement relative to world axes. For camera-relative, need to use camera.Front/Right
                if (keyboardState.IsKeyDown(Keys.W)) moveDirection -= Vector3.UnitZ; // Move forward (along world -Z)
                if (keyboardState.IsKeyDown(Keys.S)) moveDirection += Vector3.UnitZ; // Move backward (along world +Z)
                if (keyboardState.IsKeyDown(Keys.A)) moveDirection -= Vector3.UnitX; // Move left (along world -X)
                if (keyboardState.IsKeyDown(Keys.D)) moveDirection += Vector3.UnitX; // Move right (along world +X)

                if (moveDirection != Vector3.Zero)
                {
                    moveDirection.Normalize();
                    playerTransform.Position += moveDirection * playerSpeed;

                    // Calculate rotation based on movement direction
                    // MathF.Atan2 returns the angle in radians whose tangent is the quotient of two specified numbers.
                    // We use X and Z for the horizontal plane, pointing the player along the moveDirection vector.
                    // The angle is around the Y axis.
                    float targetAngleY = MathF.Atan2(moveDirection.X, moveDirection.Z);
                    playerTransform.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, targetAngleY);

                    // Update the component in the ECS
                    AddComponent(_playerEntity, playerTransform); 
                }

                // --- Camera Follow Player ---
                // Calculate desired camera position based on player's current position and offset
                Vector3 targetCameraPosition = playerTransform.Position + _cameraOffset;
                
                // For a simple third-person follow, directly set camera position:
                _camera.Position = targetCameraPosition;
                
                // Make the camera look at the player's position (e.g., center of the player model)
                _camera.LookAt(playerTransform.Position + new Vector3(0, 0.9f, 0)); // Look at player's center (assuming 0.5f height)
            }
            // --- End Player Movement ---


            // Update all registered systems
            // Use ToList() to create a copy, allowing systems to safely add/remove entities/components during iteration
            foreach (var system in _systems.ToList())
            {
                try
                {
                    // Pass necessary context to each system's Update method
                    system.Update(this, deltaTime, keyboardState, mouseState);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating system {system.GetType().Name}", ex); // Use logger wrapper method
                }
            }

            // Optional: Add entity/component cleanup logic here if needed (e.g., for entities marked for deletion)
        }

        // Renamed from Update_Old - Handles camera input specifically
        // private void UpdateCameraInput(float deltaTime, KeyboardState keyboardState, MouseState mouseState, Camera camera) // No longer used directly for player follow
        // {
        //     // Process keyboard input for camera movement
        //     if (keyboardState.IsKeyDown(Keys.W))
        //         camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
        //     if (keyboardState.IsKeyDown(Keys.S))
        //         camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
        //     if (keyboardState.IsKeyDown(Keys.A))
        //         camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
        //     if (keyboardState.IsKeyDown(Keys.D))
        //         camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
        //     camera.ProcessMouseMovement(mouseState.Delta.X, mouseState.Delta.Y);
        //     if (keyboardState.IsKeyDown(Keys.M))
        //         Console.WriteLine(camera.Position);
        // }

        private void GenerateAndPlaceMaze(int mazeGridWidth, int mazeGridHeight, float floorSize, float cubeSize, float cellWidthOnFloor, float cellHeightOnFloor)
        {
            // int mazeGridWidth = 21; 
            // int mazeGridHeight = 21; 
            // float floorSize = 20.0f; 
            // float cubeSize = 1.0f; 

            // float cellWidthOnFloor = floorSize / mazeGridWidth;
            // float cellHeightOnFloor = floorSize / mazeGridHeight;

            var mazeGenerator = new MazeGenerator();
            int[,] mazeLayout = mazeGenerator.GenerateMazeGrid(mazeGridHeight, mazeGridWidth);

            if (_wallModel != null && _lightingShader != null)
            {
                float floorYPosition = -0.55f; 
                float cubeYPosition = floorYPosition + cubeSize / 2.0f;

                int cubesCreated = 0;
                for (int r = 0; r < mazeGridHeight; r++)
                {
                    for (int c = 0; c < mazeGridWidth; c++)
                    {
                        if (mazeLayout[r, c] == 1) // 1 means wall, place a cube
                        {
                            float xPos = (c - mazeGridWidth / 2.0f + 0.5f) * cellWidthOnFloor;
                            float zPos = (r - mazeGridHeight / 2.0f + 0.5f) * cellHeightOnFloor;
                            Vector3 cubePosition = new Vector3(xPos, cubeYPosition, zPos);

                            var cubeEntity = CreateEntity();
                            var rotation = Quaternion.Identity; 
                            AddComponent(cubeEntity, new TransformComponent(cubePosition, rotation, new Vector3(cellWidthOnFloor * 1.0f, cubeSize, cellHeightOnFloor * 1.0f))); 
                            // Use _wallModel for maze walls, which has brick textures
                            AddComponent(cubeEntity, new RenderComponent(_wallModel, _lightingShader, 32.0f)); 
                            cubesCreated++;
                        }
                    }
                }
                _logger.LogInformation($"Created {cubesCreated} maze wall entities.");
            }
            else
            {
                _logger.LogError("Wall model or lighting shader not available. Cannot create maze wall entities.");
            }
        }


        public void Cleanup()
        {
             _logger.LogInformation("Cleaning up World resources..."); // Use logger wrapper method
             _lightingShader?.Dispose();
             _lightCubeShader?.Dispose();
             _sampleModel?.Dispose(); // Model should implement IDisposable
             _cubeMesh?.Dispose();    // Mesh now implements IDisposable

             // Dispose floor resources
             _floorModel?.Dispose();
             _floorDiffuseTexture?.Dispose();
             _floorSpecularTexture?.Dispose();
             
             // Dispose wall textures
             _brickDiffuseTexture?.Dispose();
             _brickSpecularTexture?.Dispose();
             // _wallModel itself doesn't need separate disposal if its meshes/textures are managed elsewhere or it doesn't own them exclusively.
             // However, if _wallModel.Meshes contains a mesh that is ONLY used by _wallModel and needs disposal, handle that.
             // In this case, _wallModel reuses _cubeMesh, which is disposed. Textures are disposed.

             // Dispose skybox resources
             _skybox?.Dispose();

             _entities.Clear();
             _components.Clear();
             _systems.Clear();
             _nextEntityId = 0;
             _logger.LogInformation("World cleanup complete."); // Use logger wrapper method
        }

        // Implement IDisposable
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Cleanup(); // Call cleanup logic
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~World()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Getters for resources
        public Skybox? GetSkybox() => _skybox;
    }
}