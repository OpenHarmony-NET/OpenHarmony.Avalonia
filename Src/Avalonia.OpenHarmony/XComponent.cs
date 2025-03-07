using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public class XComponent
{
    public IntPtr XComponentHandle {get; private set;}
    public IntPtr WindowHandle { get; private set; }

    public XComponent(IntPtr XComponentHandle, IntPtr WindowHandle)
    {
        this.XComponentHandle = XComponentHandle;
        this.WindowHandle = WindowHandle;
    }

    public virtual unsafe Size GetSize()
    {
        ulong Width = 0;
        ulong Height = 0;
        Ace.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &Width, &Height);
        return new Size(Width, Height); 
    }

    public virtual void OnSurfaceCreated()
    {

    }


    public virtual void OnSurfaceDestroyed() 
    { 
    }


    public virtual void OnSurfaceRendered(ulong timestamp, ulong targetTimestamp)
    {
    }

    public virtual void OnSurfaceChanged()
    {

    }

    public virtual void DispatchTouchEvent()
    {

    }

}
