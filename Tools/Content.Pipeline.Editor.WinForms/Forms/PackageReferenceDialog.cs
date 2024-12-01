using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.Text.RegularExpressions;

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
                string version = (item.Version != String.Empty) ? item.Version : "*";
                string[] itemValues = new string[] { item.Name, version };
                listView1.Items.Add(new ListViewItem(itemValues));
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Forms.PackageDialog packageDialog = new Forms.PackageDialog();

            DialogResult res = packageDialog.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                Package package = default;
                package.Name = packageDialog.tbName.Text.Trim();
                package.Version = packageDialog.tbVersion.Text.Trim();

                if (String.IsNullOrWhiteSpace(package.Name))
                    return;

                Match match = Regex.Match(package.Name,
                     @"(dotnet add package )?(?<Name>[^\s]+)(\s+--version)?(\s+(?<VersionNumber>[^\s]+))");
                if (match.Success)
                {
                    package.Name = match.Groups["Name"].Value;
                    if (package.Version == String.Empty && match.Groups["VersionNumber"].Success)
                        package.Version = match.Groups["VersionNumber"].Value;
                }

                foreach (Package line in this.Lines)
                {
                    if (line.Name == package.Name)
                        return;
                }

                this.Lines.Add(package);

                string version = (package.Version != String.Empty) ? package.Version : "*";
                string[] itemValues = new string[] { package.Name, version };
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
