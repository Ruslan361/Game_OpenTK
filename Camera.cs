using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Collections.Generic;
using System.IO;

namespace Simple3DGame {

    public class Camera 
    {
        public Vector3 Position;
        public float pitch = 0, yaw = -90f;
        private float fov = 45f;
        public float AspectRatio;
        private Vector3 target = Vector3.Zero;
        private float moveSpeed = 5.0f;
        
        // Параметры для мыши
        private Vector2 lastMousePosition;
        private bool firstMouse = true;
        private float mouseSensitivity = 0.1f;
        
        // Режим свободной камеры
        public bool FreeCamera { get; set; } = false;

        public Camera(Vector3 position, float aspect) {
            Position = position;
            AspectRatio = aspect;
        }
        
        public void Update(float dt, KeyboardState input, MouseState mouse) {
            // --- ВРАЩЕНИЕ КАМЕРЫ ---
            // Обработка ввода мыши для поворота камеры
            if (mouse.IsButtonDown(MouseButton.Right) || FreeCamera)
            {
                Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
                
                if (firstMouse)
                {
                    lastMousePosition = mousePos;
                    firstMouse = false;
                }
                
                // Вычисляем смещение курсора с прошлого кадра
                float xOffset = mousePos.X - lastMousePosition.X;
                float yOffset = lastMousePosition.Y - mousePos.Y; // Инвертировано
                
                lastMousePosition = mousePos;
                
                // Умножаем на чувствительность
                xOffset *= mouseSensitivity;
                yOffset *= mouseSensitivity;
                
                // Обновляем углы направления камеры
                yaw += xOffset;
                
                // В свободном режиме позволяем вертикальное вращение
                if (FreeCamera) {
                    pitch += yOffset;
                    pitch = Math.Clamp(pitch, -89.0f, 89.0f);
                }
            }
            
            // Если кнопка мыши отпущена и не в свободном режиме, сбрасываем флаг первого движения
            if (!mouse.IsButtonDown(MouseButton.Right) && !FreeCamera)
            {
                firstMouse = true;
            }
            
            // Переключение режима свободной камеры по клавише F
            if (input.IsKeyPressed(Keys.F))
            {
                FreeCamera = !FreeCamera;
                Console.WriteLine($"Режим свободной камеры: {FreeCamera}");
            }
            
            // --- ПЕРЕМЕЩЕНИЕ КАМЕРЫ ---
            // Движение камеры только в режиме свободной камеры или при зажатой правой кнопке мыши
            if (FreeCamera || mouse.IsButtonDown(MouseButton.Right))
            {
                float cameraSpeed = moveSpeed * dt;
                
                // Получаем векторы направления
                Vector3 front = GetFront();
                Vector3 right = Vector3.Cross(front, Vector3.UnitY).Normalized();
                Vector3 up = Vector3.UnitY;
                
                // Только горизонтальное движение (для простоты управления)
                Vector3 horizontalFront = new Vector3(front.X, 0, front.Z).Normalized();
                Vector3 horizontalRight = new Vector3(right.X, 0, right.Z).Normalized();
                
                Vector3 moveDirection = Vector3.Zero;
                
                // Движение вперед-назад по оси Z
                if (input.IsKeyDown(Keys.W))
                    moveDirection += horizontalFront;
                if (input.IsKeyDown(Keys.S))
                    moveDirection -= horizontalFront;
                
                // Движение влево-вправо по оси X
                if (input.IsKeyDown(Keys.A))
                    moveDirection -= horizontalRight;
                if (input.IsKeyDown(Keys.D))
                    moveDirection += horizontalRight;
                
                // Движение вверх-вниз по оси Y
                if (input.IsKeyDown(Keys.Space))
                    moveDirection += up;
                if (input.IsKeyDown(Keys.LeftShift))
                    moveDirection -= up;
                
                // Если есть направление движения, нормализуем его и применяем
                if (moveDirection != Vector3.Zero)
                {
                    moveDirection = moveDirection.Normalized();
                    Position += moveDirection * cameraSpeed;
                }
                
                // Регулировка скорости перемещения колесиком мыши
                float scrollDelta = mouse.ScrollDelta.Y;
                if (scrollDelta != 0)
                {
                    moveSpeed += scrollDelta * 0.5f;
                    moveSpeed = Math.Clamp(moveSpeed, 1.0f, 20.0f);
                    Console.WriteLine($"Скорость камеры: {moveSpeed:F1}");
                }
                
                // Регулировка поля зрения (FOV) колесиком мыши при зажатом Alt
                if (input.IsKeyDown(Keys.LeftAlt) && scrollDelta != 0)
                {
                    fov -= scrollDelta * 2.0f;
                    fov = Math.Clamp(fov, 10.0f, 120.0f);
                    Console.WriteLine($"FOV: {fov:F1}");
                }
            }
        }
        
        public void LookAt(Vector3 target)
        {
            this.target = target;
            
            if (!FreeCamera) // Только если не в свободном режиме
            {
                // Вычисляем направление от камеры к цели
                Vector3 direction = (target - Position).Normalized();
                
                // Вычисляем углы pitch и yaw из направления
                pitch = MathHelper.RadiansToDegrees((float)Math.Asin(direction.Y));
                yaw = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Z, direction.X));
            }
        }

        public Matrix4 GetViewMatrix() {
            if (FreeCamera)
            {
                Vector3 front = GetFront();
                return Matrix4.LookAt(Position, Position + front, Vector3.UnitY);
            }
            else
            {
                return Matrix4.LookAt(Position, target, Vector3.UnitY);
            }
        }

        public Matrix4 GetProjectionMatrix() {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), AspectRatio, 0.1f, 100f);
        }

        public Vector3 GetFront() {
            return new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch))
            ).Normalized();
        }
    }
}