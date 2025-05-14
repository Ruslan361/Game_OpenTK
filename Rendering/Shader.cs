using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Simple3DGame.Rendering
{
    // Implement IDisposable
    public class Shader : IDisposable
    {
        public readonly int Handle;
        private readonly Dictionary<string, int> _uniformLocations;
        private bool disposedValue = false; // To detect redundant calls
        private readonly string _vertexPath;
        private readonly string _fragmentPath;

        public Shader(string vertexPath, string fragmentPath, bool removeComments = false)
        {
            _vertexPath = vertexPath;
            _fragmentPath = fragmentPath;
            
            // Load vertex shader source
            string vertexShaderSource;
            try
            {
                using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
                {
                    vertexShaderSource = reader.ReadToEnd();
                }
                
                if (removeComments)
                {
                    vertexShaderSource = RemoveComments(vertexShaderSource);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading vertex shader source: {e.Message}");
                throw;
            }

            // Load fragment shader source
            string fragmentShaderSource;
            try
            {
                using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
                {
                    fragmentShaderSource = reader.ReadToEnd();
                }
                
                if (removeComments)
                {
                    fragmentShaderSource = RemoveComments(fragmentShaderSource);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading fragment shader source: {e.Message}");
                throw;
            }

            // Compile vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                Console.WriteLine($"ERROR::SHADER::VERTEX::COMPILATION_FAILED\n{infoLog}");
                throw new Exception("Vertex shader compilation failed.");
            }

            // Compile fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                Console.WriteLine($"ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n{infoLog}");
                throw new Exception("Fragment shader compilation failed.");
            }

            // Link shaders into a shader program
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine($"ERROR::SHADER::PROGRAM::LINKING_FAILED\n{infoLog}");
                throw new Exception("Shader program linking failed.");
            }

            // Detach and delete individual shaders as they are now linked
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Cache uniform locations
            _uniformLocations = new Dictionary<string, int>();
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }
        
        // Returns the shader program handle
        public int GetHandle()
        {
            return Handle;
        }
        
        // Returns the fragment shader path for shader identification
        public string GetFilePath()
        {
            return _fragmentPath;
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, data);
            }
            else
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
            }
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, data);
            }
            else
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
            }
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.UniformMatrix4(location, true, ref data);
            }
            else
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
            }
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform3(location, data);
            }
            else
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
            }
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform2(location, data);
            }
            else
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader.");
            }
        }

        // Implement Dispose pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        ~Shader()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             if (!disposedValue) // Ensure cleanup even if Dispose wasn't called explicitly
             {
                 // This finalizer thread might not have an active OpenGL context.
                 // Deleting GL resources here is often problematic.
                 // Prefer explicit Dispose() call.
                 Console.WriteLine("Shader finalizer called. Explicit Dispose() is recommended.");
                 // GL.DeleteProgram(Handle); // Avoid GL calls in finalizer if possible
             }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private string RemoveComments(string shaderSource)
        {
            var result = new StringBuilder();
            bool inMultiLineComment = false;
            int i = 0;

            while (i < shaderSource.Length)
            {
                // Проверка начала многострочного комментария
                if (!inMultiLineComment && i + 1 < shaderSource.Length && shaderSource[i] == '/' && shaderSource[i + 1] == '*')
                {
                    inMultiLineComment = true;
                    i += 2;
                    continue;
                }

                // Проверка конца многострочного комментария
                if (inMultiLineComment && i + 1 < shaderSource.Length && shaderSource[i] == '*' && shaderSource[i + 1] == '/')
                {
                    inMultiLineComment = false;
                    i += 2;
                    continue;
                }

                // Проверка однострочного комментария
                if (!inMultiLineComment && i + 1 < shaderSource.Length && shaderSource[i] == '/' && shaderSource[i + 1] == '/')
                {
                    // Пропускаем до конца строки или конца файла
                    while (i < shaderSource.Length && shaderSource[i] != '\n')
                    {
                        i++;
                    }
                    continue;
                }

                // Если не в комментарии, добавляем символ в результат
                if (!inMultiLineComment)
                {
                    result.Append(shaderSource[i]);
                }

                i++;
            }

            return result.ToString();
        }
    }
}