using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class MessageBox
    {
        private Task<int?> PlatformShow(string title, string description, List<string> buttons)
        {
            throw new NotImplementedException("MessageBox is not implemented on this platform.");
        }

        private void PlatformCancel(int? result)
        {
            throw new NotImplementedException("MessageBox is not implemented on this platform.");
        }
    }
}
