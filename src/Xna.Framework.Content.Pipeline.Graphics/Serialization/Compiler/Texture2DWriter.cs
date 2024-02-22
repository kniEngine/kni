// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class Texture2DWriter : ContentTypeWriterBase<Texture2DContent>
    {
        protected override void Write(ContentWriter output, Texture2DContent value)
        {
            MipmapChain mipmaps = value.Faces[0];   // Mipmap chain.
            BitmapContent level0 = mipmaps[0];        // Most detailed mipmap level.

            SurfaceFormat format;
            if (!level0.TryGetFormat(out format))
                throw new Exception("Couldn't get Format for TextureContent.");

            output.Write((int)format);
            output.Write(level0.Width);
            output.Write(level0.Height);
            output.Write(mipmaps.Count);    // Number of mipmap levels.

            foreach (BitmapContent level in mipmaps)
            {
                byte[] pixelData = level.GetPixelData();
                output.Write(pixelData.Length);
                output.Write(pixelData);
            }
        }
        
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }
    }
}
