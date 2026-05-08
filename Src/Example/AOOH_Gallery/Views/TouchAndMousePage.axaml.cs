using System;
using System.Collections.Specialized;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenHarmony;
using Avalonia.Threading;

namespace AOOH_Gallery.Views;

public partial class TouchAndMousePage : UserControl
{
    public TouchAndMousePage()
    {
        InitializeComponent();
        (OHDebugHelper.Logs as INotifyCollectionChanged).CollectionChanged += PreferencesPage_CollectionChanged;

        TestButton.PointerEntered += OnPointerEntered;
        TestButton.PointerExited += OnPointerExited;
        TestButton.PointerMoved += OnPointerMoved;
        TestButton.PointerPressed += OnPointerPressed;
        TestButton.PointerReleased += OnPointerReleased;
        TestButton.Click += OnClick;

        TestTextBlock.PointerEntered += OnPointerEntered;
        TestTextBlock.PointerExited += OnPointerExited;
        TestTextBlock.PointerMoved += OnPointerMoved;
        TestTextBlock.PointerPressed += OnPointerPressed;
        TestTextBlock.PointerReleased += OnPointerReleased;
    }

    private void OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OHDebugHelper.Info($"Button Clicked");
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var pointerId = e.Pointer.Id;
        var pointerType = e.Pointer.Type.ToString();
        var pressure = e.GetCurrentPoint(this).Properties.Pressure;
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var timestamp = e.Timestamp;
        OHDebugHelper.Info($"PointerExited Pointer:{e.Pointer} Type:{pointerType} Id:{pointerId} " +
                           $"Position:({position.X:F2},{position.Y:F2}) Pressure:{pressure:F2} " +
                           $"LeftButton:{isLeftButtonPressed} Timestamp:{timestamp}");

    }
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var pointerId = e.Pointer.Id;
        var pointerType = e.Pointer.Type.ToString();
        var pressure = e.GetCurrentPoint(this).Properties.Pressure;
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var timestamp = e.Timestamp;
        OHDebugHelper.Info($"PointerMoved Pointer:{e.Pointer} Type:{pointerType} Id:{pointerId} " +
                           $"Position:({position.X:F2},{position.Y:F2}) Pressure:{pressure:F2} " +
                           $"LeftButton:{isLeftButtonPressed} Timestamp:{timestamp}");

    }
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {

        var position = e.GetPosition(this);
        var pointerId = e.Pointer.Id;
        var pointerType = e.Pointer.Type.ToString();
        var pressure = e.GetCurrentPoint(this).Properties.Pressure;
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var timestamp = e.Timestamp;
        OHDebugHelper.Info($"PointerPressed Pointer:{e.Pointer} Type:{pointerType} Id:{pointerId} " +
                           $"Position:({position.X:F2},{position.Y:F2}) Pressure:{pressure:F2} " +
                           $"LeftButton:{isLeftButtonPressed} Timestamp:{timestamp}");
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var position = e.GetPosition(this);
        var pointerId = e.Pointer.Id;
        var pointerType = e.Pointer.Type.ToString();
        var pressure = e.GetCurrentPoint(this).Properties.Pressure;
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var timestamp = e.Timestamp;
        OHDebugHelper.Info($"PointerReleased Pointer:{e.Pointer} Type:{pointerType} Id:{pointerId} " +
                           $"Position:({position.X:F2},{position.Y:F2}) Pressure:{pressure:F2} " +
                           $"LeftButton:{isLeftButtonPressed} Timestamp:{timestamp}");
    }



    private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var pointerId = e.Pointer.Id;
        var pointerType = e.Pointer.Type.ToString();
        var pressure = e.GetCurrentPoint(this).Properties.Pressure;
        var isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var timestamp = e.Timestamp;

        OHDebugHelper.Info($"PointerEntered Pointer:{e.Pointer} Type:{pointerType} Id:{pointerId} " +
                           $"Position:({position.X:F2},{position.Y:F2}) Pressure:{pressure:F2} " +
                           $"LeftButton:{isLeftButtonPressed} Timestamp:{timestamp}");
    }

    private void PreferencesPage_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (MessageListBox.ItemCount > 0)
            {
                MessageListBox.ScrollIntoView(MessageListBox.ItemCount - 1);
            }
        });
    }
}
