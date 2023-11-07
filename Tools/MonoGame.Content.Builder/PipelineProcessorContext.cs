// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class PipelineProcessorContext : ContentProcessorContext
    {
        ConsoleLogger _logger;
        private readonly PipelineManager _manager;

        private readonly PipelineBuildEvent _pipelineEvent;

        public PipelineProcessorContext(ConsoleLogger logger, PipelineManager manager, PipelineBuildEvent pipelineEvent)
        {
            _logger = logger;
            _manager = manager;
            _pipelineEvent = pipelineEvent;
        }

        public override TargetPlatform TargetPlatform { get { return _manager.Platform; } }
        public override GraphicsProfile TargetProfile { get { return _manager.Profile; } }

        public override string BuildConfiguration { get { return _manager.Config; } }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override string OutputFilename { get { return _pipelineEvent.DestFile; } }

        public override OpaqueDataDictionary Parameters { get { return _pipelineEvent.Parameters; } }

        public override ContentBuildLogger Logger { get { return _logger; } }

        public override void AddDependency(string filename)
        {
            if (!_pipelineEvent.Dependencies.Contains(filename))
                _pipelineEvent.Dependencies.Add(filename);
        }

        public override void AddOutputFile(string filename)
        {
            if (!_pipelineEvent.BuildOutput.Contains(filename))
                _pipelineEvent.BuildOutput.Add(filename);
        }

        public override TOutput Convert<TInput, TOutput>(   TInput input, 
                                                            string processorName,
                                                            OpaqueDataDictionary processorParameters)
        {
            var processor = _manager.CreateProcessor(processorName, processorParameters);
            var processContext = new PipelineProcessorContext(this._logger, _manager, new PipelineBuildEvent { Parameters = processorParameters } );
            var processedObject = processor.Process(input, processContext);
           
            // Add its dependencies and built assets to ours.
            foreach (string dependency in processContext._pipelineEvent.Dependencies)
            {
                if (!_pipelineEvent.Dependencies.Contains(dependency))
                    _pipelineEvent.Dependencies.Add(dependency);
            }
            foreach (string buildAsset in processContext._pipelineEvent.BuildAsset)
            {
                if (!_pipelineEvent.BuildAsset.Contains(buildAsset))
                    _pipelineEvent.BuildAsset.Add(buildAsset);
            }

            return (TOutput)processedObject;
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                    string processorName,
                                                                    OpaqueDataDictionary processorParameters,
                                                                    string importerName)
        {
            var sourceFilepath = PathHelper.Normalize(sourceAsset.Filename);

            // The processorName can be null or empty. In this case the asset should
            // be imported but not processed. This is, for example, necessary to merge
            // animation files as described here:
            // http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/merging-animation-files.aspx.

            bool processAsset = !string.IsNullOrEmpty(processorName);
            _manager.ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref processorName);

            var buildEvent = new PipelineBuildEvent 
            { 
                SourceFile = sourceFilepath,
                Importer = importerName,
                Processor = processAsset ? processorName : null,
                Parameters = _manager.ValidateProcessorParameters(processorName, processorParameters),
            };

            var processedObject = _manager.ProcessContent(this._logger, buildEvent);

            // Record that we processed this dependent asset.
            if (!_pipelineEvent.Dependencies.Contains(sourceFilepath))
                _pipelineEvent.Dependencies.Add(sourceFilepath);

            return (TOutput)processedObject;
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                                string processorName,
                                                                                OpaqueDataDictionary processorParameters,
                                                                                string importerName, 
                                                                                string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = _manager.GetAssetName(this._logger, sourceAsset.Filename, importerName, processorName, processorParameters);

            // Build the content.
            PipelineBuildEvent buildEvent = _manager.BuildContent(this._logger, sourceAsset.Filename, assetName, importerName, processorName, processorParameters);

            // Record that we built this dependent asset.
            if (!_pipelineEvent.BuildAsset.Contains(buildEvent.DestFile))
                _pipelineEvent.BuildAsset.Add(buildEvent.DestFile);

            return new ExternalReference<TOutput>(buildEvent.DestFile);
        }
    }
}
