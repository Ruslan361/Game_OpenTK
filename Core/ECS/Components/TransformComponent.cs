using OpenTK.Mathematics;

namespace Simple3DGame.Core.ECS.Components
{
    // Keep as struct for potential performance benefits
    public struct TransformComponent : IComponent
    {
        public Vector3 Position;
        public Quaternion Rotation; // Using Quaternion for rotation is generally better
        public Vector3 Scale;

        // Ensure constructor is public
        public TransformComponent(Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        {
            Position = position;
            Rotation = rotation ?? Quaternion.Identity; // Default to no rotation
            Scale = scale ?? Vector3.One;       // Default to no scaling
        }

        // Helper method to get the model matrix
        public Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }
    }
}
