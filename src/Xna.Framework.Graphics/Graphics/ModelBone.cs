using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents bone data for a model.
    /// </summary>
    public sealed class ModelBone
    {
        private List<ModelBone> _children = new List<ModelBone>();

        internal Matrix _transform;

        /// <summary>
        /// Gets a collection of bones that are children of this bone.
        /// </summary>
        public ModelBoneCollection Children { get; private set; }

        /// <summary>
        /// Gets the index of this bone in the Bones collection.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the name of this bone.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parent of this bone.
        /// </summary>
        public ModelBone Parent { get; internal set; }

        /// <summary>
        /// Gets or sets the matrix used to transform this bone relative to its parent bone.
        /// </summary>
        public Matrix Transform
        { 
            get { return this._transform; }
            set { this._transform = value; }
        }
        
        internal ModelBone(int index, string name, Matrix transform)
        {
            Children = new ModelBoneCollection(new List<ModelBone>());

            this.Index = index;
            this.Name = name;
            this.Transform = transform;
        }

        // tests only
        internal ModelBone()
        {
            Children = new ModelBoneCollection(new List<ModelBone>());
        }

        internal void AddChild(ModelBone childModelBone)
        {
            _children.Add(childModelBone);
            Children = new ModelBoneCollection(_children);
        }
    }
}
