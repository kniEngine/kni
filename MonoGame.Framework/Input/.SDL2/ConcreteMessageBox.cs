// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMessageBox : MessageBoxStrategy
    {
        public override Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            throw new NotImplementedException("MessageBox is not implemented on this platform.");
        }

        public override void PlatformCancel(int? result)
        {
            throw new NotImplementedException("MessageBox is not implemented on this platform.");
        }
    }
}
