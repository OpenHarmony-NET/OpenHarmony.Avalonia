using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Storage;
using OpenHarmony.NDK.Bindings.Core_File_Kit;

namespace Avalonia.OpenHarmony;

public abstract class OpenHarmonyStorageBookmarkItemBase : IStorageBookmarkItem
{
    private readonly string sourceUri;

    internal unsafe OpenHarmonyStorageBookmarkItemBase(string uri)
    {
        sourceUri = uri;
        var ptr = (char*)Marshal.StringToHGlobalAnsi(uri);
        char* result = null;
        FileUri.OH_FileUri_GetPathFromUri(ptr, (uint)Encoding.ASCII.GetBytes(uri).Length, &result);
        var path = Marshal.PtrToStringAnsi((IntPtr)result);
        path ??= "沙箱路径转换失败";
        Name = System.IO.Path.GetFileName(path);
        Path = new Uri(path);
        CanBookmark = false;
        Marshal.FreeHGlobal((nint)ptr);
    }

    public void Dispose()
    {
        // TODO 在此释放托管资源
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
}