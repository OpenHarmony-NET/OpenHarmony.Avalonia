using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

/// <summary>
/// XComponent 封装了 OpenHarmony 原生 XComponent 的基本操作和生命周期事件。
/// 用于管理与原生 XComponent 交互的句柄，并为派生类提供生命周期相关的虚方法。
/// </summary>
public class XComponent
{
    /// <summary>
    /// 构造函数，初始化 XComponent 实例并保存原生句柄。
    /// </summary>
    /// <param name="xComponentHandle">原生 XComponent 句柄。</param>
    /// <param name="windowHandle">窗口句柄。</param>
    public XComponent(IntPtr xComponentHandle, IntPtr windowHandle)
    {
        XComponentHandle = xComponentHandle;
        WindowHandle = windowHandle;
    }

    /// <summary>
    /// 获取原生 XComponent 句柄。
    /// </summary>
    public IntPtr XComponentHandle { get; }

    /// <summary>
    /// 获取窗口句柄。
    /// </summary>
    public IntPtr WindowHandle { get; }

    /// <summary>
    /// 获取当前 XComponent 的尺寸。
    /// </summary>
    /// <returns>返回 Size 结构体，包含宽度和高度。</returns>
    public virtual unsafe Size GetSize()
    {
        ulong width = 0;
        ulong height = 0;
        // 调用原生方法获取 XComponent 的宽高
        _ = Ace.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &width,
            &height);
        return new Size(width, height);
    }

    /// <summary>
    /// 当 XComponent Surface 创建时调用。可由派生类重写以实现自定义逻辑。
    /// </summary>
    public virtual void OnSurfaceCreated()
    {
    }

    /// <summary>
    /// 当 XComponent Surface 销毁时调用。可由派生类重写以实现自定义逻辑。
    /// </summary>
    public virtual void OnSurfaceDestroyed()
    {
    }

    /// <summary>
    /// 当 XComponent 渲染一帧时调用。可由派生类重写以实现自定义逻辑。
    /// </summary>
    /// <param name="timestamp">当前帧时间戳。</param>
    /// <param name="targetTimestamp">目标帧时间戳。</param>
    public virtual void OnSurfaceRendered(ulong timestamp, ulong targetTimestamp)
    {
    }

    /// <summary>
    /// 当 XComponent Surface 发生变化（如尺寸变化）时调用。可由派生类重写。
    /// </summary>
    public virtual void OnSurfaceChanged()
    {
    }

    /// <summary>
    /// 分发触摸事件。可由派生类重写以处理触摸输入。
    /// </summary>
    public virtual void DispatchTouchEvent()
    {
    }

    /// <summary>
    /// 分发鼠标事件。可由派生类重写以处理鼠标输入。
    /// </summary>
    public virtual void DispatchMouseEvent()
    {
    }

    /// <summary>
    /// 分发鼠标悬停事件。可由派生类重写以处理悬停状态变化。
    /// </summary>
    /// <param name="isHover">是否悬停。</param>
    public virtual void DispatchHoverEvent(bool isHover)
    {
    }
}
