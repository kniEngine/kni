// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;
using Microsoft.Xna.Platform.Utilities;

using Microsoft.Xna.Framework;
using System.Globalization;
using Android.Content.PM;
using Android.Content;
using Android.Media;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioServiceDroid: ConcreteAudioService
    {
    
        internal ConcreteAudioServiceDroid() : base()
        {
        }

        public override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
#if !XAMARIN
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                Context appContext = Android.App.Application.Context;

                AudioManager audioManager = appContext.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    AudioDeviceInfo[] devices = audioManager.GetDevices(GetDevicesTargets.Inputs);
                    foreach (var device in devices)
                    {
                        if (device.Type == AudioDeviceType.BuiltinMic
                        ||  device.Type == AudioDeviceType.WiredHeadset
                        ||  device.Type == AudioDeviceType.UsbDevice
                        ||  device.Type == AudioDeviceType.BluetoothSco
                        )
                        {
                            string deviceIdentifier = device.ProductName;
                            if (!String.IsNullOrWhiteSpace(device.Address))
                                deviceIdentifier = deviceIdentifier + "_" + device.Address;
                            Microphone microphone = base.CreateMicrophone(deviceIdentifier);
                            microphones.Add(microphone);
                        }
                    }
                }

                // set the default Microphone
                if (microphones.Count > 0)
                {
                    defaultMicrophone = base.CreateMicrophone("Default");
                    microphones.Insert(0, defaultMicrophone);
                }
                return;
            }            
#endif

            // falback to OpenAL Mic
            base.PlatformPopulateCaptureDevices(microphones, ref defaultMicrophone);
        }

        public override int PlatformGetMaxPlayingInstances()
        {
            return 32;
        }

        public override void Suspend()
        {
            // Pause all currently playing sounds by pausing the mixer
            OpenAL.ALC.DevicePause(base.ALDevice);
        }

        public override void Resume()
        {
            // Resume all sounds that were playing when the activity was paused
            OpenAL.ALC.DeviceResume(base.ALDevice);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}

