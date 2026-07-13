# Xna.Framework.Content.Pipeline.Media

Video and audio streaming asset processing and compilation.

## Overview

The `Xna.Framework.Content.Pipeline.Media` package provides importers and processors for media assets. It includes:

- **Video Importers**: Video streaming for video files
- **Song Importers**: Audio streaming for background music
- **Video Processing**: Format conversion and optimization
- **Song Processing**: Audio stream processing

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content.Pipeline.Media
```

## Configuration

Add to your `.mgcb` project file:

```
#begin Videos/intro.webm
/importer:VideoImporter
/processor:VideoProcessor
/processorParam:Quality=Medium
/build:Videos/intro.webm

#begin Audio/theme.mp3
/importer:Mp3Importer
/processor:SongProcessor
/build:Audio/theme.mp3
```

## Importers

- **VideoImporter**: Common video formats
- **WmvImporter**: Windows Media Video files
- **H264Importer**: H.264 video files
- **Mp3Importer**: MPEG Layer III audio files

## Processors

- **VideoProcessor**: Converts video to platform-specific streaming format
- **SongProcessor**: Converts audio to platform-specific streaming format for background music playback
