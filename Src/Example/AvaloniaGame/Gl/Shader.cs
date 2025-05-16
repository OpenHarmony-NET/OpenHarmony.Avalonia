using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using AvaloniaGame.Gl;
using OpenHarmony.NDK.Bindings.Native;
using Silk.NET.OpenGLES;

namespace AvaloniaGame.Gl
{
    public class Shader : IDisposable
    {
        private uint _handle;
        private GL _gl;

        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            _gl = gl;

            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            _handle = _gl.CreateProgram();
            _gl.AttachShader(_handle, vertex);
            _gl.AttachShader(_handle, fragment);
            _gl.LinkProgram(_handle);
            _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
            }
            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        public void Use()
        {
            _gl.UseProgram(_handle);
        }

        public void SetUniform(string name, int value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                Logger.LogError("shader", $"{name} uniform not found on shader.");
                throw new Exception($"{name} uniform not found on shader.");
            }
            _gl.Uniform1(location, value);
        }

        public void SetUniform(string name, float value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                Logger.LogError("shader", $"{name} uniform not found on shader.");
                throw new Exception($"{name} uniform not found on shader.");
            }
            _gl.Uniform1(location, value);
        }

        public void Dispose()
        {
            _gl.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            using var fs = new StreamReader(AssetLoader.Open(new Uri($"avares://AvaloniaGame/Assets/{path}")));
            var src = fs.ReadToEnd();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                src = src.Replace("300 es", "330");
            }
            uint handle = _gl.CreateShader(type);
            _gl.ShaderSource(handle, src);
            _gl.CompileShader(handle);
            string infoLog = _gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Logger.LogError($"shader {path}",infoLog);
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }
    }
}
