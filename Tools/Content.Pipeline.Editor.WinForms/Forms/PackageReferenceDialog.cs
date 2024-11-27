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
            Forms.PackageDialog packageDialog = new Forms.PackageDialog();

            DialogResult res = packageDialog.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                string name = packageDialog.tbName.Text.Trim();
                string version = packageDialog.tbVersion.Text.Trim();

                if (String.IsNullOrWhiteSpace(name))
                    return;

                Package package = Package.Parse(name);
                if (version != String.Empty)
                    package.Version = version;

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
