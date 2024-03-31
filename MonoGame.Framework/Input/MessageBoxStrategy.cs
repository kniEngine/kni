// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class MessageBoxStrategy
    {
        public abstract void PlatformCancel(int? result);
        public abstract Task<int?> PlatformShow(string title, string description, List<string> buttonsList);
    }
}