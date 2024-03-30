using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class MessageBox
    {
        private readonly CoreDispatcher _dispatcher;
        private TaskCompletionSource<int?> _tcs;
        private IAsyncOperation<IUICommand> _dialogResult;

        private MessageBox()
        {
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        private Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            // TODO: MessageDialog only supports two buttons
            if (buttons.Count == 3)
                throw new NotSupportedException("This platform does not support three buttons");

            _tcs = new TaskCompletionSource<int?>();

            MessageDialog dialog = new MessageDialog(description, title);
            foreach (string button in buttons)
                dialog.Commands.Add(new UICommand(button, null, dialog.Commands.Count));

            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        // PlatformSetResult will cancel the task, resulting in an exception
                        _dialogResult = dialog.ShowAsync();
                        IUICommand result = await _dialogResult;
                        if (!_tcs.Task.IsCompleted)
                            _tcs.SetResult(result == null ? null : (int?)result.Id);
                    }
                    catch (TaskCanceledException)
                    {
                        if (!_tcs.Task.IsCompleted)
                            _tcs.SetResult(null);
                    }
                });

            return _tcs.Task;
        }

        private void PlatformCancel(int? result)
        {
            // TODO: MessageDialog doesn't hide on Windows Phone 8.1
            _tcs.SetResult(result);
            _dialogResult.Cancel();
        }
    }
}
