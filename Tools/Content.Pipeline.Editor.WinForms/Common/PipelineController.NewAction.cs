﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Content.Pipeline.Editor
{
    internal partial class PipelineController
    {        
        private class NewAction : IProjectAction
        {
            private readonly PipelineController _con;
            private readonly string _name;
            private readonly string _location;
            private readonly ContentItemTemplate _template;

            public NewAction(PipelineController controller, string name, string location, ContentItemTemplate template)
            {
                _con = controller;
                _name = name;
                _location = location;
                _template = template;                
            }

            public bool Do()
            {
                var ext = Path.GetExtension(_template.TemplateFile);
                var filename = Path.ChangeExtension(_name, ext);
                var fullpath = _con.GetFullPath(Path.Combine(_location, filename));

                if (File.Exists(fullpath))
                {
                    _con.View.ShowError("Error", string.Format("File already exists: '{0}'.", fullpath));
                    return false;
                }

                File.Copy(_template.TemplateFile, fullpath);

                var parser = new PipelineProjectParser(_con, _con._project);
                _con.View.BeginTreeUpdate();

                _con.Selection.Clear(_con);

                if (parser.AddContent(fullpath, skipDuplicates: true, inorder: true, out int index))
                {
                    var item = _con._project.ContentItems[index];
                    item.Observer = _con;
                    item.ImporterName = _template.ImporterName;
                    item.ProcessorName = _template.ProcessorName;
                    item.ResolveTypes();

                    _con.View.AddTreeItem(item);
                    _con.Selection.Add(item, _con);
                }

                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }

            public bool Undo()
            {
                var ext = Path.GetExtension(_template.TemplateFile);
                var filename = Path.ChangeExtension(_name, ext);
                var fullpath = Path.Combine(_location, filename);

                if (!File.Exists(fullpath))
                {
                    _con.View.ShowError("Error", string.Format("File does not exist: '{0}'.", fullpath));
                    return false;
                }

                File.Delete(fullpath);
                
                _con.View.BeginTreeUpdate();

                for (var i = 0; i < _con._project.ContentItems.Count; i++)
                {
                    var item = _con._project.ContentItems[i];
                    var path = Path.GetFullPath(_con._project.Location + "\\" + item.OriginalPath);

                    if (fullpath == path)
                    {
                        _con._project.ContentItems.Remove(item);
                        _con.View.RemoveTreeItem(item);
                        _con.Selection.Remove(item, _con);
                    }
                }
                    
                _con.View.EndTreeUpdate();
                _con.ProjectDirty = true;

                return true;
            }
        }
    }
}