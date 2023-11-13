// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class ImporterContext : ContentImporterContext
    {
        private readonly PipelineManager _manager;
        ConsoleLogger _logger;
        private readonly PipelineBuildEvent _buildEvent;

        public ImporterContext(PipelineManager manager, ConsoleLogger logger, PipelineBuildEvent buildEvent)
        {
            _manager = manager;
            _logger = logger;
            _buildEvent = buildEvent;
        }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override ContentBuildLogger Logger { get { return _logger; } }

        public override void AddDependency(string filename)
        {            
            if (!_buildEvent.Dependencies.Contains(filename))
                _buildEvent.Dependencies.Add(filename);
        }
    }
}
