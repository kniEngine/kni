using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;

        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static VertexPosition()
        {
            VertexElement[] elements = { new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0) };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
