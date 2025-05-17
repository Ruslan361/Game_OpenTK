using Simple3DGame.Models;
using Simple3DGame.Rendering;

namespace Simple3DGame.Core.ECS.Components
{
    public class RenderComponent : IComponent
    {
        public Model? Model { get; set; }
        public Shader? Shader { get; set; }
        public float Shininess { get; set; }
        
        public RenderComponent(Model? model = null, Shader? shader = null, float shininess = 32.0f)
        {
            Model = model;
            Shader = shader;
            Shininess = shininess;
        }
    }
}
