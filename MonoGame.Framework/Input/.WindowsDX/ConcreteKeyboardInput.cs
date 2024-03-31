// Copyright (C)2024 Nick Kastellanos

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboardInput : KeyboardInputStrategy
    {
        private Form _dialog;
        private TaskCompletionSource<string> _tcs;
        
        public override Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            _tcs = new TaskCompletionSource<string>();

            Form parent = Application.OpenForms[0];

            parent.Invoke(new MethodInvoker(() =>
            {
                Form dialog = _dialog = new Form();
                dialog.Text = title;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ControlBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;

                Label desc = new Label();
                desc.Text = description;
                desc.Parent = dialog;
                desc.Top = 25;
                desc.TextAlign = ContentAlignment.MiddleCenter;
                desc.AutoSize = true;
                desc.Margin = new Padding(25, 0, 25, 0);
                desc.Left = (desc.Parent.ClientSize.Width - desc.Width) / 2;

                TextBox input = new TextBox();
                input.Text = defaultText;
                input.Parent = dialog;
                input.Top = desc.Bottom + 15;
                input.UseSystemPasswordChar = usePasswordMode;
                input.AutoSize = true;
                input.Margin = new Padding(25, 0, 25, 0);
                input.Left = 25;
                input.Width = input.Parent.ClientSize.Width - 25;
                input.TabIndex = 0;

                FlowLayoutPanel bgroup = new FlowLayoutPanel();
                bgroup.FlowDirection = FlowDirection.LeftToRight;
                bgroup.Parent = dialog;
                bgroup.Top = input.Bottom + 15;
                bgroup.AutoSize = true;
                bgroup.Margin = new Padding(15);
                bgroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                Button acceptButton = new Button();
                acceptButton.Text = "&Ok";
                acceptButton.DialogResult = DialogResult.OK;
                acceptButton.Parent = bgroup;
                acceptButton.TabIndex = 1;
                dialog.AcceptButton = acceptButton;

                Button cancelButton = new Button();
                cancelButton.Text = "&Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Parent = bgroup;
                cancelButton.TabIndex = 2;
                dialog.CancelButton = cancelButton;

                bgroup.Left = (bgroup.Parent.ClientSize.Width - bgroup.Width) / 2;
                dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialog.AutoSize = true;

                DialogResult result = dialog.ShowDialog(parent);
                _dialog = null;

                if (_tcs.Task.IsCompleted)
                    return;

                if (result == DialogResult.OK)
                    _tcs.SetResult(input.Text);
                else
                    _tcs.SetResult(null);

            }));

            return _tcs.Task;
        }

        public override void PlatformCancel(string result)
        {
            if (_dialog != null)
                _dialog.Close();
            _tcs.SetResult(result);
        }

    }
}
