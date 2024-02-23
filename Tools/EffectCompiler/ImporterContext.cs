using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace EffectCompiler
{
    class ImporterContext : ContentImporterContext
    {
        ContentBuildLogger _logger;

        public ImporterContext(ContentBuildLogger logger) : base()
        {
            _logger = logger;
        }

        public override string IntermediateDirectory { get { throw new NotImplementedException(); } }

        public override string OutputDirectory { get { throw new NotImplementedException(); } }

        public override ContentBuildLogger Logger { get { return _logger; } }

        public override void AddDependency(string filename)
        {
            
        }
    }
}
