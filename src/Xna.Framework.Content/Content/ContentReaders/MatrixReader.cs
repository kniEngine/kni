// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content
{
    class MatrixReader : ContentTypeReader<Matrix>
    {
        protected internal override Matrix Read(ContentReader input, Matrix existingInstance)
        {
            //return input.ReadMatrix();

            Matrix result;
            result.M11 = input.ReadSingle();
            result.M12 = input.ReadSingle();
            result.M13 = input.ReadSingle();
            result.M14 = input.ReadSingle();
            result.M21 = input.ReadSingle();
            result.M22 = input.ReadSingle();
            result.M23 = input.ReadSingle();
            result.M24 = input.ReadSingle();
            result.M31 = input.ReadSingle();
            result.M32 = input.ReadSingle();
            result.M33 = input.ReadSingle();
            result.M34 = input.ReadSingle();
            result.M41 = input.ReadSingle();
            result.M42 = input.ReadSingle();
            result.M43 = input.ReadSingle();
            result.M44 = input.ReadSingle();
            return result;
        }
    }
}
