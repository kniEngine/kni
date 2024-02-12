// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Media;
using SharpDX.Win32;
using DX = SharpDX;
using DXRaw = SharpDX.Mathematics.Interop;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
        , MediaFoundation.IAsyncCallback
    {
        private MediaFoundation.MediaSession _session;
        private MediaFoundation.AudioStreamVolume _volumeController;
        private readonly object _volumeLock = new object();
        private MediaFoundation.PresentationClock _clock;

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
            _audioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(MediaFoundation.AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaFoundation.MediaManager.Startup(true);
            MediaFoundation.MediaFactory.CreateMediaSession(null, out _session);

            _session.BeginGetEvent(this, null);

            _clock = _session.Clock.QueryInterface<MediaFoundation.PresentationClock>();
        }

        #region IAsyncCallback

        protected override void Dispose(bool disposing)
        {

            MediaFoundation.MediaManager.Shutdown();

            base.Dispose(disposing);
        }

        public IDisposable Shadow { get; set; }

        public void Invoke(MediaFoundation.AsyncResult asyncResultRef)
        {
            using (MediaFoundation.MediaEvent mediaEvent = _session.EndGetEvent(asyncResultRef))
            {
                switch (mediaEvent.TypeInfo)
                {
                    case MediaFoundation.MediaEventTypes.SessionTopologyStatus:
                        if (mediaEvent.Get(MediaFoundation.EventAttributeKeys.TopologyStatus) == MediaFoundation.TopologyStatus.Ready)
                            OnTopologyReady();
                        break;

                    case MediaFoundation.MediaEventTypes.SessionEnded:
                        _sessionState = SessionState.Ended;
                        base.OnSongFinishedPlaying();
                        break;

                    case MediaFoundation.MediaEventTypes.SessionStopped:
                        OnSessionStopped();
                        break;
                }

                IDisposable evValue = mediaEvent.Value.Value as IDisposable;
                if (evValue != null)
                    evValue.Dispose();
            }

            _session.BeginGetEvent(this, null);
        }

        public MediaFoundation.AsyncCallbackFlags Flags { get; private set; }
        public MediaFoundation.WorkQueueId WorkQueueId { get; private set; }

        #endregion IAsyncCallback

        #region Properties

        public override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                SetChannelVolumes();
            }
        }

        public override TimeSpan PlatformPlayPosition
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
                            catch (DX.SharpDXException)
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

        public override bool PlatformGameHasControl
        {
            get { return true; }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        public override float PlatformVolume
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

        public override void PlatformPlaySong(Song song)
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

        public override void PlatformPause()
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

        public override void PlatformResume()
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

        public override void PlatformStop()
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
           _session.SetTopology(MediaFoundation.SessionSetTopologyFlags.Immediate, mediaPlatformStream.Topology);

            // The volume service won't be available until the session topology
            // is ready, so we now need to wait for the event indicating this
        }

        private void OnTopologyReady()
        {
            lock (_volumeLock)
            {
                IntPtr volumeObjectPtr;
                MediaFoundation.MediaFactory.GetService(_session, MediaFoundation.MediaServiceKeys.StreamVolume, _audioStreamVolumeGuid, out volumeObjectPtr);
                _volumeController = DX.CppObject.FromPointer<MediaFoundation.AudioStreamVolume>(volumeObjectPtr);
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
        private MediaFoundation.Topology _topology;

        internal MediaFoundation.Topology Topology { get { return _topology; } }

        internal MediaPlatformStream(Uri streamSource)
        {
            MediaFoundation.MediaManager.Startup(true);
            this._topology = CreateTopology(streamSource);
        }

        private MediaFoundation.Topology CreateTopology(Uri streamSource)
        {
            MediaFoundation.Topology topology;
            MediaFoundation.MediaFactory.CreateTopology(out topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                string filename = streamSource.OriginalString;

                MediaFoundation.SourceResolver resolver = new MediaFoundation.SourceResolver();
                DX.ComObject source = resolver.CreateObjectFromURL(filename, MediaFoundation.SourceResolverFlags.MediaSource);
                mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            MediaFoundation.PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (int i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                DXRaw.RawBool selected;
                MediaFoundation.StreamDescriptor desc;
                presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    MediaFoundation.TopologyNode sourceNode;
                    MediaFoundation.MediaFactory.CreateTopologyNode(MediaFoundation.TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(MediaFoundation.TopologyNodeAttributeKeys.Source, mediaSource);
                    sourceNode.Set(MediaFoundation.TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(MediaFoundation.TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    MediaFoundation.TopologyNode outputNode;
                    MediaFoundation.MediaFactory.CreateTopologyNode(MediaFoundation.TopologyType.OutputNode, out outputNode);

                    var typeHandler = desc.MediaTypeHandler;
                    Guid majorType = typeHandler.MajorType;
                    if (majorType != MediaFoundation.MediaTypeGuids.Audio)
                        throw new NotSupportedException("The song contains video data!");

                    MediaFoundation.Activate activate;
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

            MediaFoundation.MediaManager.Shutdown();

            //base.Dispose(disposing);

        }
        #endregion
    }
}

