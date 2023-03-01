using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Framework
{
    internal class MockWindow : GameWindow
    {
        public override bool AllowUserResizing { get; set; }

        public override Rectangle ClientBounds
        {
            get { throw new NotImplementedException(); }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { throw new NotImplementedException(); }
        }

        public override IntPtr Handle
        {
            get { throw new NotImplementedException(); }
        }

        public override string ScreenDeviceName
        {
            get { throw new NotImplementedException(); }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            throw new NotImplementedException();
        }

        protected override void SetTitle(string title)
        {
            throw new NotImplementedException();
        }
    }
}
