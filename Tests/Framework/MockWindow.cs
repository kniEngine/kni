using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Framework
{
    internal class MockWindow : GameWindow
    {
        public override bool AllowUserResizing { get; set; }

        public override Rectangle ClientBounds
        {
            get { return new Rectangle(100, 10, 800, 480); }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { throw new NotImplementedException(); }
        }

        public override IntPtr Handle
        {
            get { return new IntPtr(this.GetHashCode()); }
        }

        public override string ScreenDeviceName
        {
            get { throw new NotImplementedException(); }
        }

        protected override void SetTitle(string title)
        {
            throw new NotImplementedException();
        }
    }
}
