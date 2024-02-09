// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class DirectionalLight
    {
        EffectParameter _directionParameter;
        EffectParameter _diffuseColorParameter;
        EffectParameter _specularColorParameter;

        bool _enabled;
        Vector3 _direction;
        Vector3 _diffuseColor;
        Vector3 _specularColor;

        internal event EventHandler EnabledChanged;

        public DirectionalLight(EffectParameter directionParameter, EffectParameter diffuseColorParameter, EffectParameter specularColorParameter, DirectionalLight cloneSource)
        {
            this._directionParameter = directionParameter;
            this._diffuseColorParameter = diffuseColorParameter;
            this._specularColorParameter = specularColorParameter;

            if (cloneSource != null)
            {;
                this._enabled = cloneSource._enabled;
                this._direction = cloneSource._direction;
                this._diffuseColor = cloneSource._diffuseColor;
                this._specularColor = cloneSource._specularColor;
            }
            else
            {
                this._directionParameter = directionParameter;
                this._diffuseColorParameter = diffuseColorParameter;
                this._specularColorParameter = specularColorParameter;
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

                    if (this._diffuseColorParameter != null)
                        this._diffuseColorParameter.SetValue(this._enabled ? this._diffuseColor : Vector3.Zero);
                    if (this._specularColorParameter != null)
                        this._specularColorParameter.SetValue(this._enabled ? this._specularColor : Vector3.Zero);

                    OnEnabledChanged(EventArgs.Empty);
                }
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

        public Vector3 DiffuseColor
        {
            get { return _diffuseColor; }
            set
            {
                _diffuseColor = value;

                if (this._diffuseColorParameter != null && this._enabled)
                    _diffuseColorParameter.SetValue(_diffuseColor);
            }
        }

        public Vector3 SpecularColor
        {
            get { return _specularColor; }
            set
            {
                _specularColor = value;

                if (this._specularColorParameter != null && this._enabled)
                    _specularColorParameter.SetValue(_specularColor);
            }
        }

        private void OnEnabledChanged(EventArgs args)
        {
            var handler = EnabledChanged;
            if (handler != null)
                handler(this, args);
        }
    }
}
