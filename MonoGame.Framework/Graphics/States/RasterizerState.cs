// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class RasterizerState : GraphicsResource
	{
        private readonly bool _isDefaultStateObject;

        private CullMode _cullMode;
        private float _depthBias;
        private FillMode _fillMode;
        private bool _multiSampleAntiAlias;
        private bool _scissorTestEnable;
        private float _slopeScaleDepthBias;
        private bool _depthClipEnable;

        public CullMode CullMode
	    {
	        get { return _cullMode; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _cullMode = value;
            }
	    }

	    public float DepthBias
	    {
	        get { return _depthBias; }
	        set
	        {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _depthBias = value;
	        }
	    }

	    public FillMode FillMode
	    {
	        get { return _fillMode; }
	        set
	        {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _fillMode = value;
	        }
	    }

	    public bool MultiSampleAntiAlias
	    {
	        get { return _multiSampleAntiAlias; }
	        set
	        {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _multiSampleAntiAlias = value;
	        }
	    }

	    public bool ScissorTestEnable
	    {
	        get { return _scissorTestEnable; }
	        set
	        {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _scissorTestEnable = value;
	        }
	    }

	    public float SlopeScaleDepthBias
	    {
	        get { return _slopeScaleDepthBias; }
	        set
	        {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _slopeScaleDepthBias = value;
	        }
	    }

        public bool DepthClipEnable
        {
            get { return _depthClipEnable; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _depthClipEnable = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_isDefaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(device != null);
                    BindGraphicsDevice(device.Strategy);
                }
                else
                    throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");
            }
        }

	    public static readonly RasterizerState CullClockwise;
        public static readonly RasterizerState CullCounterClockwise;
        public static readonly RasterizerState CullNone;

        public RasterizerState()
            : base()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
            DepthClipEnable = true;
		}

	    private RasterizerState(string name, CullMode cullMode)
            : this()
	    {
	        Name = name;
	        _cullMode = cullMode;
	        _isDefaultStateObject = true;
	    }

	    private RasterizerState(RasterizerState cloneSource)
	    {
	        Name = cloneSource.Name;
	        _cullMode = cloneSource._cullMode;
	        _fillMode = cloneSource._fillMode;
	        _depthBias = cloneSource._depthBias;
	        _multiSampleAntiAlias = cloneSource._multiSampleAntiAlias;
	        _scissorTestEnable = cloneSource._scissorTestEnable;
	        _slopeScaleDepthBias = cloneSource._slopeScaleDepthBias;
	        _depthClipEnable = cloneSource._depthClipEnable;
	    }

		static RasterizerState()
		{
		    CullClockwise = new RasterizerState("RasterizerState.CullClockwise", CullMode.CullClockwiseFace);
		    CullCounterClockwise = new RasterizerState("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);
		    CullNone = new RasterizerState("RasterizerState.CullNone", CullMode.None);
		}

	    internal RasterizerState Clone()
	    {
	        return new RasterizerState(this);
	    }

        partial void PlatformDispose(bool disposing);

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            PlatformDispose(disposing);
            base.Dispose(disposing);
        }
    }
}
