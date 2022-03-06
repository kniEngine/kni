using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal partial class ConstantBufferData
    {
        public void Write(int version, BinaryWriter writer, Options options)
        {
            writer.Write(Name);

            writer.Write((ushort)Size);

            writer.Write(ParameterIndex.Count);
            for (var i=0; i < ParameterIndex.Count; i++)
            {
                writer.Write(ParameterIndex[i]);
                writer.Write((ushort)ParameterOffset[i]);
            }
        }
    }
}
