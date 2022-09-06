// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        private SoundEffectInstance _sound;

        internal override void PlatformInitialize(string fileName)
        {

#if MONOMAC || DESKTOPGL

            using (var s = File.OpenRead(_name))
            {
                var soundEffect = SoundEffect.FromStream(s);
                _sound = soundEffect.CreateInstance();
            }
#endif
        }

        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_sound != null)
                    _sound.Dispose();

                _sound = null;
            }
        }

		internal void OnFinishedPlaying (object sender, EventArgs args)
		{
			if (DonePlaying == null)
				return;
			
			DonePlaying(sender, args);
		}

		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			if (DonePlaying == null)
			    DonePlaying += handler;
		}

		internal void Play(TimeSpan? startPosition)
		{	
			if (startPosition.HasValue)
				throw new Exception("startPosition not implemented on this Platform"); //Should be possible to implement in OpenAL
			if ( _sound == null )
				return;

            PlatformPlay();

            _playCount++;
        }

        private void PlatformPlay()
        {
            _sound.Play();
        }

		internal void Resume()
		{
			if (_sound == null)
				return;

            PlatformResume();
		}

        private void PlatformResume()
        {
            _sound.Resume();
        }
		
		internal void Pause()
		{			            
			if ( _sound == null )
				return;
			
			_sound.Pause();
        }
		
		internal void Stop()
		{
			if ( _sound == null )
				return;
			
			_sound.Stop();
			_playCount = 0;
		}

		internal float Volume
		{
			get
			{
				if (_sound != null)
					return _sound.Volume;
				else
					return 0.0f;
			}
			
			set
			{
				if ( _sound != null && _sound.Volume != value )
					_sound.Volume = value;
			}			
		}

		internal TimeSpan Position
        {
            get
            {
                // TODO: Implement
                return new TimeSpan(0);				
            }
        }

        internal override Album PlatformGetAlbum()
        {
            return null;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            
        }

        internal override Artist PlatformGetArtist()
        {
            return null;
        }

        internal override Genre PlatformGetGenre()
        {
            return null;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            return false;
        }

        internal override bool PlatformIsRated()
        {
            return false;
        }

        internal override string PlatformGetName()
        {
            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}