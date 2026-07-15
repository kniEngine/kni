# Xna.Framework.Media

Media library, music, and video playback support.

## Overview

The `Xna.Framework.Media` package provides access to device media libraries and playback control for music and video. It includes:

- **MediaPlayer**: Music playback with state control
- **VideoPlayer**: Video playback with audio sync
- **MediaLibrary**: Access to system music, videos, albums and playlists

## Installation

```bash
dotnet add package nkast.Xna.Framework.Media
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Media;

// Play a song
Song song = contentManager.Load<Song>("Music/background");
MediaPlayer.IsRepeating = true;
MediaPlayer.Volume = 0.8f;
MediaPlayer.Play(song);

// Play a video
Video video = contentManager.Load<Video>("Videos/intro");
MediaPlayer.Volume = 0.5f;
MediaPlayer.Play(video);
// Call every frame to get the current video frame
Texture2D videoTexture = video.GetTexture();

// Access media library
MediaLibrary library = new MediaLibrary();
foreach (Artist artist in library.Artists)
{
	foreach (Album album in artist.Albums)
	{
		foreach (Song song in album.Songs)
		{
			// Process songs
		}
	}
}
```
