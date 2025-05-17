using Microsoft.Extensions.Logging; // Keep for logger instance type
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Simple3DGame.Config;
using Simple3DGame.Core;
using Simple3DGame.Core.Logging; // Use the specific logger wrapper
using Simple3DGame.Rendering;
using Simple3DGame.UI;
using Simple3DGame.Core.ECS.Systems; // for RenderSystem
using System;

namespace Simple3DGame
{
    public class Game : GameWindow
    {
        private readonly GameLogger _logger; // Use the specific logger wrapper
        private readonly ConfigSettings _config;
        private World? _world;
        private Camera? _camera;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private GameStateManager _gameStateManager;
        private ImGuiController _imGuiController;
        private Simple3DGame.UI.UIManager _uiManager;
        private bool _sceneLoaded = false;
        private RenderSystem _renderSystem;

        // Constructor receives dependencies from DI
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, ILogger<Game> logger, ConfigSettings config)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _logger = new GameLogger(logger);
            _config = config;
            _logger.Initialized(nativeWindowSettings.ClientSize);
            // Setup state manager
            _gameStateManager = new GameStateManager(logger);
            // Subscribe to state changes
            _gameStateManager.OnGameStateChanged += state => {
                if (state == Core.GameState.Playing)
                {
                    if (!_sceneLoaded) InitializeScene();
                    CursorState = CursorState.Grabbed;
                }
                else if (state == Core.GameState.Paused)
                {
                    CursorState = CursorState.Normal;
                }
                else if (state == Core.GameState.MainMenu)
                {
                    CursorState = CursorState.Normal;
                }
            };
            // Start in menu: show cursor
            CursorState = CursorState.Normal;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            // Initialize UI
            _imGuiController = new ImGuiController(Size.X, Size.Y);
            _uiManager = new Simple3DGame.UI.UIManager(_gameStateManager);
        }

        // Add method to initialize the scene when playing begins
        private void InitializeScene()
        {
            _sceneLoaded = true;
            // Create camera and world as before
            _camera = new Camera(
                new Vector3(0.0f, 5.0f, 10.0f),
                Size.X / (float)Size.Y,
                ApplicationLogger.CreateLogger<Camera>());
            _world = new World(ApplicationLogger.CreateLogger<World>(), _config, _camera);
            // Add game systems including rendering
            AddGameSystems();
            _world.Load(Size.X, Size.Y);
            // Prepare GL state
            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            // Hide menu cursor, grab for gameplay
            CursorState = CursorState.Grabbed;
        }

        // Add method to initialize game systems including rendering
        private void AddGameSystems()
        {
            if (_world == null || _camera == null) return;
            _renderSystem = new RenderSystem(_camera);
            _world.AddSystem(_renderSystem);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            // Always update UI frame
            _imGuiController.Update(this, (float)e.Time);
            if (_gameStateManager.CurrentState == Core.GameState.Playing && _world != null)
            {
                // Render 3D scene via RenderSystem
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                _renderSystem?.Render(_world);
            }
            // Render UI overlay
            _uiManager.RenderUI();
            _imGuiController.Render();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // UI and menu navigation
            if (_gameStateManager.CurrentState == Core.GameState.MainMenu)
                return;
            // Get input states
            var keyboard = KeyboardState;
            var mouse = MouseState;
            // Toggle pause with P
            if (_gameStateManager.CurrentState == Core.GameState.Playing && keyboard.IsKeyPressed(Keys.P))
            {
                _gameStateManager.ChangeState(Core.GameState.Paused);
                return;
            }
            if (_gameStateManager.CurrentState == Core.GameState.Paused && keyboard.IsKeyPressed(Keys.P))
            {
                _gameStateManager.ChangeState(Core.GameState.Playing);
                return;
            }
            if (_gameStateManager.CurrentState == Core.GameState.Paused)
            {
                // Unlock cursor in pause
                if (CursorState != CursorState.Normal)
                    CursorState = CursorState.Normal;
                return;
            }
            // In gameplay, update world
            if (_world == null || _camera == null || !IsFocused) return;
            if (keyboard.IsKeyDown(Keys.Escape))
            {
                _gameStateManager.ChangeState(Core.GameState.MainMenu);
            }
            _world.Update((float)e.Time, keyboard, mouse);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            // Allow camera rotation during pause
            if (_gameStateManager.CurrentState == Core.GameState.Paused && _camera != null)
            {
                _camera.ProcessMouseMovement(e.DeltaX, e.DeltaY);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera?.ProcessMouseScroll(e.OffsetY);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            if (e.Width == 0 || e.Height == 0) return;
            GL.Viewport(0, 0, Size.X, Size.Y);
            if (_camera != null)
            {
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
             _logger.LogInformation($"Window resized to {Size.X}x{Size.Y}. Aspect ratio updated."); // Use logger wrapper method
        }

         protected override void OnUnload()
        {
            _logger.GameUnloading(); // Use logger wrapper method
            _world?.Dispose();
            _world = null;
            base.OnUnload();
             _logger.GameUnloadedSuccessfully(); // Use logger wrapper method
             // ApplicationLogger.Cleanup(); // Cleanup is called in Program.cs finally block
        }
    }
}