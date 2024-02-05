using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace Content.Pipeline.Editor
{
    public partial class ReferenceDialog : Form
    {
        public List<string> Lines;

        public ReferenceDialog()
        {
            InitializeComponent();
        }

        private void ReferenceDialog_Load(object sender, EventArgs e)
        {
            foreach (string item in this.Lines)
            {
                string[] itemValues = new string[] { Path.GetFileName(item), item };
                listView1.Items.Add(new ListViewItem(itemValues));
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Dll Files (.dll)|*.dll|All Files (*.*)|*.*";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string pl = ((PipelineController)MainView._controller).ProjectLocation;
                if (!pl.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    pl += Path.DirectorySeparatorChar;

                Uri folderUri = new Uri(pl);

                foreach(string filename in dialog.FileNames)
                {
                    Uri pathUri = new Uri(filename);
                    string fl = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));

                    if (!this.Lines.Contains(fl))
                    {
                        this.Lines.Add(fl);
                        string[] itemValues = new string[] { Path.GetFileName(fl), fl };
                        listView1.Items.Add(new ListViewItem(itemValues));
                    }
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
    }

    /// <summary>
    /// Custom editor for a the References property of a PipelineProject.
    /// </summary>    
    public class AssemblyReferenceListEditor : UITypeEditor
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
                using (var form = new ReferenceDialog())
                {
                    form.Lines = lines;
                    if (svc.ShowDialog(form) == DialogResult.OK)
                    {
                        lines = form.Lines;
                        MainView._controller.OnProjectModified();
                    }
                }
            }

            return lines;
        }
    }
}
