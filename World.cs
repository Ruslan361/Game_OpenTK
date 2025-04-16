using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Simple3DGame {
    public class World {
        private List<Entity> entities;
        private Shader shader;

        public World() {
            entities = new List<Entity>();
        }
        
        public void Load(int screenWidth, int screenHeight) {
            // Создаем базовый шейдер
            shader = new Shader("assets/shaders/shader.vert", "assets/shaders/shader.frag");
            
            // Загружаем модель и размещаем ее перед камерой
            LoadModel("assets/models/demon.obj");
            
            Console.WriteLine($"Загружено объектов: {entities.Count}");
        }

        public void Update(float dt, KeyboardState keyboard, MouseState mouse, Camera camera) {
            // Только обновление камеры
            camera.Update(dt, keyboard, mouse);
        }

        public void Render(Camera camera) {
            // Рендерим сущности
            foreach (var entity in entities) {
                var transform = entity.GetComponent<TransformComponent>();
                
                if (entity.TryGetComponent<ModelComponent>(out var modelComponent)) {
                    modelComponent.Render(transform, camera);
                }
                
                if (entity.TryGetComponent<RenderComponent>(out var renderComponent)) {
                    renderComponent.Render(transform, camera);
                }
            }
        }

        public void LoadModel(string modelPath) {
            // Создаем шейдер для модели
            var modelShader = new Shader("assets/shaders/shader.vert", "assets/shaders/shader.frag");
            
            // Загружаем модель с помощью CjObjLoader
            var model = ObjLoader.Model.LoadFromFile(modelPath);
            
            Console.WriteLine($"Загружена модель {modelPath}");
            Console.WriteLine($"Количество мешей: {model.Meshes.Count}");
            Console.WriteLine($"Количество текстур: {model.Textures.Count}");
            
            // Создаем сущность для модели
            var entity = new Entity();
            
            // Настраиваем положение и масштаб модели
            var transform = new TransformComponent();
            transform.Scale = new Vector3(1.0f);
            transform.Position = new Vector3(0, 0, -3); // Перед камерой
            entity.AddComponent(transform);
            
            // Создаем компонент для отрисовки модели
            entity.AddComponent(new ModelComponent(model, modelShader));
            
            entities.Add(entity);
        }
    }
}