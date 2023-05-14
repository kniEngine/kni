// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
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
            private ConcreteVideoPlayerStrategy _player;

            public Callback(ConcreteVideoPlayerStrategy player)
            {
                _player = player;
            }

            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }
            public void Invoke(AsyncResult asyncResultRef)
            {
                using (MediaEvent mediaEvent = _session.EndGetEvent(asyncResultRef))
                {
                    switch (mediaEvent.TypeInfo)
                    {
                        case MediaEventTypes.SessionTopologyStatus:
                            // Trigger an "on Video Ended" event here if needed
                            if (mediaEvent.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                                _player.OnTopologyReady();
                            break;

                        case MediaEventTypes.SessionEnded:
                            _player.OnMediaEngineEvent(mediaEvent);
                            break;

                        case MediaEventTypes.SessionStopped:
                            _player.OnMediaEngineEvent(mediaEvent);
                            break;
                    }

                    IDisposable evValue = mediaEvent.Value.Value as IDisposable;
                    if (evValue != null)
                        evValue.Dispose();
                }

                _session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                if (_volumeController != null)
                    SetChannelVolumes();
            }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    if (_volumeController != null)
                        SetChannelVolumes();
            }
        }

        public ConcreteVideoPlayerStrategy()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManager.Startup(true);
            MediaFactory.CreateMediaSession(null, out _session);
        }


        private void OnMediaEngineEvent(MediaEvent mediaEvent)
        {
            switch (mediaEvent.TypeInfo)
            {
                case MediaEventTypes.SessionTopologyStatus:
                    break;

                case MediaEventTypes.SessionEnded:
                    if (IsLooped)
                    {
                        _session.Start(null, _positionBeginning);
                        return;
                    }
                    break;

                case MediaEventTypes.SessionStopped:
                    break;
            }
        }

        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != base.Video.Width || _lastFrame.Height != base.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(base.Video.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Bgr32);


            VideoSampleGrabber sampleGrabber = ((ConcreteVideoStrategy)base.Video.Strategy).SampleGrabber;
            byte[] texData = sampleGrabber.TextureData;
            if (texData != null)
                _lastFrame.SetData(texData);
            
            return _lastFrame;
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            if (_clock != null)
            {
                ClockState clockState;
                _clock.GetState(0, out clockState);

                switch (clockState)
                {
                    case ClockState.Running:
                        state = MediaState.Playing;
                        return;

                    case ClockState.Paused:
                        state = MediaState.Paused;
                        return;
                }
            }

            state = MediaState.Stopped;
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

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
            _session.SetTopology(SessionSetTopologyFlags.Immediate, ((ConcreteVideoStrategy)base.Video.Strategy).Topology);

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            _session.Start(null, _positionCurrent);

            State = MediaState.Playing;


            // XNA doesn't return until the video is playing
            const int timeOutMs = 500;
            Stopwatch timer = Stopwatch.StartNew();
            while (State != MediaState.Playing)
            {
                Thread.Sleep(0);
                if (timer.ElapsedMilliseconds > timeOutMs)
                {
                    timer.Stop();

                    // attempt to stop to fix any bad state
                    if (base.Video != null)
                        PlatformStop();

                    throw new InvalidOperationException("cannot start video");
                }
            }
            timer.Stop();

        }

        public override void PlatformPause()
        {
            _session.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            _session.Start(null, _positionCurrent);
            State = MediaState.Playing;
        }

        public override void PlatformStop()
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

            State = MediaState.Stopped;
        }

        private void SetChannelVolumes()
        {
            if (_volumeController != null && !_volumeController.IsDisposed)
            {
                float volume = base.Volume;
                if (IsMuted)
                    volume = 0.0f;

                for (int i = 0; i < _volumeController.ChannelCount; i++)
                {
                    _volumeController.SetChannelVolume(i, volume);
                }
            }
        }

        public override TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromTicks(_clock.Time);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lastFrame != null)
                    _lastFrame.Dispose();
                _lastFrame = null;
            }

            MediaManager.Shutdown();

            base.Dispose(disposing);
        }
    }
}
