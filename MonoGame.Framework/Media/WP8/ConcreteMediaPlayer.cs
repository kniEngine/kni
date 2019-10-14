// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using MsMediaPlayer = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaPlayer;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {
        internal static MediaElement _mediaElement;
        private Uri _source;
        private TimeSpan _elapsedTime;

        // track state of player before game is deactivated
        private MediaState _deactivatedState;
        private bool _wasDeactivated;

        // _playingInternal should default to true to be to work with the user's default playing music
        private bool _playingInternal = true;

        internal ConcreteMediaPlayerStrategy()
        {
            PhoneApplicationService.Current.Activated += (sender, e) =>
                {
                    if (_mediaElement != null)
                    {
                        if (_mediaElement.Source == null && _source != null)
                        {
                            _mediaElement.AutoPlay = false;
                            Deployment.Current.Dispatcher.BeginInvoke(() => _mediaElement.Source = _source);
                        }

                        // Ensure only one subscription
                        _mediaElement.MediaOpened -= MediaElement_MediaOpened;
                        _mediaElement.MediaOpened += MediaElement_MediaOpened;
                    }
                };

            PhoneApplicationService.Current.Deactivated += (sender, e) => 
                {
                    if (_mediaElement != null)
                    {
                        _source = _mediaElement.Source;
                        _elapsedTime = _mediaElement.Position;

                        _wasDeactivated = true;
                        _deactivatedState = State;
                    }
                };
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_elapsedTime != TimeSpan.Zero)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Position = _elapsedTime;
                    _elapsedTime = TimeSpan.Zero;
                });

            if (_wasDeactivated)
            {
                if (_deactivatedState == MediaState.Playing)
                    _mediaElement.Play();
 
                //reset the deactivated flag
                _wasDeactivated = false;
 
                //set auto-play back to default
                _mediaElement.AutoPlay = true;
            }
        }

        #region Properties
        internal override bool PlatformGetIsMuted()
        {
            if (_playingInternal)
                return MsMediaPlayer.IsMuted;

            return base.PlatformGetIsMuted();
        }

        internal override void PlatformSetIsMuted(bool muted)
        {
            base.PlatformSetIsMuted(muted);

            if (_playingInternal)
                MsMediaPlayer.IsMuted = muted;
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.IsMuted = muted;
                });
            }
        }

        internal override void PlatformSetIsRepeating(bool repeating)
        {
            base.PlatformSetIsRepeating(repeating);

            if (_playingInternal)
                MsMediaPlayer.IsRepeating = repeating;
        }

        internal override void PlatformSetIsShuffled(bool shuffled)
        {
            base.PlatformSetIsShuffled(shuffled);

            if (_playingInternal)
                MsMediaPlayer.IsShuffled = shuffled;
        }

        internal override TimeSpan PlatformGetPlayPosition()
        {
            if (_playingInternal)
                return MsMediaPlayer.PlayPosition;

            if (_mediaElement == null)
                return TimeSpan.Zero;

            if (_mediaElement.Dispatcher.CheckAccess())
                return _mediaElement.Position;

            TimeSpan pos;
            EventWaitHandle Wait = new AutoResetEvent(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                pos = _mediaElement.Position;
                Wait.Set();
            });
            Wait.WaitOne();
            return pos;
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            if (_playingInternal)
            {
                switch (MsMediaPlayer.State)
                {
                    case MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaState.Paused:
                        state = MediaState.Paused;
                        return true;
                    case MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaState.Playing:
                        state = MediaState.Playing;
                        return true;
                    default:
                        state = MediaState.Stopped;
                        return true;
                }
            }

            return false;
        }

        internal override bool PlatformGetGameHasControl()
        {
            return (!_playingInternal && State == MediaState.Playing) || MsMediaPlayer.GameHasControl;
        }

        internal override float PlatformGetVolume()
        {
            if (_playingInternal)
                return MsMediaPlayer.Volume;

            return base.PlatformGetVolume();
        }

        internal override void PlatformSetVolume(float volume)
        {
            base.PlatformSetVolume(volume);

            if (_playingInternal)
                MsMediaPlayer.Volume = volume;
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Unlike other implementations, MediaElement uses a linear scale for volume
                    // On WP8 a volume of 0.85 seems to refer to 50% volume according to MSDN
                    // http://msdn.microsoft.com/EN-US/library/windowsphone/develop/system.windows.controls.mediaelement.volume%28v=vs.105%29.aspx
                    // Therefore a good approximation could be to use the 4th root of volume
                    _mediaElement.Volume = Math.Pow(base.PlatformGetVolume(), 1/4d);
                });
            }
        }

        #endregion

        protected override void PlatformPause()
        {
            if (_playingInternal)
                MsMediaPlayer.Pause();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Pause();
                });
            }
        }

        protected override void PlatformPlaySong(Song song)
        {
            if (song.InternalSong != null)
            {
                _playingInternal = true;

                // Ensure only one subscription
                MsMediaPlayer.MediaStateChanged -= MsMediaStateChanged;
                MsMediaPlayer.MediaStateChanged += MsMediaStateChanged;
                MsMediaPlayer.ActiveSongChanged -= MsActiveSongChanged;
                MsMediaPlayer.ActiveSongChanged += MsActiveSongChanged;

                MsMediaPlayer.Play(song.InternalSong);
            }
            else
            {
                _playingInternal = false;

                MsMediaPlayer.MediaStateChanged -= MsMediaStateChanged;
                MsMediaPlayer.ActiveSongChanged -= MsActiveSongChanged;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Source = new Uri(song.FilePath, UriKind.Relative);
                    _mediaElement.Play();

                    // Ensure only one subscribe
                    _mediaElement.MediaEnded -= OnSongFinishedPlaying;
                    _mediaElement.MediaEnded += OnSongFinishedPlaying;
                });
            }
        }

        private void MsMediaStateChanged(object sender, EventArgs args)
        {
            OnPlatformMediaStateChanged(EventArgs.Empty);
        }

        private void MsActiveSongChanged(object sender, EventArgs args)
        {
            OnPlatformActiveSongChanged(EventArgs.Empty);
        }
        
        protected override void OnPlatformMediaStateChanged(EventArgs args)
        {
            // Playing music using XNA, we shouldn't fire extra state changed events
            if (_playingInternal)
                return;

            base.OnPlatformMediaStateChanged(args);
        }
        
        protected override void PlatformOnSongRepeat()
        {
#if WP8
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Position = TimeSpan.Zero;
                _mediaElement.Play();
            });
#endif
        }

        protected override void PlatformResume()
        {
            if (_playingInternal)
                MsMediaPlayer.Resume();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Play();
                });
            }
        }

        protected override void PlatformStop()
        {
            if (_playingInternal)
                MsMediaPlayer.Stop();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Stop();
                });
            }
        }
    }
}

