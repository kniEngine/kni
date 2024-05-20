// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Javax.Microedition.Khronos.Egl;

namespace Microsoft.Xna.Platform.Graphics
{
    internal struct SurfaceConfig
    {
        public int Red;
        public int Green;
        public int Blue;
        public int Alpha;

        public int Depth;
        public int Stencil;

        public int SampleBuffers;
        public int Samples;

        public int[] ToConfigAttribs()
        {
            List<int> attribs = new List<int>();
            if (Red != 0)
            {
                attribs.Add(EGL11.EglRedSize);
                attribs.Add(Red);
            }
            if (Green != 0)
            {
                attribs.Add(EGL11.EglGreenSize);
                attribs.Add(Green);
            }
            if (Blue != 0)
            {
                attribs.Add(EGL11.EglBlueSize);
                attribs.Add(Blue);
            }
            if (Alpha != 0)
            {
                attribs.Add(EGL11.EglAlphaSize);
                attribs.Add(Alpha);
            }
            if (Depth != 0)
            {
                attribs.Add(EGL11.EglDepthSize);
                attribs.Add(Depth);
            }
            if (Stencil != 0)
            {
                attribs.Add(EGL11.EglStencilSize);
                attribs.Add(Stencil);
            }
            if (SampleBuffers != 0)
            {
                attribs.Add(EGL11.EglSampleBuffers);
                attribs.Add(SampleBuffers);
            }
            if (Samples != 0)
            {
                attribs.Add(EGL11.EglSamples);
                attribs.Add(Samples);
            }
            attribs.Add(EGL11.EglRenderableType);
            attribs.Add(4);
            attribs.Add(EGL11.EglNone);

            return attribs.ToArray();
        }

        static int[] data = new int[1];

        static int GetAttribute(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay,int attribute)
        {
            egl.EglGetConfigAttrib(eglDisplay, config, attribute, data);
            return data[0];
        }

        public static SurfaceConfig FromEGLConfig(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay)
        {
            lock (data)
            {
                return new SurfaceConfig()
                {
                    Red = GetAttribute(config, egl, eglDisplay, EGL11.EglRedSize),
                    Green = GetAttribute(config, egl, eglDisplay, EGL11.EglGreenSize),
                    Blue = GetAttribute(config, egl, eglDisplay, EGL11.EglBlueSize),
                    Alpha = GetAttribute(config, egl, eglDisplay, EGL11.EglAlphaSize),
                    Depth = GetAttribute(config, egl, eglDisplay, EGL11.EglDepthSize),
                    Stencil = GetAttribute(config, egl, eglDisplay, EGL11.EglStencilSize),
                    SampleBuffers = GetAttribute(config, egl, eglDisplay, EGL11.EglSampleBuffers),
                    Samples = GetAttribute(config, egl, eglDisplay, EGL11.EglSamples)
                };
            }
        }

        public override string ToString()
        {
            return string.Format("Red:{0} Green:{1} Blue:{2} Alpha:{3} Depth:{4} Stencil:{5} SampleBuffers:{6} Samples:{7}", Red, Green, Blue, Alpha, Depth, Stencil, SampleBuffers, Samples);
        }
    }
}
