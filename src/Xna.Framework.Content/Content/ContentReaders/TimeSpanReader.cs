// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Content
{
    internal class TimeSpanReader : ContentTypeReader<TimeSpan>
    {
        protected internal override TimeSpan Read(ContentReader input, TimeSpan existingInstance)
        {
            return new TimeSpan(input.ReadInt64());
        }
    }
}
