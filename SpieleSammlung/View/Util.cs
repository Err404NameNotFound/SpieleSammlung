using System;
using System.Windows;
using System.Windows.Threading;

namespace SpieleSammlung.View;

public static class Util
{
    public static void AllowUiToUpdate()
    {
        DispatcherFrame frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(
            delegate
            {
                frame.Continue = false;
                return null;
            }), null);

        Dispatcher.PushFrame(frame);
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
            new Action(delegate { }));
    }
}