# Xna.Framework.Storage

Player save game storage and file system abstraction.

## Overview

The `Xna.Framework.Storage` package provides abstraction for player save games and persistent storage. It includes:

- **StorageDevice**: Abstract storage device selection and management
- **StorageContainer**: Scoped access to player-specific save data directories
- **Cross-Platform Support**: Unified API across different platforms

## Installation

```bash
dotnet add package nkast.Xna.Framework.Storage
```

## Quick Start

```csharp
using Microsoft.Xna.Framework.Storage;

// Select storage device
StorageDevice.BeginShowSelector(OnStorageDeviceSelected, null);

private static void OnStorageDeviceSelected(IAsyncResult result)
{
	StorageDevice device = StorageDevice.EndShowSelector(result);

	if (device != null && device.IsConnected)
	{
		// Open a container for this player
		device.BeginOpenContainer("SaveData", OnContainerOpened, null);
	}
}

private static void OnContainerOpened(IAsyncResult result)
{
	StorageContainer container = StorageDevice.EndOpenContainer(result);

	// Create or open a save file
	using (Stream file = container.CreateFile("save.dat"))
	{
		// Write save data
		BinaryWriter writer = new BinaryWriter(file);
		writer.Write(playerLevel);
		writer.Write(playerScore);
	}

	container.Dispose();
}
```
