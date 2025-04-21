using Avalonia.Input.TextInput;
using OpenHarmony.NDK.Bindings.Native;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Avalonia.OpenHarmony;

public unsafe class OpenHarmonyInputMethod : ITextInputMethodImpl
{
    private TopLevelImpl _topLevelImpl;
    private static OpenHarmonyInputMethod? _instance;

    public OpenHarmonyInputMethod(TopLevelImpl topLevelImpl)
    {
        if (_instance is not null)
        {
            throw new InvalidOperationException("Only one instance of OpenHarmonyInputMethod is allowed.");
        }

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

    private readonly InputMethod_TextEditorProxy* textEditorProxy;
    InputMethod_InputMethodProxy* inputMethodProxy = null;
    private static TextInputMethodClient? _client;

    public void Reset()
    {
        OHDebugHelper.AddLog("OpenHarmonyInputMethod.Reset is called");

        // Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.Reset is called");
    }

    public void SetClient(TextInputMethodClient? client)
    {
        _client = client;

        if (client?.TextViewVisual is Control visual)
        {
            visual.LostFocus += VisualOnLostFocus;

            void VisualOnLostFocus(object? sender, RoutedEventArgs e)
            {
                visual.LostFocus -= VisualOnLostFocus;
                var result = input_method.OH_InputMethodController_Detach(inputMethodProxy);
                inputMethodProxy = null;
                OHDebugHelper.AddLog(
                    $"""
                     在输入控件失焦后解绑输入法。
                     解绑结果：{result}
                     """);
            }
        }

        var options = input_method.OH_AttachOptions_Create(true);
        InputMethod_InputMethodProxy* ptr = null;
        var code = input_method.OH_InputMethodController_Attach(textEditorProxy, options, &ptr);
        inputMethodProxy = ptr;

        OHDebugHelper.AddLog(
            $"""
             在输入控件获得焦点后绑定输入法。
             绑定结果：{code}
             """);
        // Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp",
        //     "OpenHarmonyInputMethod.SetClient inputMethodProxy == null " + (inputMethodProxy == null));
        // Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetClient code = " + code);
    }

    public void SetCursorRect(Rect rect)
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetCursorRect");
    }

    public void SetOptions(TextInputOptions options)
    {
        // Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetOptions");
        // OHDebugHelper.AddLog("OpenHarmonyInputMethod.SetOptions is called");
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void input_method_harmony_get_text_config(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_TextConfig* config)
    {
    }

    private static void UpdateText(string insertText)
    {
        if (_client is null) return;
        var oldText = _client.SurroundingText;
        var start = _client.Selection.Start;
        var end = _client.Selection.End;
        _client.SetPreeditText(_client.SurroundingText + insertText);
        // _client.Selection = new TextSelection(start + insertText.Length, end + insertText.Length);
    }

    /// <summary>
    /// 输入法应用插入文本时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    /// <param name="text"></param>
    /// <param name="length"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_insert_text(InputMethod_TextEditorProxy* textEditorProxy, char* text, ulong length)
    {
        if (_client is null) return;
        string? insetText = Marshal.PtrToStringUni((IntPtr)text, (int)length);
        try
        {
            _instance?._topLevelImpl.TextInput(insetText);
        }
        catch (Exception e)
        {
            OHDebugHelper.AddLog($"""
                                  输入法应用插入文本时触发的函数。
                                  插入文本失败，错误信息：{e.Message}
                                  """);
        }

        OHDebugHelper.AddLog($"""
                              输入法应用插入文本时触发的函数。
                              在光标处输入了：{insetText}
                              """);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void* input_method_harmony_delete_backward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
        return null;
    }

    /// <summary>
    /// 输入法删除光标右侧文本时触发的函数。
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_delete_forward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
        OHDebugHelper.AddLog("输入法删除光标右侧文本时触发的函数。");
    }


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_send_keyboard_status(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_KeyboardStatus keyboardStatus)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_send_enter_key(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_EnterKeyType enterKeyType)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_move_cursor(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_Direction direction)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_handle_set_selection(InputMethod_TextEditorProxy* textEditorProxy, int start,
        int end)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_handle_extend_action(InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_ExtendAction action)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_get_left_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy, int number,
        char* text, ulong* length)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_get_right_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy, int number,
        char* text, ulong* length)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_get_text_index_at_cursor(InputMethod_TextEditorProxy* textEditorProxy) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_receive_private_command(
        InputMethod_TextEditorProxy* textEditorProxy,
        InputMethod_PrivateCommand** privateCommand, ulong size) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_set_preview_text(InputMethod_TextEditorProxy* textEditorProxy, char* text,
        ulong length, int start, int end) => 0;

    /// <summary>
    /// 输入法结束预上屏时触发的函数。
    /// </summary>
    /// <param name="textEditorProxy"></param>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_finish_text_preview(InputMethod_TextEditorProxy* textEditorProxy)
    {
        OHDebugHelper.AddLog("输入法结束预上屏时触发的函数。");
    }
}