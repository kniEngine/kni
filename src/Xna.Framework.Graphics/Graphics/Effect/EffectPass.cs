using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        private readonly Effect _effect;

        private readonly PixelShader _pixelShader;
        private readonly VertexShader _vertexShader;

        private readonly BlendState _blendState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;

        public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        internal EffectPass(    Effect effect, 
                                string name,
                                VertexShader vertexShader, 
                                PixelShader pixelShader,
                                BlendState blendState, 
                                DepthStencilState depthStencilState, 
                                RasterizerState rasterizerState,
                                EffectAnnotationCollection annotations )
        {
            Debug.Assert(effect != null, "Got a null effect!");
            Debug.Assert(annotations != null, "Got a null annotation collection!");

            _effect = effect;

            Name = name;

            _vertexShader = vertexShader;
            _pixelShader = pixelShader;

            _blendState = blendState;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;

            Annotations = annotations;
        }
        
        internal EffectPass(Effect effect, EffectPass cloneSource)
        {
            Debug.Assert(effect != null, "Got a null effect!");
            Debug.Assert(cloneSource != null, "Got a null cloneSource!");

            _effect = effect;

            // Share all the immutable types.
            Name = cloneSource.Name;
            _blendState = cloneSource._blendState;
            _depthStencilState = cloneSource._depthStencilState;
            _rasterizerState = cloneSource._rasterizerState;
            Annotations = cloneSource.Annotations;
            _vertexShader = cloneSource._vertexShader;
            _pixelShader = cloneSource._pixelShader;
        }

        public void Apply()
        {
            EffectTechnique currentTechnique = _effect.CurrentTechnique;

            _effect.OnApply();

            if (_effect.CurrentTechnique != currentTechnique)
                throw new InvalidOperationException("CurrentTechnique changed during Effect.OnApply().");

            GraphicsContext context = _effect.GraphicsDevice.CurrentContext;

            if (_vertexShader != null)
            {
                context.VertexShader = _vertexShader;

                // Update the texture parameters.
                SetShaderSamplers(_vertexShader, context.VertexTextures, context.VertexSamplerStates);

                // Update the constant buffers.
                for (int c = 0; c < _vertexShader.CBuffers.Length; c++)
                {
                    ConstantBuffer constantBuffer = _effect.ConstantBuffers[_vertexShader.CBuffers[c]];
                    ((IPlatformConstantBuffer)constantBuffer).Strategy.Update(_effect.Parameters);
                    ((IPlatformGraphicsContext)context).Strategy._vertexConstantBuffers[c] = constantBuffer;
                }
            }

            if (_pixelShader != null)
            {
                context.PixelShader = _pixelShader;

                // Update the texture parameters.
                SetShaderSamplers(_pixelShader, context.Textures, context.SamplerStates);

                // Update the constant buffers.
                for (int c = 0; c < _pixelShader.CBuffers.Length; c++)
                {
                    ConstantBuffer constantBuffer = _effect.ConstantBuffers[_pixelShader.CBuffers[c]];
                    ((IPlatformConstantBuffer)constantBuffer).Strategy.Update(_effect.Parameters);
                    ((IPlatformGraphicsContext)context).Strategy._pixelConstantBuffers[c] = constantBuffer;
                }
            }

            // Set the render states if we have some.
            if (_rasterizerState != null)
                context.RasterizerState = _rasterizerState;
            if (_blendState != null)
                context.BlendState = _blendState;
            if (_depthStencilState != null)
                context.DepthStencilState = _depthStencilState;
        }

        private void SetShaderSamplers(Shader shader, TextureCollection textures, SamplerStateCollection samplerStates)
        {
            SamplerInfo[] samplers = shader.Samplers;
            for (int i = 0; i < samplers.Length; i++)
            {
                // Set Texture
                if (samplers[i].textureParameter != -1)
                {
                    EffectParameter param = _effect.Parameters[samplers[i].textureParameter];
                    textures[samplers[i].textureSlot] = param.Data as Texture;
                }

                // Set SamplerState
                if (samplers[i].state != null)
                    samplerStates[samplers[i].samplerSlot] = samplers[i].state;
            }
        }
    }
}
