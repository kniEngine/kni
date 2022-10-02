// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content
{
    internal class ComplexReader : ContentTypeReader<Complex>
    {
        protected internal override Complex Read(ContentReader input, Complex existingInstance)
        {
            return input.ReadComplex();
        }
    }
}
