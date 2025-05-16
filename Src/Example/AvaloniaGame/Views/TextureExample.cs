using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.OpenHarmony;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Silk.NET.OpenGLES;
using Logger = AvaloniaGame.Gl.Logger;
using Texture = AvaloniaGame.Gl.Texture;

namespace AvaloniaGame.Views
{
    public class TextureExample : OpenGlControlBase
    {
        private static GL _gl;

        private static uint _vao;
        private static uint _vbo;
        private static uint _ebo;

        private static uint _program;

        private Texture _texture;

        private Texture[] _textures = new Texture[14];
        private DateTime _gameTime = DateTime.Now;
        private DateTime _animation = DateTime.Now;
        private int _current = 0;

        protected unsafe override void OnOpenGlInit(GlInterface gl)
        {
            base.OnOpenGlInit(gl);
            _gl = GL.GetApi(gl.GetProcAddress);
            Logger.Log("Device", gl.Renderer ?? string.Empty);
            _gl.ClearColor(System.Drawing.Color.CornflowerBlue);

            // Create the VAO.
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            // The quad vertices data.
            // You may have noticed an addition - texture coordinates!
            // Texture coordinates are a value between 0-1 (see more later about this) which tell the GPU which part
            // of the texture to use for each vertex.
            float[] vertices =
            {
              // aPosition--------   aTexCoords
                 0.5f,  0.5f, 0.0f,  1.0f, 1-1.0f,
                 0.5f, -0.5f, 0.0f,  1.0f, 1-0.0f,
                -0.5f, -0.5f, 0.0f,  0.0f, 1-0.0f,
                -0.5f,  0.5f, 0.0f,  0.0f, 1-1.0f
            };

            // Create the VBO.
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // Upload the vertices data to the VBO.
            fixed (float* buf = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

            // The quad indices data.
            uint[] indices =
            {
                0u, 1u, 3u,
                1u, 2u, 3u
            };

            // Create the EBO.
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

            // Upload the indices data to the EBO.
            fixed (uint* buf = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            // The vertex shader code.
            const string vertexCode = @"#version 300 es
        
        layout (location = 0) in vec3 aPosition;

        // On top of our aPosition attribute, we now create an aTexCoords attribute for our texture coordinates.
        layout (location = 1) in vec2 aTexCoords;

        // Likewise, we also assign an out attribute to go into the fragment shader.
        out vec2 frag_texCoords;
        
        void main()
        {
            gl_Position = vec4(aPosition, 1.0);

            // This basic vertex shader does no additional processing of texture coordinates, so we can pass them
            // straight to the fragment shader.
            frag_texCoords = aTexCoords;
        }";

            // The fragment shader code.
            const string fragmentCode = @"#version 300 es
        #ifdef GL_ES
        precision mediump float;
        #endif
        // This in attribute corresponds to the out attribute we defined in the vertex shader.
        in vec2 frag_texCoords;
        
        out vec4 out_color;

        // Now we define a uniform value!
        // A uniform in OpenGL is a value that can be changed outside of the shader by modifying its value.
        // A sampler2D contains both a texture and information on how to sample it.
        // Sampling a texture is basically calculating the color of a pixel on a texture at any given point.
        uniform sampler2D uTexture;
        
        void main()
        {
            // We use GLSL's texture function to sample from the texture at the given input texture coordinates.
            out_color = texture(uTexture, frag_texCoords);
        }";

            // Create our vertex shader, and give it our vertex shader source code.
            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, vertexCode);

            // Attempt to compile the shader.
            _gl.CompileShader(vertexShader);

            // Check to make sure that the shader has successfully compiled.
            _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
            {
                var log = _gl.GetShaderInfoLog(vertexShader);
                throw new Exception("Vertex shader failed to compile: " + log);

            }

            // Repeat this process for the fragment shader.
            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, fragmentCode);

            _gl.CompileShader(fragmentShader);

            _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
            {
                var log = _gl.GetShaderInfoLog(fragmentShader);
                throw new Exception("Fragment shader failed to compile: " + log);
            }

            // Create our shader program, and attach the vertex & fragment shaders.
            _program = _gl.CreateProgram();

            _gl.AttachShader(_program, vertexShader);
            _gl.AttachShader(_program, fragmentShader);

            // Attempt to "link" the program together.
            _gl.LinkProgram(_program);

            // Similar to shader compilation, check to make sure that the shader program has linked properly.
            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int)GLEnum.True)
                throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

            // Detach and delete our shaders. Once a program is linked, we no longer need the individual shader objects.
            _gl.DetachShader(_program, vertexShader);
            _gl.DetachShader(_program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            // Set up our vertex attributes! These tell the vertex array (VAO) how to process the vertex data we defined
            // earlier. Each vertex array contains attributes. 

            // Our stride constant. The stride must be in bytes, so we take the first attribute (a vec3), multiply it
            // by the size in bytes of a float, and then take our second attribute (a vec2), and do the same.
            const uint stride = (3 * sizeof(float)) + (2 * sizeof(float));

            // Enable the "aPosition" attribute in our vertex array, providing its size and stride too.
            const uint positionLoc = 0;
            _gl.EnableVertexAttribArray(positionLoc);
            _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride, (void*)0);

            // Now we need to enable our texture coordinates! We've defined that as location 1 so that's what we'll use
            // here. The code is very similar to above, but you must make sure you set its offset to the **size in bytes**
            // of the attribute before.
            const uint textureLoc = 1;
            _gl.EnableVertexAttribArray(textureLoc);
            _gl.VertexAttribPointer(textureLoc, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

            // Unbind everything as we don't need it.
            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);


            // Now we create our texture!
            // First, we create the texture itself. Then, we must set an active texture unit. Each texture unit is a
            // separate bindable texture that we can use in a shader. GPUs have a maximum number of texture units they
            // can use, however the OpenGL spec states there MUST be at least 32 units available.
            // Much like buffers, we then bind the texture to a Texture2D target.
            for (var i = 0; i < _textures.Length; i++)
            {
                _textures[i] = new Texture(_gl, $"person/{(i + 1).ToString().PadLeft(2, '0')}.png");
            }
            _texture = _textures[0];
            // Get our texture uniform, and set it to 0.
            // We can easily do this by using glGetUniformLocation and giving it a name.
            // Setting it to 0 tells it that you want it to use the 0th texture unit.
            // Generally, OpenGL should automatically initialize all uniform values to their default value (which is
            // almost always 0), however you should get into the practice of initializing all uniform values to a known
            // value, before you use them in your shader.
            int location = _gl.GetUniformLocation(_program, "uTexture");
            _gl.Uniform1(location, 0);

            // Finally a bit of blending!
            // If you disable blending, you'll notice a black border around the texture.
            // The texture is partially transparent, however OpenGL doesn't know how to handle this by default.
            // By enabling blending, and giving it a blend function, you can tell OpenGL how to handle transparency.
            // In this case, it removes the black background and just leaves the texture on its own.
            // The blend function is out of scope for this tutorial, so don't worry if you don't understand it too much.
            // The program will function just fine without blending!
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            SitSize();
            _gameTime = DateTime.Now;
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            base.OnOpenGlDeinit(gl);
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
        {

            try
            {
                // Clear the window to the color we set earlier.
                _gl.Clear(ClearBufferMask.ColorBufferBit);

                // Bind our VAO, then the program.
                _gl.BindVertexArray(_vao);
                _gl.UseProgram(_program);

                _texture.Bind();
                // Much like our texture creation earlier, we must first set our active texture unit, and then bind the
                // texture to use it during draw!
                if (_animation.AddMilliseconds(100) < _gameTime)
                {
                    _current += 1;
                    if (_current > 13)
                    {
                        _current = 0;
                    }
                    _texture = _textures[_current];
                    _animation = _gameTime;
                }

                // Draw our quad! We use a count of 6 here because we have 6 total vertices that makes up a quad.
                _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
                _gameTime = DateTime.Now;
                Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                Logger.LogError("render", $"{ex.Message}");
            }
        }
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            SitSize();
        }

        private void SitSize()
        {
            if (_gl == null) return;
            var topLevel = TopLevel.GetTopLevel(this)!;
            if (topLevel.PlatformImpl is TopLevelImpl platform)
            {
                var size = platform!.Size;
                var scal = platform!.Scaling;
                Logger.Log("frameSzie", $"{size.Width},{size.Height} scaling: ${scal}");
                var y = (size.Height - _texture.Height * scal) / 2;
                _gl.Viewport(0, (int)y, (uint)(_texture.Width * scal), (uint)(_texture.Height * scal));
            }
            else
            {
                var window = this.GetVisualRoot() as Window;
                var bounds = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
                var y = (bounds.Height - _texture.Height) / 2;
                Logger.Log("resize", "y:" + y);
                _gl.Viewport(0, y, (uint)(_texture.Width * window!.RenderScaling), (uint)(_texture.Height * window!.RenderScaling));
            }
        }

    }
}