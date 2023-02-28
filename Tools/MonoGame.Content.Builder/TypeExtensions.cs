// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    static internal class TypeExtensions
    {
        public static void AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        public static void AddRangeUnique<T>(this List<T> dstList, List<T> list)
        {
            foreach (var i in list)
            {
                if (!dstList.Contains(i))
                    dstList.Add(i);
            }
        }
    }
}
