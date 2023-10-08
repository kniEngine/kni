// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    static partial class GraphicsExtensions
    {
        public static ComparisonFunc ToGLComparisonFunction(CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return ComparisonFunc.Always;
                case CompareFunction.Equal:
                    return ComparisonFunc.Equal;
                case CompareFunction.Greater:
                    return ComparisonFunc.Greater;
                case CompareFunction.GreaterEqual:
                    return ComparisonFunc.Gequal;
                case CompareFunction.Less:
                    return ComparisonFunc.Less;
                case CompareFunction.LessEqual:
                    return ComparisonFunc.Lequal;
                case CompareFunction.Never:
                    return ComparisonFunc.Never;
                case CompareFunction.NotEqual:
                    return ComparisonFunc.Notequal;

                default:
                    return ComparisonFunc.Always;
            }
        }

    }
        
}
