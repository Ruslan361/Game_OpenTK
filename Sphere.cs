using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Simple3DGame
{
    public class Sphere
    {
        public int VertexCount { get; private set; }
        public int IndicesCount { get; private set; }
        private int vao, vbo, ebo;

        public Sphere(float radius, int sectors, int stacks)
        {
            // Создаем сетку сферы
            CreateSphere(radius, sectors, stacks);
        }

        private void CreateSphere(float radius, int sectors, int stacks)
        {
            float sectorStep = 2.0f * MathF.PI / sectors;
            float stackStep = MathF.PI / stacks;
            float sectorAngle, stackAngle;

            var vertices = new List<float>();
            var indices = new List<int>();

            // Генерируем координаты вершин, нормали и текстурные координаты
            for (int i = 0; i <= stacks; ++i)
            {
                stackAngle = MathF.PI / 2 - i * stackStep;        // от pi/2 до -pi/2
                float xy = radius * MathF.Cos(stackAngle);        // r * cos(u)
                float z = radius * MathF.Sin(stackAngle);         // r * sin(u)

                // Добавляем вершины текущего стека
                for (int j = 0; j <= sectors; ++j)
                {
                    sectorAngle = j * sectorStep;           // от 0 до 2pi

                    // Координаты вершины
                    float x = xy * MathF.Cos(sectorAngle);       // r * cos(u) * cos(v)
                    float y = xy * MathF.Sin(sectorAngle);       // r * cos(u) * sin(v)

                    // Нормали (нормализованный вектор вершины)
                    float nx = x / radius;
                    float ny = y / radius;
                    float nz = z / radius;

                    // Текстурные координаты
                    float s = (float)j / sectors;
                    float t = (float)i / stacks;

                    // Добавляем в массив вершин
                    vertices.Add(x);
                    vertices.Add(y);
                    vertices.Add(z);
                    vertices.Add(nx);
                    vertices.Add(ny);
                    vertices.Add(nz);
                    vertices.Add(s);
                    vertices.Add(t);
                }
            }

            // Генерация индексов
            for (int i = 0; i < stacks; ++i)
            {
                int k1 = i * (sectors + 1);
                int k2 = k1 + sectors + 1;

                for (int j = 0; j < sectors; ++j, ++k1, ++k2)
                {
                    // 2 треугольника на каждый сектор, кроме первого и последнего стека
                    if (i != 0)
                    {
                        indices.Add(k1);
                        indices.Add(k2);
                        indices.Add(k1 + 1);
                    }

                    if (i != (stacks - 1))
                    {
                        indices.Add(k1 + 1);
                        indices.Add(k2);
                        indices.Add(k2 + 1);
                    }
                }
            }

            // Создаем VAO, VBO и EBO
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            // Привязываем VAO
            GL.BindVertexArray(vao);

            // Привязываем и заполняем VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

            // Привязываем и заполняем EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);

            // Настраиваем атрибуты вершин
            // Позиция
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // Нормали
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            // Текстурные координаты
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            // Отвязываем VAO
            GL.BindVertexArray(0);

            VertexCount = vertices.Count / 8;
            IndicesCount = indices.Count;
        }

        public Mesh CreateMesh()
        {
            return new Mesh { vao = vao, vbo = vbo, indicesCount = IndicesCount };
        }
    }
}