using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Simple3DGame.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ObjLoader.Loader.Loaders;

namespace Simple3DGame.Models
{
    public static class CjObjLoader
    {
        // Кэш для текстур
        private static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();
        
        public static Model LoadModel(string path, Shader shader)
        {
            ObjLoaderFactory objLoaderFactory = new ObjLoaderFactory();
            IObjLoader loader = objLoaderFactory.Create();
            Model model = new Model(new List<Mesh>());
            
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
                    ConvertToGameModel(loadedObj, path, model, shader);
                }
                
                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке модели: {ex.Message}");
                return model; // Возвращаем пустую модель в случае ошибки
            }
        }
        
        private static void ConvertToGameModel(LoadResult loadResult, string modelPath, Model model, Shader shader)
        {
            string directory = Path.GetDirectoryName(modelPath) ?? string.Empty;
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
                            vertices.Add(texCoord.Y);
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
                Mesh mesh = new Mesh(vertices.ToArray(), indices.ToArray());
                model.Meshes.Add(mesh);
                
                // Загружаем текстуры материала
                LoadMaterialTextures(group.Material, directory, model);
            }
        }

        // Method to handle loading material textures
        private static void LoadMaterialTextures(global::ObjLoader.Loader.Data.Material? material, string directory, Model model)
        {
            string defaultDiffusePath = Path.Combine(directory, "../Textures/container2.png");
            string defaultSpecularPath = Path.Combine(directory, "../Textures/container2_specular.png");

            // Load Diffuse Map
            string diffuseTexturePath = material?.DiffuseTextureMap ?? string.Empty;
            if (!string.IsNullOrEmpty(diffuseTexturePath))
            {
                string fullDiffusePath = Path.Combine(directory, diffuseTexturePath);
                Console.WriteLine($"Загружаем диффузную текстуру: {fullDiffusePath}");
                if (File.Exists(fullDiffusePath))
                {
                    model.DiffuseMap = LoadTexture(fullDiffusePath);
                }
                else
                {
                    Console.WriteLine($"Диффузная текстура не найдена: {fullDiffusePath}, используем стандартную.");
                    model.DiffuseMap = LoadTextureOrDefault(defaultDiffusePath);
                }
            }
            else
            {
                Console.WriteLine("Диффузная текстура не указана в материале, используем стандартную.");
                model.DiffuseMap = LoadTextureOrDefault(defaultDiffusePath);
            }

            // Load Specular Map
            string specularTexturePath = material?.SpecularTextureMap ?? string.Empty;
            if (!string.IsNullOrEmpty(specularTexturePath))
            {
                string fullSpecularPath = Path.Combine(directory, specularTexturePath);
                Console.WriteLine($"Загружаем спекулярную текстуру: {fullSpecularPath}");
                if (File.Exists(fullSpecularPath))
                {
                    model.SpecularMap = LoadTexture(fullSpecularPath);
                }
                else
                {
                    Console.WriteLine($"Спекулярная текстура не найдена: {fullSpecularPath}, используем стандартную.");
                    model.SpecularMap = LoadTextureOrDefault(defaultSpecularPath);
                }
            }
            else
            {
                Console.WriteLine("Спекулярная текстура не указана в материале, используем стандартную.");
                model.SpecularMap = LoadTextureOrDefault(defaultSpecularPath);
            }

            // Add diffuse map to the general Textures list for backward compatibility or other uses
            if (model.DiffuseMap != null)
            {
                model.Textures.Add(model.DiffuseMap);
            }
        }

        // Helper method to load texture or return default
        private static Texture LoadTextureOrDefault(string texturePath)
        {
            if (File.Exists(texturePath))
            {
                return LoadTexture(texturePath);
            }
            else
            {
                Console.WriteLine($"Стандартная текстура не найдена: {texturePath}, создаем белую текстуру.");
                return CreateDefaultTexture(); // Создание дефолтной текстуры
            }
        }
        
        // Создаем белую текстуру по умолчанию
        private static Texture CreateDefaultTexture()
        {
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            
            // Создаем белую текстуру 1x1
            byte[] data = { 255, 255, 255, 255 }; // RGBA, белый цвет
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            
            // Устанавливаем параметры текстуры
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            return new Texture(handle);
        }
        
        private static Texture LoadTexture(string path)
        {
            if (textureCache.TryGetValue(path, out Texture? texture) && texture != null)
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
                return CreateDefaultTexture();
            }
        }
    }
}
