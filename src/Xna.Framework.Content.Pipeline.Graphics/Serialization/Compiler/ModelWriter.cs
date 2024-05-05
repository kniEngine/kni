// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class ModelWriter : ContentTypeWriterBase<ModelContent>
    {
        protected override void Write(ContentWriter output, ModelContent value)
        {
            WriteBones(output, value.Bones);

            output.Write((uint)value.Meshes.Count);
            foreach (ModelMeshContent mesh in value.Meshes)
            {
                output.WriteObject(mesh.Name);
                WriteBoneReference(output, mesh.ParentBone, value.Bones);
                WriteBoundingSphere(output, mesh.BoundingSphere);
                output.WriteObject(mesh.Tag);

                output.Write((uint)mesh.MeshParts.Count);
                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    output.Write((uint)part.VertexOffset);
                    output.Write((uint)part.NumVertices);
                    output.Write((uint)part.StartIndex);
                    output.Write((uint)part.PrimitiveCount);
                    output.WriteObject(part.Tag);

                    output.WriteSharedResource(part.VertexBuffer);
                    output.WriteSharedResource(part.IndexBuffer);
                    output.WriteSharedResource(part.Material);
                }
            }

            WriteBoneReference(output, value.Root, value.Bones);
            output.WriteObject(value.Tag);
        }

        private void WriteBones(ContentWriter output, ModelBoneContentCollection bones)
        {
            output.Write((uint)bones.Count);

            // Bone properties
            foreach (ModelBoneContent bone in bones)
            {
                output.WriteObject(bone.Name);
                output.Write(bone.Transform);
            }

            // Hierarchy
            foreach (ModelBoneContent bone in bones)
            {
                WriteBoneReference(output, bone.Parent, bones);

                output.Write((uint)bone.Children.Count);
                foreach (ModelBoneContent child in bone.Children)
                    WriteBoneReference(output, child, bones);
            }
        }

        private void WriteBoneReference(ContentWriter output, ModelBoneContent bone, ModelBoneContentCollection bones)
        {
            int boneCount = (bones != null)
                          ? bones.Count
                          : 0;
            int boneId = (bone != null)
                       ? bone.Index + 1
                       : 0;

            if (boneCount < 255)
                output.Write((byte)boneId);
            else
                output.Write((uint)boneId);
        }

        private static void WriteBoundingSphere(ContentWriter output, BoundingSphere value)
        {
            //output.WriteRawObject(boundingSphere);
            output.Write(value.Center);
            output.Write(value.Radius);
        }
    }
}
