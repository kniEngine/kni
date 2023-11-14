// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace Content.Pipeline.Editor.Windows.Controls
{
    public partial class FilterOutputControl : TreeView
    {
        const int WM_VSCROLL = 0x0115;
        const int SB_BOTTOM  = 0x07;
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private BuildIcons _buildIcons;
        private TreeNode _lastTreeNode;

        private PipelineProject _project;
        private List<IProjectItem> _contentItems;
        Dictionary<string, TreeNode> _assetMap = new Dictionary<string, TreeNode>();
        Dictionary<string, TreeNode> _xnbMap = new Dictionary<string, TreeNode>();

        private OutputParser _outputParser;
        private string _prevFilename;

        Uri _folderUri;
        Uri _outputUri;


        public FilterOutputControl() : base()
        {
            this._buildIcons = new BuildIcons();
            this.ImageList = _buildIcons.Icons;
            _outputParser = new OutputParser();
        }

        internal void SetBaseFolder(IController controller)
        {
            string pl = ((PipelineController)controller).ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;
            _folderUri = new Uri(pl);

            string pod = ((PipelineController)controller).ProjectOutputDir;
            if (!pod.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pod += System.IO.Path.DirectorySeparatorChar;
            pod = Path.Combine(pl, pod);
            _outputUri = new Uri(pod);

            _outputParser.Reset();
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            // disable selection for all nodes
            e.Cancel = true;
        }

        internal void WriteLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            this.SuspendLayout();
            this.BeginUpdate();
            try
            {
                _outputParser.Parse(line);

                line = line.TrimEnd(new[] { ' ', '\n', '\r', '\t' });

                switch (_outputParser.State)
                {
                    case OutputState.BuildBegin:
                        {
                            var tn = AddItem(BuildIcons.BeginEnd, line);
                            PopulateAssets();
                            break;
                        }

                    case OutputState.Cleaning:
                        {
                            var tn = AddItem(_outputParser.Filename, BuildIcons.Clean, "Cleaned " + GetRelativeOutputPath(_outputParser.Filename));
                            tn.ToolTipText = line;
                            AddSubItem(tn, line);
                            break;
                        }
                    case OutputState.Skipping:
                        {
                            var tn = AddItem(_outputParser.Filename, BuildIcons.Skip, "Skipped " + GetRelativePath(_outputParser.Filename));
                            tn.ToolTipText = line;
                            AddSubItem(tn, line);
                            break;
                        }
                    case OutputState.BuildAsset:
                        {
                            var tn = AddItem(_outputParser.Filename, BuildIcons.Processing, "Building " + GetRelativePath(_outputParser.Filename));
                            tn.ToolTipText = line;
                            AddSubItem(tn, line);
                            break;
                        }

                    case OutputState.BuildError:
                        {
                            _lastTreeNode.ImageIndex = BuildIcons.Fail;
                            _lastTreeNode.SelectedImageIndex = BuildIcons.Fail;
                            _lastTreeNode.ToolTipText += Environment.NewLine + Environment.NewLine + _outputParser.ErrorMessage;
                            AddSubItem(_lastTreeNode, _outputParser.ErrorMessage).ForeColor = System.Drawing.Color.DarkRed;
                            break;
                        }
                    case OutputState.BuildErrorContinue:
                        {
                            _lastTreeNode.ToolTipText += Environment.NewLine + _outputParser.ErrorMessage;
                            AddSubItem(_lastTreeNode, _outputParser.ErrorMessage).ForeColor = System.Drawing.Color.DarkRed;
                            break;
                        }

                    case OutputState.BuildEnd:
                        {
                            var tn = AddItem(BuildIcons.BeginEnd, line);
                            break;
                        }
                    case OutputState.BuildTime:
                        {
                            _lastTreeNode.Text = _lastTreeNode.Text.TrimEnd(new[] { '.', ' ' }) + ", " + line;
                            SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end
                            break;
                        }
                }

                _prevFilename = _outputParser.Filename;
            }
            catch (Exception ex)
            {
                string msg = String.Format("output: \"{0}\"\n\nError message: {1}", line, ex.Message);
                MessageBox.Show(msg, "Failed to parse output", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
            finally
            {
                this.EndUpdate();
                this.ResumeLayout();
            }
        }

        private TreeNode AddItem(int iconIdx, string text, TreeNode node = null)
        {
            if (node == null)
            {
                node = new TreeNode(text, iconIdx, iconIdx);
                this.Nodes.Add(node);
            }
            else
            {
                node.Text = text;
                node.ImageIndex = iconIdx;
                node.SelectedImageIndex = iconIdx;
            }

            if (_lastTreeNode != null && _lastTreeNode.ImageIndex == BuildIcons.Processing)
            {
                _lastTreeNode.ImageIndex = BuildIcons.Succeed;
                _lastTreeNode.SelectedImageIndex = BuildIcons.Succeed;

                if (_lastTreeNode.Text.StartsWith("Building"))
                    _lastTreeNode.Text = _lastTreeNode.Text.Substring(9);
            }

            _lastTreeNode = node;

            node.EnsureVisible();
            //SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end

            return node;
        }

        private TreeNode AddItem(string filename, int iconIdx, string text)
        {
            TreeNode item = null;
            var key = filename;
            //normalize key
            key = Path.ChangeExtension(key, null);
            key = key.Replace('\\', '/');
            key = new Uri(key).AbsolutePath;

            //get node
            _assetMap.TryGetValue(key, out item);
            if (item == null)
                _xnbMap.TryGetValue(key, out item);

            return AddItem(iconIdx, text, item);
        }

        private static TreeNode AddSubItem(TreeNode treeNode, string text)
        {
            var subTreeNode = new TreeNode(text, BuildIcons.Null, BuildIcons.Null);
            treeNode.Nodes.Add(subTreeNode);
            return subTreeNode;
        }

        private string GetRelativePath(string path)
        {
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(_folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }

        private string GetRelativeOutputPath(string path)
        {
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(_outputUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }

        internal void Clear()
        {
            this.Nodes.Clear();

            _project = null;
            _contentItems = null;
            _assetMap.Clear();
            _xnbMap.Clear();
        }

        internal void PopulateAssets(PipelineProject project, IEnumerable<IProjectItem> items)
        {
            this._project = project;
            this._contentItems = new List<IProjectItem>(items);
        }

        internal void PopulateAssets()
        {
            // Suspend FilterOutput
            this.SuspendLayout();

            foreach (var ContentItem in _contentItems)
            {
                var node = new TreeNode(ContentItem.OriginalPath, BuildIcons.Queued, BuildIcons.Queued);

                this.Nodes.Add(node);

                string key = Path.Combine(_project.Location, ContentItem.OriginalPath);
                //normalize key
                key = Path.ChangeExtension(key, null);
                key = key.Replace('\\', '/');
                key = new Uri(key).AbsolutePath;
                _assetMap.Add(key, node); //map key to node

                var resolvedOutputDir = ReplaceSymbols(_project, _project.OutputDir);
                key = Path.Combine(_project.Location, resolvedOutputDir, ContentItem.OriginalPath);
                //normalize key
                key = Path.ChangeExtension(key, null);
                key = key.Replace('\\', '/');
                key = new Uri(key).AbsolutePath;
                _xnbMap.Add(key, node); //map key to node
            }

            // Resume FilterOutput Layout
            this.ResumeLayout();
        }

        string ReplaceSymbols(PipelineProject project, string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return parameter;
            return parameter
                .Replace("$(Platform)", project.Platform.ToString())
                .Replace("$(Configuration)", project.Config)
                .Replace("$(Config)", project.Config)
                .Replace("$(Profile)", project.Profile.ToString());
        }

    }
}

