// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class DirectionalLight
    {
        internal EffectParameter _diffuseColorParameter;
        internal EffectParameter _directionParameter;
        internal EffectParameter _specularColorParameter;

        Vector3 _diffuseColor;
        Vector3 _direction;
        Vector3 _specularColor;
        bool _enabled;

        public DirectionalLight(EffectParameter directionParameter, EffectParameter diffuseColorParameter, EffectParameter specularColorParameter, DirectionalLight cloneSource)
        {
            this._diffuseColorParameter = diffuseColorParameter;
            this._directionParameter = directionParameter;
            this._specularColorParameter = specularColorParameter;

            if (cloneSource != null)
            {
                this._diffuseColor = cloneSource._diffuseColor;
                this._direction = cloneSource._direction;
                this._specularColor = cloneSource._specularColor;
                this._enabled = cloneSource._enabled;
            }
            else
            {
                this._diffuseColorParameter = diffuseColorParameter;
                this._directionParameter = directionParameter;
                this._specularColorParameter = specularColorParameter;
            }
        }

        public Vector3 DiffuseColor
        {
            get { return _diffuseColor; }
            set
            {
                _diffuseColor = value;
                if (this._enabled && this._diffuseColorParameter != null)
                    _diffuseColorParameter.SetValue(_diffuseColor);
            }
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                if (this._directionParameter != null)
                    _directionParameter.SetValue(_direction);
            }
        }

        public Vector3 SpecularColor
        {
            get { return _specularColor; }
            set
            {
                _specularColor = value;
                if (this._enabled && this._specularColorParameter != null)
                    _specularColorParameter.SetValue(_specularColor);
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;

                    if (this._enabled)
                    {
                        if (this._diffuseColorParameter != null)
                        {
                            this._diffuseColorParameter.SetValue(this._diffuseColor);
                        }
                        if (this._specularColorParameter != null)
                        {
                            this._specularColorParameter.SetValue(this._specularColor);
                        }
                    }
                    else
                    {
                        if (this._diffuseColorParameter != null)
                        {
                            this._diffuseColorParameter.SetValue(Vector3.Zero);
                        }
                        if (this._specularColorParameter != null)
                        {
                            this._specularColorParameter.SetValue(Vector3.Zero);
                        }
                    }
                }

            }
        }
    }
}

