using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Storage;
using OpenHarmony.NDK.Bindings.Core_File_Kit;

namespace Avalonia.OpenHarmony;

public abstract class OpenHarmonyStorageBookmarkItemBase : IStorageBookmarkItem
{
    protected readonly string _sourceUri;
    protected FileSystemInfo _base;

    protected unsafe OpenHarmonyStorageBookmarkItemBase(string uri)
    {
        _sourceUri = uri;
        var path = OpenHarmonyStorageProvider.GetPathByOpenHarmonyUri(uri);
        path ??= "沙箱路径转换失败";
        Name = System.IO.Path.GetFileName(path);
        Path = new Uri(path);
        CanBookmark = true;
    }

    public void Dispose()
    {
        // TODO 在此释放托管资源
    }

    public Task<StorageItemProperties> GetBasicPropertiesAsync()
    {
        return Task.FromResult(new StorageItemProperties(null, _base.CreationTime, _base.LastWriteTime));
    }

#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
    public Task<string?> SaveBookmarkAsync() => Task.FromResult(_sourceUri);
#pragma warning restore CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。

    [Obsolete("你无权对用户文件执行此操作。")]
    public virtual Task<IStorageFolder?> GetParentAsync()
    {
        // return Task.FromResult<IStorageFolder?>(_base.);
        throw new NotSupportedException("你无权对用户文件执行此操作。");
    }

    [Obsolete("你无权对用户文件执行此操作。")]
    public Task DeleteAsync()
    {
        throw new NotSupportedException("你无权对用户文件执行此操作。");
    }

    [Obsolete("你无权对用户文件执行此操作。")]
    public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
    {
        throw new NotSupportedException("你无权对用户文件执行此操作。");
    }

    public string Name { get; }
    public Uri Path { get; }
    public bool CanBookmark { get; }

    public Task ReleaseBookmarkAsync()
    {
        return Task.CompletedTask;
    }
}