// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMessageBox : MessageBoxStrategy
    {
        private Form _dialog;
        private TaskCompletionSource<int?> _tcs;

        public override Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            _tcs = new TaskCompletionSource<int?>();

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

                FlowLayoutPanel bgroup = new FlowLayoutPanel();
                bgroup.FlowDirection = FlowDirection.LeftToRight;
                bgroup.Parent = dialog;
                bgroup.Top = desc.Bottom + 25;
                bgroup.AutoSize = true;
                bgroup.Margin = new Padding(15);
                bgroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                for (int i = 0; i < buttons.Count; i++)
                {
                    string btext = buttons[i];
                    Button button = new Button();
                    button.Text = btext;
                    button.DialogResult = (DialogResult)i+1;
                    button.Parent = bgroup;
                    if (i == 0)
                        dialog.AcceptButton = button;
                }

                bgroup.Left = (bgroup.Parent.ClientSize.Width - bgroup.Width) / 2;
                dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialog.AutoSize = true;

                int result = (int)dialog.ShowDialog(parent);
                _dialog = null;

                if (_tcs.Task.IsCompleted)
                    return;

                _tcs.SetResult(result-1);
            }));

            return _tcs.Task;
        }

        public override void PlatformCancel(int? result)
        {
            if (_dialog != null)
                _dialog.Close();
            _tcs.SetResult(result);
        }
    }
}
