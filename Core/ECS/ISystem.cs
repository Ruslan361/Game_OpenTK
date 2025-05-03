using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Simple3DGame.Core.ECS
{
    // Interface for systems that process entities with specific components.
    public interface ISystem
    {
        // Called every frame to update the system's logic.
        void Update(World world, float deltaTime, KeyboardState? keyboardState = null, MouseState? mouseState = null);
    }
}
