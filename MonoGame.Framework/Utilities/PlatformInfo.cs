// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Utilities
{
    /// <summary>
    /// Utility class that returns information about the underlying platform
    /// </summary>
    public static class PlatformInfo
    {
        /// <summary>
        /// Underlying game platform type
        /// </summary>
        public static MonoGamePlatform MonoGamePlatform
        {
            get
            {
#if ANDROID
                return MonoGamePlatform.Android;
#elif DESKTOPGL
                return MonoGamePlatform.DesktopGL;
#elif IOS
                return MonoGamePlatform.iOS;
#elif TVOS
                return MonoGamePlatform.tvOS;
#elif BLAZORGL
                return MonoGamePlatform.BlazorGL;
#elif WINDOWSDX
                return MonoGamePlatform.Windows;
#elif UAP || WINUI
                return MonoGamePlatform.WindowsUniversal;
#elif SWITCH
                return MonoGamePlatform.NintendoSwitch;
#elif XB1
                return MonoGamePlatform.XboxOne;
#elif PLAYSTATION4
                return MonoGamePlatform.PlayStation4;
#elif PLAYSTATION5
                return MonoGamePlatform.PlayStation5;
#elif REF
                throw new PlatformNotSupportedException();
#endif
            }
        }

        /// <summary>
        /// Graphics backend
        /// </summary>
        public static GraphicsBackend GraphicsBackend
        {
            get
            {
#if DIRECTX
                return GraphicsBackend.DirectX;
#else
                return GraphicsBackend.OpenGL;
#endif
            }
        }
    }
}
