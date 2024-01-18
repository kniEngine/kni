// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using nkast.Wasm.XHR;


namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        public override string Location { get { return string.Empty; } }

        public ConcreteTitleContainer() : base()
        {
        }

        public override Stream PlatformOpenStream(string name)
        {
            XMLHttpRequest request = new XMLHttpRequest();

            request.Open("GET", name, false);
            request.OverrideMimeType("text/plain; charset=x-user-defined");
            request.Send();

            if (request.Status == 200)
            {
                string responseText = request.ResponseText;

                byte[] buffer = new byte[responseText.Length];
                for (int i = 0; i < responseText.Length; i++)
                    buffer[i] = (byte)(responseText[i] & 0xff);

                Stream ms = new MemoryStream(buffer);

                return ms;
            }
            else
            {
                throw new IOException("HTTP request failed. Status:" + request.Status);
            }
        }
    }
}

