// Copyright (C)2022 Nick Kastellanos

using Microsoft.Xna.Framework.Content.Pipeline;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class BuildAsyncState
    {
        public string SourceFile { get; internal set; }
        public string OutputFile { get; internal set; }
        public string Importer  { get; internal set; }
        public string Processor { get; internal set; }
        public OpaqueDataDictionary ProcessorParams { get; internal set; }
        public ConsoleAsyncLogger Logger { get; internal set; }
    }
}
