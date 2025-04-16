using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Simple3DGame
{
    public class Physics
    {
        private const float Gravity = 9.8f;
        private const float GroundY = -2.0f; // Уровень пола
        
        public class PhysicsBody
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public Vector3 Acceleration;
            public float Mass;
            public float Radius; // Для сферической коллизии
            public bool IsStatic; // Неподвижный объект
            public bool IsPlayer; // Это игрок
            public bool IsEnemy; // Враждебный объект
            public bool IsOnGround; // На земле
            
            public void ApplyForce(Vector3 force)
            {
                Acceleration += force / Mass;
            }
            
            public void Update(float dt)
            {
                if (IsStatic) return;
                
                // Применяем гравитацию
                ApplyForce(new Vector3(0, -Gravity * Mass, 0));
                
                // Обновляем скорость
                Velocity += Acceleration * dt;
                
                // Трение
                if (IsOnGround)
                {
                    float friction = 0.03f;
                    Velocity.X *= (1 - friction);
                    Velocity.Z *= (1 - friction);
                }
                
                // Обновляем позицию
                Position += Velocity * dt;
                
                // Проверяем коллизию с полом
                if (Position.Y - Radius < GroundY)
                {
                    Position.Y = GroundY + Radius;
                    Velocity.Y = -Velocity.Y * 0.5f; // Отскок с потерей энергии
                    IsOnGround = true;
                }
                else if (Position.Y - Radius > GroundY + 0.1f)
                {
                    IsOnGround = false;
                }
                
                // Сбрасываем ускорение
                Acceleration = Vector3.Zero;
            }
            
            public bool CheckCollision(PhysicsBody other)
            {
                float distance = Vector3.Distance(Position, other.Position);
                return distance < (Radius + other.Radius);
            }
            
            public void ResolveCollision(PhysicsBody other)
            {
                if (IsStatic && other.IsStatic) return;
                
                Vector3 direction = Vector3.Normalize(Position - other.Position);
                float overlap = Radius + other.Radius - Vector3.Distance(Position, other.Position);
                
                // Если один из объектов статический
                if (IsStatic)
                {
                    other.Position -= direction * overlap;
                }
                else if (other.IsStatic)
                {
                    Position += direction * overlap;
                }
                else
                {
                    // Оба объекта подвижные
                    Position += direction * (overlap * 0.5f);
                    other.Position -= direction * (overlap * 0.5f);
                }
                
                // Обмен импульсами
                if (!IsStatic && !other.IsStatic)
                {
                    Vector3 v1 = Velocity;
                    Vector3 v2 = other.Velocity;
                    
                    float restitution = 0.8f; // Коэффициент упругости
                    
                    float dot = Vector3.Dot(v1 - v2, direction);
                    Velocity = v1 - (1 + restitution) * dot / (1/Mass + 1/other.Mass) * direction / Mass;
                    other.Velocity = v2 + (1 + restitution) * dot / (1/Mass + 1/other.Mass) * direction / other.Mass;
                }
                
                // Враждебный объект
                if ((IsPlayer && other.IsEnemy) || (IsEnemy && other.IsPlayer))
                {
                    // Будет обрабатываться в игровой логике
                }
            }
        }
        
        public List<PhysicsBody> Bodies { get; private set; } = new List<PhysicsBody>();
        
        public void AddBody(PhysicsBody body)
        {
            Bodies.Add(body);
        }
        
        public void RemoveBody(PhysicsBody body)
        {
            Bodies.Remove(body);
        }
        
        public void Update(float dt)
        {
            // Обновляем все тела
            foreach (var body in Bodies)
            {
                body.Update(dt);
            }
            
            // Проверяем и разрешаем коллизии
            for (int i = 0; i < Bodies.Count; i++)
            {
                for (int j = i + 1; j < Bodies.Count; j++)
                {
                    if (Bodies[i].CheckCollision(Bodies[j]))
                    {
                        Bodies[i].ResolveCollision(Bodies[j]);
                    }
                }
            }
        }
    }
}