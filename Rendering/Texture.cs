using StbImageSharp;
using OpenTK.Graphics.OpenGL4;
using System.IO;
using System;

namespace Simple3DGame.Rendering
{
    public class Texture
    {
        private readonly int handle;
        
        // Add a public property to access the handle
        public int Handle => handle;

        // Constructor for loading texture from file
        public Texture(string path)
        {
            // Generate handle
            handle = GL.GenTexture();

            // Bind the handle
            Use();

            // Enable vertical flipping of textures to match OpenGL coordinate system
            StbImage.stbi_set_flip_vertically_on_load(1);

            // Load the image
            ImageResult image;
            using (var stream = File.OpenRead(path))
            {
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            }

            // Set texture parameters
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        // Constructor that takes an existing handle
        public Texture(int textureHandle)
        {
            if (textureHandle <= 0)
                throw new ArgumentException("Invalid texture handle provided", nameof(textureHandle));
                
            handle = textureHandle;
        }

        // Static method to load a texture from file
        public static Texture LoadFromFile(string path)
        {
            try
            {
                return new Texture(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load texture {path}: {ex.Message}");
                return CreateDefaultTexture();
            }
        }
        
        // Static method to create a default white texture
        public static Texture CreateDefaultTexture()
        {
            // Create a 2x2 white texture
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);
            
            // Create basic white texture data (RGBA format)
            byte[] data = new byte[16];
            for (int i = 0; i < data.Length; i += 4)
            {
                // White color: 255, 255, 255, 255 (RGBA)
                data[i] = 255;     // R
                data[i + 1] = 255; // G
                data[i + 2] = 255; // B
                data[i + 3] = 255; // A
            }
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
                2, 2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            return new Texture(handle);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}