using OpenTK.Mathematics;
using System;

namespace Simple3DGame
{
    public class EnemyComponent : Component
    {
        public Physics.PhysicsBody PhysicsBody { get; private set; }
        public int Damage { get; set; } = 10;
        private float moveSpeed = 2.0f;
        private Vector3 patrolStart;
        private Vector3 patrolEnd;
        private bool patrolForward = true;
        private float detectionRadius = 10.0f; // Увеличенный радиус обнаружения
        
        public EnemyComponent(Physics physics, Vector3 position, Vector3 patrolTarget, float radius = 0.5f)
        {
            patrolStart = position;
            patrolEnd = patrolTarget;
            
            // Создаем физическое тело для врага
            PhysicsBody = new Physics.PhysicsBody
            {
                Position = position,
                Velocity = Vector3.Zero,
                Acceleration = Vector3.Zero,
                Mass = 2.0f,
                Radius = radius,
                IsStatic = false,
                IsEnemy = true
            };
            
            physics.AddBody(PhysicsBody);
        }
        
        public void Update(float dt, Vector3 playerPosition)
        {
            // Вычисляем расстояние до игрока
            float distanceToPlayer = Vector3.Distance(PhysicsBody.Position, playerPosition);
            
            // Если игрок близко, преследуем его
            if (distanceToPlayer < detectionRadius)
            {
                // Направление к игроку
                Vector3 direction = playerPosition - PhysicsBody.Position;
                direction.Y = 0; // Игнорируем высоту
                direction = direction.Normalized();
                
                // Более высокая скорость при преследовании
                float chaseSpeed = moveSpeed * 1.5f;
                
                // Применяем движение к врагу
                PhysicsBody.ApplyForce(direction * chaseSpeed);
            }
            else
            {
                // Если игрок далеко, патрулируем
                Vector3 target = patrolForward ? patrolEnd : patrolStart;
                Vector3 direction = target - PhysicsBody.Position;
                
                // Проверка достижения целевой точки
                if (direction.Length < 0.5f)
                {
                    patrolForward = !patrolForward;
                    direction = Vector3.Zero;
                }
                
                if (direction.Length > 0.001f)
                {
                    direction.Y = 0; // Игнорируем высоту
                    direction = direction.Normalized();
                    PhysicsBody.ApplyForce(direction * moveSpeed);
                }
            }
        }
    }
}