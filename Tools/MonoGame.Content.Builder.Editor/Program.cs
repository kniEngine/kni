// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Tools.Pipeline
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string project = null;

            if (args.Length == 1)
                project = args[0];

        }
    }
}
