// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMessageBox : MessageBoxStrategy
    {
        private TaskCompletionSource<int?> tcs;
        private UIAlertView alert;

        public override Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            tcs = new TaskCompletionSource<int?>();
            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertView();
                alert.Title = title;
                alert.Message = description;
                foreach (string button in buttons)
                    alert.AddButton(button);
                alert.Dismissed += (sender, e) =>
                {
                    if (!tcs.Task.IsCompleted)
					    tcs.SetResult((int)e.ButtonIndex);
                };
                alert.Show();
            });

            return tcs.Task;
        }

        public override void PlatformCancel(int? result)
        {
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(result);

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert.DismissWithClickedButtonIndex(0, true);
            });
        }
    }
}
