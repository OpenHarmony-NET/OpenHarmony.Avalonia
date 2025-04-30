using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Storage;
using OpenHarmony.NDK.Bindings.Core_File_Kit;

namespace Avalonia.OpenHarmony;

internal class OpenHarmonyStorageBookmarkFile : OpenHarmonyStorageBookmarkItemBase, IStorageBookmarkFile
{
    internal OpenHarmonyStorageBookmarkFile(string uri) : base(uri)
    {
        _base = new FileInfo(Path.ToString());
    }

    public Task<Stream> OpenReadAsync()
    {
        return Task.Run<Stream>(() => File.OpenRead(Path.ToString()));
    }

    public Task<Stream> OpenWriteAsync()
    {
        return Task.Run<Stream>(() => File.OpenWrite(Path.ToString()));
    }
}