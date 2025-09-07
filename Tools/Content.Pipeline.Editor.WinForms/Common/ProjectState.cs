﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace Content.Pipeline.Editor
{
    /// <summary>
    /// Snapshot of a PipelineProject's state, used for undo/redo.
    /// </summary>
    internal class ProjectState
    {
        public string OutputDir;
        public string IntermediateDir;
        public List<string> References;
        public List<Package> PackageReferences;
        public TargetPlatform Platform;
        public GraphicsProfile Profile;
        public string Config;
        public string OriginalPath;

        /// <summary>
        /// Create a ProjectState storing member values of the passed PipelineProject.
        /// </summary>
        public static ProjectState Get(PipelineProject proj)
        {
            ProjectState state = new ProjectState()
                {
                    OriginalPath = proj.OriginalPath,
                    OutputDir = proj.OutputDir,
                    IntermediateDir = proj.IntermediateDir,
                    References = new List<string>(proj.References),
                    PackageReferences = new List<Package>(proj.PackageReferences),
                    Platform = proj.Platform,
                    Profile = proj.Profile,
                    Config = proj.Config,        
                };

            return state;
        }

        /// <summary>
        /// Set a PipelineProject's member values from this state object.
        /// </summary>
        public void Apply(PipelineProject proj)
        {
            proj.OutputDir = OutputDir;
            proj.IntermediateDir = IntermediateDir;
            proj.References = new List<string>(this.References);
            proj.PackageReferences = new List<Package>(this.PackageReferences);
            proj.Platform = Platform;
            proj.Profile = Profile;
            proj.Config = Config;
        }
    }    
}
