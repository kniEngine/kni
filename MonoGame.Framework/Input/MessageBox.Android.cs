// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMessageBox : MessageBoxStrategy
    {
        private TaskCompletionSource<int?> tcs;
        private AlertDialog alert;

        public override Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
            AndroidGameWindow.Activity.RunOnUiThread(() =>
            {
                alert = new AlertDialog.Builder(AndroidGameWindow.Activity).Create();

                alert.SetTitle(title);
                alert.SetMessage(description);

                alert.SetButton((int)DialogButtonType.Positive, buttons[0], (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(0);
                });

                if (buttons.Count > 1)
                {
                    alert.SetButton((int)DialogButtonType.Negative, buttons[1], (sender, args) =>
                    {
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(1);
                    });
                }

                if (buttons.Count > 2)
                {
                    alert.SetButton((int)DialogButtonType.Neutral, buttons[2], (sender, args) =>
                    {
                        if (!tcs.Task.IsCompleted)
                            tcs.SetResult(2);
                    });
                }

                alert.CancelEvent += (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(null);
                };

                alert.Show();
            });

            return tcs.Task;
        }

        public override void PlatformCancel(int? result)
        {
            alert.Dismiss();
            tcs.SetResult(result);
        }
    }
}
