using Microsoft.Extensions.Logging; // Keep for logger instance type
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Simple3DGame.Config;
using Simple3DGame.Core;
using Simple3DGame.Core.Logging; // Use the specific logger wrapper
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

        // Constructor receives dependencies from DI
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, ILogger<Game> logger, ConfigSettings config)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _logger = new GameLogger(logger); // Wrap the provided logger
            _config = config;
            _logger.Initialized(nativeWindowSettings.ClientSize); // Use ClientSize instead of Size
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _logger.GameLoading(); // Use logger wrapper method

            // Use correct Camera constructor with logger
            _camera = new Camera(
                new Vector3(0.0f, 0.0f, 3.0f), 
                Size.X / (float)Size.Y, 
                ApplicationLogger.CreateLogger<Camera>());
            // Position is set in constructor, no need to set again

            // Create World using logger from ApplicationLogger (assuming it's initialized)
            try
            {
                _world = new World(ApplicationLogger.CreateLogger<World>(), _config, _camera);
                _world.Load(Size.X, Size.Y);
            }
            catch (InvalidOperationException ex) // Catch if ApplicationLogger wasn't initialized
            {
                 _logger.LogCritical("Failed to create logger for World. ApplicationLogger not initialized?", ex); // Use logger wrapper method
                 Close();
                 return;
            }
            catch (Exception ex)
            {
                 _logger.LogCritical("Failed to load World.", ex); // Use logger wrapper method
                 Close();
                 return;
            }

            CursorState = CursorState.Grabbed;
             if (CursorState == CursorState.Grabbed)
             {
                 _logger.LogInformation("Cursor grabbed."); // Use logger wrapper method
             } else {
                 _logger.LogWarning("Failed to grab cursor."); // Use logger wrapper method
             }
            _firstMove = true;

            _logger.GameLoadedSuccessfully(); // Use logger wrapper method
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if (_world == null) return;
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (_world == null || _camera == null || !IsFocused) return;

            var keyboardState = KeyboardState;
            var mouseState = MouseState;

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                _logger.LogInformation("Escape key pressed. Closing window."); // Use logger wrapper method
                Close();
                return;
            }

            // --- Mouse Input Handling ---
            if (CursorState == CursorState.Grabbed)
            {
                if (_firstMove)
                {
                    _lastPos = new Vector2(mouseState.X, mouseState.Y);
                    _firstMove = false;
                }
                else
                {
                    var deltaX = mouseState.X - _lastPos.X;
                    var deltaY = mouseState.Y - _lastPos.Y;
                    _lastPos = new Vector2(mouseState.X, mouseState.Y);
                    if (deltaX != 0 || deltaY != 0)
                    {
                        _camera.ProcessMouseMovement(deltaX, deltaY);
                    }
                }
            }
            else
            {
                 _firstMove = true;
            }

            // --- World Update ---
            try
            {
                _world.Update((float)e.Time, keyboardState, mouseState);
            }
            catch (Exception ex)
            {
                 _logger.LogError("Error during World Update.", ex); // Use logger wrapper method
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
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