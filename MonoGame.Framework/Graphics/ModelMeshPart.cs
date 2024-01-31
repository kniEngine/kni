using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class ModelMeshPart
    {
        private Effect _effect;

        internal ModelMesh _parent;

        public Effect Effect
        {
            get { return _effect; }
            set 
            {
                if (value == _effect)
                    return;

                if (_effect != null)
                {
                    // First check to see any other parts are also using this effect.
                    bool removeEffect = true;
                    foreach (ModelMeshPart part in _parent.MeshParts)
                    {
                        if (part != this && part._effect == _effect)
                        {
                            removeEffect = false;
                            break;
                        }
                    }

                    if (removeEffect)
                        _parent.Effects.Remove(_effect);
                }

                // Set the new effect.
                _effect = value;
                
                if (_effect != null && !_parent.Effects.Contains(_effect))                
                    _parent.Effects.Add(_effect);
            }
        }

        public IndexBuffer IndexBuffer { get; internal set; }

        public int NumVertices { get; internal set; }

        public int PrimitiveCount { get; internal set; }

        public int StartIndex { get; internal set; }

        public object Tag { get; set; }

        public VertexBuffer VertexBuffer { get; internal set; }

        public int VertexOffset { get; internal set; }

        internal int VertexBufferIndex { get; set; }

        internal int IndexBufferIndex { get; set; }

        internal int EffectIndex { get; set; }
        
        internal ModelMeshPart()
        { 
        }
    }
}
