using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.EffectCompiler
{
    internal class ProcessorContext : ContentProcessorContext
    {
        ContentBuildLogger _logger;
        TargetPlatform _targetPlatform;
        string _outputFilename;

        public ProcessorContext(ContentBuildLogger logger, TargetPlatform targetPlatform, string outputFilename) : base()
        {
            _logger = logger;
            _targetPlatform = targetPlatform;
            _outputFilename = outputFilename;
        }

        public override string IntermediateDirectory { get { throw new NotImplementedException(); } }

        public override string OutputDirectory { get { throw new NotImplementedException(); } }

        public override string BuildConfiguration { get { throw new NotImplementedException(); } }

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
