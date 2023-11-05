// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class RasterizerStateStrategy : IRasterizerStateStrategy
    {
        private CullMode _cullMode;
        private float _depthBias;
        private FillMode _fillMode;
        private bool _multiSampleAntiAlias;
        private bool _scissorTestEnable;
        private float _slopeScaleDepthBias;
        private bool _depthClipEnable;

        public virtual CullMode CullMode
        {
            get { return _cullMode; }
            set { _cullMode = value; }
        }

        public virtual float DepthBias
        {
            get { return _depthBias; }
            set { _depthBias = value; }
        }

        public virtual FillMode FillMode
        {
            get { return _fillMode; }
            set { _fillMode = value; }
        }

        public virtual bool MultiSampleAntiAlias
        {
            get { return _multiSampleAntiAlias; }
            set { _multiSampleAntiAlias = value; }
        }

        public virtual bool ScissorTestEnable
        {
            get { return _scissorTestEnable; }
            set { _scissorTestEnable = value; }
        }

        public virtual float SlopeScaleDepthBias
        {
            get { return _slopeScaleDepthBias; }
            set { _slopeScaleDepthBias = value; }
        }

        public virtual bool DepthClipEnable
        {
            get { return _depthClipEnable; }
            set { _depthClipEnable = value; }
        }

        public RasterizerStateStrategy()
        {
        }
    }
}
