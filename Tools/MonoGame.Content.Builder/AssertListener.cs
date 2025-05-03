// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    internal class AssertListener : DefaultTraceListener
    {
        public override void Fail(string message)
        {
            if (message == null)
                message = "";

            throw new Exception("Debug assertion failed: " + message);
        }

        public override void Fail(string message, string detailMessage)
        {
            if (message == null)
                message = "";
            if (detailMessage == null)
                detailMessage = "";

            throw new Exception("Debug assertion failed: " + message + "\n" + detailMessage);
        }
    }
}
