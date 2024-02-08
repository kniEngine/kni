// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public class PresentationEventArgs : EventArgs
    {
        public PresentationParameters PresentationParameters { get; private set; }

        internal PresentationEventArgs(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
        }
    }
}
