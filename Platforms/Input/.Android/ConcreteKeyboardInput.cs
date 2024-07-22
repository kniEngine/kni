// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Android.App;
using Android.Content;
using Android.Widget;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboardInput : KeyboardInputStrategy
    {
        private TaskCompletionSource<string> tcs;
        private AlertDialog alert;

        public override Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();

            AndroidGameWindow.Activity.RunOnUiThread(() =>
            {
                alert = new AlertDialog.Builder(AndroidGameWindow.Activity).Create();

                alert.SetTitle(title);
                alert.SetMessage(description);

                EditText input = new EditText(AndroidGameWindow.Activity) { Text = defaultText };

                if (defaultText != null)
                    input.SetSelection(defaultText.Length);

                if (usePasswordMode)
                    input.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;

                alert.SetView(input);

                alert.SetButton((int)DialogButtonType.Positive, "Ok", (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(input.Text);
                });

                alert.SetButton((int)DialogButtonType.Negative, "Cancel", (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(null);
                });

                alert.CancelEvent += (sender, args) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(null);
                };

                alert.Show();
            });

            return tcs.Task;
        }

        public override void PlatformCancel(string result)
        {
            alert.Dismiss();
            tcs.SetResult(result);
        }

    }
}
