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

namespace Simple3DGame.Core
{
    public class World : IDisposable
    {
        private readonly WorldLogger _logger; // Use the specific logger wrapper
        private readonly ConfigSettings _config;
        private readonly Camera _camera;

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


        // --- Removed old fields ---
        // private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f); // Removed - Use components
        private readonly Vector3[] _pointLightPositions =
        {
            new Vector3( 0.7f,  0.2f,  2.0f),
            new Vector3( 2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f,  2.0f, -12.0f),
            new Vector3( 0.0f,  0.0f, -3.0f)
        };
        private readonly Vector3[] _cubePositions =
        {
            new Vector3( 0.0f,  0.0f,  0.0f),
            new Vector3( 2.0f,  5.0f, -15.0f),
            new Vector3(-1.5f, -2.2f, -2.5f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3( 2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f,  3.0f, -7.5f),
            new Vector3( 1.3f, -2.0f, -2.5f),
            new Vector3( 1.5f,  2.0f, -2.5f),
            new Vector3( 1.5f,  0.2f, -1.5f),
            new Vector3(-1.3f,  1.0f, -1.5f)
        };
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
                // Pass shader to LoadModel - Assuming CjObjLoader needs it
                // Check CjObjLoader.cs if this is correct
                _sampleModel = CjObjLoader.LoadModel(modelPath, _lightingShader);

                // Pass shader to CreateCube - Assuming ModelFactory needs it
                _cubeMesh = ModelFactory.CreateCube(_lightCubeShader);

                // 2. Create Entities and Components

                // Create Cube Entities
                if (_sampleModel != null && _lightingShader != null) // Check Model (class) for null
                {
                    float angle = 0f;
                    foreach (var pos in _cubePositions)
                    {
                        var cubeEntity = CreateEntity();
                        angle += 20.0f;
                        var rotation = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f).Normalized(), MathHelper.DegreesToRadians(angle));
                        // Use correct TransformComponent constructor
                        AddComponent(cubeEntity, new TransformComponent(pos, rotation, Vector3.One));
                        // Use correct RenderComponent constructor
                        AddComponent(cubeEntity, new RenderComponent(_sampleModel, _lightingShader, 32.0f));
                    }
                     _logger.LogInformation($"Created {_cubePositions.Length} cube entities."); // Use logger wrapper method
                }
                else
                {
                     _logger.LogError("Failed to load sample model or lighting shader. Cannot create cube entities."); // Use logger wrapper method
                }


                // Create Point Light Entities
                if (_cubeMesh != null && _lightCubeShader != null) // Check Mesh (class) for null
                {
                    foreach (var pos in _pointLightPositions)
                    {
                        var lightEntity = CreateEntity();
                        AddComponent(lightEntity, new TransformComponent(pos, Quaternion.Identity, new Vector3(0.2f)));
                        AddComponent(lightEntity, LightComponent.CreatePointLight(pos));
                        // RenderComponent needs Model, not Mesh. Create a Model from the Mesh.
                        // Assuming Model constructor takes a Mesh.
                        var lightModel = new Model(new List<Mesh> { _cubeMesh });
                        AddComponent(lightEntity, new RenderComponent(lightModel, _lightCubeShader));
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


                // 3. Add Systems
                AddSystem(new RenderSystem(_camera)); // Pass the camera to the RenderSystem

                _logger.WorldLoadingSuccessful(); // Use logger wrapper method
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Critical error during World loading.", ex); // Use logger wrapper method
                throw;
            }
        }

        // Render method is removed - RenderSystem handles this in its Update
        // public void Render(Camera camera) { ... } // REMOVED

        public void Update(float deltaTime, KeyboardState keyboardState, MouseState mouseState) // Remove Camera parameter
        {
            // --- Camera Input Update ---
            // Keep camera updates here for now, driven by Game class input.
            // An InputSystem would be a better place for this.
            UpdateCameraInput(deltaTime, keyboardState, mouseState, _camera);
            // --- End Camera Input Update ---


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
        private void UpdateCameraInput(float deltaTime, KeyboardState keyboardState, MouseState mouseState, Camera camera)
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
        }

        public void Cleanup()
        {
             _logger.LogInformation("Cleaning up World resources..."); // Use logger wrapper method
             _lightingShader?.Dispose();
             _lightCubeShader?.Dispose();
             _sampleModel?.Dispose(); // Model should implement IDisposable
             _cubeMesh?.Dispose();    // Mesh now implements IDisposable

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
    }
}