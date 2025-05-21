using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.OpenHarmony;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaGame.Gl;
using Silk.NET.OpenGLES;
using Shader = AvaloniaGame.Gl.Shader;

namespace AvaloniaGame.Views
{
    public class ShaderExample : OpenGlControlBase
    {
        private GL Gl;
        private BufferObject<float> Vbo;
        private BufferObject<uint> Ebo;
        private VertexArrayObject<float, uint> Vao;
        private Shader Shader;
        private PixelSize _pixelSize = new PixelSize();
        private static readonly float[] Vertices =
        {
            //X    Y      Z     R  G  B  A
            0.5f,  0.5f, 0.0f, 1, 0, 0, 1,
            0.5f, -0.5f, 0.0f, 0, 0, 0, 1,
            -0.5f, -0.5f, 0.0f, 0, 0, 1, 1,
            -0.5f,  0.5f, 0.5f, 0, 0, 0, 1
        };

        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };



        protected override void OnOpenGlInit(GlInterface gl)
        {
            base.OnOpenGlInit(gl);

            Gl = GL.GetApi(gl.GetProcAddress);
            SetSize();
            //Instantiating our new abstractions
            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Logger.Log("Ebo", "loaded");

            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Logger.Log("Vbo", "loaded");

            Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);
            Logger.Log("Vao", "loaded");
            //Telling the VAO object how to lay out the attribute pointers
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 7, 0);
            Logger.Log("VaoVertex0", "loaded");
            Vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 7, 3);
            Logger.Log("VaoVertex1", "loaded");
            Shader = new Shader(Gl, "shader.vert", "shader.frag");
            Logger.Log("shader", "loaded");
        }
        private void SetSize()
        {
            if (Gl == null) return;
            var topLevel = TopLevel.GetTopLevel(this)!;
            if ( topLevel.PlatformImpl is TopLevelImpl platform)
            {
                var size = platform!.Size;
                if (_pixelSize == size)
                {
                    return;
                }
                _pixelSize = size;
                var scal = platform!.Scaling;
                Logger.Log("frameSzie", $"{size.Width},{size.Height} scaling: ${scal}");
                Gl.Viewport(0, 0, (uint)size.Width, (uint)size.Height);
            }
            else
            {
                var window = this.GetVisualRoot() as Window;
                var bounds = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
                if (_pixelSize == bounds) return;
                _pixelSize = bounds;
                Gl.Viewport(0, 0, (uint)Bounds.Width,(uint)Bounds.Height);
            }
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            Shader.Dispose();
            base.OnOpenGlDeinit(gl);
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
        {

            try
            {

                Gl.ClearColor(System.Drawing.Color.Firebrick);
                Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                Gl.Enable(EnableCap.DepthTest);
                

                Ebo.Bind();
                Vbo.Bind();
                Vao.Bind();
                Shader.Use();
                Shader.SetUniform("uBlue", (float)Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));
                Gl.Viewport(0, 0, (uint)_pixelSize.Width,(uint)_pixelSize.Height);
                Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
                Logger.Log("render", $"draw {Indices.Length}");
                Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
                Logger.Log("render", $"next render");
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

    }
}