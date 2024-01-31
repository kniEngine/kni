using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    // Summary:
    //     Represents bone data for a model. Reference page contains links to related
    //     conceptual articles.
    public sealed class ModelBone
    {
        private List<ModelBone> _children = new List<ModelBone>();
        
        private List<ModelMesh> _meshes = new List<ModelMesh>();

        // Summary:
        //     Gets a collection of bones that are children of this bone.
        public ModelBoneCollection Children { get; private set; }
        //
        // Summary:
        //     Gets the index of this bone in the Bones collection.
        public int Index { get; internal set; }
        //
        // Summary:
        //     Gets the name of this bone.
        public string Name { get; internal set; }
        //
        // Summary:
        //     Gets the parent of this bone.
        public ModelBone Parent { get; internal set; }
        //
        // Summary:
        //     Gets or sets the matrix used to transform this bone relative to its parent
        //     bone.
        internal Matrix transform;
        public Matrix Transform 
        { 
            get { return this.transform; } 
            set { this.transform = value; }
        }
        
        internal ModelBone()	
        {
            Children = new ModelBoneCollection(new List<ModelBone>());
        }
        
        internal void AddMesh(ModelMesh mesh)
        {
            _meshes.Add(mesh);
        }

        internal void AddChild(ModelBone modelBone)
        {
            _children.Add(modelBone);
            Children = new ModelBoneCollection(_children);
        }
    }

    //// Summary:
    ////     Represents bone data for a model. Reference page contains links to related
    ////     conceptual articles.
    //public sealed class ModelBone
    //{
    //    // Summary:
    //    //     Gets a collection of bones that are children of this bone.
    //    public ModelBoneCollection Children { get { throw new NotImplementedException(); } }
    //    //
    //    // Summary:
    //    //     Gets the index of this bone in the Bones collection.
    //    public int Index { get { throw new NotImplementedException(); } }
    //    //
    //    // Summary:
    //    //     Gets the name of this bone.
    //    public string Name { get { throw new NotImplementedException(); } }
    //    //
    //    // Summary:
    //    //     Gets the parent of this bone.
    //    public ModelBone Parent { get { throw new NotImplementedException(); } }
    //    //
    //    // Summary:
    //    //     Gets or sets the matrix used to transform this bone relative to its parent
    //    //     bone.
    //    public Matrix Transform { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
    //}
}
