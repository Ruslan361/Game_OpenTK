using System;
using System.Collections.Generic;
using Simple3DGame.Rendering;

namespace Simple3DGame.Models
{
    public class Model : IDisposable
    {
        // Directory path for texture loading
        public string Directory { get; set; } = string.Empty;
        
        // Model meshes
        public List<Mesh> Meshes { get; }
        
        // Loaded textures
        public List<Texture> Textures { get; }
        
        // Material properties
        public Texture? DiffuseMap { get; set; }
        public Texture? SpecularMap { get; set; }
        
        public Model(List<Mesh> meshes)
        {
            Meshes = meshes;
            Textures = new List<Texture>();
        }

        public Model(List<Mesh> meshes, List<Texture> textures)
        {
            Meshes = meshes;
            Textures = textures;
        }

        // Called when rendering the model
        public void Render()
        {
            // Bind textures if available
            if (DiffuseMap != null)
            {
                DiffuseMap.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            }
            
            if (SpecularMap != null)
            {
                SpecularMap.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture1);
            }

            // Render all meshes
            foreach (var mesh in Meshes)
            {
                mesh.Render();
            }
        }

        // Dispose pattern implementation
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed objects
                    foreach (var mesh in Meshes)
                    {
                        mesh.Dispose();
                    }
                    
                    // Dispose textures
                    foreach (var texture in Textures)
                    {
                        texture.Dispose();
                    }

                    // Dispose material textures if they're not in the Textures list
                    if (DiffuseMap != null && !Textures.Contains(DiffuseMap))
                    {
                        DiffuseMap.Dispose();
                    }
                    
                    if (SpecularMap != null && !Textures.Contains(SpecularMap))
                    {
                        SpecularMap.Dispose();
                    }

                    Meshes.Clear();
                    Textures.Clear();
                }

                disposedValue = true;
            }
        }

        ~Model()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}