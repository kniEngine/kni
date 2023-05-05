// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
#if IOS || TVOS
using UIKit;
#endif

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaSource
    {
		private string _name;
		private MediaSourceType _type;

		internal MediaSource(string name, MediaSourceType type)
		{
			_name = name;
			_type = type;
		}

        public string Name
        {
            get { return _name; }
        }
				
        public MediaSourceType MediaSourceType
        {
            get { return _type; }
        }
	
		public static IList<MediaSource> GetAvailableMediaSources()
        {
#if IOS || TVOS
			MediaSource[] result = { new MediaSource(UIDevice.CurrentDevice.SystemName, MediaSourceType.LocalDevice) };
			return result;
#else
            MediaSource[] result = { new MediaSource("DummpMediaSource", MediaSourceType.LocalDevice) };
			return result;
#endif
        }
    }
}
