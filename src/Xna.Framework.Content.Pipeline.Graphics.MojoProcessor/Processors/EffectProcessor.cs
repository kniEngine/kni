// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes a string representation to a platform-specific compiled effect.
    /// </summary>
    [ContentProcessor(DisplayName = "Effect - KNI")]
    public class EffectProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
    {
        private EffectProcessorDebugMode _debugMode;
        private string _defines;

        /// <summary>
        /// The debug mode for compiling effects.
        /// </summary>
        /// <value>The debug mode to use when compiling effects.</value>
        public virtual EffectProcessorDebugMode DebugMode
        {
            get { return _debugMode; }
            set { _debugMode = value; }
        }

        /// <summary>
        /// Define assignments for the effect.
        /// </summary>
        /// <value>A list of define assignments delimited by semicolons.</value>
        public virtual string Defines
        {
            get { return _defines; }
            set { _defines = value; }
        }


        /// <summary>
        /// Initializes a new instance of EffectProcessor.
        /// </summary>
        public EffectProcessor()
        {
        }

        /// <summary>
        /// Processes the string representation of the specified effect into a platform-specific binary format using the specified context.
        /// </summary>
        /// <param name="input">The effect string to be processed.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>A platform-specific compiled binary effect.</returns>
        public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            MojoEffectProcessor mojoProcessor = new MojoEffectProcessor();
            mojoProcessor.DebugMode = this.DebugMode;
            mojoProcessor.Defines = this.Defines;
            return mojoProcessor.Process(input, context);
        }
    }
}
