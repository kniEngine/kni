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
        public List<Package> Lines;

        public PackageReferenceDialog()
        {
            InitializeComponent();
        }

        private void ReferenceDialog_Load(object sender, EventArgs e)
        {
            foreach (Package item in this.Lines)
            {
                string[] itemValues = new string[] { item.Name, item.Version };
                listView1.Items.Add(new ListViewItem(itemValues));
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Package package;
            package.Name = String.Empty;
            package.Version = String.Empty;

            if (ShowInputDialog("Package name", ref package) == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(package.Name))
                    return;

                foreach (Package line in this.Lines)
                {
                    if (line.Name == package.Name)
                        return;
                }

                this.Lines.Add(package);

                string[] itemValues = new string[] { package.Name, package.Version };
                listView1.Items.Add(new ListViewItem(itemValues));
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

        private DialogResult ShowInputDialog(string name, ref Package package)
        {
            System.Drawing.Size size = new System.Drawing.Size(400, 80);

            Form inputBox = new Form();
            inputBox.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            inputBox.ClientSize = size;
            inputBox.Text = name;
            inputBox.Parent = this.Parent;

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);            
            textBox.Text = package.ToString();
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 25);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 25);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog(this);

            string packagename = textBox.Text.Trim();

            if (!String.IsNullOrWhiteSpace(packagename))
            {
                package = Package.Parse(packagename);
            }

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
            List<Package> lines = (List<Package>)value;
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
