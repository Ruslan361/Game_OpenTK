using System;
using System.Collections.Generic;
using System.IO;
using Assimp;
using Assimp.Configs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Simple3DGame
{
    public class ModelLoader
    {
        private static readonly AssimpContext importer = new AssimpContext();
        
        public class Model
        {
            public List<Mesh> Meshes { get; } = new List<Mesh>();
            public List<Texture> Textures { get; } = new List<Texture>();
            public string Directory { get; private set; }
            
            public Model(string directory)
            {
                Directory = directory;
            }
            
            // Загрузка модели из файла
            public static Model LoadFromFile(string path)
            {
                return ModelLoader.LoadModel(path);
            }
            
            // Отрисовка модели
            public void Draw(Shader shader)
            {
                for (int i = 0; i < Meshes.Count; i++)
                {
                    // Привязка текстур, если они есть
                    if (i < Textures.Count && Textures[i] != null)
                    {
                        Textures[i].Use(TextureUnit.Texture0);
                    }
                    Meshes[i].Draw();
                }
            }
        }
        
        // Кэш для текстур, чтобы избежать повторной загрузки
        private static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();
        
        // Загрузка модели
        public static Model LoadModel(string path)
        {
            // Настройка параметров импорта Assimp
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            
            // Импорт модели
            var scene = importer.ImportFile(path, 
                PostProcessSteps.Triangulate | // Преобразование всех полигонов в треугольники
                PostProcessSteps.GenerateSmoothNormals | // Генерация сглаженных нормалей
                PostProcessSteps.FlipUVs | // Переворот текстурных координат
                PostProcessSteps.CalculateTangentSpace); // Вычисление тангенциального пространства
            
            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                throw new Exception($"Не удалось загрузить модель:");
            }
            
            var model = new Model(Path.GetDirectoryName(path)); // Сохранение директории модели
            
            // Рекурсивная обработка узлов сцены
            ProcessNode(scene.RootNode, scene, model);
            
            return model;
        }
        
        // Обработка узла сцены
        private static void ProcessNode(Node node, Scene scene, Model model)
        {
            // Обработка всех мешей в текущем узле
            for (int i = 0; i < node.MeshCount; i++)
            {
                var mesh = scene.Meshes[node.MeshIndices[i]];
                var processedMesh = ProcessMesh(mesh, scene, model);
                model.Meshes.Add(processedMesh);
            }
            
            // Рекурсивная обработка дочерних узлов
            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene, model);
            }
        }
        
        // Обработка меша
        private static Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene, Model model)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            
            // Обработка вершин
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                // Позиция
                vertices.Add(mesh.Vertices[i].X);
                vertices.Add(mesh.Vertices[i].Y);
                vertices.Add(mesh.Vertices[i].Z);
                
                // Нормали
                if (mesh.HasNormals)
                {
                    vertices.Add(mesh.Normals[i].X);
                    vertices.Add(mesh.Normals[i].Y);
                    vertices.Add(mesh.Normals[i].Z);
                }
                else
                {
                    vertices.Add(0.0f);
                    vertices.Add(0.0f);
                    vertices.Add(1.0f);
                }
                
                // Текстурные координаты
                if (mesh.HasTextureCoords(0))
                {
                    vertices.Add(mesh.TextureCoordinateChannels[0][i].X);
                    vertices.Add(mesh.TextureCoordinateChannels[0][i].Y);
                }
                else
                {
                    vertices.Add(0.0f);
                    vertices.Add(0.0f);
                }
            }
            
            // Обработка индексов
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                var face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add((uint)face.Indices[j]);
                }
            }
            
            // Загрузка текстур
            if (mesh.MaterialIndex >= 0)
            {
                var material = scene.Materials[mesh.MaterialIndex];
                
                // Загрузка диффузных текстур
                var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse, scene, model.Directory);
                if (diffuseMaps.Count > 0)
                {
                    model.Textures.Add(diffuseMaps[0]);
                }
            }
            
            // Создание VAO, VBO и EBO
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();
            
            GL.BindVertexArray(vao);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
            
            // Настройка атрибутов вершин
            // Атрибут позиции
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            
            // Атрибут нормалей
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            
            // Атрибут текстурных координат
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            
            GL.BindVertexArray(0);
            
            // Создание меша
            return new Mesh { vao = vao, vbo = vbo, indicesCount = indices.Count };
        }
        
        // Загрузка текстур материала
        private static List<Texture> LoadMaterialTextures(Material material, TextureType type, Scene scene, string directory)
        {
            var textures = new List<Texture>();
            
            for (int i = 0; i < material.GetMaterialTextureCount(type); i++)
            {
                material.GetMaterialTexture(type, i, out TextureSlot textureSlot);
                
                string path = textureSlot.FilePath;
                
                // Обработка встроенных текстур
                if (path.StartsWith("*"))
                {
                    int textureIndex = int.Parse(path.Substring(1));
                    if (textureIndex < scene.TextureCount)
                    {
                        // Встроенные текстуры пока не поддерживаются
                        Console.WriteLine("Встроенные текстуры пока не поддерживаются");
                        continue;
                    }
                }
                
                // Исправление пути
                path = path.Replace("\\", "/");
                string fullPath = Path.Combine(directory, path);
                
                // Проверка, была ли текстура уже загружена
                if (textureCache.ContainsKey(fullPath))
                {
                    textures.Add(textureCache[fullPath]);
                }
                else
                {
                    // Загрузка текстуры
                    var texture = new Texture(fullPath);
                    textureCache[fullPath] = texture;
                    textures.Add(texture);
                }
            }
            
            return textures;
        }
    }
}