// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Platform.Media
{
    [DebuggerDisplay("TrackTime: {TrackTime}, AbsoluteTimecode: {AbsoluteTimecode}")]
    internal struct TrackData
    {
        public readonly byte[] Data;
        public readonly TimeSpan TrackTime;
        public readonly long AbsoluteTimecode;
        public readonly int LoopCount;

        public TrackData(byte[] frameData, TimeSpan trackTime, long absoluteTimecode)
        {
            Data = frameData;
            TrackTime = trackTime;
            AbsoluteTimecode = absoluteTimecode;
            LoopCount = 0;
        }

        public TrackData(byte[] frameData, TimeSpan trackTime, long absoluteTimecode, int loopCount = 0)
        {
            Data = frameData;
            TrackTime = trackTime;
            AbsoluteTimecode = absoluteTimecode;
            LoopCount = loopCount;
        }
    }
}
