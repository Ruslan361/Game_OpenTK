using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Simple3DGame {
    public class Mesh {
        public int vao, vbo, indicesCount;


        public static Mesh CreateCube() {
            float[] vertices = {
                // positions            // texture coords
                -0.5f, -0.5f, -0.5f,    0.0f, 0.0f,
                 0.5f, -0.5f, -0.5f,    1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,    1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,    0.0f, 1.0f,
                
                -0.5f, -0.5f,  0.5f,    0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,    1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,    1.0f, 1.0f,
                -0.5f,  0.5f,  0.5f,    0.0f, 1.0f
            };

            uint[] indices = {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,
                0, 1, 5, 5, 4, 0,
                2, 3, 7, 7, 6, 2,
                0, 3, 7, 7, 4, 0,
                1, 2, 6, 6, 5, 1
            };

            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Texture coord attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            return new Mesh { vao = vao, vbo = vbo };
        }

        public void Draw() {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        }
    }
}