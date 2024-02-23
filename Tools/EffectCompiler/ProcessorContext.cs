// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace EffectCompiler
{
    internal class ProcessorContext : ContentProcessorContext
    {
        ContentBuildLogger _logger;
        TargetPlatform _targetPlatform;
        string _buildConfiguration;
        string _outputFilename;

        public ProcessorContext(ContentBuildLogger logger, TargetPlatform targetPlatform, string outputFilename, string config) : base()
        {
            _logger = logger;
            _targetPlatform = targetPlatform;
            _outputFilename = outputFilename;
            _buildConfiguration = config;
        }

        public override string IntermediateDirectory { get { throw new NotImplementedException(); } }

        public override string OutputDirectory { get { throw new NotImplementedException(); } }

        public override string BuildConfiguration { get { return _buildConfiguration; } }

        public override ContentBuildLogger Logger { get { return _logger; } }


        public override string OutputFilename { get { return _outputFilename; } }

        public override OpaqueDataDictionary Parameters { get { throw new NotImplementedException(); } }

        public override TargetPlatform TargetPlatform { get { return _targetPlatform; } }

        public override GraphicsProfile TargetProfile { get { throw new NotImplementedException(); } }

        public override void AddDependency(string filename)
        {
            
        }

        public override void AddOutputFile(string filename)
        {
            throw new System.NotImplementedException();
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            throw new System.NotImplementedException();
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            throw new System.NotImplementedException();
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
