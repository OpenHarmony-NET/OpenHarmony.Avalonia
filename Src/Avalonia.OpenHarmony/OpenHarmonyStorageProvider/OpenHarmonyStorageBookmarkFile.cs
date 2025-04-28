using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Storage;
using OpenHarmony.NDK.Bindings.Core_File_Kit;

namespace Avalonia.OpenHarmony;

internal unsafe class OpenHarmonyStorageBookmarkFile : IStorageBookmarkFile
{
    internal OpenHarmonyStorageBookmarkFile(string uri)
    {
        var ptr = (char*)Marshal.StringToHGlobalAnsi(uri);
        char* result = null;
        var code = FileUri.OH_FileUri_GetPathFromUri(ptr, (uint)Encoding.ASCII.GetBytes(uri).Length, &result);
        // OHDebugHelper.Debug($"沙箱路径转换结果：{code}");
        var path = Marshal.PtrToStringAnsi((IntPtr)result);
        path ??= "沙箱路径转换失败";
        Name = System.IO.Path.GetFileName(path);
        Path = new Uri(path);
        CanBookmark = false;
        Marshal.FreeHGlobal((nint)ptr);
    }

    public void Dispose()
    {
    }

    public Task<StorageItemProperties> GetBasicPropertiesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string?> SaveBookmarkAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFolder?> GetParentAsync()
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
    {
        throw new NotImplementedException();
    }

    public string Name { get; }
    public Uri Path { get; }
    public bool CanBookmark { get; }

    public Task ReleaseBookmarkAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenReadAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenWriteAsync()
    {
        throw new NotImplementedException();
    }
}