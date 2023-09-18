// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The default effect used by SpriteBatch.
    /// </summary>
    public class SpriteEffect : Effect
    {
        private EffectParameter _matrixParam;
        private Viewport _lastViewport;
        private Matrix _projection;

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, device.Strategy.GetResourceStreamBytes(device.Strategy.ResourceNameSpriteEffect))
        {
            CacheEffectParameters();

            // initialize static values of _projection.
            _projection = Matrix.CreateTranslation(-1f, 1f, 0);
        }

        /// <summary>
        /// An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.
        /// </summary>
        public Matrix? TransformMatrix { get; set; }

        /// <summary>
        /// Creates a new SpriteEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected SpriteEffect(SpriteEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();

            // initialize static values of _projection.
            _projection = Matrix.CreateTranslation(-1f, 1f, 0);
        }


        /// <summary>
        /// Creates a clone of the current SpriteEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new SpriteEffect(this);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            _matrixParam = Parameters["MatrixTransform"];
        }

        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            var vp = GraphicsDevice.Viewport;
            if ((vp.Width != _lastViewport.Width) || (vp.Height != _lastViewport.Height))
            {
                // Normal 3D cameras look into the -z direction (z = 1 is in front of z = 0). The
                // sprite batch layer depth is the opposite (z = 0 is in front of z = 1).
                // --> We get the correct matrix with near plane 0 and far plane -1.
                //Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out _projection);
                // static values of _projection were set in the constuctor as Matrix.CreateTranslation(-1f, 1f, 0). 
                // we calculate only M11,M22 and M41,M42

                if (GraphicsDevice.UseHalfPixelOffset)
                {
                    var texelW = (1f / vp.Width);
                    var texelH = (1f / vp.Height);
                    _projection.M11 =  (2f * texelW);
                    _projection.M22 = -(2f * texelH);
                    _projection.M41 = -(1f + texelW);
                    _projection.M42 =  (1f + texelH);
                }
                else
                {
                    _projection.M11 =  (2f / vp.Width);
                    _projection.M22 = -(2f / vp.Height);
                }

                _lastViewport = vp;
            }

            if (TransformMatrix.HasValue)
            {
                // OPT: Matrix transform = TransformMatrix.Value * _projection;
                Matrix transform = TransformMatrix.Value; // copy M13,M14,M23,M24,M33,M34,M43,M44.
                // calculate M11,M12,M21,M22,M31,M32,M41,M42.
                if (GraphicsDevice.UseHalfPixelOffset)
                {
                    transform.M11 = ((transform.M11 * _projection.M11) + (transform.M14 * _projection.M41));
                    transform.M12 = ((transform.M12 * _projection.M22) + (transform.M14 * _projection.M42));
                    transform.M21 = ((transform.M21 * _projection.M11) + (transform.M24 * _projection.M41));
                    transform.M22 = ((transform.M22 * _projection.M22) + (transform.M24 * _projection.M42));
                    transform.M31 = ((transform.M31 * _projection.M11) + (transform.M34 * _projection.M41));
                    transform.M32 = ((transform.M32 * _projection.M22) + (transform.M34 * _projection.M42));
                    transform.M41 = ((transform.M41 * _projection.M11) + (transform.M44 * _projection.M41));
                    transform.M42 = ((transform.M42 * _projection.M22) + (transform.M44 * _projection.M42));
                }
                else
                {
                    transform.M11 = ((transform.M11 * _projection.M11) - transform.M14);
                    transform.M12 = ((transform.M12 * _projection.M22) + transform.M14);
                    transform.M21 = ((transform.M21 * _projection.M11) - transform.M24);
                    transform.M22 = ((transform.M22 * _projection.M22) + transform.M24);
                    transform.M31 = ((transform.M31 * _projection.M11) - transform.M34);
                    transform.M32 = ((transform.M32 * _projection.M22) + transform.M34);
                    transform.M41 = ((transform.M41 * _projection.M11) - transform.M44);
                    transform.M42 = ((transform.M42 * _projection.M22) + transform.M44);
                }
                _matrixParam.SetValue(transform);
            }
            else
                _matrixParam.SetValue(_projection);
        }
    }
}
