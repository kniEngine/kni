# Xna.Framework.Content.Pipeline

Asset compilation and processing pipeline for KNI games.

## Overview

The `Xna.Framework.Content.Pipeline` is a development-time package that provides the base infrastructure for compiling game assets into optimized binary formats. It includes:

- **Content Processor Framework**: Base classes and attributes for processors
- **Content Compiler**: Asset compilation to binary XNB format with optional compression
- **Intermediate Serialization**: XML-based intermediate format for asset data
- **Type Writers**: Binary serialization writers for custom types
- **Importers**: Base infrastructure for custom asset importers
- **Compression Support**: LZ4 and Brotli compression codecs

## Installation

```bash
dotnet add package nkast.Xna.Framework.Content.Pipeline
```

## Quick Start

Create a custom content processor:

```csharp
using Microsoft.Xna.Framework.Content.Pipeline;

[ContentProcessor(DisplayName = "My Custom Processor")]
public class MyProcessor : ContentProcessor<MyData, ProcessedData>
{
	public override ProcessedData Process(MyData input, ContentProcessorContext context)
	{
		// Transform input data
		ProcessedData output = new ProcessedData();
		output.ProcessedValue = input.Value * 2;
		return output;
	}
}

[ContentTypeWriter]
public class ProcessedDataWriter : ContentTypeWriter<ProcessedData>
{
	public override void Write(ContentWriter output, ProcessedData value)
	{
		output.Write(value.ProcessedValue);
	}
}
```

## Importers

- **XmlImporter**: Imports XML-based intermediate format files for asset data

## Processors

- **PassThroughProcessor**: Pass-through processor that outputs input without modification
