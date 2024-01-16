// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class ModelReader : ContentTypeReader<Model>
    {
//      List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();
//      List<IndexBuffer> indexBuffers = new List<IndexBuffer>();
//      List<Effect> effects = new List<Effect>();
//      List<GraphicsResource> sharedResources = new List<GraphicsResource>();

        protected internal override Model Read(ContentReader input, Model existingInstance)
        {
            // Read the bone names and transforms.
            uint boneCount = input.ReadUInt32();
            bool is8BitBoneReference = (boneCount < 255);
            //Debug.WriteLine("Bone count: {0}", boneCount);

            List<ModelBone> bones = new List<ModelBone>((int)boneCount);

            for (uint i = 0; i < boneCount; i++)
            {
                string name = input.ReadObject<string>();
                var matrix = input.ReadMatrix();
                var bone = new ModelBone { Transform = matrix, Index = (int)i, Name = name };
                bones.Add(bone);
            }
            
            // Read the bone hierarchy.
            for (int i = 0; i < boneCount; i++)
            {
                var bone = bones[i];

                //Debug.WriteLine("Bone {0} hierarchy:", i);

                // Read the parent bone reference.
                //Debug.WriteLine("Parent: ");
                var parentIndex = ReadBoneReference(input, is8BitBoneReference);

                if (parentIndex != -1)
                {
                    bone.Parent = bones[parentIndex];
                }

                // Read the child bone references.
                uint childCount = input.ReadUInt32();

                if (childCount != 0)
                {
                    //Debug.WriteLine("Children:");

                    for (uint j = 0; j < childCount; j++)
                    {
                        var childIndex = ReadBoneReference(input, is8BitBoneReference);
                        if (childIndex != -1)
                        {
                            bone.AddChild(bones[childIndex]);
                        }
                    }
                }
            }

            List<ModelMesh> meshes = new List<ModelMesh>();

            //// Read the mesh data.
            int meshCount = input.ReadInt32();
            //Debug.WriteLine("Mesh count: {0}", meshCount);

            for (int i = 0; i < meshCount; i++)
            {

                //Debug.WriteLine("Mesh {0}", i);
                string name = input.ReadObject<string>();
                var parentBoneIndex = ReadBoneReference(input, is8BitBoneReference);
                //Opt: var boundingSphere = input.ReadRawObject<BoundingSphere>();
                var boundingSphere = new BoundingSphere(input.ReadVector3(), input.ReadSingle());

                // Tag
                var meshTag = input.ReadObject<object>();

                // Read the mesh part data.
                int partCount = input.ReadInt32();
                //Debug.WriteLine("Mesh part count: {0}", partCount);

                List<ModelMeshPart> parts = new List<ModelMeshPart>(partCount);

                for (uint j = 0; j < partCount; j++)
                {
                    ModelMeshPart part;
                    if (existingInstance != null)
                        part = existingInstance.Meshes[i].MeshParts[(int)j];
                    else
                        part = new ModelMeshPart();

                    part.VertexOffset = input.ReadInt32();
                    part.NumVertices = input.ReadInt32();
                    part.StartIndex = input.ReadInt32();
                    part.PrimitiveCount = input.ReadInt32();

                    // tag
                    part.Tag = input.ReadObject<object>();
                    
                    parts.Add(part);
                    
                    int jj = (int)j;
                    input.ReadSharedResource<VertexBuffer>(delegate (VertexBuffer v)
                    {
                        parts[jj].VertexBuffer = v;
                    });
                    input.ReadSharedResource<IndexBuffer>(delegate (IndexBuffer v)
                    {
                        parts[jj].IndexBuffer = v;
                    });
                    input.ReadSharedResource<Effect>(delegate (Effect v)
                    {
                        parts[jj].Effect = v;
                    });

                    
                }

                if (existingInstance != null)
                    continue;

                ModelMesh mesh = new ModelMesh(input.GetGraphicsDevice(), parts);

                // Tag reassignment
                mesh.Tag = meshTag;

                mesh.Name = name;
                mesh.ParentBone = bones[parentBoneIndex];
                mesh.ParentBone.AddMesh(mesh);
                mesh.BoundingSphere = boundingSphere;
                meshes.Add(mesh);
            }

            if (existingInstance != null)
            {
                // Read past remaining data and return existing instance
                ReadBoneReference(input, is8BitBoneReference);
                input.ReadObject<object>();
                return existingInstance;
            }

            // Read the final pieces of model data.
            var rootBoneIndex = ReadBoneReference(input, is8BitBoneReference);

            Model model = new Model(input.GetGraphicsDevice(), bones, meshes);

            model.Root = bones[rootBoneIndex];

            // Tag?
            model.Tag = input.ReadObject<object>();
            
            return model;
        }

        static int ReadBoneReference(ContentReader input, bool is8BitBoneReference)
        {
            // Read the bone ID, which may be encoded as either an 8 or 32 bit value.
            uint boneId = (is8BitBoneReference)
                        ? input.ReadByte()
                        : input.ReadUInt32()
                        ;

            // Print out the bone ID.
            //if (boneId == 0)
            //    Debug.WriteLine("null");
            //else
            //    Debug.WriteLine("bone #{0}", boneId - 1);

            return (int)(boneId - 1);
        }

    }
}

