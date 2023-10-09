// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    static class GLExtensions
    {
        public static ComparisonFunc ToGLComparisonFunction(this CompareFunction compare)
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
