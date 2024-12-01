using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Content.Pipeline.Editor.Forms
{
    public partial class PackageDialog : Form
    {
        public PackageDialog()
        {
            InitializeComponent();
        }

        private void llSearchNuget_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string nugetUrl = "https://www.nuget.org/packages?q=";

            string packageName = this.tbName.Text;
            string tags = "Tags:\"kni\",\"pipeline\"";
            string query = string.Format("{0} {1}", packageName, tags);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = nugetUrl + Uri.EscapeDataString(query);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }
    }
}
