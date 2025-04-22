using System.Collections.Generic;
using Simple3DGame.Rendering;

namespace Simple3DGame.Models
{
    public class ObjLoader
    {
        public class Model
        {
            public List<Mesh> Meshes { get; set; } = new List<Mesh>();
            public List<Texture> Textures { get; set; } = new List<Texture>();
            public string Directory { get; set; } = string.Empty;
            
            // Added properties for material textures
            public Texture DiffuseMap { get; set; } = null!;
            public Texture SpecularMap { get; set; } = null!;
            
            public void Render()
            {
                for (int i = 0; i < Meshes.Count; i++)
                {
                    // Bind diffuse and specular maps if they exist
                    if (DiffuseMap != null)
                    {
                        DiffuseMap.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
                    }
                    else if (i < Textures.Count)
                    {
                        Textures[i].Use();
                    }
                    
                    if (SpecularMap != null)
                    {
                        SpecularMap.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture1);
                    }
                    
                    Meshes[i].Render();
                }
            }
        }
    }
}