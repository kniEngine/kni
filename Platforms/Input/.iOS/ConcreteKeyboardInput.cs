﻿// Copyright (C)2024 Nick Kastellanos

using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using UIKit;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboardInput : KeyboardInputStrategy
    {
        private TaskCompletionSource<string> tcs;
        private UIAlertView alert;

        public override Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();

            UIApplication.SharedApplication.InvokeOnMainThread(delegate
            {
                alert = new UIAlertView();
                alert.Title = title;
                alert.Message = description;
                alert.AlertViewStyle = usePasswordMode ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput;
                alert.AddButton("Cancel");
                alert.AddButton("Ok");
                UITextField alertTextField = alert.GetTextField(0);
                alertTextField.KeyboardType = UIKeyboardType.ASCIICapable;
                alertTextField.AutocorrectionType = UITextAutocorrectionType.No;
                alertTextField.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
                alertTextField.Text = defaultText;
                alert.Dismissed += (sender, e) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult((int)e.ButtonIndex == 0 ? null : alert.GetTextField(0).Text);
                };

                // UIAlertView's textfield does not show keyboard in iOS8
                // http://stackoverflow.com/questions/25563108/uialertviews-textfield-does-not-show-keyboard-in-ios8
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                    alert.Presented += (sender, args) => alertTextField.SelectAll(alert);

                alert.Show();
            });

            return tcs.Task;
        }

        public override void PlatformCancel(string result)
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
