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
using Shader = AvaloniaGame.Gl.Shader;
namespace AvaloniaGame.Views
{
    public unsafe class TextureExample : OpenGlControlBase
    {
        private static GL? _gl = null;

        private static uint _vao;
        private static uint _vbo;
        private static uint _ebo;

        private Texture _texture = null!;

        private Texture[] _textures = new Texture[14];
        private DateTime _gameTime = DateTime.Now;
        private DateTime _animation = DateTime.Now;
        private int _current = 0;
        private Shader _shader = null!;
        private double _scale = 1;
        private PixelSize _windowSize = new PixelSize();
        protected override void OnOpenGlInit(GlInterface gl)
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
            [
                // positions         // texture coords
                1f,  -1f, 0.0f,     1.0f, 1.0f, // top right
                1f,  1f, 0.0f,      1.0f, 0.0f, // bottom right
                -1f, 1f, 0.0f,      0.0f, 0.0f, // bottom left
                -1f, -1f, 0.0f,     0.0f, 1.0f  // top left 
            ];

            // Create the VBO.
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            // Upload the vertices data to the VBO.
            fixed (float* buf = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

            // The quad indices data.
            uint[] indices =
            [
                0u, 1u, 3u,
                1u, 2u, 3u
            ];

            // Create the EBO.
            _ebo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

            // Upload the indices data to the EBO.
            fixed (uint* buf = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            _shader = new Shader(_gl, "shaders/shader.vert", "shaders/shader.frag");

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
            int location = _gl.GetUniformLocation(_shader.Handle, "uTexture");
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

            SetSize();
            _gameTime = DateTime.Now;
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            base.OnOpenGlDeinit(gl);
            _shader.Dispose();
            foreach (var texture in _textures)
            {
                texture.Dispose();
            }
            _textures = [];
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
        {

            try
            {
                // Clear the window to the color we set earlier.
                _gl!.Clear(ClearBufferMask.ColorBufferBit);

                // Bind our VAO, then the program.
                _gl.BindVertexArray(_vao);
                // _gl.UseProgram(_program);
                _shader.Use();
                Viewport(0, (int)(_windowSize.Height - _texture.Height * _scale) / 2, (uint)_texture.Width, (uint)_texture.Height);
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
            SetSize();
        }

        private void SetSize()
        {
            if (_gl == null) return;
            var topLevel = TopLevel.GetTopLevel(this)!;
            if (topLevel.PlatformImpl is TopLevelImpl platform)
            {
                _windowSize = platform.Size;
                _scale = platform.Scaling;
                Logger.Log("resize", $"{_windowSize.Width},{_windowSize.Height} scaling:{_scale}");
                _gl.Viewport(0, 0, (uint)_windowSize.Width, (uint)_windowSize.Height);
            }
            else
            {
                var window = this.GetVisualRoot() as Window;
                _scale = window!.RenderScaling;
                _windowSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
                Logger.Log("resize", $"{Bounds.Width},{Bounds.Height}  scaling:{_scale}");
                _gl.Viewport(0, 0, (uint)Bounds.Width, (uint)Bounds.Height);
            }
        }
        
        private void Viewport(int x, int y,uint width,uint height)
        {
            _gl!.Viewport(x, y, (uint)(width * _scale), (uint)(height * _scale));
        }

    }
}