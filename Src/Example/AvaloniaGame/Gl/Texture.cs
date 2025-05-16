using Silk.NET.OpenGLES;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Avalonia.Platform;
using StbImageSharp;
using PixelFormat = Silk.NET.OpenGLES.PixelFormat;

namespace AvaloniaGame.Gl
{
    public class Texture : IDisposable
    {
        private uint _handle;
        private GL _gl;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public unsafe Texture(GL gl, string path)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            using var stream = AssetLoader.Open(new Uri($"avares://AvaloniaGame/Assets/{path}"));
            ImageResult result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            fixed (byte* ptr = result.Data)
            {
                // Upload our texture data to the GPU.
                // Let's go over each parameter used here:
                // 1. Tell OpenGL that we want to upload to the texture bound in the Texture2D target.
                // 2. We are uploading the "base" texture level, therefore this value should be 0. You don't need to
                //    worry about texture levels for now.
                // 3. We tell OpenGL that we want the GPU to store this data as RGBA formatted data on the GPU itself.
                // 4. The image's width.
                // 5. The image's height.
                // 6. This is the image's border. This valu MUST be 0. It is a leftover component from legacy OpenGL, and
                //    it serves no purpose.
                // 7. Our image data is formatted as RGBA data, therefore we must tell OpenGL we are uploading RGBA data.
                // 8. StbImageSharp returns this data as a byte[] array, therefore we must tell OpenGL we are uploading
                //    data in the unsigned byte format.
                // 9. The actual pointer to our data!
                Width = result.Width;
                Height = result.Height;
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                    (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            SetParameters();

        }

        private void SetParameters()
        {
            //Setting some texture perameters so the texture behaves as expected.
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            //Generating mipmaps.
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            //When we bind a texture we can choose which textureslot we can bind it to.
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void Dispose()
        {
            //In order to dispose we need to delete the opengl handle for the texure.
            _gl.DeleteTexture(_handle);
        }
    }
}
