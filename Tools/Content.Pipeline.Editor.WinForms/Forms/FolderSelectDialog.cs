using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Content.Pipeline.Editor
{
    public class FolderSelectEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            var project = context.Instance as PipelineProjectProxy;
            
            var initialDir = (string)value;
            if (initialDir == null || !Directory.Exists(initialDir))
                initialDir = project.Location;

            var dlg = new FolderSelectDialog()
                {                    
                    InitialDirectory = initialDir,
                    Title = "Select Folder",
                };

            if (dlg.ShowDialog(MainView.Form))
                return dlg.FileName;

            return value;            
        }
    }
    
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.
    /// </summary>
    public class FolderSelectDialog
	{
		// Wrapped dialog
		System.Windows.Forms.OpenFileDialog ofd = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public FolderSelectDialog()
		{
			ofd = new System.Windows.Forms.OpenFileDialog();

			ofd.Filter = "Folders|\n";
			ofd.AddExtension = false;
			ofd.CheckFileExists = false;
			ofd.DereferenceLinks = true;
			ofd.Multiselect = false;
		}

		#region Properties

		/// <summary>
		/// Gets/Sets the initial folder to be selected. A null value selects the current directory.
		/// </summary>
		public string InitialDirectory
		{
			get { return ofd.InitialDirectory; }
			set { ofd.InitialDirectory = value == null || value.Length == 0 ? Environment.CurrentDirectory : value; }
		}

		/// <summary>
		/// Gets/Sets the title to show in the dialog
		/// </summary>
		public string Title
		{
			get { return ofd.Title; }
			set { ofd.Title = value == null ? "Select a folder" : value; }
		}

		/// <summary>
		/// Gets the selected folder
		/// </summary>
		public string FileName
		{
			get { return ofd.FileName; }
		}

		#endregion

		#region Methods

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="wndOwner">Handle of the control to be parent</param>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog(IWin32Window wndOwner)
		{
			if (Environment.OSVersion.Version.Major >= 6)
			{
                Type tofd = ofd.GetType();
                Assembly asm = tofd.Assembly;
                string nsForms = asm.GetName().Name + ".";

                Type tFileDialog = typeof(FileDialog);               
                Type tIFileDialog = asm.GetType(nsForms + "FileDialogNative+IFileDialog");
                Type tFOS = asm.GetType(nsForms + "FileDialogNative+FOS");
                Type tVistaDialogEvents = asm.GetType(nsForms + "FileDialog+VistaDialogEvents");
                
                MethodInfo miCreateVistaDialog = tofd.GetMethod("CreateVistaDialog", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                MethodInfo miOnBeforeVistaDialog = tofd.GetMethod("OnBeforeVistaDialog", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                MethodInfo miGetOptions = tFileDialog.GetMethod("GetOptions", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                MethodInfo miSetOptions = tIFileDialog.GetMethod("SetOptions", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

                object dialog = miCreateVistaDialog.Invoke(ofd, new object[] { });

                miOnBeforeVistaDialog.Invoke(ofd, new [] { dialog });

                uint options = (uint)miGetOptions.Invoke(ofd, new object[] { });                
                uint FOS_PICKFOLDERS = (uint)tFOS.GetField("FOS_PICKFOLDERS").GetValue(null);
                options |= FOS_PICKFOLDERS;

                miSetOptions.Invoke(dialog, new object[] { options });

                object pfde = null;   
                ConstructorInfo[] ctorInfos = tVistaDialogEvents.GetConstructors();
                foreach (ConstructorInfo ci in ctorInfos)
                {
                    try
                    {
                        pfde = ci.Invoke(new[] { ofd });
                        break;
                    }
                    catch { }
                }

                MethodInfo miAdvise = tIFileDialog.GetMethod("Advise", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                MethodInfo miShow = tIFileDialog.GetMethod("Show", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                MethodInfo miUnadvise = tIFileDialog.GetMethod("Unadvise", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

				uint num = 0;
				object[] parameters = new [] { pfde, num };
                miAdvise.Invoke(dialog, parameters);
                num = (uint)parameters[1];
				try
				{
                    int num2 = (int)miShow.Invoke(dialog, new object[] { wndOwner.Handle });
                    return (0 == num2);

                }
				finally
				{
                    miUnadvise.Invoke(dialog, new object[] { num });
                    GC.KeepAlive(pfde);
				}
			}
			else
			{
				var fbd = new FolderBrowserDialog();
				fbd.Description = this.Title;
				fbd.SelectedPath = this.InitialDirectory;
				fbd.ShowNewFolderButton = false;
				if (fbd.ShowDialog(wndOwner) != DialogResult.OK)
                    return false;
				ofd.FileName = fbd.SelectedPath;
                return true;
			}
		}

        #endregion
    }

}

