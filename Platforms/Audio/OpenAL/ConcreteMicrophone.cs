// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

#if OPENAL
using Microsoft.Xna.Platform.Audio.OpenAL;
#endif
#if OPENAL && (IOS || TVOS)
using AudioToolbox;
using AudioUnit;
using AVFoundation;
#endif

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.
    /// </summary>
    public sealed class ConcreteMicrophone : MicrophoneStrategy
    {
        private IntPtr _captureDevice = IntPtr.Zero;

        internal AL OpenAL { get { return AL.Current; } }

        private void CheckALCError(string operation)
        {
            AlcError error = OpenAL.ALC.GetErrorForDevice(_captureDevice);
            if (error != AlcError.NoError)
            {
                string msg = String.Format("{0} - OpenAL Error: {1}", operation, error);
                throw new NoMicrophoneConnectedException(msg);
            }
        }

        public override void PlatformStart(string deviceName)
        {
            int sampleSizeInBytes = GetSampleSizeInBytes(BufferDuration) * 2;
            _captureDevice = OpenAL.ALC.CaptureOpenDevice(deviceName, checked((uint)SampleRate), ALFormat.Mono16, sampleSizeInBytes);
            CheckALCError("Failed to open capture device.");

            OpenAL.ALC.CaptureStart(_captureDevice);
            CheckALCError("Failed to start capture.");
        }

        public override void PlatformStop()
        {
            OpenAL.ALC.CaptureStop(_captureDevice);
            CheckALCError("Failed to stop capture.");
            OpenAL.ALC.CaptureCloseDevice(_captureDevice);
            CheckALCError("Failed to close capture device.");
            _captureDevice = IntPtr.Zero;
        }

        private int GetQueuedSampleCount()
        {
            int sampleCount = OpenAL.ALC.GetInteger(_captureDevice, AlcGetInteger.CaptureSamples);
            CheckALCError("Failed to query capture samples.");
            return sampleCount;
        }

        public override bool PlatformIsHeadset()
        {
            throw new NotImplementedException();
        }

        public override bool PlatformUpdate()
        {
            int sampleCount = GetQueuedSampleCount();
            return (sampleCount > 0);
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            int sampleCount = GetQueuedSampleCount();
            sampleCount = Math.Min(count / 2, sampleCount); // 16bit adjust

            if (sampleCount > 0)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    OpenAL.ALC.CaptureSamples(_captureDevice, handle.AddrOfPinnedObject() + offset, sampleCount);
                    CheckALCError("Failed to capture samples.");
                }
                finally
                {
                    handle.Free();
                }

                return sampleCount * 2; // 16bit adjust
            }

            return 0;
        }
    }
}
