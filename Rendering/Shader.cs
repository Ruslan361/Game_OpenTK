using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Simple3DGame.Rendering
{
    public class Shader
    {
        private readonly int handle;
        private readonly Dictionary<string, int> uniformLocations;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource;
            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                vertexShaderSource = reader.ReadToEnd();
            }

            string fragmentShaderSource;
            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                fragmentShaderSource = reader.ReadToEnd();
            }

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexStatus);
            if (vertexStatus != (int)All.True)
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                throw new Exception($"Error compiling vertex shader: {infoLog}");
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentStatus);
            if (fragmentStatus != (int)All.True)
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                throw new Exception($"Error compiling fragment shader: {infoLog}");
            }

            handle = GL.CreateProgram();
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            GL.LinkProgram(handle);
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus != (int)All.True)
            {
                string infoLog = GL.GetProgramInfoLog(handle);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            uniformLocations = new Dictionary<string, int>();
            int uniformCount;
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out uniformCount);
            for (int i = 0; i < uniformCount; i++)
            {
                string key = GL.GetActiveUniform(handle, i, out _, out _);
                int location = GL.GetUniformLocation(handle, key);
                uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(handle);
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(handle);
            GL.Uniform3(uniformLocations[name], data);
        }
    }
}