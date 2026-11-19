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

        // Shader indexing: the shader is selected by the value of an int effect parameter.
        private readonly VertexShader[] _vertexShaders;
        private readonly PixelShader[] _pixelShaders;
        private readonly int _vertexShaderSelector;
        private readonly int _pixelShaderSelector;

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
        
        internal EffectPass(    Effect effect,
                                string name,
                                VertexShader vertexShader,
                                PixelShader pixelShader,
                                VertexShader[] vertexShaders,
                                int vertexShaderSelector,
                                PixelShader[] pixelShaders,
                                int pixelShaderSelector,
                                BlendState blendState,
                                DepthStencilState depthStencilState,
                                RasterizerState rasterizerState,
                                EffectAnnotationCollection annotations )
            : this(effect, name, vertexShader, pixelShader, blendState, depthStencilState, rasterizerState, annotations)
        {
            _vertexShaders = vertexShaders;
            _vertexShaderSelector = vertexShaderSelector;
            _pixelShaders = pixelShaders;
            _pixelShaderSelector = pixelShaderSelector;
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
            _vertexShaders = cloneSource._vertexShaders;
            _vertexShaderSelector = cloneSource._vertexShaderSelector;
            _pixelShaders = cloneSource._pixelShaders;
            _pixelShaderSelector = cloneSource._pixelShaderSelector;
        }

        public void Apply()
        {
            EffectTechnique currentTechnique = _effect.CurrentTechnique;

            _effect.OnApply();

            if (_effect.CurrentTechnique != currentTechnique)
                throw new InvalidOperationException("CurrentTechnique changed during Effect.OnApply().");

            GraphicsContext context = _effect.GraphicsDevice.CurrentContext;

            VertexShader vertexShader = _vertexShader;
            if (_vertexShaders != null)
                vertexShader = _vertexShaders[_effect.Parameters[_vertexShaderSelector].GetValueInt32()];

            PixelShader pixelShader = _pixelShader;
            if (_pixelShaders != null)
                pixelShader = _pixelShaders[_effect.Parameters[_pixelShaderSelector].GetValueInt32()];

            if (vertexShader != null)
            {
                context.VertexShader = vertexShader;

                // Update the texture parameters.
                SetShaderSamplers(vertexShader, context.VertexTextures, context.VertexSamplerStates);

                // Update the constant buffers.
                for (int c = 0; c < vertexShader.CBuffers.Length; c++)
                {
                    ConstantBuffer constantBuffer = _effect.ConstantBuffers[vertexShader.CBuffers[c]];
                    ((IPlatformConstantBuffer)constantBuffer).Strategy.Update(_effect.Parameters);
                    ((IPlatformGraphicsContext)context).Strategy._vertexConstantBuffers[c] = constantBuffer;
                }
            }

            if (pixelShader != null)
            {
                context.PixelShader = pixelShader;

                // Update the texture parameters.
                SetShaderSamplers(pixelShader, context.Textures, context.SamplerStates);

                // Update the constant buffers.
                for (int c = 0; c < pixelShader.CBuffers.Length; c++)
                {
                    ConstantBuffer constantBuffer = _effect.ConstantBuffers[pixelShader.CBuffers[c]];
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
