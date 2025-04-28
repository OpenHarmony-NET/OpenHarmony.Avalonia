using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform.Storage;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyStorageProvider : IStorageProvider
{
    public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        StartDocumentViewPicker?.Invoke(new DocumentSelectOptions(options));
        if (_result is not null) return Task.FromResult<IReadOnlyList<IStorageFile>>([]);
        _result ??= new TaskCompletionSource<IReadOnlyList<string>>();
        return Task.Run<IReadOnlyList<IStorageFile>>(async () =>
        {
            return (await _result.Task).Select(x => new OpenHarmonyStorageBookmarkFile(x)).ToList();
        });
    }

    public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        StartDocumentViewPicker?.Invoke(new DocumentSelectOptions(options));
        if (_result is not null) return Task.FromResult<IReadOnlyList<IStorageFolder>>([]);
        _result ??= new TaskCompletionSource<IReadOnlyList<string>>();
        return Task.Run<IReadOnlyList<IStorageFolder>>(async () =>
        {
            return (await _result.Task).Select(x => new OpenHarmonyStorageBookmarkFolder(x)).ToList();
        });
    }

    public Task<IStorageBookmarkFile?> OpenFileBookmarkAsync(string bookmark)
    {
        throw new NotImplementedException();
    }

    public Task<IStorageBookmarkFolder?> OpenFolderBookmarkAsync(string bookmark)
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFile?> TryGetFileFromPathAsync(Uri filePath)
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFolder?> TryGetFolderFromPathAsync(Uri folderPath)
    {
        throw new NotImplementedException();
    }

    public Task<IStorageFolder?> TryGetWellKnownFolderAsync(WellKnownFolder wellKnownFolder)
    {
        throw new NotImplementedException();
    }

    public bool CanOpen { get; }
    public bool CanSave { get; }
    public bool CanPickFolder { get; }

    #region ArkTS

    private static napi_env _env_StartDocumentViewPicker;
    private static napi_ref _callbackRef_StartDocumentViewPicker;


    internal static Action<DocumentSelectOptions>? StartDocumentViewPicker;

    internal record DocumentSelectOptions
    {
        internal DocumentSelectOptions(PickerOptions options)
        {
            defaultFilePathUri = options.SuggestedStartLocation?.Path.ToString();
            if (options is FilePickerOpenOptions openOptions)
            {
                fileSuffixFilters = openOptions.FileTypeFilter?.Where(x => x.Patterns is not null)
                    .Select(x => $"{x.Name}|{string.Join(",", x.Patterns!.Select(y => y.Remove(0, 1)))}").ToArray();
                if (fileSuffixFilters is not null)
                    foreach (var fileSuffixFilter in fileSuffixFilters)
                        OHDebugHelper.Info("in .NET fileSuffixFilters:" + fileSuffixFilter);
                maxSelectNumber = openOptions.AllowMultiple ? 500 : 1;
            }

            if (options is FolderPickerOpenOptions folderPickerOpenOptions)
                maxSelectNumber = folderPickerOpenOptions.AllowMultiple ? 500 : 1;

            selectMode = options is FilePickerOpenOptions ? DocumentSelectMode.FILE : DocumentSelectMode.FOLDER;
        }

        public int? maxSelectNumber { get; set; }
        public string? defaultFilePathUri { get; set; }
        public string[]? fileSuffixFilters { get; set; }
        public DocumentSelectMode? selectMode { get; set; }
        public bool? authMode { get; set; }
    }

    internal enum DocumentSelectMode
    {
        FILE = 0, //文件类型。
        FOLDER = 1, //文件夹类型。
        MIXED = 2 //文件和文件夹混合类型。
    }

    internal static TaskCompletionSource<IReadOnlyList<string>>? _result;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setPickerResult")]
    public static unsafe napi_value SetPickerResult(napi_env env, napi_callback_info info)
    {
        try
        {
            ulong argc = 1;
            var args = stackalloc napi_value[1];
            node_api.napi_get_cb_info(env, info, &argc, args, null, null);
            var result = args[0];
            bool isArray;
            node_api.napi_is_array(_env_StartDocumentViewPicker, result, &isArray);
            if (isArray)
            {
                uint arrayLength;
                node_api.napi_get_array_length(_env_StartDocumentViewPicker, result, &arrayLength);
                List<string> resultList = new();
                var buf = stackalloc sbyte[2048];
                for (var i = 0; i < arrayLength; i++)
                {
                    napi_value element;
                    ulong resultLength;
                    node_api.napi_get_element(_env_StartDocumentViewPicker, result, (uint)i, &element);
                    node_api.napi_get_value_string_utf8(_env_StartDocumentViewPicker, element, buf, 2048,
                        &resultLength);
                    var str = Marshal.PtrToStringUTF8((IntPtr)buf, (int)resultLength);
                    resultList.Add(str);
                    //todo 这里在将来需要更新一下值的获取，否则当返回的路径长度大于2048时将无法完整的获得路径。
                }

                _result?.TrySetResult(resultList);
                _result = null;
            }

            _result?.SetResult([]);
        }
        catch (Exception e)
        {
            OHDebugHelper.Error(nameof(SetPickerResult), e);
        }

        return default;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setStartDocumentViewPicker")]
    public static unsafe napi_value SetStartDocumentViewPicker(napi_env env, napi_callback_info info)
    {
        ulong argc = 1;
        var args = stackalloc napi_value[1];
        node_api.napi_get_cb_info(env, info, &argc, args, null, null);
        napi_ref ptr;
        node_api.napi_create_reference(env, args[0], 1, &ptr);
        _env_StartDocumentViewPicker = env;
        _callbackRef_StartDocumentViewPicker = ptr;
        StartDocumentViewPicker = options =>
        {
            var args = stackalloc napi_value[5];
            napi_value arkTSFunc;
            node_api.napi_get_reference_value(env, _callbackRef_StartDocumentViewPicker, &arkTSFunc);
            if (options.maxSelectNumber.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, options.maxSelectNumber.Value, args);
            if (options.defaultFilePathUri is not null)
            {
                var ptr = Marshal.StringToCoTaskMemUTF8(options.defaultFilePathUri);
                node_api.napi_create_string_utf8(_env_StartDocumentViewPicker, (sbyte*)ptr,
                    (ulong)Encoding.UTF8.GetBytes(options.defaultFilePathUri).Length, &args[1]);
                Marshal.ZeroFreeCoTaskMemUTF8(ptr);
            }

            if (options.fileSuffixFilters is not null)
            {
                napi_value array;
                node_api.napi_create_array_with_length(_env_StartDocumentViewPicker,
                    (ulong)options.fileSuffixFilters.Length, &array);
                for (var i = 0; i < options.fileSuffixFilters.Length; i++)
                {
                    napi_value str;
                    var ptr = Marshal.StringToCoTaskMemUTF8(options.fileSuffixFilters[i]);
                    var code = node_api.napi_create_string_utf8(_env_StartDocumentViewPicker, (sbyte*)ptr, (ulong)Encoding.UTF8.GetBytes(options.fileSuffixFilters[i]).Length, &str);
                    OHDebugHelper.Info("node_api.napi_create_string_utf8:" + code);
                    code = node_api.napi_set_element(_env_StartDocumentViewPicker, array, (uint)i, str);
                    OHDebugHelper.Info("node_api.napi_set_element:" + code);
                    Marshal.ZeroFreeCoTaskMemUTF8(ptr);
                }

                args[2] = array;
            }

            if (options.selectMode.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, (int)options.selectMode.Value, &args[3]);
            if (options.authMode.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, Convert.ToInt32(options.authMode.Value),
                    &args[4]);

            node_api.napi_call_function(_env_StartDocumentViewPicker, default, arkTSFunc, 5, args, null);
        };
        return default;
    }

    #endregion
}