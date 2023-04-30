// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;


namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static Guid AudioStreamVolumeGuid;

        private Texture2D _lastFrame;

        private readonly Variant _positionCurrent = new Variant();
        private readonly Variant _positionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

        private static Callback _callback;

        private class Callback : IAsyncCallback
        {
            private VideoPlayer _player;

            public Callback(VideoPlayer player)
            {
                _player = player;
            }

            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }
            public void Invoke(AsyncResult asyncResultRef)
            {
                MediaEvent mediaEvent = _session.EndGetEvent(asyncResultRef);

                switch (mediaEvent.TypeInfo)
                {
                    case MediaEventTypes.SessionTopologyStatus:
                        // Trigger an "on Video Ended" event here if needed
                        if (mediaEvent.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                            _player.OnTopologyReady();
                        break;

                    case MediaEventTypes.SessionEnded:
                        break;

                    case MediaEventTypes.SessionStopped:
                        break;
                }

                _session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }


        private void PlatformInitialize()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);

        }

        private Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != _currentVideo.Width || _lastFrame.Height != _currentVideo.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(_currentVideo.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);

            VideoSampleGrabber sampleGrabber = _currentVideo.SampleGrabber;
            byte[] texData = sampleGrabber.TextureData;
            if (texData != null)
                _lastFrame.SetData(texData);
            
            return _lastFrame;
        }

        private void PlatformGetState(ref MediaState result)
        {
            if (_clock != null)
            {
                ClockState state;
                _clock.GetState(0, out state);

                switch (state)
                {
                    case ClockState.Running:
                        result = MediaState.Playing;
                        return;

                    case ClockState.Paused:
                        result = MediaState.Paused;
                        return;
                }
            }

            result = MediaState.Stopped;
        }

        private void PlatformPause()
        {
            _session.Pause();
        }

        private void PlatformPlay()
        {
            // Cleanup the last song first.
            if (State != MediaState.Stopped)
            {
                _session.Stop();
                _session.ClearTopologies();
                _session.Close();
                if (_volumeController != null)
                {
                    _volumeController.Dispose();
                    _volumeController = null;
                }
                _clock.Dispose();
            }

            //create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _callback = new Callback(this);
                _session.BeginGetEvent(_callback, null);
            }

            // Set the new song.
            _session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            _session.Start(null, _positionCurrent);
        }

        private void PlatformResume()
        {
            _session.Start(null, _positionCurrent);
        }

        private void PlatformStop()
        {
            _session.ClearTopologies();
            _session.Stop();
            _session.Close();
            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }
            if (_clock != null)
            {
                _clock.Dispose();
            }
            _clock = null;
        }

        private void SetChannelVolumes()
        {
            if (_volumeController != null && !_volumeController.IsDisposed)
            {
                float volume = _volume;
                if (IsMuted)
                    volume = 0.0f;

                for (int i = 0; i < _volumeController.ChannelCount; i++)
                {
                    _volumeController.SetChannelVolume(i, volume);
                }
            }
        }

        private void PlatformSetVolume()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromTicks(_clock.Time);
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_lastFrame != null)
                    _lastFrame.Dispose();
                _lastFrame = null;
            }

        }

        private void OnTopologyReady()
        {
            if (_session.IsDisposed)
                return;

            // Get the volume interface.
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

            SetChannelVolumes();
        }
    }
}
