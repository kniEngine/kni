// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Model - KNI")]
    public class ModelProcessor : ContentProcessor<NodeContent, ModelContent>
    {
        private ContentIdentity _identity;

        #region Fields for default values

        private bool _colorKeyEnabled = true;
        private bool _generateMipmaps = true;
        private bool _premultiplyTextureAlpha = true;
        private bool _premultiplyVertexColors = true;
        private float _scale = 1.0f;
        private TextureProcessorOutputFormat _textureFormat = TextureProcessorOutputFormat.Compressed;

        #endregion

        public ModelProcessor()
        {
        }

        #region Properties

        public virtual Color ColorKeyColor { get; set; }

        [DefaultValue(true)]
        public virtual bool ColorKeyEnabled
        {
            get { return _colorKeyEnabled; }
            set { _colorKeyEnabled = value; }
        }

        public virtual MaterialProcessorDefaultEffect DefaultEffect { get; set; }

        [DefaultValue(true)]
        public virtual bool GenerateMipmaps
        {
            get { return _generateMipmaps; }
            set { _generateMipmaps = value; }
        }

        public virtual bool GenerateTangentFrames { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyTextureAlpha
        {
            get { return _premultiplyTextureAlpha; }
            set { _premultiplyTextureAlpha = value; }
        }

        [DefaultValue(true)]
        public virtual bool PremultiplyVertexColors
        {
            get { return _premultiplyVertexColors; }
            set { _premultiplyVertexColors = value; }
        }

        public virtual bool ResizeTexturesToPowerOfTwo { get; set; }

        public virtual float RotationX { get; set; }

        public virtual float RotationY { get; set; }

        public virtual float RotationZ { get; set; }

        [DefaultValue(1.0f)]
        public virtual float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public virtual bool SwapWindingOrder { get; set; }

		[DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat
        {
            get { return _textureFormat; }
            set { _textureFormat = value; }
        }

        #endregion

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            _identity = input.Identity;

            // Perform the processor transforms.
            if (RotationX != 0.0f || RotationY != 0.0f || RotationZ != 0.0f || Scale != 1.0f)
            {
                Matrix rotX  = Matrix.CreateRotationX(MathHelper.ToRadians(RotationX));
                Matrix rotY  = Matrix.CreateRotationY(MathHelper.ToRadians(RotationY));
                Matrix rotZ  = Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ));
                Matrix scale = Matrix.CreateScale(Scale);
                MeshHelper.TransformScene(input, rotZ * rotX * rotY * scale);
            }

            // Gather all the nodes in tree traversal order.
            List<NodeContent> nodes = input.AsEnumerable().SelectDeep(n => n.Children).ToList();

            List<MeshContent> meshes = nodes.FindAll(n => n is MeshContent).Cast<MeshContent>().ToList();
            List<GeometryContent> geometries = meshes.SelectMany(m => m.Geometry).ToList();
            List<MaterialContent> distinctMaterials = geometries.Select(g => g.Material).Distinct().ToList();

            // Loop through all distinct materials, passing them through the conversion method
            // only once, and then processing all geometries using that material.
            foreach (MaterialContent inputMaterial in distinctMaterials)
            {
                List<GeometryContent> geomsWithMaterial = geometries.Where(g => g.Material == inputMaterial).ToList();
                MaterialContent material = ConvertMaterial(inputMaterial, context);

                ProcessGeometryUsingMaterial(material, geomsWithMaterial, context);
            }

            List<ModelBoneContent> boneList = new List<ModelBoneContent>();
            List<ModelMeshContent> meshList = new List<ModelMeshContent>();
            ModelBoneContent rootNode = ProcessNode(input, null, boneList, meshList, context);

            return new ModelContent(rootNode, boneList, meshList);
        }

        private ModelBoneContent ProcessNode(NodeContent node, ModelBoneContent parent, List<ModelBoneContent> boneList, List<ModelMeshContent> meshList, ContentProcessorContext context)
        {
            ModelBoneContent result = new ModelBoneContent(node.Name, boneList.Count, node.Transform, parent);
            boneList.Add(result);

            if (node is MeshContent)
                meshList.Add(ProcessMesh(node as MeshContent, result, context));

            List<ModelBoneContent> children = new List<ModelBoneContent>();
            foreach (NodeContent child in node.Children)
                children.Add(ProcessNode(child, result, boneList, meshList, context));
            result.Children = new ModelBoneContentCollection(children);

            return result;
        }

        private ModelMeshContent ProcessMesh(MeshContent mesh, ModelBoneContent parent, ContentProcessorContext context)
        {
            List<ModelMeshPartContent> parts = new List<ModelMeshPartContent>();
            VertexBufferContent vertexBuffer = null;
            IndexCollection indexBuffer = new IndexCollection();
            
            if (GenerateTangentFrames)
            {
                context.Logger.LogMessage("Generating tangent frames.");
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Normal(0)))
                    {
                        MeshHelper.CalculateNormals(geometry, true);
                    }

                    if(!geometry.Vertices.Channels.Contains(VertexChannelNames.Tangent(0))
                    || !geometry.Vertices.Channels.Contains(VertexChannelNames.Binormal(0)))
                    {
                        MeshHelper.CalculateTangentFrames(geometry, VertexChannelNames.TextureCoordinate(0), VertexChannelNames.Tangent(0),
                            VertexChannelNames.Binormal(0));
                    }
                }
            }

            int startVertex = 0;
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                VertexContent vertices = geometry.Vertices;
                int vertexCount = vertices.VertexCount;
                ModelMeshPartContent partContent;
                if (vertexCount == 0)
                {
                    partContent = new ModelMeshPartContent();
                }
                else
                {
                    if (vertexBuffer == null)
                    {
                        vertexBuffer = geometry.Vertices.CreateVertexBuffer();
                    }
                    else
                    {
                        VertexBufferContent geomBuffer = geometry.Vertices.CreateVertexBuffer();
                        
                        if (vertexBuffer.VertexDeclaration.VertexStride != geomBuffer.VertexDeclaration.VertexStride
                        ||  vertexBuffer.VertexDeclaration.VertexElements.Count != geomBuffer.VertexDeclaration.VertexElements.Count)
                            throw new InvalidOperationException("Invalid geometry");

                        int bufferOffset = vertexBuffer.VertexData.Length;
                        vertexBuffer.Write(bufferOffset, 1, geomBuffer.VertexData);
                    }

                    int startIndex = indexBuffer.Count;
                    indexBuffer.AddRange(geometry.Indices);

                    partContent = new ModelMeshPartContent(vertexBuffer, indexBuffer, startVertex, vertexCount, startIndex, geometry.Indices.Count / 3);

                    startVertex += vertexCount;
                }

                partContent.Material = geometry.Material;
                parts.Add(partContent);
            }

            BoundingSphere bounds = new BoundingSphere();
            if (mesh.Positions.Count > 0)
                bounds = BoundingSphere.CreateFromPoints(mesh.Positions);

            return new ModelMeshContent(mesh.Name, mesh, parent, bounds, parts);
        }

        protected virtual MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            OpaqueDataDictionary parameters = new OpaqueDataDictionary();
            parameters.Add("ColorKeyColor", ColorKeyColor);
            parameters.Add("ColorKeyEnabled", ColorKeyEnabled);
            parameters.Add("GenerateMipmaps", GenerateMipmaps);
            parameters.Add("PremultiplyTextureAlpha", PremultiplyTextureAlpha);
            parameters.Add("ResizeTexturesToPowerOfTwo", ResizeTexturesToPowerOfTwo);
            parameters.Add("TextureFormat", TextureFormat);
            parameters.Add("DefaultEffect", DefaultEffect);

            return context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor", parameters);
        }

        protected virtual void ProcessGeometryUsingMaterial(MaterialContent material,
                                                            IEnumerable<GeometryContent> geometryCollection,
                                                            ContentProcessorContext context)
        {
            // If we don't get a material then assign a default one.
            if (material == null)
                material = MaterialProcessor.CreateDefaultMaterial(DefaultEffect);

            // Test requirements from the assigned material.
            int textureChannels;
            bool vertexWeights = false;
            if (material is DualTextureMaterialContent)
            {
                textureChannels = 2;
            }
            else if (material is SkinnedMaterialContent)
            {
                textureChannels = 1;
                vertexWeights = true;
            }
            else if (material is EnvironmentMapMaterialContent)
            {
                textureChannels = 1;
            }
            else if (material is AlphaTestMaterialContent)
            {
                textureChannels = 1;
            }
            else
            {
                // Just check for a "Texture" which should cover custom Effects
                // and BasicEffect which can have an optional texture.
                textureChannels = material.Textures.ContainsKey("Texture") ? 1 : 0;                
            }

            // By default we must set the vertex color property
            // to match XNA behavior.
            material.OpaqueData["VertexColorEnabled"] = false;

            // If we run into a geometry that requires vertex
            // color we need a seperate material for it.
            MaterialContent colorMaterial = material.Clone();
            colorMaterial.OpaqueData["VertexColorEnabled"] = true;    

            foreach (GeometryContent geometry in geometryCollection)
            {
                // Process the geometry.
                for (int i = 0; i < geometry.Vertices.Channels.Count; i++)
                    ProcessVertexChannel(geometry, i, context);

                // Verify we have the right number of texture coords.
                for (int i = 0; i < textureChannels; i++)
                {
                    if (!geometry.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(i)))
                        throw new InvalidContentException(
                            String.Format("The mesh \"{0}\", using {1}, contains geometry that is missing texture coordinates for channel {2}.", 
                            geometry.Parent.Name,
                            MaterialProcessor.GetDefaultEffect(material),
                            i),
                            _identity);
                }

                // Do we need to enable vertex color?
                if (geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
                    geometry.Material = colorMaterial;
                else
                    geometry.Material = material;

                // Do we need vertex weights?
                if (vertexWeights)
                {
                    string weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, 0);
                    if (!geometry.Vertices.Channels.Contains(weightsName))
                        throw new InvalidContentException(
                            String.Format("The skinned mesh \"{0}\" contains geometry without any vertex weights.", geometry.Parent.Name),
                            _identity);                    
                }
            }
        }

        protected virtual void ProcessVertexChannel(GeometryContent geometry,
                                                    int vertexChannelIndex,
                                                    ContentProcessorContext context)
        {
            VertexChannel vertexChannel = geometry.Vertices.Channels[vertexChannelIndex];

            // TODO: According to docs, channels with VertexElementUsage.Color -> Color

            // Channels[VertexChannelNames.Weights] -> { Byte4 boneIndices, Color boneWeights }
            if (vertexChannel.Name.StartsWith(VertexChannelNames.Weights()))
                ProcessWeightsChannel(geometry, vertexChannelIndex, _identity);
        }

        private static void ProcessWeightsChannel(GeometryContent geometry, int vertexChannelIndex, ContentIdentity identity)
        {
            // NOTE: Portions of this code is from the XNA CPU Skinning 
            // sample under Ms-PL, (c) Microsoft Corporation.

            // create a map of Name->Index of the bones
            BoneContent skeleton = MeshHelper.FindSkeleton(geometry.Parent);
            if (skeleton == null)
            {
                throw new InvalidContentException(
                    "Skeleton not found. Meshes that contain a Weights vertex channel cannot be processed without access to the skeleton data.",
                    identity);                     
            }

            Dictionary<string, int> boneIndices = new Dictionary<string, int>();
            IList<BoneContent> flattenedBones = MeshHelper.FlattenSkeleton(skeleton);
            for (int i = 0; i < flattenedBones.Count; i++)
                boneIndices.Add(flattenedBones[i].Name, i);

            VertexChannel vertexChannel = geometry.Vertices.Channels[vertexChannelIndex];
            VertexChannel<BoneWeightCollection> inputWeights = vertexChannel as VertexChannel<BoneWeightCollection>;
            if (inputWeights == null)
            {
                throw new InvalidContentException(
                    String.Format(
                        "Vertex channel \"{0}\" is the wrong type. It has element type {1}. Type {2} is expected.",
                        vertexChannel.Name,
                        vertexChannel.ElementType.FullName,
                        "Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneWeightCollection"),
                    identity);                          
            }
            Byte4[] outputIndices = new Byte4[inputWeights.Count];
            Vector4[] outputWeights = new Vector4[inputWeights.Count];
            for (int i = 0; i < inputWeights.Count; i++)
                ConvertWeights(inputWeights[i], boneIndices, outputIndices, outputWeights, i);

            // create our new channel names
            int usageIndex = VertexChannelNames.DecodeUsageIndex(inputWeights.Name);
            string indicesName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, usageIndex);
            string weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, usageIndex);

            // add in the index and weight channels
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 1, indicesName, outputIndices);
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 2, weightsName, outputWeights);

            // remove the original weights channel
            geometry.Vertices.Channels.RemoveAt(vertexChannelIndex);
        }

        // From the XNA CPU Skinning Sample under Ms-PL, (c) Microsoft Corporation
        private static void ConvertWeights(BoneWeightCollection weights, Dictionary<string, int> boneIndices, Byte4[] outIndices, Vector4[] outWeights, int vertexIndex)
        {
            // we only handle 4 weights per bone
            const int maxWeights = 4;

            // create some temp arrays to hold our values
            int[] tempIndices = new int[maxWeights];
            float[] tempWeights = new float[maxWeights];

            // cull out any extra bones
            weights.NormalizeWeights(maxWeights);

            // get our indices and weights
            for (int i = 0; i < weights.Count; i++)
            {
                BoneWeight weight = weights[i];

                if (!boneIndices.ContainsKey(weight.BoneName))
                {
                    string errorMessage = String.Format("Bone '{0}' was not found in the skeleton! Skeleton bones are: '{1}'.", weight.BoneName, string.Join("', '", boneIndices.Keys));
                    throw new Exception(errorMessage);
                }

                tempIndices[i] = boneIndices[weight.BoneName];
                tempWeights[i] = weight.Weight;
            }

            // zero out any remaining spaces
            for (int i = weights.Count; i < maxWeights; i++)
            {
                tempIndices[i] = 0;
                tempWeights[i] = 0;
            }

            // output the values
            outIndices[vertexIndex] = new Byte4(tempIndices[0], tempIndices[1], tempIndices[2], tempIndices[3]);
            outWeights[vertexIndex] = new Vector4(tempWeights[0], tempWeights[1], tempWeights[2], tempWeights[3]);
        }
    }

    internal static class ModelEnumerableExtensions
    {
        /// <summary>
        /// Returns each element of a tree structure in hierarchical order.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The enumeration to traverse.</param>
        /// <param name="selector">A function which returns the children of the element.</param>
        /// <returns>An IEnumerable whose elements are in tree structure heriarchical order.</returns>
        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            Stack<T> stack = new Stack<T>(source.Reverse());
            while (stack.Count > 0)
            {
                // Return the next item on the stack.
                T item = stack.Pop();
                yield return item;

                // Get the children from this item.
                IEnumerable<T> children = selector(item);

                // If we have no children then skip it.
                if (children == null)
                    continue;

                // We're using a stack, so we need to push the
                // children on in reverse to get the correct order.
                foreach (T child in children.Reverse())
                    stack.Push(child);
            }
        }

        /// <summary>
        /// Returns an enumerable from a single element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
