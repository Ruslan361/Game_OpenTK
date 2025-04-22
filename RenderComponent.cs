using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Simple3DGame.Core;
using Simple3DGame.Models;
using Simple3DGame.Rendering;

namespace Simple3DGame
{
    public class RenderComponent : Component
    {
        private Shader shader;
        private Mesh mesh;
        private Texture texture;

        public RenderComponent(Shader shader, Mesh mesh, Texture texture)
        {
            this.shader = shader;
            this.mesh = mesh;
            this.texture = texture;
        }

        public void Render(TransformComponent transform, Camera camera)
        {
            shader.Use();

            // Устанавливаем матрицы в шейдере
            shader.SetMatrix4("model", transform.GetModelMatrix());
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            // Привязываем текстуру
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

            // Рисуем меш
            GL.BindVertexArray(mesh.vao);
            GL.DrawElements(PrimitiveType.Triangles, mesh.indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}
