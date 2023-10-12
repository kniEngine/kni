using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal class MarshalHelper
	{
        public static T Unmarshal<T>(IntPtr ptr)
        {
            Type type = typeof(T);
            T result = (T)Marshal.PtrToStructure(ptr, type);
            return result;
        }

		public static T[] UnmarshalArray<T>(IntPtr ptr, int count) 
        {
			Type type = typeof(T);
            int size = Marshal.SizeOf(type);            
            T[] ret = new T[count];

            for (int i = 0; i < count; i++)
            {
                int offset = i * size;
				IntPtr structPtr = new IntPtr(ptr.ToInt64() + offset);
                ret[i] = (T)Marshal.PtrToStructure(structPtr, type);
            }

			return ret;
		}

        public static byte[] UnmarshalArray(IntPtr ptr, int count)
        {
            byte[] result = new byte[count];
            Marshal.Copy(ptr, result, 0, count);
            return result;
        }	
	}
}

