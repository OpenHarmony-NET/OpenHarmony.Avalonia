using Avalonia.Input.TextInput;
using OpenHarmony.Sdk;
using OpenHarmony.Sdk.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia.OpenHarmony;

public unsafe class OpenHarmonyInputMethod : ITextInputMethodImpl
{
    public OpenHarmonyInputMethod()
    {


    }
    InputMethod_TextEditorProxy* textEditorProxy;
    InputMethod_InputMethodProxy* inputMethodProxy;
    public void Reset()
    {
    }

    public void SetClient(TextInputMethodClient? client)
    {
        var options = input_method.OH_AttachOptions_Create(true);

        /*
        textEditorProxy = input_method.OH_TextEditorProxy_Create();
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetClient textEditorProxy == nullptr " + (textEditorProxy == null));
        input_method.OH_TextEditorProxy_SetGetTextConfigFunc(textEditorProxy, &input_method_harmony_get_text_config);
        input_method.OH_TextEditorProxy_SetInsertTextFunc(textEditorProxy, &input_method_harmony_insert_text);

        input_method.OH_TextEditorProxy_SetDeleteBackwardFunc(textEditorProxy, &input_method_harmony_delete_backward);
        input_method.OH_TextEditorProxy_SetDeleteForwardFunc(textEditorProxy, &input_method_harmony_delete_forward);
        input_method.OH_TextEditorProxy_SetSendKeyboardStatusFunc(textEditorProxy, &input_method_harmony_send_keyboard_status);
        input_method.OH_TextEditorProxy_SetSendEnterKeyFunc(textEditorProxy, &input_method_harmony_send_enter_key);
        input_method.OH_TextEditorProxy_SetMoveCursorFunc(textEditorProxy, &input_method_harmony_move_cursor);
        input_method.OH_TextEditorProxy_SetHandleSetSelectionFunc(textEditorProxy, &input_method_harmony_handle_set_selection);
        input_method.OH_TextEditorProxy_SetHandleExtendActionFunc(textEditorProxy, &input_method_harmony_handle_extend_action);
        input_method.OH_TextEditorProxy_SetGetLeftTextOfCursorFunc(textEditorProxy, &input_method_harmony_get_left_text_of_cursor);
        input_method.OH_TextEditorProxy_SetGetRightTextOfCursorFunc(textEditorProxy, &input_method_harmony_get_right_text_of_cursor);
        input_method.OH_TextEditorProxy_SetGetTextIndexAtCursorFunc(textEditorProxy, &input_method_harmony_get_text_index_at_cursor);
        input_method.OH_TextEditorProxy_SetReceivePrivateCommandFunc(textEditorProxy, &input_method_harmony_receive_private_command);
        input_method.OH_TextEditorProxy_SetSetPreviewTextFunc(textEditorProxy, &input_method_harmony_set_preview_text);
        input_method.OH_TextEditorProxy_SetFinishTextPreviewFunc(textEditorProxy, &input_method_harmony_finish_text_preview);


        InputMethod_InputMethodProxy* ptr;
        var code = input_method.OH_InputMethodController_Attach(textEditorProxy, options, &ptr);
        inputMethodProxy = ptr;
        
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetClient inputMethodProxy == null " + (inputMethodProxy == null));
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetClient code = " + code);
        */
    }

    public void SetCursorRect(Rect rect)
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetCursorRect");
    }

    public void SetOptions(TextInputOptions options)
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "OpenHarmonyInputMethod.SetOptions");
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void input_method_harmony_get_text_config(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_TextConfig* config)
    {

    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_insert_text(InputMethod_TextEditorProxy* textEditorProxy, char* text,
                                             ulong length)
    {

    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_delete_backward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_delete_forward(InputMethod_TextEditorProxy* textEditorProxy, int length)
    {
    }

    /*
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_send_keyboard_status(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_KeyboardStatus keyboardStatus)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_send_enter_key(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_EnterKeyType enterKeyType)
    {
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_move_cursor(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_Direction direction)
    { 
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_handle_set_selection(InputMethod_TextEditorProxy* textEditorProxy, int start, int end)
    { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_handle_extend_action(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_ExtendAction action)
    { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_get_left_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy, int number, char* text, ulong* length)
    { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_get_right_text_of_cursor(InputMethod_TextEditorProxy* textEditorProxy, int number, char* text, ulong* length)
    { }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_get_text_index_at_cursor(InputMethod_TextEditorProxy* textEditorProxy) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_receive_private_command(InputMethod_TextEditorProxy* textEditorProxy, InputMethod_PrivateCommand** privateCommand, ulong size) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static int input_method_harmony_set_preview_text(InputMethod_TextEditorProxy* textEditorProxy, char* text, ulong length, int start, int end) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void input_method_harmony_finish_text_preview(InputMethod_TextEditorProxy* textEditorProxy) { }
    */

}
