// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A basic 3D model with per mesh parent bones.
    /// </summary>
    public sealed class Model
    {
        private static Matrix[] _sharedDrawBoneMatrices;
        
        /// <summary>
        /// A collection of <see cref="ModelBone"/> objects which describe how each mesh in the
        /// mesh collection for this model relates to its parent mesh.
        /// </summary>
        public ModelBoneCollection Bones { get; private set; }

        /// <summary>
        /// A collection of <see cref="ModelMesh"/> objects which compose the model. Each <see cref="ModelMesh"/>
        /// in a model may be moved independently and may be composed of multiple materials
        /// identified as <see cref="ModelMeshPart"/> objects.
        /// </summary>
        public ModelMeshCollection Meshes { get; private set; }

        /// <summary>
        /// Root bone for this model.
        /// </summary>
        public ModelBone Root { get; internal set; }

        /// <summary>
        /// Custom attached object.
        /// <remarks>
        /// Skinning data is example of attached object for model.
        /// </remarks>
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Constructs a model. 
        /// </summary>
        /// <param name="graphicsDevice">A valid reference to <see cref="GraphicsDevice"/>.</param>
        /// <param name="bones">The collection of bones.</param>
        /// <param name="meshes">The collection of meshes.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graphicsDevice"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bones"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="meshes"/> is null.
        /// </exception>
        internal Model(List<ModelBone> bones, List<ModelMesh> meshes)
        {
            Bones = new ModelBoneCollection(bones);
            Meshes = new ModelMeshCollection(meshes);
        }

        /// <summary>
        /// Draws the model meshes.
        /// </summary>
        /// <param name="world">The world transform.</param>
        /// <param name="view">The view transform.</param>
        /// <param name="projection">The projection transform.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Draw(Matrix world, Matrix view, Matrix projection) 
        {       
            int boneCount = this.Bones.Count;
            
            if (_sharedDrawBoneMatrices == null ||
                _sharedDrawBoneMatrices.Length < boneCount)
            {
                _sharedDrawBoneMatrices = new Matrix[boneCount];    
            }
            
            // Look up combined bone matrices for the entire model.            
            CopyAbsoluteBoneTransformsTo(_sharedDrawBoneMatrices);

            // Draw the model.
            foreach (ModelMesh mesh in Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    IEffectMatrices effectMatricies = effect as IEffectMatrices;
                    if (effectMatricies != null)
                    {						
                        effectMatricies.World = _sharedDrawBoneMatrices[mesh.ParentBone.Index] * world;
                        effectMatricies.View = view;
                        effectMatricies.Projection = projection;
                    }
                    else
                        throw new InvalidOperationException("This model contains a custom effect which does not implement the IEffectMatrices interface, so it cannot be drawn using Model.Draw(...). Instead, call ModelMesh.Draw() after setting the appropriate effect parameters.");
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
        /// </summary>
        /// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
        public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");
            if (destinationBoneTransforms.Length < this.Bones.Count)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");
            int count = this.Bones.Count;
            for (int i = 0; i < count; i++)
            {
                ModelBone modelBone = (this.Bones)[i];
                if (modelBone.Parent == null)
                {
                    destinationBoneTransforms[i] = modelBone.transform;
                }
                else
                {
                    int index = modelBone.Parent.Index;
                    Matrix.Multiply(ref modelBone.transform, ref destinationBoneTransforms[index], out destinationBoneTransforms[i]);
                }
            }
        }

        /// <summary>
        /// Copies bone transforms relative to <see cref="Model.Root"/> bone from a given array to this model.
        /// </summary>
        /// <param name="sourceBoneTransforms">The array of prepared bone transform data.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sourceBoneTransforms"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceBoneTransforms"/> is invalid.
        /// </exception>
        public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms)
        {
            if (sourceBoneTransforms == null)
                throw new ArgumentNullException("sourceBoneTransforms");
            if (sourceBoneTransforms.Length < Bones.Count)
                throw new ArgumentOutOfRangeException("sourceBoneTransforms");

            int count = Bones.Count;
            for (int i = 0; i < count; i++)
            {
                Bones[i].Transform = sourceBoneTransforms[i];
            }
        }

        /// <summary>
        /// Copies bone transforms relative to <see cref="Model.Root"/> bone from this model to a given array.
        /// </summary>
        /// <param name="destinationBoneTransforms">The array receiving the transformed bones.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destinationBoneTransforms"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="destinationBoneTransforms"/> is invalid.
        /// </exception>
        public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");
            if (destinationBoneTransforms.Length < Bones.Count)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            int count = Bones.Count;
            for (int i = 0; i < count; i++)
            {
                destinationBoneTransforms[i] = Bones[i].Transform;
            }
        }
    }
}
