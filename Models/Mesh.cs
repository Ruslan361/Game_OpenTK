using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Simple3DGame.Models
{
    // Implement IDisposable
    public class Mesh : IDisposable
    {
        public readonly int Vao;
        public readonly int Vbo;
        public readonly int Ebo;
        public readonly int VertexCount;
        public readonly int IndexCount;
        public readonly PrimitiveType PrimitiveType;

        private bool disposedValue;

        public Mesh(float[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            VertexCount = vertices.Length / 8; // Assuming 8 floats per vertex (pos, normal, tex)
            IndexCount = indices.Length;
            PrimitiveType = primitiveType;

            // Create Vertex Array Object (VAO)
            Vao = GL.GenVertexArray();
            GL.BindVertexArray(Vao);

            // Create Vertex Buffer Object (VBO)
            Vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Create Element Buffer Object (EBO)
            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Define vertex attributes
            // Position attribute (location = 0)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            // Normal attribute (location = 1)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            // Texture coordinate attribute (location = 2)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            // Unbind VAO
            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType, IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0); // Unbind after drawing
        }

        // Implement Dispose pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }

                // Free unmanaged resources (OpenGL objects)
                GL.DeleteBuffer(Vbo);
                GL.DeleteBuffer(Ebo);
                GL.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

         ~Mesh()
         {
             // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             // Avoid GL calls here if possible, prefer explicit Dispose()
             // Console.WriteLine("Mesh finalizer called. Explicit Dispose() is recommended.");
             Dispose(disposing: false);
         }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}