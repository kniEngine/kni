// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using DX = SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private MediaSession _session;
        private AudioStreamVolume _volumeController;
        private PresentationClock _clock;

        private Texture2D _lastFrame;

        private readonly Variant _positionCurrent = new Variant();
        private readonly Variant _positionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

        private Callback _callback;

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
                using (MediaEvent mediaEvent = _player._session.EndGetEvent(asyncResultRef))
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

                _player._session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
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

        public override TimeSpan PlayPosition
        {
            get { return TimeSpan.FromTicks(_clock.Time); }
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

        internal ConcreteVideoPlayerStrategy()
        {
            MediaManager.Startup(true);
            MediaFoundation.MediaFactory.CreateMediaSession(null, out _session);
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

            VideoPlatformStream _videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            byte[] texData = _videoPlatformStream.SampleGrabber.TextureData;
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

            // Set the new video.
            VideoPlatformStream _videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            _session.SetTopology(SessionSetTopologyFlags.Immediate, _videoPlatformStream.Topology);

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

        private void OnTopologyReady()
        {
            if (_session.IsDisposed)
                return;

            // HACK: Need SharpDX to fix this.
            // The GUID is specified in a GuidAttribute attached to the class
            Type audioStreamVolumeType = typeof(AudioStreamVolume);
            GuidAttribute audioStreamVolumeGuidAttrib = (GuidAttribute)audioStreamVolumeType.GetCustomAttributes(typeof(GuidAttribute), false)[0];
            Guid audioStreamVolumeGuid = Guid.Parse(audioStreamVolumeGuidAttrib.Value); 

            // Get the volume interface.
            IntPtr volumeObjectPtr;
            MediaFoundation.MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, audioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = DX.CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

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


    internal sealed class VideoPlatformStream : IDisposable
    {
        private Topology _topology;
        private VideoSampleGrabber _sampleGrabber;
        MediaType _mediaType;

        internal Topology Topology { get { return _topology; } }
        internal VideoSampleGrabber SampleGrabber { get { return _sampleGrabber; } }

        internal VideoPlatformStream(string filename)
        {
            MediaManager.Startup(true);

            MediaFoundation.MediaFactory.CreateTopology(out _topology);

            MediaFoundation.MediaSource mediaSource;
            {
                SourceResolver resolver = new SourceResolver();

                ObjectType otype;
                DX.ComObject source = resolver.CreateObjectFromURL(filename, SourceResolverFlags.MediaSource, null, out otype);
                mediaSource = source.QueryInterface<MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (int i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                SharpDX.Mathematics.Interop.RawBool selected;
                StreamDescriptor desc;
                presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    TopologyNode sourceNode;
                    MediaFoundation.MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFoundation.MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                    Guid majorType = desc.MediaTypeHandler.MajorType;
                    if (majorType == MediaTypeGuids.Video)
                    {
                        _mediaType = new MediaType();
                        _mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                        // Specify that we want the data to come in as RGB32.
                        _mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

                        _sampleGrabber = new VideoSampleGrabber();
                        Activate activate;
                        MediaFoundation.MediaFactory.CreateSampleGrabberSinkActivate(_mediaType, _sampleGrabber, out activate);
                        outputNode.Object = activate;
                    }

                    if (majorType == MediaTypeGuids.Audio)
                    {
                        Activate activate;
                        MediaFoundation.MediaFactory.CreateAudioRendererActivate(out activate);
                        outputNode.Object = activate;
                    }

                    _topology.AddNode(sourceNode);
                    _topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();
            mediaSource.Dispose();
        }


        #region IDisposable
        ~VideoPlatformStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_topology != null)
                {
                    _topology.Dispose();
                    _topology = null;
                }

                if (_sampleGrabber != null)
                {
                    _sampleGrabber.Dispose();
                    _sampleGrabber = null;
                }

            }

            MediaManager.Shutdown();

            //base.Dispose(disposing);
        }
        #endregion
    }
}
