using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ObjLoader.Loader.Loaders;

namespace Simple3DGame
{
    public class CjObjLoader
    {
        // Кэш для текстур
        private static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();
        
        public static ObjLoader.Model LoadModel(string path)
        {
            ObjLoaderFactory objLoaderFactory = new ObjLoaderFactory();
            IObjLoader loader = objLoaderFactory.Create();
            ObjLoader.Model model = new ObjLoader.Model();
            
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    // Загрузка объекта с помощью CjClutter.ObjLoader
                    LoadResult loadedObj = loader.Load(fileStream);
                    
                    Console.WriteLine($"Загружена модель: {path}");
                    Console.WriteLine($"Вершин: {loadedObj.Vertices.Count}");
                    Console.WriteLine($"Нормалей: {loadedObj.Normals.Count}");
                    Console.WriteLine($"Текстурных координат: {loadedObj.Textures.Count}");
                    Console.WriteLine($"Граней: {loadedObj.Groups.Sum(g => g.Faces.Count)}");
                    
                    // Преобразуем данные из формата CjClutter в наш формат
                    ConvertToGameModel(loadedObj, path, model);
                }
                
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке модели: {ex.Message}");
                return model; // Возвращаем пустую модель в случае ошибки
            }
        }
        
        private static void ConvertToGameModel(LoadResult loadResult, string modelPath, ObjLoader.Model model)
        {
            string directory = Path.GetDirectoryName(modelPath);
            model.Directory = directory;
            
            // Перебираем все группы и создаем меши для каждой
            foreach (var group in loadResult.Groups)
            {
                List<float> vertices = new List<float>();
                List<uint> indices = new List<uint>();
                uint vertexCount = 0;
                
                foreach (var face in group.Faces)
                {
                    // Для всех треугольников грани (CjClutter уже триангулирует грани)
                    for (int i = 0; i < face.Count; i++)
                    {
                        var faceVertex = face[i];
                        
                        // Позиция
                        var position = loadResult.Vertices[faceVertex.VertexIndex - 1];
                        vertices.Add(position.X);
                        vertices.Add(position.Y);
                        vertices.Add(position.Z);
                        
                        // Нормаль (если есть)
                        if (faceVertex.NormalIndex > 0)
                        {
                            var normal = loadResult.Normals[faceVertex.NormalIndex - 1];
                            vertices.Add(normal.X);
                            vertices.Add(normal.Y);
                            vertices.Add(normal.Z);
                        }
                        else
                        {
                            vertices.Add(0.0f);
                            vertices.Add(1.0f);
                            vertices.Add(0.0f);
                        }
                        
                        // Текстурные координаты (если есть)
                        if (faceVertex.TextureIndex > 0)
                        {
                            var texCoord = loadResult.Textures[faceVertex.TextureIndex - 1];
                            vertices.Add(texCoord.X);
                            vertices.Add(texCoord.Y); // Без инвертирования для проверки
                        }
                        else
                        {
                            vertices.Add(0.0f);
                            vertices.Add(0.0f);
                        }
                        
                        // Добавляем индекс
                        indices.Add(vertexCount);
                        vertexCount++;
                    }
                }
                
                // Создаем меш
                Mesh mesh = CreateMesh(vertices, indices);
                model.Meshes.Add(mesh);
                
                // Ищем материал для группы и загружаем текстуру
                if (group.Material != null && !string.IsNullOrEmpty(group.Material.DiffuseTextureMap))
                {
                    string texturePath = Path.Combine(directory, group.Material.DiffuseTextureMap);
                    Console.WriteLine($"Загружаем текстуру: {texturePath}");
                    Console.WriteLine($"Файл существует: {File.Exists(texturePath)}");
                    if (File.Exists(texturePath))
                    {
                        Texture texture = LoadTexture(texturePath);
                        model.Textures.Add(texture);
                    }
                    else
                    {
                        model.Textures.Add(null);
                        Console.WriteLine($"Текстура не найдена: {texturePath}");
                    }
                }
                else
                {
                    // Если нет материала, используем текстуру по умолчанию
                    string defaultTexturePath = Path.Combine(directory, "assets/textures/container.jpg");
                    if (File.Exists(defaultTexturePath))
                    {
                        Texture texture = LoadTexture(defaultTexturePath);
                        model.Textures.Add(texture);
                    }
                    else
                    {
                        model.Textures.Add(null);
                    }
                }
            }
        }
        
        private static Mesh CreateMesh(List<float> vertices, List<uint> indices)
        {
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();
            
            GL.BindVertexArray(vao);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
            
            // Позиция (3 float)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            
            // Нормаль (3 float)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            
            // Текстурные координаты (2 float)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            
            GL.BindVertexArray(0);
            
            return new Mesh { vao = vao, vbo = vbo, indicesCount = indices.Count };
        }
        
        private static Texture LoadTexture(string path)
        {
            if (textureCache.TryGetValue(path, out Texture texture))
            {
                return texture;
            }
            
            try
            {
                texture = new Texture(path);
                textureCache[path] = texture;
                return texture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке текстуры {path}: {ex.Message}");
                return null;
            }
        }
    }
}