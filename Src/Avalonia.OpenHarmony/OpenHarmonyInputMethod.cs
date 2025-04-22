using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Input.TextInput;
using Avalonia.Threading;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public unsafe class OpenHarmonyInputMethod : ITextInputMethodImpl
{
    private static OpenHarmonyInputMethod? _instance;
    private static TextInputMethodClient? _client;

    private static InputMethod_KeyboardStatus _keyboardStatus;

    private static TextInputOptions? _options;
    private readonly TopLevelImpl _topLevelImpl;

    private readonly InputMethod_TextEditorProxy* textEditorProxy;
    private InputMethod_InputMethodProxy* _inputMethodProxy = null;

    private InputMethod_TextAvoidInfo* _inputMethodTextAvoidInfo;

    private double _inputPanelHeight;

    private bool _isEqualToPreviousValue;

    private double _positionY;

    public OpenHarmonyInputMethod(TopLevelImpl topLevelImpl)
    {
        if (_instance is not null)
            throw new InvalidOperationException("Only one instance of OpenHarmonyInputMethod is allowed.");

        _instance = this;
        _topLevelImpl = topLevelImpl;
        textEditorProxy = input_method.OH_TextEditorProxy_Create();
        input_method.OH_TextEditorProxy_SetGetTextConfigFunc(textEditorProxy,
            &input_method_harmony_get_text_config);
        input_method.OH_TextEditorProxy_SetInsertTextFunc(textEditorProxy,
            &input_method_harmony_insert_text);
        input_method.OH_TextEditorProxy_SetDeleteBackwardFunc(textEditorProxy,
            &input_method_harmony_delete_backward);
        input_method.OH_TextEditorProxy_SetDeleteForwardFunc(textEditorProxy,
            &input_method_harmony_delete_forward);
        input_method.OH_TextEditorProxy_SetSendKeyboardStatusFunc(textEditorProxy,
            &input_method_harmony_send_keyboard_status);
        input_method.OH_TextEditorProxy_SetSendEnterKeyFunc(textEditorProxy,
            &input_method_harmony_send_enter_key);
        input_method.OH_TextEditorProxy_SetMoveCursorFunc(textEditorProxy,
            &input_method_harmony_move_cursor);
        input_method.OH_TextEditorProxy_SetHandleSetSelectionFunc(textEditorProxy,
            &input_method_harmony_handle_set_selection);
        input_method.OH_TextEditorProxy_SetHandleExtendActionFunc(textEditorProxy,
            &input_method_harmony_handle_extend_action);
        input_method.OH_TextEditorProxy_SetGetLeftTextOfCursorFunc(textEditorProxy,
            &input_method_harmony_get_left_text_of_cursor);
        input_method.OH_TextEditorProxy_SetGetRightTextOfCursorFunc(textEditorProxy,
            &input_method_harmony_get_right_text_of_cursor);
        input_method.OH_TextEditorProxy_SetGetTextIndexAtCursorFunc(textEditorProxy,
            &input_method_harmony_get_text_index_at_cursor);
        input_method.OH_TextEditorProxy_SetReceivePrivateCommandFunc(textEditorProxy,
            &input_method_harmony_receive_private_command);
        input_method.OH_TextEditorProxy_SetSetPreviewTextFunc(textEditorProxy,
            &input_method_harmony_set_preview_text);
        input_method.OH_TextEditorProxy_SetFinishTextPreviewFunc(textEditorProxy,
            &input_method_harmony_finish_text_preview);
    }

    public double InputPanelHeight
    {
        get => _inputPanelHeight;
        set
        {
            if (Math.Abs(_inputPanelHeight - value) >= 0.001f)
            {
                _inputPanelHeight = value;
                InputPanelHeightChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetInputPanelHeight(double value)
    {
        if (Math.Abs(_inputPanelHeight - value) >= 0.00f)
        {
            _inputPanelHeight = value;
            try
            {
                var result = input_method.OH_TextAvoidInfo_SetHeight(_inputMethodTextAvoidInfo, _inputPanelHeight);
                OHDebugHelper.Debug($"输入法面板的新高度设置结果：{result}");
            }
            catch (Exception e)
            {
                OHDebugHelper.Error($"输入法面板的新高度设置失败。\n新位置：{_inputPanelHeight}", e);
            }

            InputPanelHeightChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public double PositionY
    {
        get => _positionY;
        private set
        {
            if (Math.Abs(_positionY - value) >= 0.001f)
            {
                _positionY = value;
                PositionYChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetPositionY(double value)
    {
        if (Math.Abs(_positionY - value) >= 0.00f)
        {
            _positionY = value;
            try
            {
                var result = input_method.OH_TextAvoidInfo_SetPositionY(_inputMethodTextAvoidInfo, _positionY);
                OHDebugHelper.Debug($"输入法面板的新位置（基于左上角与物理屏幕的距离）设置结果：{result}");
            }
            catch (Exception e)
            {
                OHDebugHelper.Error($"输入法面板的新位置（基于左上角与物理屏幕的距离）设置失败。\n新位置：{_positionY}", e);
            }

            PositionYChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Reset()
    {
        var result = input_method.OH_InputMethodController_Detach(_inputMethodProxy);
        _inputMethodProxy = null;
        _inputMethodTextAvoidInfo = null;
        UpdateTextAvoidInfo();
        OHDebugHelper.Debug(
            $"""
             在输入控件失焦后解绑输入法。
             解绑结果：{result}
             """);
    }

    public void SetClient(TextInputMethodClient? client)
    {
        _isEqualToPreviousValue = _client == client;
        if (_client is not null) _client.SelectionChanged -= NotifySelectionChange;

        _client = client;

        if (_client is not null)
            _client.SelectionChanged += NotifySelectionChange;
        else Reset();
    }

    public void SetCursorRect(Rect rect)
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetCursorRect");
    }

    public void SetOptions(TextInputOptions options)
    {
        _options = options;
        if (_inputMethodProxy is not null)
            if (_keyboardStatus is InputMethod_KeyboardStatus.IME_KEYBOARD_STATUS_HIDE)
            {
                var result = input_method.OH_InputMethodProxy_ShowKeyboard(_inputMethodProxy);
                if (result is InputMethod_ErrorCode.IME_ERR_IMCLIENT)
                {
                    OH_InputMethodController_Attach();
                    OHDebugHelper.Debug(
                        $"""
                         在应用进入后台后重新进入应用时重新绑定输入法。
                         绑定结果：{result}
                         """);
                }
                else
                {
                    OHDebugHelper.Debug(
                        $"""
                         在输入控件没有失焦但输入法面板已关闭的情况下重新拉起输入法。
                         拉起结果：{result}
                         """);
                }

                if (_isEqualToPreviousValue) return;
            }

        if (_client is not null)
        {
            var code = OH_InputMethodController_Attach();
            OHDebugHelper.Debug(
                $"""
                 在输入控件获得焦点后绑定输入法。
                 绑定结果：{code}
                 """);
        }

        return;

        InputMethod_ErrorCode OH_InputMethodController_Attach()
        {
            var ohAttachOptionsCreate = input_method.OH_AttachOptions_Create(true);
            InputMethod_InputMethodProxy* ptr = null;
            var code = input_method.OH_InputMethodController_Attach(textEditorProxy, ohAttachOptionsCreate, &ptr);
            _inputMethodProxy = ptr;
            return code;
        }
    }

    /// <summary>
    ///     通知光标位置变化。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NotifySelectionChange(object? sender, EventArgs e)
    {
        if (_client is null) return;
        try
        {
            var strPtr = Marshal.StringToHGlobalUni(_client.SurroundingText);
            input_method.OH_InputMethodProxy_NotifySelectionChange(_inputMethodProxy,
                (char*)strPtr,
                (ulong)_client.SurroundingText.Length,
                _client.Selection.Start,
                _client.Selection.End);
            Marshal.FreeHGlobal(strPtr);
            OHDebugHelper.Debug(
                $"""
                 通知光标位置变化。
                 光标位置：{_client.Selection.Start} 至 {_client.Selection.End}
                 """);
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("通知光标位置变化失败。", exception);
        }
    }

    public event EventHandler? InputPanelHeightChanged;
    public event EventHandler? PositionYChanged;

    /// <summary>
    ///     输入法获取输入框配置时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="config"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void input_method_harmony_get_text_config(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_TextConfig* config)
    {
        OHDebugHelper.Debug("输入法获取输入框配置时触发的函数。被调用了");
        if (_client is null ||
            _instance is null ||
            _options is null) return;
        try
        {
            var enterKeyType = _options.ReturnKeyType switch
            {
                TextInputReturnKeyType.Default => InputMethod_EnterKeyType.IME_ENTER_KEY_UNSPECIFIED,
                TextInputReturnKeyType.Return => InputMethod_EnterKeyType.IME_ENTER_KEY_NEWLINE,
                TextInputReturnKeyType.Done => InputMethod_EnterKeyType.IME_ENTER_KEY_DONE,
                TextInputReturnKeyType.Go => InputMethod_EnterKeyType.IME_ENTER_KEY_GO,
                TextInputReturnKeyType.Send => InputMethod_EnterKeyType.IME_ENTER_KEY_SEND,
                TextInputReturnKeyType.Search => InputMethod_EnterKeyType.IME_ENTER_KEY_SEARCH,
                TextInputReturnKeyType.Next => InputMethod_EnterKeyType.IME_ENTER_KEY_NEXT,
                TextInputReturnKeyType.Previous => InputMethod_EnterKeyType.IME_ENTER_KEY_PREVIOUS,
                _ => throw new ArgumentOutOfRangeException()
            };
            input_method.OH_TextConfig_SetEnterKeyType(config, enterKeyType);
            var inputType = _options.ContentType switch
            {
                TextInputContentType.Normal => enterKeyType is InputMethod_EnterKeyType.IME_ENTER_KEY_NEWLINE
                    ? InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_MULTILINE
                    : InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_NONE,
                TextInputContentType.Alpha => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_TEXT,
                TextInputContentType.Digits => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_NUMBER,
                TextInputContentType.Pin => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_SCREEN_LOCK_PASSWORD,
                TextInputContentType.Number => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_NUMBER_DECIMAL,
                TextInputContentType.Email => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_EMAIL_ADDRESS,
                TextInputContentType.Url => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_URL,
                TextInputContentType.Name => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_USER_NAME,
                TextInputContentType.Password => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_VISIBLE_PASSWORD,
                TextInputContentType.Social => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_TEXT,
                TextInputContentType.Search => InputMethod_TextInputType.IME_TEXT_INPUT_TYPE_TEXT,
                _ => throw new ArgumentOutOfRangeException()
            };
            input_method.OH_TextConfig_SetInputType(config, inputType);
            input_method.OH_TextConfig_SetPreviewTextSupport(config, _client.SupportsPreedit);
            var selection = Dispatcher.UIThread.Invoke(() => _client.Selection);
            input_method.OH_TextConfig_SetSelection(config, selection.Start, selection.End);
            input_method.OH_TextConfig_SetWindowId(config, (int)_instance._topLevelImpl.programId);

            InputMethod_TextAvoidInfo* ptr = null;
            input_method.OH_TextConfig_GetTextAvoidInfo(config, &ptr);
            _instance._inputMethodTextAvoidInfo = ptr;
            _instance.UpdateTextAvoidInfo();
            DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(5);
            _disposable?.Dispose();
            _disposable = DispatcherTimer.Run(() =>
                {
                    _instance.UpdateTextAvoidInfo();
                    return DateTime.Now < dateTime;
                },
                TimeSpan.FromSeconds(0.1));
            OHDebugHelper.Debug($"输入法获取输入框配置时触发的函数。\n回车键模式：{enterKeyType}\n输入框类型：{inputType}");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法获取输入框配置时触发的函数。", e);
        }
    }

    private static IDisposable? _disposable;

    private void UpdateTextAvoidInfo()
    {
        if (_inputMethodTextAvoidInfo is null)
        {
            InputPanelHeight = 0;
            PositionY = 0;
            return;
        }

        try
        {
            double ptrDouble;
            var code = input_method.OH_TextAvoidInfo_GetHeight(_inputMethodTextAvoidInfo, &ptrDouble);
            // OHDebugHelper.Debug($"获取输入法高度的结果：{code}");
            if (code is not InputMethod_ErrorCode.IME_ERR_OK) InputPanelHeight = 0;
            else InputPanelHeight = ptrDouble;
            ptrDouble = 0;
            code = input_method.OH_TextAvoidInfo_GetPositionY(_inputMethodTextAvoidInfo, &ptrDouble);
            // OHDebugHelper.Debug($"获取输入法Y轴坐标的结果：{code}");
            if (code is not InputMethod_ErrorCode.IME_ERR_OK) PositionY = 0;
            else PositionY = ptrDouble;
        }
        catch (Exception e)
        {
            OHDebugHelper.Error($"输入法面板的新高度与位置设置失败。\n新位置：{_inputPanelHeight}", e);
        }
    }

    /// <summary>
    ///     输入法应用插入文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="text"></param>
    /// <param name="length"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_insert_text(InputMethod_TextEditorProxy* textEditorProxy, char* text,
        ulong length)
    {
        if (_client is null) return;
        try
        {
            var insetText = Marshal.PtrToStringUni((IntPtr)text, (int)length);
            _instance?._topLevelImpl.TextInput(insetText);
            OHDebugHelper.Debug($"输入法应用插入文本时触发的函数。\n插入文本：{insetText}");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法应用插入文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法删除光标左侧文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_delete_backward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
        if (_client is null ||
            _instance?._topLevelImpl.InputRoot is null) return;
        try
        {
            for (var i = 0; i < length; i++)
            {
                var args = Dispatcher.UIThread.Invoke(() => new RawKeyEventArgs(
                    OpenHarmonyKeyboardDevice.Instance,
                    (ulong)DateTime.Now.Ticks,
                    _instance._topLevelImpl.InputRoot,
                    RawKeyEventType.KeyDown,
                    Key.Back,
                    RawInputModifiers.None,
                    PhysicalKey.Backspace,
                    "\b"
                ));
                _instance._topLevelImpl.Input?.Invoke(args);
            }

            OHDebugHelper.Debug("输入法删除光标左侧文本时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法删除光标左侧文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法删除光标右侧文本时触发的函数。
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_delete_forward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
        if (_client is null ||
            _instance?._topLevelImpl.InputRoot is null) return;
        try
        {
            for (var i = 0; i < length; i++)
            {
                var args = Dispatcher.UIThread.Invoke(() => new RawKeyEventArgs(
                    OpenHarmonyKeyboardDevice.Instance,
                    (ulong)DateTime.Now.Ticks,
                    _instance._topLevelImpl.InputRoot,
                    RawKeyEventType.KeyDown,
                    Key.Delete,
                    RawInputModifiers.None,
                    PhysicalKey.Delete,
                    ((sbyte)46).ToString()
                ));
                _instance._topLevelImpl.Input?.Invoke(args);
            }

            OHDebugHelper.Debug("输入法删除光标右侧文本时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法删除光标右侧文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法通知键盘状态时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="keyboardStatus"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_send_keyboard_status(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_KeyboardStatus keyboardStatus)
    {
        if (_instance is null) return;
        if (_client is null) return;
        if (_keyboardStatus == keyboardStatus) return;
        _keyboardStatus = keyboardStatus;
        _instance.UpdateTextAvoidInfo();
        OHDebugHelper.Debug($"输入法通知键盘状态时触发的函数。\n新的键盘状态：{_keyboardStatus}");
    }

    /// <summary>
    ///     输入法发送回车键时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="enterKeyType"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_send_enter_key(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_EnterKeyType enterKeyType)
    {
        if (_client is null ||
            _instance?._topLevelImpl.InputRoot is null) return;

        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                var args = new RawKeyEventArgs(
                    OpenHarmonyKeyboardDevice.Instance,
                    (ulong)DateTime.Now.Ticks,
                    _instance._topLevelImpl.InputRoot,
                    RawKeyEventType.KeyDown,
                    Key.Enter,
                    RawInputModifiers.None,
                    PhysicalKey.Enter,
                    "\n"
                );
                _instance._topLevelImpl.Input?.Invoke(args);
            });
            OHDebugHelper.Debug("输入法发送回车键时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法发送回车键时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法移动光标时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="direction"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_move_cursor(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_Direction direction)
    {
        if (_client is null ||
            _instance?._topLevelImpl.InputRoot is null) return;
        try
        {
            var key = direction switch
            {
                InputMethod_Direction.IME_DIRECTION_LEFT => Key.Left,
                InputMethod_Direction.IME_DIRECTION_RIGHT => Key.Right,
                InputMethod_Direction.IME_DIRECTION_UP => Key.Up,
                InputMethod_Direction.IME_DIRECTION_DOWN => Key.Down,
                _ => Key.None
            };
            var physicalKey = direction switch
            {
                InputMethod_Direction.IME_DIRECTION_LEFT => PhysicalKey.ArrowLeft,
                InputMethod_Direction.IME_DIRECTION_RIGHT => PhysicalKey.ArrowRight,
                InputMethod_Direction.IME_DIRECTION_UP => PhysicalKey.ArrowUp,
                InputMethod_Direction.IME_DIRECTION_DOWN => PhysicalKey.ArrowDown,
                _ => PhysicalKey.None
            };
            var keySymbol = direction switch
            {
                InputMethod_Direction.IME_DIRECTION_LEFT => "\u2190",
                InputMethod_Direction.IME_DIRECTION_RIGHT => "\u2192",
                InputMethod_Direction.IME_DIRECTION_UP => "\u2191",
                InputMethod_Direction.IME_DIRECTION_DOWN => "\u2193",
                _ => null
            };
            Dispatcher.UIThread.Post(() =>
            {
                var args = new RawKeyEventArgs(
                    OpenHarmonyKeyboardDevice.Instance,
                    (ulong)DateTime.Now.Ticks,
                    _instance._topLevelImpl.InputRoot,
                    RawKeyEventType.KeyDown,
                    key,
                    RawInputModifiers.None,
                    physicalKey,
                    keySymbol
                );
                _instance._topLevelImpl.Input?.Invoke(args);
            });
            OHDebugHelper.Debug("输入法移动光标时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法移动光标时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法请求选中文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_handle_set_selection(InputMethod_TextEditorProxy* textEditorProxy,
        int start,
        int end)
    {
        if (_client is null) return;
        try
        {
            OHDebugHelper.Debug("输入法请求选中文本时触发的函数。");
            Dispatcher.UIThread.Post(() => _client.Selection = new TextSelection(start, end));
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法请求选中文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法发送扩展编辑操作时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="action"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_handle_extend_action(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_ExtendAction action)
    {
        if (_client is null) return;
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                _client.ExecuteContextMenuAction(action switch
                {
                    InputMethod_ExtendAction.IME_EXTEND_ACTION_COPY => ContextMenuAction.Copy,
                    InputMethod_ExtendAction.IME_EXTEND_ACTION_CUT => ContextMenuAction.Cut,
                    InputMethod_ExtendAction.IME_EXTEND_ACTION_PASTE => ContextMenuAction.Paste,
                    InputMethod_ExtendAction.IME_EXTEND_ACTION_SELECT_ALL => ContextMenuAction.SelectAll,
                    _ => throw new ArgumentOutOfRangeException()
                });
            });
            OHDebugHelper.Debug("输入法发送扩展编辑操作时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法发送扩展编辑操作时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法获取光标左侧文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="number"></param>
    /// <param name="text"></param>
    /// <param name="length"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_get_left_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy,
        int number,
        char* text, ulong* length)
    {
        if (_client?.TextViewVisual is not TextBox textBox) return;
        try
        {
            var str = Dispatcher.UIThread.Invoke(() =>
            {
                if (textBox.Text?.Length is not { } bl || bl is 0) return string.Empty;
                var caretIndex = textBox.CaretIndex;
                var l = (int)*length;
                if (l > bl) l = bl;

                return textBox.Text?.Substring(caretIndex, l) ?? string.Empty;
            });
            Marshal.Copy(Encoding.Unicode.GetBytes(str), 0, (IntPtr)text, (int)*length);
            OHDebugHelper.Debug("输入法获取光标左侧文本时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法获取光标左侧文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法获取光标右侧文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="number"></param>
    /// <param name="text"></param>
    /// <param name="length"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_get_right_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy,
        int number,
        char* text, ulong* length)
    {
        if (_client?.TextViewVisual is not TextBox textBox) return;
        try
        {
            var str = Dispatcher.UIThread.Invoke(() =>
            {
                if (textBox.Text?.Length is 0) return string.Empty;
                var caretIndex = textBox.CaretIndex;
                var l = (int)*length;
                var startIndex = caretIndex - l;
                if (startIndex < 0) startIndex = 0;

                return textBox.Text?.Substring(startIndex, caretIndex) ?? string.Empty;
            });
            Marshal.Copy(Encoding.Unicode.GetBytes(str), 0, (IntPtr)text, (int)*length);
            OHDebugHelper.Debug("输入法获取光标右侧文本时触发的函数。");
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法获取光标右侧文本时触发的函数。", e);
        }
    }

    /// <summary>
    ///     输入法获取光标所在输入框文本索引时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int input_method_harmony_get_text_index_at_cursor(InputMethod_TextEditorProxy* textEditorProxy)
    {
        if (_client?.TextViewVisual is not TextBox textBox) return 0;
        try
        {
            var result = Dispatcher.UIThread.Invoke(() => textBox.CaretIndex);
            OHDebugHelper.Debug("输入法获取光标所在输入框文本索引时触发的函数。");
            return result;
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法获取光标所在输入框文本索引时触发的函数。", e);
        }

        return 0;
    }

    /// <summary>
    ///     输入法应用发送私有数据命令时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="privateCommand"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int input_method_harmony_receive_private_command(
        InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_PrivateCommand** privateCommand, ulong size)
    {
        return -1;
    }

    /// <summary>
    ///     输入法设置预上屏文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="text"></param>
    /// <param name="length"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int input_method_harmony_set_preview_text(InputMethod_TextEditorProxy* textEditorProxy, char* text,
        ulong length, int start, int end)
    {
        if (_client is null) return 0;
        try
        {
            if (_client.SupportsPreedit is false) return 0;
            var preeditText = Marshal.PtrToStringUni((IntPtr)text, (int)length);

            Dispatcher.UIThread.Post(() =>
            {
                _client.SetPreeditText(preeditText);
                _client.Selection = new TextSelection(start, end);
            });
            OHDebugHelper.Debug(
                $"""
                 输入法设置预上屏文本时触发的函数。
                 设置预上屏文本：{preeditText}
                 """);
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("输入法设置预上屏文本时触发的函数。", e);
        }

        return 0;
    }

    /// <summary>
    ///     输入法结束预上屏时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_finish_text_preview(InputMethod_TextEditorProxy* textEditorProxy)
    {
        OHDebugHelper.Debug("输入法结束预上屏时触发的函数。");
        if (_client is null) return;
        if (_client.SupportsPreedit is false) return;
        Dispatcher.UIThread.Post(() => _client?.SetPreeditText(null));
    }
}