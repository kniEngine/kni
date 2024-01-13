// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
        , IAsyncCallback
    {
        private MediaSession _session;
        private AudioStreamVolume _volumeController;
        private readonly object _volumeLock = new object();
        private PresentationClock _clock;

        private Song _nextSong;
        private Song _currentSong;

        private enum SessionState { Stopped, Stopping, Started, Paused, Ended }
        private SessionState _sessionState = SessionState.Stopped;

        private Guid _audioStreamVolumeGuid;

        private readonly Variant _positionCurrent = new Variant();
        private readonly Variant _positionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };


        internal ConcreteMediaPlayerStrategy()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            _audioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManager.Startup(true);
            MediaFoundation.MediaFactory.CreateMediaSession(null, out _session);

            _session.BeginGetEvent(this, null);

            _clock = _session.Clock.QueryInterface<PresentationClock>();
        }

        #region IAsyncCallback

        protected override void Dispose(bool disposing)
        {

            MediaManager.Shutdown();

            base.Dispose(disposing);
        }

        public IDisposable Shadow { get; set; }

        public void Invoke(AsyncResult asyncResultRef)
        {
            using (MediaEvent mediaEvent = _session.EndGetEvent(asyncResultRef))
            {
                switch (mediaEvent.TypeInfo)
                {
                    case MediaEventTypes.SessionTopologyStatus:
                        if (mediaEvent.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                            OnTopologyReady();
                        break;

                    case MediaEventTypes.SessionEnded:
                        _sessionState = SessionState.Ended;
                        base.OnSongFinishedPlaying();
                        break;

                    case MediaEventTypes.SessionStopped:
                        OnSessionStopped();
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

        #endregion IAsyncCallback

        #region Properties

        internal override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                SetChannelVolumes();
            }
        }

        internal override TimeSpan PlatformPlayPosition
        {
            get
            {
                SessionState sessionState = _sessionState;
                switch (sessionState)
                {
                    case SessionState.Started:
                        return TimeSpan.Zero;

                    case SessionState.Paused:
                        return TimeSpan.Zero;

                    case SessionState.Stopping:
                    case SessionState.Stopped:
                        {
                            try
                            {
                                return TimeSpan.FromTicks(_clock.Time);
                            }
                            catch (SharpDXException)
                            {
                                // The presentation clock is most likely not quite ready yet
                                return TimeSpan.Zero;
                            }
                        }

                    case SessionState.Ended:
                        return TimeSpan.Zero;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        internal override bool PlatformGameHasControl
        {
            get { return true; }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override float PlatformVolume
        {
            set
            {
                base.PlatformVolume = value;
                SetChannelVolumes();
            }
        }

        #endregion

        private void SetChannelVolumes()
        {
            lock (_volumeLock)
            {
                if (_volumeController == null)
                    return;

                float volume = base.PlatformIsMuted ? 0f : base.PlatformVolume;
                for (int i = 0; i < _volumeController.ChannelCount; i++)
                {
                    _volumeController.SetChannelVolume(i, volume);
                }
            }
        }

        internal override void PlatformPlaySong(Song song)
        {
            SessionState sessionState = _sessionState;
            switch (sessionState)
            {
                case SessionState.Started:
                    {
                        if (_currentSong == song)
                        {
                            _sessionState = SessionState.Started;
                            _session.Start(null, _positionBeginning);
                        }
                        else
                        {
                            // The new song will be started after the SessionStopped event is received
                            _nextSong = song;
                            // The session needs to be stopped to reset the play position
                            _sessionState = SessionState.Stopping;
                            _session.Stop();
                        }
                    }
                    break;

                case SessionState.Paused:
                    {
                        if (_currentSong == song)
                        {
                            _sessionState = SessionState.Started;
                            _session.Start(null, _positionBeginning);
                        }
                        else
                        {
                            // The new song will be started after the SessionStopped event is received
                            _nextSong = song;
                            // The session needs to be stopped to reset the play position
                            _sessionState = SessionState.Stopping;
                            _session.Stop();
                        }
                    }
                    break;

                case SessionState.Stopping:
                    {
                        // The song will be started after the SessionStopped event is received
                        _nextSong = song;
                    }
                    break;

                case SessionState.Stopped:
                    {
                        if (_currentSong != song)
                            StartNewSong(song);
                        _sessionState = SessionState.Started;
                        _session.Start(null, _positionBeginning);
                    }
                    break;

                case SessionState.Ended:
                    {
                        if (_currentSong == song)
                        {
                            _sessionState = SessionState.Started;
                            _session.Start(null, _positionBeginning);
                        }
                        else
                        {
                            // The new song will be started after the SessionStopped event is received
                            _nextSong = song;
                            // The session needs to be stopped to reset the play position
                            _sessionState = SessionState.Stopping;
                            // The play position needs to be reset before stopping otherwise the next song may not start playing
                            _session.Start(null, _positionBeginning);
                            _session.Stop();
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal override void PlatformPause()
        {
            SessionState sessionState = _sessionState;
            switch (sessionState)
            {
                case SessionState.Started:
                    {
                        _sessionState = SessionState.Paused;
                        _session.Pause();
                    }
                    break;

                case SessionState.Paused:
                    break;

                case SessionState.Stopping:
                    break;

                case SessionState.Stopped:
                    break;

                case SessionState.Ended:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal override void PlatformResume()
        {
            SessionState sessionState = _sessionState;
            switch (sessionState)
            {
                case SessionState.Started:
                    break;

                case SessionState.Paused:
                    {
                        _sessionState = SessionState.Started;
                        _session.Start(null, _positionCurrent);
                    }
                    break;

                case SessionState.Stopping:
                    break;

                case SessionState.Stopped:
                    break;

                case SessionState.Ended:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal override void PlatformStop()
        {
            SessionState sessionState = _sessionState;
            switch (sessionState)
            {
                case SessionState.Started:
                    {
                        _sessionState = SessionState.Stopping;
                        _session.Stop();
                    }
                    break;

                case SessionState.Paused:
                    {
                        _sessionState = SessionState.Stopping;
                        _session.Stop();
                    }
                    break;

                case SessionState.Stopping:
                    {
                    }
                    break;

                case SessionState.Stopped:
                    {
                    }
                    break;

                case SessionState.Ended:
                    {
                        _sessionState = SessionState.Stopping;
                        // The play position needs to be reset before stopping otherwise the next song may not start playing
                        _session.Start(null, _positionBeginning);
                        _session.Stop();
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void StartNewSong(Song song)
        {
            lock (_volumeLock)
            {
                if (_volumeController != null)
                {
                    _volumeController.Dispose();
                    _volumeController = null;
                }
            }

            _currentSong = song;
            MediaPlatformStream mediaPlatformStream = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().GetMediaPlatformStream();
           _session.SetTopology(SessionSetTopologyFlags.Immediate, mediaPlatformStream.Topology);

            // The volume service won't be available until the session topology
            // is ready, so we now need to wait for the event indicating this
        }

        private void OnTopologyReady()
        {
            lock (_volumeLock)
            {
                IntPtr volumeObjectPtr;
                MediaFoundation.MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, _audioStreamVolumeGuid, out volumeObjectPtr);
                _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);
            }

            SetChannelVolumes();
        }

        private void OnSessionStopped()
        {
            _sessionState = SessionState.Stopped;
            if (_nextSong != null)
            {
                if (_nextSong != _currentSong)
                    StartNewSong(_nextSong);
                _sessionState = SessionState.Started;
                _session.Start(null, _positionBeginning);

                _nextSong = null;
            }
        }
    }

    internal sealed class MediaPlatformStream : IDisposable
    {
        private Topology _topology;

        internal Topology Topology { get { return _topology; } }

        internal MediaPlatformStream(Uri streamSource)
        {
            MediaManager.Startup(true);
            this._topology = CreateTopology(streamSource);
        }

        private Topology CreateTopology(Uri streamSource)
        {
            Topology topology;
            MediaFoundation.MediaFactory.CreateTopology(out topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                string filename = streamSource.OriginalString;

                SourceResolver resolver = new SourceResolver();
                ComObject source = resolver.CreateObjectFromURL(filename, SourceResolverFlags.MediaSource);
                mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
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

                    var typeHandler = desc.MediaTypeHandler;
                    var majorType = typeHandler.MajorType;
                    if (majorType != MediaTypeGuids.Audio)
                        throw new NotSupportedException("The song contains video data!");

                    Activate activate;
                    MediaFoundation.MediaFactory.CreateAudioRendererActivate(out activate);
                    outputNode.Object = activate;

                    topology.AddNode(sourceNode);
                    topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                    typeHandler.Dispose();
                    activate.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();
            mediaSource.Dispose();

            return topology;
        }


        #region IDisposable
        ~MediaPlatformStream()
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
            }

            MediaManager.Shutdown();

            //base.Dispose(disposing);

        }
        #endregion
    }
}

