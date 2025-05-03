using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Simple3DGame.Rendering;

namespace Simple3DGame.Models
{
    /// <summary>
    /// Фабричный класс для создания базовых 3D примитивов
    /// </summary>
    public static class ModelFactory
    {
        /// <summary>
        /// Создает модель куба с заданным размером
        /// </summary>
        /// <param name="shader">Шейдер для рендеринга куба</param>
        /// <param name="size">Размер куба (по умолчанию 1.0)</param>
        /// <returns>Меш куба</returns>
        public static Mesh CreateCube(Shader shader, float size = 1.0f)
        {
            // Масштабируем вершины в соответствии с размером
            float halfSize = size * 0.5f;
            
            // Вершины куба (позиция, нормаль, текстурные координаты)
            float[] vertices = {
                // Передняя грань
                -halfSize, -halfSize,  halfSize,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f, // Нижний левый
                 halfSize, -halfSize,  halfSize,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f, // Нижний правый
                 halfSize,  halfSize,  halfSize,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f, // Верхний правый
                -halfSize,  halfSize,  halfSize,  0.0f,  0.0f,  1.0f,  0.0f, 1.0f, // Верхний левый

                // Задняя грань
                 halfSize, -halfSize, -halfSize,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f, // Нижний левый
                -halfSize, -halfSize, -halfSize,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f, // Нижний правый
                -halfSize,  halfSize, -halfSize,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f, // Верхний правый
                 halfSize,  halfSize, -halfSize,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f, // Верхний левый

                // Левая грань
                -halfSize, -halfSize, -halfSize, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f, // Нижний левый
                -halfSize, -halfSize,  halfSize, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f, // Нижний правый
                -halfSize,  halfSize,  halfSize, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f, // Верхний правый
                -halfSize,  halfSize, -halfSize, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f, // Верхний левый

                // Правая грань
                 halfSize, -halfSize,  halfSize,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f, // Нижний левый
                 halfSize, -halfSize, -halfSize,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, // Нижний правый
                 halfSize,  halfSize, -halfSize,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, // Верхний правый
                 halfSize,  halfSize,  halfSize,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f, // Верхний левый

                // Верхняя грань
                -halfSize,  halfSize,  halfSize,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f, // Нижний левый
                 halfSize,  halfSize,  halfSize,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f, // Нижний правый
                 halfSize,  halfSize, -halfSize,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f, // Верхний правый
                -halfSize,  halfSize, -halfSize,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f, // Верхний левый

                // Нижняя грань
                -halfSize, -halfSize, -halfSize,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f, // Нижний левый
                 halfSize, -halfSize, -halfSize,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f, // Нижний правый
                 halfSize, -halfSize,  halfSize,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f, // Верхний правый
                -halfSize, -halfSize,  halfSize,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f  // Верхний левый
            };

            // Индексы для отрисовки граней
            uint[] indices = {
                0, 1, 2, 2, 3, 0,       // Передняя грань
                4, 5, 6, 6, 7, 4,       // Задняя грань
                8, 9, 10, 10, 11, 8,     // Левая грань
                12, 13, 14, 14, 15, 12,  // Правая грань
                16, 17, 18, 18, 19, 16,  // Верхняя грань
                20, 21, 22, 22, 23, 20   // Нижняя грань
            };

            return new Mesh(vertices, indices);
        }

        /// <summary>
        /// Создает модель куба с заданным размером
        /// </summary>
        /// <param name="shader">Шейдер для рендеринга куба</param>
        /// <param name="size">Размер куба (по умолчанию 1.0)</param>
        /// <returns>Полную модель куба</returns>
        public static Model CreateCubeModel(Shader shader, float size = 1.0f)
        {
            var mesh = CreateCube(shader, size);
            return new Model(new List<Mesh> { mesh });
        }
    }
}