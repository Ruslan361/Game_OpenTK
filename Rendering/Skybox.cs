using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Collections.Generic;
using StbImageSharp;

namespace Simple3DGame.Rendering
{
    public class Skybox : IDisposable
    {
        private int _vao;
        private int _vbo;
        private int _cubemapTexture;
        private Shader _shader;
        private float _scale = 1.0f; // Коэффициент масштаба скайбокса

        public float Scale
        {
            get => _scale;
            set => _scale = value;
        }

        // Конструктор для загрузки кубической текстуры из 6 отдельных файлов
        public Skybox(string[] faces, float scale = 1.0f)
        {
            _scale = scale;
            
            // Загрузка шейдеров для скайбокса
            string shaderPath = Path.GetDirectoryName(faces[0]);
            shaderPath = Path.GetDirectoryName(shaderPath) ?? "";
            string vertexPath = Path.Combine(shaderPath, "Shaders", "skybox.vert");
            string fragmentPath = Path.Combine(shaderPath, "Shaders", "skybox.frag");
            
            _shader = new Shader(vertexPath, fragmentPath);
            
            // Загружаем кубическую текстуру
            _cubemapTexture = LoadCubemap(faces);
            
            SetupMesh();
        }

        // Устаревший конструктор для совместимости, если понадобится
        public Skybox(string texturePath, float scale = 1.0f)
        {
            throw new NotSupportedException("Этот конструктор больше не поддерживается. Используйте конструктор с массивом путей к текстурам.");
        }

        // Метод для загрузки cubemap текстуры
        private int LoadCubemap(string[] faces)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            // Порядок загрузки кубических текстур:
            // +X (правая), -X (левая), +Y (верх), -Y (низ), +Z (перед), -Z (зад)
            for (int i = 0; i < faces.Length; i++)
            {
                if (File.Exists(faces[i]))
                {
                    StbImage.stbi_set_flip_vertically_on_load(0); // Отключаем переворот для cubemap
                    
                    using (var stream = File.OpenRead(faces[i]))
                    {
                        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                        
                        GL.TexImage2D(
                            TextureTarget.TextureCubeMapPositiveX + i, 
                            0, 
                            PixelInternalFormat.Rgba, 
                            image.Width, 
                            image.Height, 
                            0, 
                            PixelFormat.Rgba, 
                            PixelType.UnsignedByte, 
                            image.Data
                        );
                    }
                }
                else
                {
                    Console.WriteLine($"Не удалось загрузить текстуру кубической карты: {faces[i]}");
                }
            }
            
            // Настройка параметров текстуры
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            
            return textureID;
        }

        private void SetupMesh()
        {
            // Определение вершин куба для скайбокса
            // Увеличиваем размер вершин до 100.0f для создания эффекта большего расстояния
            float size = 1.0f;
            float[] skyboxVertices = {
                // Позиции
                -size,  size, -size,
                -size, -size, -size,
                 size, -size, -size,
                 size, -size, -size,
                 size,  size, -size,
                -size,  size, -size,

                -size, -size,  size,
                -size, -size, -size,
                -size,  size, -size,
                -size,  size, -size,
                -size,  size,  size,
                -size, -size,  size,

                 size, -size, -size,
                 size, -size,  size,
                 size,  size,  size,
                 size,  size,  size,
                 size,  size, -size,
                 size, -size, -size,

                -size, -size,  size,
                -size,  size,  size,
                 size,  size,  size,
                 size,  size,  size,
                 size, -size,  size,
                -size, -size,  size,

                -size,  size, -size,
                 size,  size, -size,
                 size,  size,  size,
                 size,  size,  size,
                -size,  size,  size,
                -size,  size, -size,

                -size, -size, -size,
                -size, -size,  size,
                 size, -size, -size,
                 size, -size, -size,
                -size, -size,  size,
                 size, -size,  size
            };

            // Создаем VAO и VBO
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
            
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            
            GL.BindVertexArray(0);
        }

        public void Draw(Matrix4 view, Matrix4 projection)
        {
            // Сохраняем текущее состояние
            bool depthMaskWasEnabled = GL.IsEnabled(EnableCap.DepthTest);
            int oldDepthFunc = GL.GetInteger(GetPName.DepthFunc);
            
            // Настройка OpenGL для рендеринга скайбокса
            GL.DepthFunc(DepthFunction.Lequal);  // Изменяем функцию глубины, чтобы скайбокс рисовался на заднем плане
            
            _shader.Use();
            
            // Убираем перемещение из матрицы вида (оставляем только вращение)
            Matrix4 viewWithoutTranslation = new Matrix4(
                view.Row0,
                view.Row1,
                view.Row2,
                new Vector4(0, 0, 0, 1)
            );
            
            // Передаем матрицы в шейдер (больше не используем масштабирование здесь, т.к. уже увеличили вершины)
            _shader.SetMatrix4("view", viewWithoutTranslation);
            _shader.SetMatrix4("projection", projection);
            
            // Активируем кубическую текстуру скайбокса
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _cubemapTexture);
            _shader.SetInt("skybox", 0);
            
            // Отрисовка скайбокса
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
            
            // Восстанавливаем предыдущее состояние
            if (!depthMaskWasEnabled)
                GL.Disable(EnableCap.DepthTest);
            GL.DepthFunc((DepthFunction)oldDepthFunc);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteTexture(_cubemapTexture);
            _shader.Dispose();
        }
    }
}