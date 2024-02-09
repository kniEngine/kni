// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Effect : GraphicsResource
    {
        struct MGFXHeader 
        {
            /// <summary>
            /// The MonoGame Effect file format header identifier ("MGFX"). 
            /// </summary>
            public static readonly int MGFXSignature = (BitConverter.IsLittleEndian) ? 0x5846474D: 0x4D474658;

            /// <summary>
            /// The current MonoGame Effect file format versions
            /// used to detect old packaged content.
            /// </summary>
            /// <remarks>
            /// We should avoid supporting old versions for very long if at all 
            /// as users should be rebuilding content when packaging their game.
            /// </remarks>
            public const int MGFXVersion = 10;

            public readonly int Signature;
            public readonly int Version;
            public readonly ShaderProfileType Profile;
            public readonly int EffectKey;
            public readonly int HeaderSize;

            public MGFXHeader(byte[] effectCode, int index)
            {
                int offset = 0;
                Signature = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                Version = (int)effectCode[index + offset]; offset += 1;
                Profile = (ShaderProfileType)effectCode[index + offset]; offset += 1;
                EffectKey = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                HeaderSize = offset;
            }
        }

        public EffectParameterCollection Parameters { get; private set; }

        public EffectTechniqueCollection Techniques { get; private set; }

        public EffectTechnique CurrentTechnique { get; set; }
  
        internal ConstantBuffer[] ConstantBuffers { get; private set; }

        private Shader[] _shaders;

	    private readonly bool _isClone;

        internal Effect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
		{
		}
			
		protected Effect(Effect cloneSource)
            : this(cloneSource.GraphicsDevice)
		{
            _isClone = true;
            Clone(cloneSource);
		}

        public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice, effectCode, 0, effectCode.Length)
        {
        }


        public Effect (GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count)
            : this(graphicsDevice)
		{
			// By default we currently cache all unique byte streams
			// and use cloning to populate the effect with parameters,
			// techniques, and passes.
			//
			// This means all the immutable types in an effect:
			//
			//  - Shaders
			//  - Annotations
			//  - Names
			//  - State Objects
			//
			// Are shared for every instance of an effect while the 
			// parameter values and constant buffers are copied.
			//
			// This might need to change slightly if/when we support
			// shared constant buffers as 'new' should return unique
			// effects without any shared instance state.
 
            //Read the header
            MGFXHeader header = new MGFXHeader(effectCode, index);
            if (header.Signature != MGFXHeader.MGFXSignature)
                throw new Exception("This does not appear to be an MGFX effect file.");
            if (header.Version > MGFXHeader.MGFXVersion)
                throw new Exception("This effect seems to be for a newer version of KNI.");
            if (header.Version == 8) // fallback to version 8
            {    System.Diagnostics.Debug.WriteLine("This effect is for an older version of KNI and needs to be rebuilt."); }
            else if (header.Version == 9) // fallback to version 9
            { }
            else
            if (header.Version < MGFXHeader.MGFXVersion)
                throw new Exception("This effect is for an older version of KNI and needs to be rebuilt.");

            // First look for it in the cache.
            //
            Effect effect;
            lock (((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache)
            {
                if (!((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.TryGetValue(header.EffectKey, out effect))
                {
                    using (var stream = new MemoryStream(effectCode, index + header.HeaderSize, count - header.HeaderSize, false))
                    {
                        if (header.Version == 8 || header.Version == 9)
                        {
                            using (var reader = new EffectReader09(stream, graphicsDevice, header))
                            {
                                // Create Effect.
                                effect = reader.ReadEffect();

                                // Cache the effect for later in its original unmodified state.
                                ((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.Add(header.EffectKey, effect);
                            }
                        }
                        else
                        {
                            using (var reader = new EffectReader10(stream, graphicsDevice, header))
                            {
                                // Create Effect.
                                effect = reader.ReadEffect();

                                // Cache the effect for later in its original unmodified state.
                                ((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.Add(header.EffectKey, effect);
                            }
                        }
                    }
                }
            }

            // Clone it.
            _isClone = true;
            Clone(effect);
        }

        /// <summary>
        /// Clone the source into this existing object.
        /// </summary>
        /// <remarks>
        /// Note this is not overloaded in derived classes on purpose.  This is
        /// only a reason this exists is for caching effects.
        /// </remarks>
        /// <param name="cloneSource">The source effect to clone from.</param>
        private void Clone(Effect cloneSource)
        {
            Debug.Assert(_isClone, "Cannot clone into non-cloned effect!");

            // Copy the mutable members of the effect.
            Parameters = cloneSource.Parameters.Clone();
            Techniques = cloneSource.Techniques.Clone(this);

            // Make a copy of the immutable constant buffers.
            ConstantBuffers = new ConstantBuffer[cloneSource.ConstantBuffers.Length];
            for (var i = 0; i < cloneSource.ConstantBuffers.Length; i++)
                ConstantBuffers[i] = new ConstantBuffer(cloneSource.ConstantBuffers[i]);

            // Find and set the current technique.
            for (var i = 0; i < cloneSource.Techniques.Count; i++)
            {
                if (cloneSource.Techniques[i] == cloneSource.CurrentTechnique)
                {
                    CurrentTechnique = Techniques[i];
                    break;
                }
            }

            // Take a reference to the original shader list.
            _shaders = cloneSource._shaders;
        }

        /// <summary>
        /// Returns a deep copy of the effect where immutable types 
        /// are shared and mutable data is duplicated.
        /// </summary>
        /// <remarks>
        /// See "Cloning an Effect" in MSDN:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ff476138(v=vs.85).aspx
        /// </remarks>
        /// <returns>The cloned effect.</returns>
		public virtual Effect Clone()
		{
            return new Effect(this);
		}

        protected internal virtual void OnApply()
        {
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                if (!_isClone)
                {
                    // Only the original source can dispose the shaders.
                    if (_shaders != null)
                    {
                        foreach (var shader in _shaders)
                            shader.Dispose();
                    }
                }

                if (ConstantBuffers != null)
                {
                    foreach (var buffer in ConstantBuffers)
                        buffer.Dispose();
                    ConstantBuffers = null;
                }
            }

            base.Dispose(disposing);
        }

        internal protected override void GraphicsContextLost()
        {
            for (int i = 0; i < ConstantBuffers.Length; i++)
                ((IPlatformConstantBuffer)ConstantBuffers[i]).Strategy.PlatformContextLost();
        }

    }
}
