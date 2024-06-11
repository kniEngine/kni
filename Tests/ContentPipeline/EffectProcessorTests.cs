// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
#if DIRECTX
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endif

namespace Kni.Tests.ContentPipeline
{
    class EffectProcessorTests
    {
        class ImporterContext : ContentImporterContext
        {
            public override string IntermediateDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override ContentBuildLogger Logger
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override void AddDependency(string filename)
            {
                throw new NotImplementedException();
            }
        }

#if DIRECTX
        [Test]
        public void TestPreprocessor()
        {
            var filename = "Assets/Effects/PreprocessorTest.fx";
            var effect = new EffectContent();
            effect.Identity = new ContentIdentity(filename);
            effect.Name = Path.GetFileNameWithoutExtension(filename);
            effect.EffectCode = File.ReadAllText(filename);

            string fullFilePath = Path.GetFullPath(effect.Identity.SourceFilename);


            var processorContext = new TestProcessorContext(TargetPlatform.Windows, Path.ChangeExtension(filename, ".xnb"));

            // Preprocess.
            Preprocessor pp = new Preprocessor(effect, processorContext, fullFilePath);
            pp.AddMacro("TEST2", "1");
            var mgPreprocessed = pp.Preprocess();

            Assert.That(processorContext._dependencies, Has.Count.EqualTo(1));
            Assert.That(Path.GetFileName(processorContext._dependencies[0]), Is.EqualTo("PreprocessorInclude.fxh"));

            Assert.That(mgPreprocessed, Does.Not.Contain("Foo"));
            Assert.That(mgPreprocessed, Does.Contain("Bar"));
            Assert.That(mgPreprocessed, Does.Not.Contain("Baz"));

            Assert.That(mgPreprocessed, Does.Contain("FOO"));
            Assert.That(mgPreprocessed, Does.Not.Contain("BAR"));

            // Check that we can actually compile this file.
            BuildEffect(filename, TargetPlatform.Windows);
        }
#endif

        [Test]
        [TestCase("Assets/Effects/ParserTest.fx")]
        public void TestParser(string effectFile)
        {
            BuildEffect(effectFile, TargetPlatform.Windows);
        }

        [Test]
        public void TestDefines()
        {
            Assert.DoesNotThrow(() => BuildEffect("Assets/Effects/DefinesTest.fx", TargetPlatform.Windows, "MACRO_DEFINE_TEST=3"));
            Assert.Throws<InvalidContentException>(() =>
                BuildEffect("Assets/Effects/DefinesTest.fx", TargetPlatform.Windows, "MACRO_DEFINE_TEST=4"));
            Assert.Throws<InvalidContentException>(() =>
                BuildEffect("Assets/Effects/DefinesTest.fx", TargetPlatform.Windows));
            Assert.Throws<InvalidContentException>(() =>
                BuildEffect("Assets/Effects/DefinesTest.fx", TargetPlatform.Windows, "INVALID_SYNTAX;ANOTHER_MACRO;MACRO_DEFINE_TEST=3"));
        }

        [Test]
        [TestCase("Assets/Effects/Stock/AlphaTestEffect.fx")]
        [TestCase("Assets/Effects/Stock/BasicEffect.fx")]
        [TestCase("Assets/Effects/Stock/DualTextureEffect.fx")]
        [TestCase("Assets/Effects/Stock/EnvironmentMapEffect.fx")]
        [TestCase("Assets/Effects/Stock/SkinnedEffect.fx")]
        [TestCase("Assets/Effects/Stock/SpriteEffect.fx")]
        public void BuildStockEffect(string effectFile)
        {
            BuildEffect(effectFile, TargetPlatform.Windows);
        }

        private void BuildEffect(string effectFile, TargetPlatform targetPlatform, string defines = null)
        {
            var importerContext = new ImporterContext();
            var importer = new EffectImporter();
            var input = importer.Import(effectFile, importerContext);

            Assert.NotNull(input);

            var processorContext = new TestProcessorContext(targetPlatform, Path.ChangeExtension(effectFile, ".xnb"));
            var processor = new EffectProcessor { Defines = defines };
            var output = processor.Process(input, processorContext);

            Assert.NotNull(output);

            // TODO: Should we test the writer?
        }
    }
}
