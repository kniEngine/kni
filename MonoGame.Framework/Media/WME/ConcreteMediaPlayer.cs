// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using Microsoft.Xna.Framework.Media;


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
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnSongFinishedPlaying(null, null)).AsTask();
            }
        }

        #region Properties

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            _mediaEngineEx.Muted = muted;
        }

        internal override void PlatformSetIsRepeating(bool repeating)
        {
            base.PlatformSetIsRepeating(repeating);

            _mediaEngineEx.Loop = repeating;
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromSeconds(_mediaEngineEx.CurrentTime);
        }

        internal override bool PlatformGetGameHasControl()
        {
            return true;
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override float PlatformGetVolume()
        {
            return base.PlatformGetVolume();
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            _mediaEngineEx.Volume = volume;
        }

        #endregion

        protected override void PlatformPlaySong(Song song)
        {
            _mediaEngineEx.Source = song.Strategy.Name;
            _mediaEngineEx.Load();
            _sessionState = SessionState.Started;

            //We start playing when we get a LoadedData event in MediaEngineExOnPlaybackEvent
        }

        protected override void PlatformPause()
        {
            if (_sessionState != SessionState.Started)
                return;
            _sessionState = SessionState.Paused;
            _mediaEngineEx.Pause();
        }

        protected override void PlatformResume()
        {
            if (_sessionState != SessionState.Paused)
                return;
            _mediaEngineEx.Play();
        }

        protected override void PlatformStop()
        {
            if (_sessionState == SessionState.Stopped)
                return;
            _mediaEngineEx.Source = null;
        }

    }
}

