using OpenTK.Graphics.OpenGL4;

namespace Simple3DGame.Models
{
    public class Mesh
    {
        public int vao;
        public int vbo;
        public int ebo;
        public int indicesCount;

        public void Render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}