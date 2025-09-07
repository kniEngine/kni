﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace Content.Pipeline.Editor
{
    internal class UpdateProjectAction : IProjectAction
    {
        private readonly IView _view;
        private readonly IController _con;
        private readonly bool _referencesChanged;
        private readonly bool _packageReferencesChanged;


        private ProjectState _state;

        public UpdateProjectAction(IView view, IController con, PipelineProject item, PropertyDescriptor property, object previousValue)
        {
            _view = view;
            _con = con;

            _state = new ProjectState(item);

            switch (property.Name)
            {
                case "OutputDir":
                    _state.OutputDir = (string)previousValue;
                    break;
                case "IntermediateDir":
                    _state.IntermediateDir = (string)previousValue;
                    break;
                case "References":
                    _state.References = new List<string>((List<string>)previousValue);
                    _referencesChanged = true;
                    break;
                case "PackageReferences":
                    _state.PackageReferences = new List<Package>((List<Package>)previousValue);
                    _packageReferencesChanged = true;
                    break;
                case "Platform":
                    _state.Platform = (TargetPlatform)previousValue;
                    break;
                case "Profile":
                    _state.Profile = (GraphicsProfile)previousValue;
                    break;
                case "Config":
                    _state.Config = (string)previousValue;
                    break;
                case "OriginalPath":
                    _state.OriginalPath = (string)previousValue;
                    break;
            }
        }

        public bool Do()
        {
            Toggle();
            return true;
        }

        public bool Undo()
        {
            Toggle();
            return true;
        }

        private void Toggle()
        {
            PipelineProject item = (PipelineProject)_con.GetItem(_state.OriginalPath);
            ProjectState state = new ProjectState(item);
            _state.Apply(item);
            _state = state;

            if (_referencesChanged || _packageReferencesChanged)
                _con.OnReferencesModified();
            else
                _con.OnProjectModified();

            _view.BeginTreeUpdate();
            _view.UpdateProperties(item);
            _view.UpdateTreeItem(item);
            _view.EndTreeUpdate();
        }
    }
}
