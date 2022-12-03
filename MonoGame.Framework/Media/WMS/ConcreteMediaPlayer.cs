// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using Microsoft.Xna.Framework.Media;


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

            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);

            _session.BeginGetEvent(this, null);

            _clock = _session.Clock.QueryInterface<PresentationClock>();
        }

        #region IAsyncCallback

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }

        public IDisposable Shadow { get; set; }

        public void Invoke(AsyncResult asyncResultRef)
        {
            using (var mediaEvent = _session.EndGetEvent(asyncResultRef))
            {
                switch (mediaEvent.TypeInfo)
                {
                    case MediaEventTypes.SessionEnded:
                        _sessionState = SessionState.Ended;
                        OnSongFinishedPlaying(null, null);
                        break;
                    case MediaEventTypes.SessionTopologyStatus:
                        if (mediaEvent.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                            OnTopologyReady();
                        break;
                    case MediaEventTypes.SessionStopped:
                        OnSessionStopped();
                        break;
                }

                var evValue = mediaEvent.Value.Value as IDisposable;
                if (evValue != null)
                    evValue.Dispose();
            }

            _session.BeginGetEvent(this, null);
        }

        public AsyncCallbackFlags Flags { get; private set; }
        public WorkQueueId WorkQueueId { get; private set; }

        #endregion IAsyncCallback

        #region Properties

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            SetChannelVolumes();
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            if ((_sessionState == SessionState.Stopped) || (_sessionState == SessionState.Stopping))
                return TimeSpan.Zero;
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

        internal override bool PlatformGetGameHasControl()
        {
            return true;
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            SetChannelVolumes();
        }

        #endregion

        private void SetChannelVolumes()
        {
            lock (_volumeLock)
            {
                if (_volumeController == null)
                    return;

                float volume = base.PlatformGetIsMuted() ? 0f : base.PlatformGetVolume();
                for (int i = 0; i < _volumeController.ChannelCount; i++)
                {
                    _volumeController.SetChannelVolume(i, volume);
                }
            }
        }

        protected override void PlatformPause()
        {
            if (_sessionState != SessionState.Started)
                return;
            _sessionState = SessionState.Paused;
            _session.Pause();
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (_currentSong == song)
                ReplayCurrentSong(song);
            else
                PlayNewSong(song);
        }

        private void ReplayCurrentSong(Song song)
        {
            if (_sessionState == SessionState.Stopping)
            {
                // The song will be started after the SessionStopped event is received
                _nextSong = song;
                return;
            }

            StartSession(_positionBeginning);
        }

        private void PlayNewSong(Song song)
        {
            if (_sessionState != SessionState.Stopped)
            {
                // The session needs to be stopped to reset the play position
                // The new song will be started after the SessionStopped event is received
                _nextSong = song;
                PlatformStop();
                return;
            }

            StartNewSong(song);
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

           _session.SetTopology(SessionSetTopologyFlags.Immediate, song.Topology);

            StartSession(_positionBeginning);

            // The volume service won't be available until the session topology
            // is ready, so we now need to wait for the event indicating this
        }

        private void StartSession(Variant startPosition)
        {
            _sessionState = SessionState.Started;
            _session.Start(null, startPosition);
        }

        private void OnTopologyReady()
        {
            lock (_volumeLock)
            {
                IntPtr volumeObjectPtr;
                MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, _audioStreamVolumeGuid, out volumeObjectPtr);
                _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);
            }

            SetChannelVolumes();
        }

        protected override void PlatformResume()
        {
            if (_sessionState != SessionState.Paused)
                return;
            StartSession(_positionCurrent);
        }

        protected override void PlatformStop()
        {
            if ((_sessionState == SessionState.Stopped) || (_sessionState == SessionState.Stopping))
                return;
            bool hasFinishedPlaying = (_sessionState == SessionState.Ended);
            _sessionState = SessionState.Stopping;
            if (hasFinishedPlaying)
            {
                // The play position needs to be reset before stopping otherwise the next song may not start playing
                _session.Start(null, _positionBeginning);
            }
            _session.Stop();
        }

        private void OnSessionStopped()
        {
            _sessionState = SessionState.Stopped;
            if (_nextSong != null)
            {
                if (_nextSong != _currentSong)
                    StartNewSong(_nextSong);
                else
                    StartSession(_positionBeginning);
                _nextSong = null;
            }
        }
    }
}

