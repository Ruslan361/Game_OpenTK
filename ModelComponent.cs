using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Simple3DGame 
{
    public class ModelComponent : Component
    {
        private ObjLoader.Model model;
        private Shader shader;

        public ModelComponent(ObjLoader.Model model, Shader shader)
        {
            this.model = model;
            this.shader = shader;
        }

        public void Render(TransformComponent transform, Camera camera)
        {
            shader.Use();

            // Передаем матрицы в шейдер
            shader.SetMatrix4("model", transform.GetModelMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            // Рендерим все меши в модели
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                
                // Привязываем текстуру, если она есть
                if (i < model.Textures.Count && model.Textures[i] != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, model.Textures[i].Handle);
                }

                // Рисуем меш
                GL.BindVertexArray(mesh.vao);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indicesCount, DrawElementsType.UnsignedInt, 0);
            }

            GL.BindVertexArray(0);
        }
    }
}