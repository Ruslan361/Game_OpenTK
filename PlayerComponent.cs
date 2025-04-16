using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Simple3DGame
{
    public class PlayerComponent : Component
    {
        public Physics.PhysicsBody PhysicsBody { get; private set; }
        public int Health { get; set; } = 100;
        public float MoveSpeed { get; set; } = 5.0f;
        public float JumpForce { get; set; } = 8.0f;
        
        private bool isJumping = false;
        private Vector3 lastPosition;
        private Vector3 rotationAxis = Vector3.UnitX;
        private float rotationAngle = 0;
        
        public PlayerComponent(Physics physics)
        {
            // Создаем физическое тело для игрока
            PhysicsBody = new Physics.PhysicsBody
            {
                Position = new Vector3(0, -1.0f, 0), // Начинаем ниже
                Velocity = Vector3.Zero,
                Acceleration = Vector3.Zero,
                Mass = 1.0f,
                Radius = 0.5f,
                IsStatic = false,
                IsPlayer = true
            };
            
            lastPosition = PhysicsBody.Position;
            physics.AddBody(PhysicsBody);
        }
        
        public void Update(float dt, KeyboardState keyboard, Camera camera)
        {
            Vector3 moveDirection = Vector3.Zero;
            
            // Получаем направление камеры (без компонента Y)
            Vector3 cameraForward = new Vector3(camera.GetFront().X, 0, camera.GetFront().Z).Normalized();
            Vector3 cameraRight = new Vector3(Vector3.Cross(cameraForward, Vector3.UnitY).X, 0, 
                                             Vector3.Cross(cameraForward, Vector3.UnitY).Z).Normalized();
            
            // Управление игроком (относительно направления камеры)
            if (keyboard.IsKeyDown(Keys.W)) moveDirection += cameraForward;
            if (keyboard.IsKeyDown(Keys.S)) moveDirection -= cameraForward;
            if (keyboard.IsKeyDown(Keys.A)) moveDirection -= cameraRight;
            if (keyboard.IsKeyDown(Keys.D)) moveDirection += cameraRight;
            
            // Нормализуем вектор движения
            if (moveDirection != Vector3.Zero)
            {
                moveDirection = moveDirection.Normalized();
            }
            
            // Применяем силу движения
            PhysicsBody.ApplyForce(moveDirection * MoveSpeed);
            
            // Прыжок
            if (keyboard.IsKeyDown(Keys.Space) && PhysicsBody.IsOnGround && !isJumping)
            {
                PhysicsBody.ApplyForce(new Vector3(0, JumpForce, 0));
                isJumping = true;
            }
            
            if (PhysicsBody.IsOnGround)
            {
                isJumping = false;
            }
            
            // Вычисляем вращение шара в зависимости от движения
            Vector3 movement = PhysicsBody.Position - lastPosition;
            
            if (movement.Length > 0.001f)
            {
                // Вычисляем ось вращения (перпендикулярно движению и вверх)
                Vector3 horizontalMovement = new Vector3(movement.X, 0, movement.Z);
                if (horizontalMovement.Length > 0.001f)
                {
                    rotationAxis = Vector3.Cross(Vector3.UnitY, horizontalMovement.Normalized());
                    rotationAngle += horizontalMovement.Length / PhysicsBody.Radius * 10f * dt;
                }
            }
            
            lastPosition = PhysicsBody.Position;
        }
        
        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
        }
        
        public Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateFromAxisAngle(rotationAxis, rotationAngle);
        }
    }
}