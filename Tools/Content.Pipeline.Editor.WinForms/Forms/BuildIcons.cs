// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Content.Pipeline.Editor
{
    public class BuildIcons: IDisposable
    {
        public ImageList Icons { get; private set; }

        public const int BeginEnd = 0;
        public const int Queued = 1;
        public const int Clean = 2;
        public const int Fail = 3;
        public const int Processing = 4;
        public const int Skip = 5;
        public const int Succeed = 6;
        public const int Null = 7; // set to >= Icons.Images.Count for no icon
     

        public BuildIcons()
        {
            Icons = new ImageList();
            Icons.ColorDepth = ColorDepth.Depth32Bit;

            var asm = Assembly.GetExecutingAssembly();
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_begin_end.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_queued.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_clean.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_fail.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_processing.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_skip.png")));
            Icons.Images.Add(Image.FromStream(asm.GetManifestResourceStream(@"Content.Pipeline.Editor.Icons.build_succeed.png")));
        }

        ~BuildIcons()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Icons.Dispose();
            }
            Icons = null;
        }
    }
}
