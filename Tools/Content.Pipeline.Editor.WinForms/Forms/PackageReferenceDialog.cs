using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace Content.Pipeline.Editor
{
    public partial class PackageReferenceDialog : Form
    {
        public List<string> Lines;

        public PackageReferenceDialog()
        {
            InitializeComponent();
        }

        private void ReferenceDialog_Load(object sender, EventArgs e)
        {
            foreach (string item in this.Lines)
            {
                string[] itemValues = new string[] { item };
                listView1.Items.Add(new ListViewItem(itemValues));
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string packageName = String.Empty;
            if (ShowInputDialog("Package name", ref packageName) == DialogResult.OK)
            {
                packageName.Trim();

                if (String.IsNullOrEmpty(packageName))
                    return;

                if (!this.Lines.Contains(packageName))
                {
                    this.Lines.Add(packageName);
                    string[] itemValues = new string[] { packageName };
                    listView1.Items.Add(new ListViewItem(itemValues));
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var indices = listView1.SelectedIndices;
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                int index = int.Parse(indices[i].ToString());
                listView1.Items.RemoveAt(indices[i]);
                this.Lines.RemoveAt(index);

            }
        }

        private DialogResult ShowInputDialog(string name, ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(400, 70);

            Form inputBox = new Form();
            inputBox.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            inputBox.ClientSize = size;
            inputBox.Text = name;

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);            
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog(this);
            input = textBox.Text;
            return result;
        }
    }

    /// <summary>
    /// Custom editor for a the PackageReferences property of a PipelineProject.
    /// </summary>    
    public class PackageReferenceListEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            List<string> lines = (List<string>)value;
            if (svc != null && lines != null)
            {
                using (var form = new PackageReferenceDialog())
                {
                    form.Lines = lines;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        lines = form.Lines;
                        MainView._controller.OnReferencesModified();
                    }
                }
            }

            return lines;
        }
    }
}
