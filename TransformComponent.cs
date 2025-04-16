using OpenTK.Mathematics;

namespace Simple3DGame
{
    public class TransformComponent : Component
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Matrix4 GetModelMatrix()
        {
            // Создаем матрицу модели из позиции, вращения и масштаба
            Matrix4 model = Matrix4.CreateScale(Scale);
            model *= Matrix4.CreateRotationX(Rotation.X);
            model *= Matrix4.CreateRotationY(Rotation.Y);
            model *= Matrix4.CreateRotationZ(Rotation.Z);
            model *= Matrix4.CreateTranslation(Position);
            return model;
        }
    }
}