// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState : GraphicsResource
    {
        internal IDepthStencilStateStrategy _strategy;

        public static readonly DepthStencilState Default;
        public static readonly DepthStencilState DepthRead;
        public static readonly DepthStencilState None;

        static DepthStencilState()
        {
            Default = new DepthStencilState("DepthStencilState.Default", true, true);
            DepthRead = new DepthStencilState("DepthStencilState.DepthRead", true, false);
            None = new DepthStencilState("DepthStencilState.None", false, false);
        }

        private readonly bool _isDefaultStateObject;

        private bool _depthBufferEnable;
        private bool _depthBufferWriteEnable;
        private StencilOperation _counterClockwiseStencilDepthBufferFail;
        private StencilOperation _counterClockwiseStencilFail;
        private CompareFunction _counterClockwiseStencilFunction;
        private StencilOperation _counterClockwiseStencilPass;
        private CompareFunction _depthBufferFunction;
        private int _referenceStencil;
        private StencilOperation _stencilDepthBufferFail;
        private bool _stencilEnable;
        private StencilOperation _stencilFail;
        private CompareFunction _stencilFunction;
        private int _stencilMask;
        private StencilOperation _stencilPass;
        private int _stencilWriteMask;
        private bool _twoSidedStencilMode;

        public bool DepthBufferEnable
        {
            get { return _depthBufferEnable; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _depthBufferEnable = value;
            }
        }

        public bool DepthBufferWriteEnable
        {
            get { return _depthBufferWriteEnable; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _depthBufferWriteEnable = value;
            }
        }

        public StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return _counterClockwiseStencilDepthBufferFail; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _counterClockwiseStencilDepthBufferFail = value;
            }
        }

        public StencilOperation CounterClockwiseStencilFail
        {
            get { return _counterClockwiseStencilFail; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _counterClockwiseStencilFail = value;
            }
        }

        public CompareFunction CounterClockwiseStencilFunction
        {
            get { return _counterClockwiseStencilFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _counterClockwiseStencilFunction = value;
            }
        }

        public StencilOperation CounterClockwiseStencilPass
        {
            get { return _counterClockwiseStencilPass; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _counterClockwiseStencilPass = value;
            }
        }

        public CompareFunction DepthBufferFunction
        {
            get { return _depthBufferFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _depthBufferFunction = value;
            }
        }

        public int ReferenceStencil
        {
            get { return _referenceStencil; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _referenceStencil = value;
            }
        }

        public StencilOperation StencilDepthBufferFail
        {
            get { return _stencilDepthBufferFail; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilDepthBufferFail = value;
            }
        }

        public bool StencilEnable
        {
            get { return _stencilEnable; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilEnable = value;
            }
        }

        public StencilOperation StencilFail
        {
            get { return _stencilFail; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilFail = value;
            }
        }

        public CompareFunction StencilFunction
        {
            get { return _stencilFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilFunction = value;
            }
        }

        public int StencilMask
        {
            get { return _stencilMask; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilMask = value;
            }
        }

        public StencilOperation StencilPass
        {
            get { return _stencilPass; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilPass = value;
            }
        }

        public int StencilWriteMask
        {
            get { return _stencilWriteMask; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _stencilWriteMask = value;
            }
        }

        public bool TwoSidedStencilMode
        {
            get { return _twoSidedStencilMode; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default depth stencil state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the depth stencil state after it has been bound to the graphics device!");

                _twoSidedStencilMode = value;
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
                    throw new InvalidOperationException("This depth stencil state is already bound to a different graphics device.");
            }
        }

        public DepthStencilState()
            : base()
        {
            _strategy = new DepthStencilStateStrategy();

            DepthBufferEnable = true;
            DepthBufferWriteEnable = true;
            DepthBufferFunction = CompareFunction.LessEqual;
            StencilEnable = false;
            StencilFunction = CompareFunction.Always;
            StencilPass = StencilOperation.Keep;
            StencilFail = StencilOperation.Keep;
            StencilDepthBufferFail = StencilOperation.Keep;
            TwoSidedStencilMode = false;
            CounterClockwiseStencilFunction = CompareFunction.Always;
            CounterClockwiseStencilFail = StencilOperation.Keep;
            CounterClockwiseStencilPass = StencilOperation.Keep;
            CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
            StencilMask = Int32.MaxValue;
            StencilWriteMask = Int32.MaxValue;
            ReferenceStencil = 0;
        }

        private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
            : this()
        {
            Name = name;
            _depthBufferEnable = depthBufferEnable;
            _depthBufferWriteEnable = depthBufferWriteEnable;
            _isDefaultStateObject = true;
        }

        private DepthStencilState(DepthStencilState cloneSource)
        {
            Name = cloneSource.Name;

            _strategy = new DepthStencilStateStrategy();

            _depthBufferEnable = cloneSource._depthBufferEnable;
            _depthBufferWriteEnable = cloneSource._depthBufferWriteEnable;
            _counterClockwiseStencilDepthBufferFail = cloneSource._counterClockwiseStencilDepthBufferFail;
            _counterClockwiseStencilFail = cloneSource._counterClockwiseStencilFail;
            _counterClockwiseStencilFunction = cloneSource._counterClockwiseStencilFunction;
            _counterClockwiseStencilPass = cloneSource._counterClockwiseStencilPass;
            _depthBufferFunction = cloneSource._depthBufferFunction;
            _referenceStencil = cloneSource._referenceStencil;
            _stencilDepthBufferFail = cloneSource._stencilDepthBufferFail;
            _stencilEnable = cloneSource._stencilEnable;
            _stencilFail = cloneSource._stencilFail;
            _stencilFunction = cloneSource._stencilFunction;
            _stencilMask = cloneSource._stencilMask;
            _stencilPass = cloneSource._stencilPass;
            _stencilWriteMask = cloneSource._stencilWriteMask;
            _twoSidedStencilMode = cloneSource._twoSidedStencilMode;
        }

        internal DepthStencilState Clone()
        {
            return new DepthStencilState(this);
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

