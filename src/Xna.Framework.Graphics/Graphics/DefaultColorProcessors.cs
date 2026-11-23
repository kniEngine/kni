// Copyright (C)2026 KNI contributors

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Color processors that can be passed to
    /// <see cref="Texture2D.FromStream(GraphicsDevice, System.IO.Stream, Action{byte[]})"/>.
    /// Each processor receives the decoded pixels as RGBA bytes (4 bytes per pixel).
    /// This class cannot be inherited.
    /// </summary>
    public static class DefaultColorProcessors
    {
        /// <summary>
        /// Sets the RGB component values of each color to zero if the alpha component is zero.
        /// </summary>
        /// <remarks>
        /// This is standard XNA behavior.
        /// </remarks>
        public static readonly Action<byte[]> ZeroTransparentPixels = data =>
        {
            for (int i = 0; i + 3 < data.Length; i += 4)
            {
                if (data[i + 3] == 0)
                {
                    data[i + 0] = 0;
                    data[i + 1] = 0;
                    data[i + 2] = 0;
                }
            }
        };

        /// <summary>
        /// Premultiplies the RGB component value of each color by its alpha component.
        /// The result is rounded to the nearest value, so alpha 128 turns 255 into 128.
        /// </summary>
        public static readonly Action<byte[]> PremultiplyAlpha = data =>
        {
            for (int i = 0; i + 3 < data.Length; i += 4)
            {
                int alpha = data[i + 3];
                if (alpha == 255)
                    continue;

                data[i + 0] = (byte)((data[i + 0] * alpha + 127) / 255);
                data[i + 1] = (byte)((data[i + 1] * alpha + 127) / 255);
                data[i + 2] = (byte)((data[i + 2] * alpha + 127) / 255);
            }
        };
    }
}
