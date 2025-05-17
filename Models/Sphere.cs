using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Simple3DGame.Rendering;

namespace Simple3DGame.Models
{
    public class Sphere
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private Shader _shader;
        private List<float> _vertices;
        private List<uint> _indices;

        public Sphere(Shader shader)
        {
            _shader = shader;
            _vertices = new List<float>();
            _indices = new List<uint>();
            GenerateSphere();
            InitializeBuffers();
        }

        private void GenerateSphere()
        {
            // Sphere generation logic
        }

        private void InitializeBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public void Render(Matrix4 model, Matrix4 view, Matrix4 projection)
        {
            _shader.Use();
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}