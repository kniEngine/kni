// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Platform.Media
{
    internal struct TrackData
    {
        public readonly byte[] Data;
        public readonly TimeSpan TrackTime;
        public readonly long AbsoluteTimecode;

        public TrackData(byte[] frameData, TimeSpan trackTime, long absoluteTimecode)
        {
            Data = frameData;
            TrackTime = trackTime;
            AbsoluteTimecode = absoluteTimecode;
        }
    }
}
