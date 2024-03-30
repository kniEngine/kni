using System;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input
{
    public sealed partial class KeyboardInput
    {
        private Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            throw new NotImplementedException("KeyboardInput is not implemented on this platform.");
        }

        private void PlatformCancel(string result)
        {
            throw new NotImplementedException("KeyboardInput is not implemented on this platform.");
        }
    }
}
