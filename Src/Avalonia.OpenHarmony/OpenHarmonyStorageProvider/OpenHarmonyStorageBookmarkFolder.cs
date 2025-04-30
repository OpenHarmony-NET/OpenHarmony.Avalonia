using Avalonia.Platform.Storage;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyStorageBookmarkFolder : OpenHarmonyStorageBookmarkItemBase, IStorageBookmarkFolder
{
    internal OpenHarmonyStorageBookmarkFolder(string uri) : base(uri)
    {
        _base = new DirectoryInfo(Path.ToString());
    }

    public async IAsyncEnumerable<IStorageItem> GetItemsAsync()
    {
        foreach (var item in await Task.Run(() => ((DirectoryInfo)_base).GetDirectories()))
        {
            yield return new OpenHarmonyStorageBookmarkFolder(item.FullName);
        }

        foreach (var item in await Task.Run(() => ((DirectoryInfo)_base).GetFiles()))
        {
            yield return new OpenHarmonyStorageBookmarkFile(item.FullName);
        }
    }

    [Obsolete("你无权对用户文件执行此操作。")]
    public Task<IStorageFile?> CreateFileAsync(string name)
    {
        throw new NotSupportedException("你无权对用户文件执行此操作。");
    }

    [Obsolete("你无权对用户文件执行此操作。")]
    public Task<IStorageFolder?> CreateFolderAsync(string name)
    {
        throw new NotSupportedException("你无权对用户文件执行此操作。");
    }
}