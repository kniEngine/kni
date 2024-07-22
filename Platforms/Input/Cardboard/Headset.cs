using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Cardboard
{
    public class Headset
    {
        internal static Android.Views.View View;

        public static HeadsetState GetState()
		{
            HeadsetState state;

            var view = View as AndroidSurfaceView;
            view.UpdateHeadsetState(out state);

            return state;
		}
    }
}
