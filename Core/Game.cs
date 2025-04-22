using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simple3DGame.Config;
using Simple3DGame.Core;
using Simple3DGame.Rendering;

namespace Simple3DGame.Core
{
    public class Game : GameWindow
    {
        private Camera _camera = null!;
        private World _world = null!;
        private readonly ILogger<Game> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConfigSettings _config;

        private Stopwatch _stopwatch = new Stopwatch();

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
                   ILogger<Game> logger, ConfigSettings config)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _logger = logger;
            _config = config;
            
            // Create a logger factory that will be used to create properly typed loggers
            _loggerFactory = LoggerFactory.Create(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            _logger.LogInformation("Игра инициализирована");
        }

        protected override void OnLoad()
        {
            try
            {
                base.OnLoad();

                _logger.LogInformation("Начало инициализации...");

                GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

                _logger.LogInformation("OpenGL настройки установлены");

                // Инициализация камеры с правильным типом логгера
                _camera = new Camera(
                    Vector3.UnitZ * 3, 
                    Size.X / (float)Size.Y, 
                    _loggerFactory.CreateLogger<Camera>()
                );
                _logger.LogInformation("Камера инициализирована");

                _logger.LogInformation("Создание игрового мира...");
                // Создаем игровой мир с правильным типом логгера
                _world = new World(
                    _loggerFactory.CreateLogger<World>(), 
                    _config
                );
                _logger.LogInformation("Игровой мир создан");

                _logger.LogInformation("Загрузка игрового мира...");
                _world.Load(Size.X, Size.Y);

                _logger.LogInformation("Игровой мир загружен");

                _stopwatch.Start();

                CursorState = CursorState.Grabbed;
                _logger.LogInformation("OnLoad завершен успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при инициализации игры");
                throw;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _world.Render(_camera);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                _logger.LogInformation("Получен сигнал выхода");
                Close();
            }

            _world.Update((float)e.Time, KeyboardState, MouseState, _camera);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;

            _logger.LogInformation($"Изменен размер окна: {Size.X}x{Size.Y}");
        }

        protected override void OnUnload()
        {
            _stopwatch.Stop();
            _logger.LogInformation($"Игра завершена. Общее время работы: {_stopwatch.Elapsed}");

            base.OnUnload();
        }
    }
}