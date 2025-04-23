using Avalonia.Input;
using Avalonia.Platform;

namespace Avalonia.OpenHarmony;

public class CursorFactory : ICursorFactory
{
    public ICursorImpl CreateCursor(IBitmapImpl cursor, PixelPoint hotSpot)
    {
        return CursorImpl.ZeroCursor;
    }

    public ICursorImpl GetCursor(StandardCursorType cursorType)
    {
        return CursorImpl.ZeroCursor;
    }

    private sealed class CursorImpl : ICursorImpl
    {
        private CursorImpl()
        {
        }

        public static CursorImpl ZeroCursor { get; } = new();

        public void Dispose()
        {
        }
    }
}