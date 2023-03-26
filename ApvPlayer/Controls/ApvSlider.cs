using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace ApvPlayer.Controls;

public class ApvSlider : Slider
{
    protected override void OnThumbDragStarted(VectorEventArgs e)
    {
        base.OnThumbDragStarted(e);
        Console.WriteLine("started");
    }

    protected override void OnThumbDragCompleted(VectorEventArgs e)
    {
        base.OnThumbDragCompleted(e);
        Console.WriteLine("completed");
    }
}