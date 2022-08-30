// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

#if OPENAL
using Microsoft.Xna.Platform.Audio.OpenAL;
#if (IOS || TVOS) || MONOMAC
using AudioToolbox;
using AudioUnit;
using AVFoundation;
#endif
#endif

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.  
    /// </summary>
    public sealed class ConcreteMicrophone : MicrophoneStrategy
    {
        private IntPtr _captureDevice = IntPtr.Zero;

        private void CheckALCError(string operation)
        {
            AlcError error = Alc.GetErrorForDevice(_captureDevice);
            if (error != AlcError.NoError)
            {
                var msg = String.Format("{0} - OpenAL Error: {1}", operation, error);
                throw new NoMicrophoneConnectedException(msg);
            }
        }

        internal override void PlatformStart(string deviceName, int sampleRate, int sampleSizeInBytes)
        {
            _captureDevice = Alc.CaptureOpenDevice(deviceName, checked((uint)sampleRate), ALFormat.Mono16, sampleSizeInBytes);
            CheckALCError("Failed to open capture device.");
          
            Alc.CaptureStart(_captureDevice);
            CheckALCError("Failed to start capture.");
        }

        internal override void PlatformStop()
        {
            Alc.CaptureStop(_captureDevice);
            CheckALCError("Failed to stop capture.");
            Alc.CaptureCloseDevice(_captureDevice);
            CheckALCError("Failed to close capture device.");
            _captureDevice = IntPtr.Zero;
        }

        private int GetQueuedSampleCount()
        {
            int sampleCount = Alc.GetInteger(_captureDevice, AlcGetInteger.CaptureSamples);
            CheckALCError("Failed to query capture samples.");
            return sampleCount;
        }

        internal override bool PlatformIsHeadset()
        {
            throw new NotImplementedException();
        }

        internal override bool PlatformUpdate()
        {
            int sampleCount = GetQueuedSampleCount();
            return (sampleCount > 0);
        }

        internal override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            int sampleCount = GetQueuedSampleCount();
            sampleCount = Math.Min(count / 2, sampleCount); // 16bit adjust

            if (sampleCount > 0)
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    Alc.CaptureSamples(_captureDevice, handle.AddrOfPinnedObject() + offset, sampleCount);
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
