// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class PipelineImporterContext : ContentImporterContext
    {
        ConsoleLogger _logger;
        private readonly PipelineManager _manager;
        private readonly PipelineBuildEvent _pipelineEvent;

        public PipelineImporterContext(ConsoleLogger logger, PipelineManager manager, PipelineBuildEvent pipelineEvent)
        {
            _logger = logger;
            _manager = manager;
        }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override ContentBuildLogger Logger { get { return _logger; } }

        public override void AddDependency(string filename)
        {            
            if (!_pipelineEvent.Dependencies.Contains(filename))
                _pipelineEvent.Dependencies.Add(filename);
        }
    }
}
