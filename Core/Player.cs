using Simple3DGame.Core.ECS;
using Simple3DGame.Core.ECS.Components; // For TransformComponent if needed directly

namespace Simple3DGame.Core
{
    public class Player
    {
        public Entity Entity { get; private set; }
        private World _world; // Optional: if player needs to interact with world directly

        public Player(Entity entity, World world)
        {
            Entity = entity;
            _world = world;
        }

        public TransformComponent GetTransform()
        {
            if (_world.TryGetComponent(Entity, out TransformComponent transform))
            {
                return transform;
            }
            // Should not happen if player is set up correctly
            throw new System.InvalidOperationException("Player does not have a TransformComponent.");
        }

        public void UpdateTransform(TransformComponent transform)
        {
            _world.AddComponent(Entity, transform);
        }
    }
}
