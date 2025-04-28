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
        if (_result is not null) return Task.FromResult<IReadOnlyList<IStorageFile>>([]);
        StartDocumentViewPicker?.Invoke(new DocumentSelectOptions(options));
        _result ??= new TaskCompletionSource<IReadOnlyList<string>>();
        return Task.Run<IReadOnlyList<IStorageFile>>(async () =>
        {
            return (await _result.Task).Select(x => new OpenHarmonyStorageBookmarkFile(x)).ToList();
        });
    }

    public Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        if (_result is not null) return Task.FromResult<IStorageFile?>(null);
        StartDocumentViewPickerSaveMode?.Invoke(new DocumentSaveOptions(options));
        _result ??= new TaskCompletionSource<IReadOnlyList<string>>();
        return Task.Run<IStorageFile?>(async () =>
        {
            var result = (await _result.Task).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(result)) return null;
            return new OpenHarmonyStorageBookmarkFile(result);
        });
    }

    public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
    {
        if (_result is not null) return Task.FromResult<IReadOnlyList<IStorageFolder>>([]);
        StartDocumentViewPicker?.Invoke(new DocumentSelectOptions(options));
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
    private static napi_env _env_StartDocumentViewPickerSaveMode;
    private static napi_ref _callbackRef_StartDocumentViewPicker;
    private static napi_ref _callbackRef_StartDocumentViewPickerSaveMode;


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

    internal record DocumentSaveOptions
    {
        internal DocumentSaveOptions(FilePickerSaveOptions options)
        {
            newFileNames = string.IsNullOrWhiteSpace(options.SuggestedFileName)
                ? null
                : [options.SuggestedFileName + options.DefaultExtension];
            defaultFilePathUri = options.SuggestedStartLocation?.Path.ToString();
            fileSuffixChoices = options.FileTypeChoices?.Where(x => x.Patterns is not null)
                .Select(x => $"{x.Name}|{string.Join(",", x.Patterns!.FirstOrDefault()?.Remove(0, 1))}").ToArray();
        }

        public string[]? newFileNames { get; set; }
        public string? defaultFilePathUri { get; set; }
        public string[]? fileSuffixChoices { get; set; }
        public DocumentPickerMode? pickerMode { get; set; }
    }

    internal enum DocumentSelectMode
    {
        FILE = 0, //文件类型。
        FOLDER = 1, //文件夹类型。
        MIXED = 2 //文件和文件夹混合类型。
    }

    internal enum DocumentPickerMode
    {
        DEFAULT = 0, //标准模式
        DOWNLOAD = 1 //下载模式
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
            var argsForATF = stackalloc napi_value[5];
            napi_value arkTSFunc;
            node_api.napi_get_reference_value(env, _callbackRef_StartDocumentViewPicker, &arkTSFunc);
            if (options.maxSelectNumber.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, options.maxSelectNumber.Value, argsForATF);
            if (options.defaultFilePathUri is not null)
            {
                var defaultFilePathUriPtr = Marshal.StringToCoTaskMemUTF8(options.defaultFilePathUri);
                node_api.napi_create_string_utf8(_env_StartDocumentViewPicker, (sbyte*)defaultFilePathUriPtr,
                    (ulong)Encoding.UTF8.GetBytes(options.defaultFilePathUri).Length, &argsForATF[1]);
                Marshal.ZeroFreeCoTaskMemUTF8(defaultFilePathUriPtr);
            }

            if (options.fileSuffixFilters is not null)
            {
                napi_value array;
                node_api.napi_create_array_with_length(_env_StartDocumentViewPicker,
                    (ulong)options.fileSuffixFilters.Length, &array);
                for (var i = 0; i < options.fileSuffixFilters.Length; i++)
                {
                    napi_value str;
                    var fileSuffixFilterPtr = Marshal.StringToCoTaskMemUTF8(options.fileSuffixFilters[i]);
                    node_api.napi_create_string_utf8(_env_StartDocumentViewPicker,
                        (sbyte*)fileSuffixFilterPtr,
                        (ulong)Encoding.UTF8.GetBytes(options.fileSuffixFilters[i]).Length, &str);
                    node_api.napi_set_element(_env_StartDocumentViewPicker, array, (uint)i, str);
                    Marshal.ZeroFreeCoTaskMemUTF8(fileSuffixFilterPtr);
                }

                argsForATF[2] = array;
            }

            if (options.selectMode.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, (int)options.selectMode.Value, &argsForATF[3]);
            if (options.authMode.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPicker, Convert.ToInt32(options.authMode.Value),
                    &argsForATF[4]);

            node_api.napi_call_function(_env_StartDocumentViewPicker, default, arkTSFunc, 5, argsForATF, null);
        };
        return default;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setStartDocumentViewPickerSaveMode")]
    public static unsafe napi_value SetStartDocumentViewPickerSaveMode(napi_env env, napi_callback_info info)
    {
        ulong argc = 1;
        var args = stackalloc napi_value[1];
        node_api.napi_get_cb_info(env, info, &argc, args, null, null);
        napi_ref ptr;
        node_api.napi_create_reference(env, args[0], 1, &ptr);
        _env_StartDocumentViewPickerSaveMode = env;
        _callbackRef_StartDocumentViewPickerSaveMode = ptr;
        StartDocumentViewPickerSaveMode = options =>
        {
            var argsForATF = stackalloc napi_value[4];
            napi_value arkTSFunc;
            node_api.napi_get_reference_value(env, _callbackRef_StartDocumentViewPickerSaveMode, &arkTSFunc);
            if (options.newFileNames is not null)
            {
                napi_value array;
                node_api.napi_create_array_with_length(_env_StartDocumentViewPickerSaveMode,
                    (ulong)options.newFileNames.Length, &array);
                for (var i = 0; i < options.newFileNames.Length; i++)
                {
                    napi_value str;
                    var fileSuffixFilterPtr = Marshal.StringToCoTaskMemUTF8(options.newFileNames[i]);
                    node_api.napi_create_string_utf8(_env_StartDocumentViewPickerSaveMode,
                        (sbyte*)fileSuffixFilterPtr,
                        (ulong)Encoding.UTF8.GetBytes(options.newFileNames[i]).Length, &str);
                    node_api.napi_set_element(_env_StartDocumentViewPicker, array, (uint)i, str);
                    Marshal.ZeroFreeCoTaskMemUTF8(fileSuffixFilterPtr);
                }

                argsForATF[0] = array;
            }

            if (options.defaultFilePathUri is not null)
            {
                var defaultFilePathUriPtr = Marshal.StringToCoTaskMemUTF8(options.defaultFilePathUri);
                node_api.napi_create_string_utf8(_env_StartDocumentViewPickerSaveMode, (sbyte*)defaultFilePathUriPtr,
                    (ulong)Encoding.UTF8.GetBytes(options.defaultFilePathUri).Length, &argsForATF[1]);
                Marshal.ZeroFreeCoTaskMemUTF8(defaultFilePathUriPtr);
            }

            if (options.fileSuffixChoices is not null)
            {
                napi_value array;
                node_api.napi_create_array_with_length(_env_StartDocumentViewPickerSaveMode,
                    (ulong)options.fileSuffixChoices.Length, &array);
                for (var i = 0; i < options.fileSuffixChoices.Length; i++)
                {
                    napi_value str;
                    var fileSuffixFilterPtr = Marshal.StringToCoTaskMemUTF8(options.fileSuffixChoices[i]);
                    node_api.napi_create_string_utf8(_env_StartDocumentViewPickerSaveMode,
                        (sbyte*)fileSuffixFilterPtr,
                        (ulong)Encoding.UTF8.GetBytes(options.fileSuffixChoices[i]).Length, &str);
                    node_api.napi_set_element(_env_StartDocumentViewPicker, array, (uint)i, str);
                    Marshal.ZeroFreeCoTaskMemUTF8(fileSuffixFilterPtr);
                }

                argsForATF[2] = array;
            }

            if (options.pickerMode.HasValue)
                node_api.napi_create_int32(_env_StartDocumentViewPickerSaveMode, (int)options.pickerMode.Value,
                    &argsForATF[3]);

            node_api.napi_call_function(_env_StartDocumentViewPickerSaveMode, default, arkTSFunc, 4, argsForATF, null);
        };
        return default;
    }

    internal static Action<DocumentSaveOptions>? StartDocumentViewPickerSaveMode { get; set; }

    #endregion
}