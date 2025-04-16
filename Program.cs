using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


using System;
using System.Collections.Generic;
using System.IO;

namespace Simple3DGame {
    class Program {
        static void Main(string[] args) {
            var nativeWindowSettings = new NativeWindowSettings() {
                ClientSize = new Vector2i(800, 600), // Используем ClientSize вместо Size
                Title = "OpenTK 3D Game Template"
            };

            using (var window = new Game(nativeWindowSettings)) {
                window.Run();
            }
        }
    }
}
