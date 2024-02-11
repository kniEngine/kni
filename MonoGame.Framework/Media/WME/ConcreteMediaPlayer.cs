// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.Xna.Framework.Media;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        // RAYB: This needs to be turned back into a readonly.
        private MediaEngine _mediaEngineEx;

        private CoreDispatcher _dispatcher;

        private enum SessionState { Stopped, Started, Paused }
        private SessionState _sessionState = SessionState.Stopped;

        internal ConcreteMediaPlayerStrategy()
        {
            MediaManager.Startup(true);
            using (var factory = new MediaEngineClassFactory())
            using (var attributes = new MediaEngineAttributes { AudioCategory = AudioStreamCategory.GameMedia })
            {
                var creationFlags = MediaEngineCreateFlags.AudioOnly;

                var mediaEngine = new MediaEngine(factory, attributes, creationFlags, MediaEngineExOnPlaybackEvent);
                _mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();
            }

            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        private void MediaEngineExOnPlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (mediaEvent == MediaEngineEvent.LoadedData)
            {
                if (_sessionState == SessionState.Started)
                    _mediaEngineEx.Play();
            }
            if (mediaEvent == MediaEngineEvent.Ended)
            {
                _sessionState = SessionState.Stopped;

                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => base.OnSongFinishedPlaying()).AsTask();
            }
        }

        #region Properties

        public override bool PlatformIsMuted
        {
            set
            {
                base.PlatformIsMuted = value;

                _mediaEngineEx.Muted = value;
            }
        }

        internal override bool PlatformIsRepeating
        {
            set
            {
                base.PlatformIsRepeating = value;

                _mediaEngineEx.Loop = value;
            }
        }

        public override TimeSpan PlatformPlayPosition
        {
            get { return TimeSpan.FromSeconds(_mediaEngineEx.CurrentTime); }
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
            get { return base.PlatformVolume; }
            set
            {
                base.PlatformVolume = value;
                _mediaEngineEx.Volume = value;
            }
        }

        #endregion

        public override void PlatformPlaySong(Song song)
        {
            _mediaEngineEx.Source = ((IPlatformSong)song).Strategy.ToConcrete<ConcreteSongStrategy>().StreamSource.OriginalString;
            _mediaEngineEx.Load();
            _sessionState = SessionState.Started;

            //We start playing when we get a LoadedData event in MediaEngineExOnPlaybackEvent
        }

        public override void PlatformPause()
        {
            if (_sessionState == SessionState.Started)
            {
                _sessionState = SessionState.Paused;
                _mediaEngineEx.Pause();
            }
        }

        public override void PlatformResume()
        {
            if (_sessionState == SessionState.Paused)
            {
                _mediaEngineEx.Play();
            }
        }

        public override void PlatformStop()
        {
            if (_sessionState != SessionState.Stopped)
            {
                _mediaEngineEx.Source = null;
            }
        }

    }
}

