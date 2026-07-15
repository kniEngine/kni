# Xna.Framework.Content.Pipeline.Audio

Audio asset processing and compilation for sound effects and music.

## Overview

The `Xna.Framework.Content.Pipeline.Audio` package provides importers and processors for audio assets. It includes:

- **Audio Importers**: WAV, MP3, OGG, and WMA file support
- **Sound Effect Processing**: Sample rate conversion, ADPCM compression
- **Audio Format Conversion**: Convert between various audio codecs

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content.Pipeline.Audio
```

## Configuration

Add to your `.mgcb` project file:

```
#begin Audio/explosion.wav
/importer:WavImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:Audio/explosion.wav

#begin Audio/music.ogg
/importer:OggImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Medium
/build:Audio/music.ogg

#begin Audio/voiceover.mp3
/importer:Mp3Importer
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:Audio/voiceover.mp3
```

## Importers

- **WavImporter**: PCM and compressed WAV files
- **Mp3Importer**: MPEG Layer III files
- **OggImporter**: OGG Vorbis files
- **WmaImporter**: Windows Media Audio files

## Processors

- **SoundEffectProcessor**: Converts audio to XNB format with optional compression
