using Avalonia.Platform.Storage;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyStorageBookmarkFolder : OpenHarmonyStorageBookmarkItemBase, IStorageBookmarkFolder
{
    internal OpenHarmonyStorageBookmarkFolder(string uri) : base(uri)
    {
    }

    public IAsyncEnumerable<IStorageItem> GetItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFile?> CreateFileAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFolder?> CreateFolderAsync(string name)
    {
        throw new NotImplementedException();
    }
}