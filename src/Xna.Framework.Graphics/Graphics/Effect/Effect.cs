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
 
            //Read the KNI fx header
            KNIFXHeader fxHeader = new KNIFXHeader(effectCode, index);
            if (fxHeader.Signature == KNIFXHeader.KNIFXSignature)
            {
                switch (fxHeader.Version)
                {
                    case 11:
                        {
                            int offset = fxHeader.HeaderSize;

                            int fxCount = BitConverter.ToInt16(effectCode, index + offset); offset += 2;
                            Debug.Assert(fxCount == 1);

                            for (int fxIdx = 0; fxIdx < fxCount; fxIdx++)
                            {
                                ShaderProfileType shaderProfile = (ShaderProfileType)effectCode[index + offset]; offset += 1;
                                int effectKey = BitConverter.ToInt32(effectCode, index + offset); offset += 4;

                                Effect effect;
                                lock (((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache)
                                {
                                    if (!((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.TryGetValue(effectKey, out effect))
                                    {
                                        int effectLength = BitConverter.ToInt32(effectCode, index + offset); offset += 4;
                                        using (Stream stream = new MemoryStream(effectCode, index + offset, effectLength, false))
                                        using (KNIFXReader11 reader = new KNIFXReader11(stream, graphicsDevice, shaderProfile))
                                            effect = reader.ReadEffect();

                                        ((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.Add(effectKey, effect);
                                    }
                                }

                                // Clone it.
                                _isClone = true;
                                Clone(effect);
                                return;
                            }

                            throw new Exception("Effect profile is not compatible with the graphics backend '" + graphicsDevice.Adapter.Backend + "'.");
                        }

                    default:
                        if (fxHeader.Version > KNIFXHeader.CurrentKNIFXVersion)
                            throw new Exception("This effect seems to be for a newer version of KNI.");
                        if (fxHeader.Version < KNIFXHeader.CurrentKNIFXVersion)
                            throw new Exception("This effect is for an older version of KNI and needs to be rebuilt.");
                        break;
                }
            }

            //Read the mgfx header
            MGFXHeader mgfxheader = new MGFXHeader(effectCode, index);
            if (mgfxheader.Signature == MGFXHeader.MGFXSignature)
            {
                switch (mgfxheader.Version)
                {
                    case 10:
                        {
                            int offset = mgfxheader.HeaderSize;

                            Effect effect;
                            lock (((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache)
                            {
                                if (!((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.TryGetValue(mgfxheader.EffectKey, out effect))
                                {
                                    int length = count - offset;
                                    using (Stream stream = new MemoryStream(effectCode, index + offset, length, false))
                                    using (MGFXReader10 reader = new MGFXReader10(stream, graphicsDevice, mgfxheader.Profile))
                                        effect = reader.ReadEffect();

                                    ((IPlatformGraphicsDevice)graphicsDevice).Strategy.EffectCache.Add(mgfxheader.EffectKey, effect);
                                }
                            }

                            // Clone it.
                            _isClone = true;
                            Clone(effect);
                            return;
                        }

                    default:
                        if (mgfxheader.Version > MGFXHeader.MGFXVersion)
                            throw new Exception("This effect seems to be for a newer version of KNI.");
                        if (mgfxheader.Version < MGFXHeader.MGFXVersion)
                            throw new Exception("This effect is for an older version of KNI and needs to be rebuilt.");
                        break;
                }
            }

            throw new Exception("This does not appear to be an FX effect file.");
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
            for (int i = 0; i < cloneSource.ConstantBuffers.Length; i++)
                ConstantBuffers[i] = new ConstantBuffer(cloneSource.ConstantBuffers[i]);

            // Find and set the current technique.
            for (int i = 0; i < cloneSource.Techniques.Count; i++)
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
                        foreach (Shader shader in _shaders)
                            shader.Dispose();
                    }
                }

                if (ConstantBuffers != null)
                {
                    foreach (ConstantBuffer buffer in ConstantBuffers)
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
