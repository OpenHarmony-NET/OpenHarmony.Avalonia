using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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

    private readonly InputMethod_TextEditorProxy* textEditorProxy;
    private readonly TopLevelImpl _topLevelImpl;
    private InputMethod_InputMethodProxy* _inputMethodProxy = null;

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

    public void Reset()
    {
        var result = input_method.OH_InputMethodController_Detach(_inputMethodProxy);
        _inputMethodProxy = null;
        OHDebugHelper.Debug(
            $"""
             在输入控件失焦后解绑输入法。
             解绑结果：{result}
             """);
    }

    public void SetClient(TextInputMethodClient? client)
    {
        if (_inputMethodProxy is not null)
        {
            if (_keyboardStatus is InputMethod_KeyboardStatus.IME_KEYBOARD_STATUS_HIDE)
            {
                var result = input_method.OH_InputMethodProxy_ShowKeyboard(_inputMethodProxy);
                OHDebugHelper.Debug(
                    $"""
                     在输入控件没有失焦但输入法面板已关闭的情况下重新拉起输入法。
                     拉起结果：{result}
                     """);
            }

            return;
        }

        if (_client is not null) _client.SelectionChanged -= NotifySelectionChange;

        _client = client;


        if (_client is not null)
        {
            var options = input_method.OH_AttachOptions_Create(true);
            InputMethod_InputMethodProxy* ptr = null;
            var code = input_method.OH_InputMethodController_Attach(textEditorProxy, options, &ptr);
            _inputMethodProxy = ptr;
            OHDebugHelper.Debug(
                $"""
                 在输入控件获得焦点后绑定输入法。
                 绑定结果：{code}
                 """);
            _client.SelectionChanged += NotifySelectionChange;
        }
    }

    public void SetCursorRect(Rect rect)
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetCursorRect");
    }

    public void SetOptions(TextInputOptions options)
    {
        if (_inputMethodProxy is null) return;
        OHDebugHelper.Debug(
            $"""
             设置输入法选项。
             设置结果：
             """);
    }

    /// <summary>
    /// 通知光标位置变化。
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
                 光标位置：{_client.Selection.Start} - {_client.Selection.End}
                 """);
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("通知光标位置变化失败。", exception);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void input_method_harmony_get_text_config(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_TextConfig* config)
    {
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
            for (int i = 0; i < length; i++)
            {
                RawKeyEventArgs args = Dispatcher.UIThread.Invoke(() => new RawKeyEventArgs(
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
            for (int i = 0; i < length; i++)
            {
                RawKeyEventArgs args = Dispatcher.UIThread.Invoke(() => new RawKeyEventArgs(
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

    private static InputMethod_KeyboardStatus _keyboardStatus;

    /// <summary>
    ///     输入法通知键盘状态时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="keyboardStatus"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void input_method_harmony_send_keyboard_status(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_KeyboardStatus keyboardStatus)
    {
        if (_client is null) return;
        _keyboardStatus = keyboardStatus;
        OHDebugHelper.Debug("输入法通知键盘状态时触发的函数。");
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
                RawKeyEventArgs args = new RawKeyEventArgs(
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
        if (_client is null) return;
        try
        {
            OHDebugHelper.Debug("输入法移动光标时触发的函数。");
            // _instance._topLevelImpl.TextInput("\n");
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
        if (_client is null) return;
        try
        {
            // {
            //     var leftText = Marshal.PtrToStringUni((IntPtr)text, (int)number);
            //     _instance?._topLevelImpl.TextInput(leftText);
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
    }

    /// <summary>
    ///     输入法获取光标所在输入框文本索引时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int input_method_harmony_get_text_index_at_cursor(InputMethod_TextEditorProxy* textEditorProxy)
    {
        if (_client is null) return 0;
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
        _client.SetPreeditText(null);
    }
}