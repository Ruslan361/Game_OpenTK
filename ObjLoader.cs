using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Simple3DGame
{
    // Адаптер для совместимости со старым кодом
    public class ObjLoader
    {
        // Оставляем прежнюю модель для совместимости
        public class Model
        {
            public List<Mesh> Meshes { get; } = new List<Mesh>();
            public List<Texture> Textures { get; } = new List<Texture>();
            public string Directory { get; set; }
            
            public Model(string directory = null)
            {
                Directory = directory;
            }
            
            public static Model LoadFromFile(string path)
            {
                return CjObjLoader.LoadModel(path);
            }
            
            public void Draw(Shader shader)
            {
                for (int i = 0; i < Meshes.Count; i++)
                {
                    if (i < Textures.Count && Textures[i] != null)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, Textures[i].Handle);
                    }
                    
                    GL.BindVertexArray(Meshes[i].vao);
                    GL.DrawElements(PrimitiveType.Triangles, (int)Meshes[i].indicesCount, DrawElementsType.UnsignedInt, 0);
                    GL.BindVertexArray(0);
                }
            }
        }
        
        public static Model LoadModel(string path)
        {
            return CjObjLoader.LoadModel(path);
        }
    }
}