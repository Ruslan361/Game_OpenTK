using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Simple3DGame
{
    public class HUD
    {
        private int vao, vbo;
        private Shader hudShader;
        private readonly int width, height;
        private int health = 100;
        private float currentFps = 0;
        
        public HUD(int screenWidth, int screenHeight)
        {
            this.width = screenWidth;
            this.height = screenHeight;
            
            // Создаем VAO и VBO для рендеринга HUD-элементов
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            
            // Создаем шейдер для HUD
            hudShader = new Shader("assets/shaders/hud.vert", "assets/shaders/hud.frag");
        }
        
        public void Update(int health, float fps)
        {
            this.health = health;
            this.currentFps = fps;
        }
        
        public void Render()
        {
            // Сохраняем текущие настройки OpenGL
            bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            
            // Отключаем тест глубины для HUD
            GL.Disable(EnableCap.DepthTest);
            
            // Используем шейдер для HUD
            hudShader.Use();
            
            // Рисуем полоску здоровья (красный прямоугольник)
            RenderHealthBar();
            
            // Восстанавливаем настройки OpenGL
            if (depthTestEnabled)
                GL.Enable(EnableCap.DepthTest);
        }
        
        private void RenderHealthBar()
        {
            // Вершины для полосы здоровья
            float healthPercent = (float)health / 100.0f;
            float barWidth = 0.3f * healthPercent; // Полная ширина полосы здоровья
            
            float[] vertices = {
                // Координаты (NDC)        // Цвет (R, G, B)
                -0.9f, 0.9f, 0.0f,         1.0f, 0.0f, 0.0f, // Верхний левый
                -0.9f + barWidth, 0.9f, 0.0f,    1.0f, 0.0f, 0.0f, // Верхний правый
                -0.9f + barWidth, 0.85f, 0.0f,   1.0f, 0.0f, 0.0f, // Нижний правый
                -0.9f, 0.85f, 0.0f,        1.0f, 0.0f, 0.0f  // Нижний левый
            };
            
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);
            
            // Позиция
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            
            // Цвет
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            
            // Рисуем полосу здоровья
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            
            // Отображаем фоновую рамку
            float[] backgroundVertices = {
                // Координаты (NDC)        // Цвет (R, G, B)
                -0.9f, 0.9f, 0.0f,         0.3f, 0.3f, 0.3f, // Верхний левый
                -0.6f, 0.9f, 0.0f,         0.3f, 0.3f, 0.3f, // Верхний правый
                -0.6f, 0.85f, 0.0f,        0.3f, 0.3f, 0.3f, // Нижний правый
                -0.9f, 0.85f, 0.0f,        0.3f, 0.3f, 0.3f  // Нижний левый
            };
            
            GL.BufferData(BufferTarget.ArrayBuffer, backgroundVertices.Length * sizeof(float), backgroundVertices, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);
            
            GL.BindVertexArray(0);
        }
    }
}