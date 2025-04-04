﻿// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public class RasterizerStateStrategy : GraphicsResourceStrategy
        , IRasterizerStateStrategy
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
            : base()
        {
            _cullMode = CullMode.CullCounterClockwiseFace;
            _fillMode = FillMode.Solid;
            _depthBias = 0;
            _multiSampleAntiAlias = true;
            _scissorTestEnable = false;
            _slopeScaleDepthBias = 0;
            _depthClipEnable = true;
        }

        internal RasterizerStateStrategy(GraphicsContextStrategy contextStrategy, IRasterizerStateStrategy source)
            : base(contextStrategy)
        {
            _cullMode = source.CullMode;
            _fillMode = source.FillMode;
            _depthBias = source.DepthBias;
            _multiSampleAntiAlias = source.MultiSampleAntiAlias;
            _scissorTestEnable = source.ScissorTestEnable;
            _slopeScaleDepthBias = source.SlopeScaleDepthBias;
            _depthClipEnable = source.DepthClipEnable;
        }

        internal RasterizerStateStrategy(IRasterizerStateStrategy source)
            : base()
        {
            _cullMode = source.CullMode;
            _fillMode = source.FillMode;
            _depthBias = source.DepthBias;
            _multiSampleAntiAlias = source.MultiSampleAntiAlias;
            _scissorTestEnable = source.ScissorTestEnable;
            _slopeScaleDepthBias = source.SlopeScaleDepthBias;
            _depthClipEnable = source.DepthClipEnable;
        }
    }
}
