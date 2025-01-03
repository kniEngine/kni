﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Content.Pipeline.Editor
{    
    partial class MainView : Form, IView, IProjectObserver
    {
        // The project which will be opened as soon as a controller is attached.
        // Is used when PipelineTool is launched to open a project, provided by the command line.
        public string OpenProjectPath;

        public static IController _controller;
        private ContentIcons _treeIcons;
        private List<ContentItemState> _oldValues = new List<ContentItemState>();

        private bool _treeUpdating;
        private bool _treeSort;

        private const string MGContentProjectFileFilter = "MG Content Build Files (*.mgcb)|*.mgcb";
        private const string XnaContentProjectFileFilter = "XNA Content Project Files (*.contentproj)|*.contentproj";

        public static MainView Form { get; private set; }

        public MainView()
        {            
            InitializeComponent();

            // Set MenuBar color to Window color if the current OS is Windows 10
            if (System.Environment.OSVersion.Version.Major == 10)
                this._mainMenu.BackColor = SystemColors.Window;

            // Set the application icon this form.
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Find an appropriate font for console like output.
            var faces = new [] { "Consolas", "Lucida Console", "Courier New" };
            for (var f=0; f < faces.Length; f++)
            {
                _outputWindow.Font = new System.Drawing.Font(faces[f], 11F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                _filterOutputWindow.Font = new System.Drawing.Font(faces[f], 11F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                if (_outputWindow.Font.Name == faces[f])
                    break;               
            }

            _outputWindow.SelectionHangingIndent = TextRenderer.MeasureText(" ", _outputWindow.Font).Width;            

            _treeIcons = new ContentIcons();
            
            _treeView.ImageList = _treeIcons.Icons;
            _treeView.BeforeExpand += TreeViewOnBeforeExpand;
            _treeView.BeforeCollapse += TreeViewOnBeforeCollapse;
            _treeView.NodeMouseClick += TreeViewOnNodeMouseClick;
            _treeView.NodeMouseDoubleClick += TreeViewOnNodeMouseDoubleClick;

            _propertyGrid.PropertyValueChanged += OnPropertyGridPropertyValueChanged;

            InitOutputWindowContextMenu();

            Form = this;
        }

        public void Attach(IController controller)
        {
            _controller = controller;

            var updateMenus = new Action(UpdateMenus);
            var invokeUpdateMenus = new Action(() => Invoke(updateMenus));

            _controller.OnBuildStarted += delegate
            {
                _filterOutputWindow.SetBaseFolder(_controller);
                UpdateMenus();
            };
            _controller.OnBuildFinished += invokeUpdateMenus;
            _controller.OnProjectLoading += invokeUpdateMenus;
            _controller.OnProjectLoaded += invokeUpdateMenus;

            var updateUndoRedo = new CanUndoRedoChanged(UpdateUndoRedo);
            var invokeUpdateUndoRedo = new CanUndoRedoChanged((u, r) => Invoke(updateUndoRedo, u, r));

            _controller.OnCanUndoRedoChanged += invokeUpdateUndoRedo;
            _controller.Selection.Modified += OnSelectionModified;
        }

        private void InitOutputWindowContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem miCopy = new ToolStripMenuItem("&Copy");
            miCopy.Click += (o, a) =>
            {
                if (!string.IsNullOrEmpty(_outputWindow.SelectedText))
                    Clipboard.SetText(_outputWindow.SelectedText);
            };

            ToolStripMenuItem miSelectAll = new ToolStripMenuItem("&Select all");
            miSelectAll.Click += (o, a) => _outputWindow.SelectAll();

            contextMenu.Items.Add(miCopy);
            contextMenu.Items.Add(miSelectAll);

            _outputWindow.ContextMenuStrip = contextMenu;
        }

        public void OnTemplateDefined(ContentItemTemplate template)
        {
            // Load icon
            try
            {
                var iconPath = Path.Combine(Path.GetDirectoryName(template.TemplateFile), template.Icon);                
                var iconName = Path.GetFileNameWithoutExtension(iconPath);

                if (!EditorIcons.Templates.Images.ContainsKey(iconName))
                {
                    var iconImage = Image.FromFile(iconPath);
                    EditorIcons.Templates.Images.Add(iconName, iconImage);
                }

                template.Icon = iconName;
            }
            catch (Exception)
            {
                template.Icon = "Default";
            }
        }

        private void OnSelectionModified(Selection selection, object sender)
        {
            if (sender == this)
                return;

            _treeView.SelectedNodes = _controller.Selection.Select(FindNode);
        }

        private TreeNode FindNode(IProjectItem projectItem)
        {
            foreach (var n in _treeView.AllNodes())
            {
                var i = n.Tag as IProjectItem;
                if (i.OriginalPath == projectItem.OriginalPath)
                    return n;                
            }

            return null;
        }

        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs args)
        {
            if (args.ChangedItem.Label == "References")
                _controller.OnReferencesModified();
            if (args.ChangedItem.Label == "PackageReferences")
                _controller.OnReferencesModified();

            var obj = _propertyGrid.SelectedObject as PipelineProjectProxy;

            if (obj != null)
            {
                var item = (PipelineProject)_controller.GetItem(obj.OriginalPath);
                var action = new UpdateProjectAction(this, _controller, item, args.ChangedItem.PropertyDescriptor, args.OldValue);
                _controller.AddAction(action);

                _controller.OnProjectModified();
            }
            else
            {
                var action = new UpdateContentItemAction(this, _controller, _oldValues);
                _controller.AddAction(action);

                _controller.OnProjectModified();
            }
        }

        private void TreeViewOnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                var p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                var node = _treeView.GetNodeAt(p);
                if (node != null)
                {
                    TreeViewShowContextMenu(node, p);
                }
            }
        }

        private void TreeViewShowContextMenu(TreeNode node, Point contextMenuLocation)
        {
            if (!_treeView.SelectedNodes.Contains(node))
            {
                _treeView.SelectedNode = node;
            }

            if (_treeView.SelectedNodes.Count() == 1)
            {
                _treeSeparator1.Visible = true;
                _treeOpenFileLocationMenuItem.Visible = true;
                _treeRenameMenuItem.Visible = true;

                if (node.Tag is ContentItem)
                    _treeAddMenu.Visible = false;
                else
                    _treeAddMenu.Visible = true;

                if (node.Tag is FolderItem)
                    _treeOpenFileMenuItem.Visible = false;
                else
                    _treeOpenFileMenuItem.Visible = true;
            }
            else
            {
                _treeAddMenu.Visible = false;
                _treeOpenFileMenuItem.Visible = false;
                _treeOpenFileLocationMenuItem.Visible = false;
                _treeRenameMenuItem.Visible = false;
                _treeSeparator1.Visible = false;
            }

            _treeContextMenu.Show(_treeView, contextMenuLocation);
        }

        private void TreeViewOnNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs args)
        {
            // Even though we support 'Open File' as an action on the root (PipelineProject)
            // double clicking on it toggles whether it is expanded. 
            // So if you want to open it just use the menu.
            if (!(args.Node.Tag is ContentItem))
                return;

            ContextMenu_OpenFile_Click(sender, args);            
        }

        public void UpdateRecentProjectList()
        {
            try
            {
                _mainMenu.SuspendLayout();

                // TODO: clear removed projects from DropDownItems

                // attach added projects to DropDownItems
                foreach (string project in PipelineSettingsMgr.Settings.ProjectHistory)
                {
                    if (_openRecentMenuItem.DropDownItems.ContainsKey(project))
                        continue;

                    ToolStripMenuItem recentItem = new ToolStripMenuItem(project);
                    recentItem.Name = project;
                    _openRecentMenuItem.DropDownItems.Insert(0, recentItem);

                    // We need a local to make the delegate work correctly.
                    string localProject = project;
                    recentItem.Click += (sender, args) =>
                    {
                        _controller.OpenProject(localProject);
                    };
                }

                _openRecentMenuItem.Enabled = (_openRecentMenuItem.DropDownItems.Count >= 1);
            }
            finally
            {
                _mainMenu.ResumeLayout();
            }
        }

        public AskResult AskSaveOrCancel()
        {
            var result = MessageBox.Show(
                this,
                "Do you want to save the project first?",
                "Save Project",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button3);

            if (result == DialogResult.Yes)
                return AskResult.Yes;
            if (result == DialogResult.No)
                return AskResult.No;

            return AskResult.Cancel;
        }

        public bool AskSaveName(ref string filePath, string title)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                RestoreDirectory = true,
                InitialDirectory = Path.GetDirectoryName(filePath),
                FileName = Path.GetFileName(filePath),
                AddExtension = true,
                CheckPathExists = true,
                Filter = MGContentProjectFileFilter,
                FilterIndex = 2,                
            };
            var result = dialog.ShowDialog(this);
            filePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public bool AskOpenProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = MGContentProjectFileFilter,
                FilterIndex = 2,
            };
            var result = dialog.ShowDialog(this);
            projectFilePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public bool AskImportProject(out string projectFilePath)
        {
            var dialog = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = XnaContentProjectFileFilter,
                FilterIndex = 2,
            };
            var result = dialog.ShowDialog(this);
            projectFilePath = dialog.FileName;
            return result != DialogResult.Cancel;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(this, message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void BeginTreeUpdate()
        {
            Debug.Assert(_treeUpdating == false, "Must finish previous tree update!");
            _treeUpdating = true;
            _treeSort = false;
            _treeView.BeginUpdate();
        }

        public void SetTreeRoot(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            _propertyGrid.SelectedObject = null;

            if(item == null)
            {
                _treeView.Nodes.Clear();
                return;
            }

            var project = item as PipelineProject;
            if (project == null)
                return;

            TreeNode root;

            if (_treeView.Nodes.Count == 0)
                root = _treeView.Nodes.Add(string.Empty, item.Name, -1);
            else
                root = _treeView.Nodes[0];

            root.Tag = new PipelineProjectProxy(project);
            root.SelectedImageIndex = ContentIcons.ProjectIcon;
            root.ImageIndex = ContentIcons.ProjectIcon;
            root.Text = item.Name;

            _propertyGrid.SelectedObject = root.Tag;

            UpdateMenus();
        }

        public void AddTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = item.Location;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;
            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                {
                    var folderNode = parent.Add(folder, folder, -1);
                    folderNode.ImageIndex = ContentIcons.FolderClosedIcon;
                    folderNode.SelectedImageIndex = ContentIcons.FolderClosedIcon;

                    var idx = path.IndexOf(folder);
                    var curPath = path.Substring(0, idx + folder.Length);
                    folderNode.Tag = new FolderItem(curPath);
                    
                    parent = folderNode.Nodes;
                }
                else
                    parent = found[0].Nodes;
            }

            string fullPath = ((PipelineController)_controller).GetFullPath(item.OriginalPath);
            int iconIdx = _treeIcons.GetIcon(item.Exists, fullPath);

            var node = parent.Add(string.Empty, item.Name, -1);
            node.Tag = item;
            node.ImageIndex = iconIdx;
            node.SelectedImageIndex = iconIdx;

            _treeView.SelectedNode = node;

            root.Expand();
        }

        public void RemoveTreeItem(ContentItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(f => f.Tag == item);
            if (node == null)
                return;

            node.Remove();

            var obj = _propertyGrid.SelectedObject as ContentItem;
            if (obj != null && obj.OriginalPath == item.OriginalPath)
                _propertyGrid.SelectedObject = null;
        }

        public void SelectTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(e => e.Tag == item);
            if (node != null)
                _treeView.SelectedNode = node;
        }

        public void UpdateTreeItem(IProjectItem item)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            var node = _treeView.AllNodes().Find(e => e.Tag == item);
            if (node != null)
			{
				// Do something useful, eg...
				/* 
				if (!node.IsValid)
				{
	                node.ForeColor = Color.Red;
				}
				else
				{
					node.ForeColor = Color.Black;
				}*/
			}
        }

        public void EndTreeUpdate()
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");

            if (_treeSort)
            {
                var node = _treeView.SelectedNode;
                _treeView.Sort();
                _treeView.SelectedNode = node;
            }
            _treeSort = false;
            _treeView.EndUpdate();

            _treeUpdating = false;

            UpdateMenus();
        }

        public void ShowProperties(IProjectItem item)
        {
            _propertyGrid.SelectedObject = item;
            _propertyGrid.ExpandAllGridItems();
        }

        public void UpdateProperties(IProjectItem item)
        {
            foreach (var obj in _controller.Selection)
            {
                if (obj.OriginalPath.Equals(item.OriginalPath, StringComparison.OrdinalIgnoreCase))
                {
                    _propertyGrid.Refresh();
                    _propertyGrid.ExpandAllGridItems();
                    break;
                }
            }

            UpdateMenus();
        }

        public void OutputAppend(string text)
        {
            if (text == null)
                return;

            // We need to append newlines.
            var line = string.Concat(text, Environment.NewLine);

            // Write the output... safely if needed.
            if (InvokeRequired)
            {
                _outputWindow.Invoke(new Action<string>(_outputWindow.AppendText), new object[] { line });
                _filterOutputWindow.Invoke(new Action<string>(_filterOutputWindow.WriteLine), new object[] { line });
            }
            else
            {
                _outputWindow.AppendText(line);
                _filterOutputWindow.WriteLine(line);
            }
        }

        public bool ChooseContentFile(string initialDirectory, out List<string> files)
        {
            var dlg = new OpenFileDialog()
            {
                RestoreDirectory = true,
                AddExtension = true,
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "All Files (*.*)|*.*",
                InitialDirectory = initialDirectory,
                Multiselect = true,
                
            };

            var result = dlg.ShowDialog(this);
            files = new List<string>();

            if (result != DialogResult.OK)
                return false;

            files.AddRange(dlg.FileNames);

            return true;
        }

        public void OutputClear()
        {
            _outputWindow.Clear();
            _filterOutputWindow.Clear();
        }

        public void OutputPopulateAssets(PipelineProject project, IEnumerable<IProjectItem> items)
        {
            _filterOutputWindow.PopulateAssets(project, items);
        }

        public Process CreateProcess(string exe, string commands)
        {
            var _buildProcess = new Process();
            _buildProcess.StartInfo.FileName = exe;
            _buildProcess.StartInfo.Arguments = commands;
            return _buildProcess;
        }

        private void ExitMenuItemClick(object sender, System.EventArgs e)
        {
            if (_controller.Exit())
                Application.Exit();
        }

        private void MainView_Load(object sender, EventArgs e)
        {            
            // We only load the PipelineSettings.StartupProject if there was not
            // already a project specified via command line.
            if (string.IsNullOrEmpty(OpenProjectPath))
            {
                var startupProject = PipelineSettingsMgr.Settings.StartupProject;
                if (!string.IsNullOrEmpty(startupProject) && File.Exists(startupProject))                
                    OpenProjectPath = startupProject;                
            }

            PipelineSettingsMgr.Settings.StartupProject = null;
            
            if (!string.IsNullOrEmpty(OpenProjectPath))
            {
                _controller.OpenProject(OpenProjectPath);
                OpenProjectPath = null;
            }

            UpdateMenus();
        }

        private void MainView_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized && PipelineSettingsMgr.Current.IsInit)
            {
                PipelineSettingsMgr.Settings.Size.X = this.Width;
                PipelineSettingsMgr.Settings.Size.Y = this.Height;
            }
        }

        private void _splitTreeProps_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized && PipelineSettingsMgr.Current.IsInit)
                PipelineSettingsMgr.Settings.HSeparator = _splitTreeProps.SplitterDistance;
        }

        private void _splitEditorOutput_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized && PipelineSettingsMgr.Current.IsInit)
                PipelineSettingsMgr.Settings.VSeparator = _splitEditorOutput.SplitterDistance;
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!_controller.Exit())
                    e.Cancel = true;

                PipelineSettingsMgr.Settings.Maximized = (this.WindowState == FormWindowState.Maximized);
                PipelineSettingsMgr.Settings.FilterOutput = _filterOutputMenuItem.Checked;
                PipelineSettingsMgr.Current.Save();
            }
        }        

        private void OnNewProjectClick(object sender, EventArgs e)
        {
            _controller.NewProject();
        }

        private void OnImportProjectClick(object sender, EventArgs e)
        {
            _controller.ImportProject();
        }

        private void OnOpenProjectClick(object sender, EventArgs e)
        {
            _controller.OpenProject();
        }

        private void OnCloseProjectClick(object sender, EventArgs e)
        {
            _controller.CloseProject();
        }

        private void OnSaveProjectClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(false);
        }

        private void OnSaveAsProjectClick(object sender, System.EventArgs e)
        {
            _controller.SaveProject(true);
        }

        private void TreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {            
            _controller.Selection.Clear(this);
            _propertyGrid.SelectedObject = null;

            _oldValues.Clear();

            foreach (var node in _treeView.SelectedNodes)
            {
                var item = node.Tag as IProjectItem;

                if (item is ContentItem)
                    _oldValues.Add(ContentItemState.Get(item as ContentItem));

                _controller.Selection.Add(item, this);
            }

            _propertyGrid.SelectedObjects = _controller.Selection.ToArray();
            _propertyGrid.ExpandAllGridItems();
        }

        private void TreeViewMouseUp(object sender, MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button != MouseButtons.Right)
                return;

            // Point where the mouse is clicked.
            var p = new Point(e.X, e.Y);

            // Get the node that the user has clicked.
            var node = _treeView.GetNodeAt(p);
            if (node == null) 
                return;

            // Select the node the user has clicked.
            _treeView.SelectedNode = node;

            // TODO: Show context menu!
        }

        private void BuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.SingleThread = _singlethreadMenuItem.Checked;
            _controller.Build(false);
        }

        private void RebuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.SingleThread = _singlethreadMenuItem.Checked;
            _controller.Build(true);
        }

        private void RebuildItemsMenuItemClick(object sender, EventArgs e)
        {
            _controller.SingleThread = _singlethreadMenuItem.Checked;
            _controller.RebuildItems(_treeView.GetSelectedContentItems());
        }

        private void CleanMenuItemClick(object sender, EventArgs e)
        {
            _controller.SingleThread = _singlethreadMenuItem.Checked;
            _controller.Clean();
        }
        
        private void FilterOutputMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _outputTabs.SelectedIndex = _filterOutputMenuItem.Checked ? 1 : 0;
            _toolFilterOutput.Checked = _filterOutputMenuItem.Checked;
        }

        private void CancelBuildMenuItemClick(object sender, EventArgs e)
        {
            _controller.CancelBuild();
        }        

        private void TreeViewOnBeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == ContentIcons.FolderOpenIcon)
            {
                e.Node.ImageIndex = ContentIcons.FolderClosedIcon;
                e.Node.SelectedImageIndex = ContentIcons.FolderClosedIcon;
            }
        }

        private void TreeViewOnBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.ImageIndex == ContentIcons.FolderClosedIcon)
            {
                e.Node.ImageIndex = ContentIcons.FolderOpenIcon;
                e.Node.SelectedImageIndex = ContentIcons.FolderOpenIcon;
            }
        }

        private void TreeViewOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Apps)
            {
                if (_treeView.SelectedNode != null)
                {
                    Point nodeCoords = _treeView.PointToScreen(_treeView.SelectedNode.Bounds.Location);
                    TreeViewShowContextMenu(_treeView.SelectedNode, nodeCoords);
                }
            }
        }

        private void MainMenuMenuActivate(object sender, EventArgs e)
        {
            UpdateMenus();
        }

        private void UpdateMenus()
        {
            var notBuilding = !_controller.ProjectBuilding;
            var projectOpen = _controller.ProjectOpen;
            var projectOpenAndNotBuilding = projectOpen && notBuilding;
            var count = _treeView.SelectedNodes.Count();

            // Update the state of all menu items.

            _newProjectMenuItem.Enabled = _toolNew.Enabled = notBuilding;
            _openProjectMenuItem.Enabled = _toolOpen.Enabled = notBuilding;
            _importProjectMenuItem.Enabled = notBuilding;

            _saveMenuItem.Enabled = _toolSave.Enabled = projectOpenAndNotBuilding && _controller.ProjectDirty;
            _saveAsMenuItem.Enabled = projectOpenAndNotBuilding;
            _closeMenuItem.Enabled = projectOpenAndNotBuilding;

            _exitMenuItem.Enabled = notBuilding;

            _addMenuItem.Enabled = _toolNewItem.Enabled = _toolAddItem.Enabled =
                _toolNewFolder.Enabled = _toolAddFolder.Enabled = projectOpen & count <= 1;
            _deleteMenuItem.Enabled = projectOpen & count > 0;
            _renameMenuItem.Enabled = projectOpen & count == 1;

            _buildMenuItem.Enabled = _toolBuild.Enabled = projectOpenAndNotBuilding;

            _treeRebuildMenuItem.Enabled = _rebuildMenuItem.Enabled = _toolRebuild.Enabled = projectOpenAndNotBuilding;
            _rebuildMenuItem.Enabled = _treeRebuildMenuItem.Enabled;

            _cleanMenuItem.Enabled = _toolClean.Enabled = projectOpenAndNotBuilding;
            _cancelBuildSeparator.Visible = !notBuilding;
            _cancelBuildMenuItem.Enabled = !notBuilding;
            _cancelBuildMenuItem.Visible = !notBuilding;
      
            UpdateUndoRedo(_controller.CanUndo, _controller.CanRedo);

            UpdateRecentProjectList();
        }
        
        private void UpdateUndoRedo(bool canUndo, bool canRedo)
        {
            _undoMenuItem.Enabled = canUndo;
            _redoMenuItem.Enabled = canRedo;
        }

        private void OnDeleteItemClick(object sender, EventArgs e)
        {
            var items = new List<ContentItem>();
            var nodes = _treeView.SelectedNodesRecursive;
            List<string> dirs = new List<string>();

            foreach (var node in nodes)
            {
                var item = node.Tag as ContentItem;
                if (item != null && !items.Contains(item))
                    items.Add(item);
                else
                    dirs.Add(node.FullPath.Substring(_treeView.Nodes[0].Text.Length + 1));
            }

            _controller.Exclude(items, dirs);
        }

        private void ViewHelpMenuItemClick(object sender, EventArgs e)
        {
            Process.Start("https://github.com/kniEngine/kni/blob/main/Documentation/articles/content/using_mgcb_editor.md");
        }

        private void AboutMenuItemClick(object sender, EventArgs e)
        {
            var about = new AboutDialog();
            about.Show();
        }

        private void OnAddItemClick(object sender, EventArgs e)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            var item = node.Tag as IProjectItem;
            _controller.Include(item.Location);
        }

        private void OnNewItemClick(object sender, System.EventArgs e)
        {
            var dlg = new NewContentDialog(_controller.Templates, EditorIcons.Templates);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                var template = dlg.Selected;
                var location = ((_treeView.SelectedNode ?? _treeView.Nodes[0]).Tag as IProjectItem).Location;

                // Ensure name is unique among files at this location?
                _controller.NewItem(dlg.NameGiven, location, template);
            }
        }

        private void OnAddFolderClick(object sender, EventArgs e)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            string location = "";

            if (node != null)
            {
                var item = node.Tag as IProjectItem;
                if (item != null)
                    location = item.Location;
                else
                    location = node.FullPath.Substring(_treeView.Nodes[0].Text.Length + 1);
            }

            _controller.IncludeFolder(location);
        }

        private void OnNewFolderClick(object sender, EventArgs e)
        {
            var node = _treeView.SelectedNode ?? _treeView.Nodes[0];
            string location = "";

            if (node != null)
            {
                var item = node.Tag as IProjectItem;
                if (item != null)
                    location = item.Location;
                else
                    location = node.FullPath.Substring(_treeView.Nodes[0].Text.Length + 1);
            }

            var dialog = new TextEditDialog("New Folder", "Folder Name:", "");
            if (dialog.ShowDialog() == DialogResult.OK)
                _controller.NewFolder(dialog.text, location);
        }

        private void OnRedoClick(object sender, EventArgs e)
        {
            _controller.Redo();
        }

        private void OnUndoClick(object sender, EventArgs e)
        {
            _controller.Undo();
        }

        private void OnRenameItemClick(object sender, EventArgs e)
        {
            FileType type = FileType.Base;

            var item = (_treeView.SelectedNode.Tag as IProjectItem);
            string path = item.OriginalPath;

            if (_treeView.SelectedNode.Tag is ContentItem)
                type = FileType.File;
            else if (_treeView.SelectedNode.Tag is FolderItem)
                type = FileType.Folder;
            else
                path = item.Name;

            TextEditDialog dialog = new TextEditDialog("Rename", "New Name:", _treeView.SelectedNode.Text);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string newpath = System.IO.Path.GetDirectoryName(path) + System.IO.Path.DirectorySeparatorChar + dialog.text;
                _controller.Move(new [] { path }, new [] { newpath.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? newpath.Substring(1) : newpath }, new[] { type });
            }
        }

        private void ContextMenu_OpenFile_Click(object sender, EventArgs e)
        {
            string filePath = (_treeView.SelectedNode.Tag as IProjectItem).OriginalPath;
            string absolutePath = _controller.GetFullPath(filePath);

            if (File.Exists(absolutePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = absolutePath;
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
            }
        }

        private void ContextMenu_OpenFileLocation_Click(object sender, EventArgs e)
        {
            var filePath = (_treeView.SelectedNode.Tag as IProjectItem).OriginalPath;
            filePath = _controller.GetFullPath(filePath);

            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                Process.Start("explorer.exe", "/select, " + filePath);

            }
        }

        // http://stackoverflow.com/a/3955553/168235
        #region Custom Word-Wrapping (Output Window)
        
        const uint EM_SETWORDBREAKPROC = 0x00D0;

        [DllImport("user32.dll")]
        extern static IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

        delegate int EditWordBreakProc(IntPtr text, int pos_in_text, int bCharSet, int action);

        event EditWordBreakProc WordWrapCallbackEvent;
        
        private int WordWrapCallback(IntPtr text, int pos_in_text, int bCharSet, int action)
        {
            return 0;
        }
        #endregion

        private void MainView_Shown(object sender, EventArgs e)
        {
            WordWrapCallbackEvent = new EditWordBreakProc(WordWrapCallback);

            IntPtr ptr_func = Marshal.GetFunctionPointerForDelegate(WordWrapCallbackEvent);

            SendMessage(_outputWindow.Handle, EM_SETWORDBREAKPROC, IntPtr.Zero, ptr_func);

            // load settings
            if (PipelineSettingsMgr.Settings.VSeparator != 0)
            {
                this.Width = PipelineSettingsMgr.Settings.Size.X;
                this.Height = PipelineSettingsMgr.Settings.Size.Y;

                _splitEditorOutput.SplitterDistance = PipelineSettingsMgr.Settings.VSeparator;
                _splitTreeProps.SplitterDistance = PipelineSettingsMgr.Settings.HSeparator;

                _filterOutputMenuItem.Checked = _toolFilterOutput.Checked = PipelineSettingsMgr.Settings.FilterOutput;

                if (PipelineSettingsMgr.Settings.Maximized)
                    this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                PipelineSettingsMgr.Settings.Size.X = this.Width;
                PipelineSettingsMgr.Settings.Size.Y = this.Height;

                PipelineSettingsMgr.Settings.VSeparator = _splitEditorOutput.SplitterDistance;
                PipelineSettingsMgr.Settings.HSeparator = _splitTreeProps.SplitterDistance;
            }
            PipelineSettingsMgr.Current.IsInit = true;

            _outputTabs.SelectedIndex = _filterOutputMenuItem.Checked ? 1 : 0;
        }

        public void AddTreeFolder(string afolder)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = afolder;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;
            foreach (var folder in folders)
            {
                var found = parent.Find(folder, false);
                if (found.Length == 0)
                {
                    var folderNode = parent.Add(folder, folder, -1);
                    folderNode.ImageIndex = ContentIcons.FolderClosedIcon;
                    folderNode.SelectedImageIndex = ContentIcons.FolderClosedIcon;

                    var idx = path.IndexOf(folder);
                    var curPath = path.Substring(0, idx + folder.Length);
                    folderNode.Tag = new FolderItem(curPath);

                    parent = folderNode.Nodes;
                }
                else
                    parent = found[0].Nodes;
            }

            root.Expand();
        }

        public void RemoveTreeFolder(string folder)
        {
            Debug.Assert(_treeUpdating, "Must call BeginTreeUpdate() first!");
            _treeSort = true;

            var path = folder;
            var folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var root = _treeView.Nodes[0];
            var parent = root.Nodes;

            for (int i = 0; i < folders.Length;i++)
            {
                var found = parent.Find(folders[i], false);

                if (found.Length == 0)
                    return;
                else if (i != folders.Length - 1)
                    parent = found[0].Nodes;
                else
                    parent.Remove(found[0]);
            }
        }

        public bool ChooseContentFolder(string initialDirectory, out string folder)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = initialDirectory;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                folder = dialog.SelectedPath;
                return true;
            }

            folder = "";
            return false;
        }

        #region drag & drop
        private void MainView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            string filename = GetDropFile(e.Data, ".mgcb");
            if (filename != null) 
                e.Effect = DragDropEffects.Copy;
        }

        private void MainView_DragDrop(object sender, DragEventArgs e)
        {
            string filename = GetDropFile(e.Data, ".mgcb");
            if (filename != null)
                _controller.OpenProject(filename);
        }

        private string GetDropFile(IDataObject dataObject, string extension)
        {
            if (dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                foreach (var filename in files)
                {
                    if (Path.GetExtension(filename).Equals(extension, StringComparison.OrdinalIgnoreCase))
                        return filename;
                }
            }
            return null;
        }

        private void _treeView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            IProjectItem targetItem = GetDropTargetItem(sender, e);
            if (GetDropAssets(e.Data, targetItem) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void _treeView_DragDrop(object sender, DragEventArgs e)
        {
            IProjectItem targetItem = GetDropTargetItem(sender, e);
            var droppedAssets = GetDropAssets(e.Data, targetItem);
            if (droppedAssets != null)
            {
                // seperate assets 
                List<string> IncludeAssets = new List<string>();
                List<string> CopyOrLinkAssets = new List<string>();
                string initialDirectory = ((PipelineController)_controller).GetFullPath("");
                foreach (var filename in droppedAssets)
                {
                    if (filename.StartsWith(initialDirectory))
                        IncludeAssets.Add(filename);
                    else
                        CopyOrLinkAssets.Add(filename);
                }

                //include assets that are under the project base directory
                List<string> files = new List<string>();
                List<string> folders = new List<string>();
                foreach (var filename in IncludeAssets)
                {
                    if (File.GetAttributes(filename).HasFlag(FileAttributes.Directory))
                        folders.Add(filename);
                    else
                        files.Add(filename);
                        
                }
                _controller.Include(files, folders);

                //TODO: copy or link assets that are not under the project base directory
                //
            }
        }

        private static IProjectItem GetDropTargetItem(object sender, DragEventArgs e)
        {
            var treeView = sender as TreeView;
            var targetPoint = treeView.PointToClient(new Point(e.X, e.Y));
            var targetNode = treeView.GetNodeAt(targetPoint);

            IProjectItem targetItem = (IProjectItem)((targetNode == null) ? null : targetNode.Tag);
            return targetItem;
        }

        // return a list of files that are allowed to drop on target item
        private List<string> GetDropAssets(IDataObject dataObject, IProjectItem targetItem)
        {
            if (!dataObject.GetDataPresent(DataFormats.FileDrop))
                return null; 
            if (targetItem is ContentItem)
                return null; //drop to ContentItem is not allowed
            
            string initialDirectory = ((PipelineController)_controller).GetFullPath("");
            string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
            
            if (targetItem is FolderItem)
            {   
                return null; //drop to FolderItem is not yet supported
            }

            if ((targetItem is PipelineProjectProxy) || targetItem == null)
            {
                List<string> result = new List<string>();
                foreach (var filename in files)
                {
                    // filter items
                    if (Path.GetExtension(filename).Equals(".MGCB", StringComparison.OrdinalIgnoreCase))
                        continue; //drop of .MGCB files is not allowed
                    if (!filename.StartsWith(initialDirectory))
                        continue; //Copy/Link items is not yet supported
                    result.Add(filename);
                }
                if (result.Count > 0)
                    return result;
            }
            
            return null;
        }

        #endregion

        private void _toolFilterOutput_Click(object sender, EventArgs e)
        {
            _filterOutputMenuItem.Checked = !_filterOutputMenuItem.Checked;
        }
    }
}
