// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WP8
extern alias MicrosoftXnaFramework;
using MsMediaQueue = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaQueue;
#endif

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaQueue
	{
        List<Song> songs = new List<Song>();
		private int _activeSongIndex = -1;
		private Random random = new Random();

#if WP8
        private MsMediaQueue mediaQueue;

        public static implicit operator MediaQueue(MsMediaQueue mediaQueue)
        {
            return new MediaQueue(mediaQueue);
        }

        private MediaQueue(MsMediaQueue mediaQueue)
        {
            this.mediaQueue = mediaQueue;
        }
#endif

		public MediaQueue()
		{
			
		}
		
		public Song ActiveSong
		{
			get
			{
#if WP8
			    if (mediaQueue != null)
			        return new Song(mediaQueue.ActiveSong);
#endif
				if (songs.Count == 0 || _activeSongIndex < 0)
					return null;
				
				return songs[_activeSongIndex];
			}
		}
		
		public int ActiveSongIndex
		{
		    get
		    {
#if WP8
			    if (mediaQueue != null)
			        return mediaQueue.ActiveSongIndex;
#endif
		        return _activeSongIndex;
		    }
		    set
		    {
#if WP8
		        if (mediaQueue != null)
		            mediaQueue.ActiveSongIndex = value;
#endif
		        _activeSongIndex = value;
		    }
		}

        internal int Count
        {
            get
            {
#if WP8
                if (mediaQueue != null)
                    return mediaQueue.Count;
#endif
                return songs.Count;
            }
        }

        public Song this[int index]
        {
            get
            {
#if WP8
                if (mediaQueue != null)
                    return new Song(mediaQueue[index]);
#endif
                return songs[index];
            }
        }

        internal IEnumerable<Song> Songs
        {
            get
            {
                return songs;
            }
        }

		internal Song GetNextSong(int direction, bool shuffle)
		{
			if (shuffle)
				_activeSongIndex = random.Next(songs.Count);
			else			
				_activeSongIndex = (int)MathHelper.Clamp(_activeSongIndex + direction, 0, songs.Count - 1);
			
			return songs[_activeSongIndex];
		}
		
		internal void Clear()
		{
			Song song;
			for(; songs.Count > 0; )
			{
				song = songs[0];
#if !DIRECTX
				song.Stop();
#endif
				songs.Remove(song);
			}	
		}

        internal void Add(Song song)
        {
            songs.Add(song);
        }

#if !DIRECTX
        internal void Stop()
        {
            int count = songs.Count;
            for (int i = 0; i < count; ++i)
                songs[i].Stop();
        }
#endif
	}
}

