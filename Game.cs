using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Simple3DGame {
    class Game : GameWindow {
        private World world;
        private Camera camera;

        public Game(NativeWindowSettings settings)
            : base(GameWindowSettings.Default, settings) {
            camera = new Camera(new Vector3(0, 0, 0), Size.X / (float)Size.Y);
            world = new World();
        }

        protected override void OnLoad() {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            
            // Включаем правильное альфа-смешивание
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Устанавливаем камеру в начальную позицию
            camera = new Camera(new Vector3(0, 0, 3), Size.X / (float)Size.Y); // Отодвигаем камеру назад
            world = new World();
            world.Load(Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args) {
            base.OnUpdateFrame(args);

            //if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

            world.Update((float)args.Time, KeyboardState, MouseState, camera);
        }

        protected override void OnRenderFrame(FrameEventArgs args) {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            world.Render(camera);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e) {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}