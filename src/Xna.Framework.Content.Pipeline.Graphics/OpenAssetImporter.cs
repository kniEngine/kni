// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Assimp;
using Assimp.Unmanaged;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Platform.Utilities;


namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [ContentImporter(
        ".dae", // Collada
        ".gltf", "glb", // glTF
        ".blend", // Blender 3D
        ".3ds", // 3ds Max 3DS
        ".ase", // 3ds Max ASE
        ".obj", // Wavefront Object
        ".ifc", // Industry Foundation Classes (IFC/Step)
        ".xgl", ".zgl", // XGL
        ".ply", // Stanford Polygon Library
        ".dxf", // AutoCAD DXF
        ".lwo", // LightWave
        ".lws", // LightWave Scene
        ".lxo", // Modo
        ".stl", // Stereolithography
        ".ac", // AC3D
        ".ms3d", // Milkshape 3D
        ".cob", ".scn", // TrueSpace
        ".bvh", // Biovision BVH
        ".csm", // CharacterStudio Motion
        ".irrmesh", // Irrlicht Mesh
        ".irr", // Irrlicht Scene
        ".mdl", // Quake I, 3D GameStudio (3DGS)
        ".md2", // Quake II
        ".md3", // Quake III Mesh
        ".pk3", // Quake III Map/BSP
        ".mdc", // Return to Castle Wolfenstein
        ".md5", // Doom 3
        ".smd", ".vta", // Valve Model 
        ".ogex", // Open Game Engine Exchange
        ".3d", // Unreal
        ".b3d", // BlitzBasic 3D
        ".q3d", ".q3s", // Quick3D
        ".nff", // Neutral File Format, Sense8 WorldToolKit
        ".off", // Object File Format
        ".ter", // Terragen Terrain
        ".hmp", // 3D GameStudio (3DGS) Terrain
        ".ndo", // Izware Nendo
        DisplayName = "Open Asset Import Library - KNI", DefaultProcessor = "ModelProcessor")]
    public class OpenAssetImporter : ContentImporter<NodeContent>
    {
        // Assimp has a few limitations (not all FBX files are supported):
        // FBX files reference objects using IDs. Therefore, it is possible to resolve
        // bones even if multiple bones/nodes have the same name. But Assimp references
        // bones only by name!
        // --> Limitation #1: A model cannot have more than one skeleton!
        // --> Limitation #2: Bone names need to be unique!
        //
        // Bones are represented by regular nodes, but there is no flag indicating whether
        // a node is a bone. A mesh in Assimp references deformation bones (= bones that
        // affect vertices) by name. That means, we can identify the nodes that represent
        // deformation bones. But there is no way to identify helper bones (= bones that 
        // belong to the skeleton, but do not affect vertices). As described in 
        // http://assimp.sourceforge.net/lib_html/data.html and 
        // http://gamedev.stackexchange.com/questions/26382/i-cant-figure-out-how-to-animate-my-loaded-model-with-assimp/26442#26442
        // we can only guess which nodes belong to a skeleton:
        // --> Limitation #3: The skeleton needs to be a direct child of the root node or
        //                    the mesh node!
        //
        // Node.Transform is irrelevant for bones. This transform is just the pose of the
        // bone at the time of the export. This could be one of the animation frames. It
        // is not necessarily the bind pose (rest pose)! For example, XNA's Dude.fbx does
        // NOT store the skeleton in bind pose.
        // The correct transform is stored in Mesh.Bones[i].OffsetMatrix. However, this
        // information is only available for deformation bones, not for helper bones.
        // --> Limitation #4: The skeleton either must not contain helper bones, or it must
        //                    be guaranteed that the skeleton is exported in bind pose!
        //
        // An FBX file does not directly store all animation values. In some FBX scene it
        // is insufficient to simply read the animation data from the file. Instead, the
        // animation properties of all relevant objects in the scene need to be evaluated.
        // For example, some animations are the result of the current skeleton pose + the
        // current animation. The current skeleton pose is not imported/processed by XNA.
        // Assimp does not include an "animation evaluater" that automatically bakes these
        // animations.
        // --> Limitation #5: All bones included in an animation need to be key framed.
        //                    (There is no automatic evaluation.)
        //
        // In FBX it is possible to define animations curves for some transform components
        // (e.g. translation X and Y) and leave other components (e.g. translation Z) undefined.
        // Assimp does not pick the right defaults for undefined components.
        // --> Limitation #6: When scale, rotation, or translation is animated, all components
        //                    X, Y, Z need to be key framed.

        #region Nested Types
        /// <summary>Defines the frame for local scale/rotation/translation of FBX nodes.</summary>
        /// <remarks>
        /// <para>
        /// The transformation pivot defines the frame for local scale/rotation/translation. The
        /// local transform of a node is:
        /// </para>
        /// <para>
        /// Local Transform = Translation * RotationOffset * RotationPivot * PreRotation
        ///                   * Rotation * PostRotation * RotationPivotInverse * ScalingOffset
        ///                   * ScalingPivot * Scaling * ScalingPivotInverse
        /// </para>
        /// <para>
        /// where the matrix multiplication order is right-to-left.
        /// </para>
        /// <para>
        /// 3ds max uses three additional transformations:
        /// </para>
        /// <para>
        /// Local Transform = Translation * Rotation * Scaling
        ///                   * GeometricTranslation * GeometricRotation * GeometricScaling
        /// </para>
        /// <para>
        /// Transformation pivots are stored per FBX node. When Assimp hits an FBX node with
        /// a transformation pivot it generates additional nodes named
        /// </para>
        /// <para>
        ///   <i>OriginalName</i>_$AssimpFbx$_<i>TransformName</i>
        /// </para>
        /// <para>
        /// where <i>TransformName</i> is one of: 
        /// </para>
        /// <para>
        ///   Translation, RotationOffset, RotationPivot, PreRotation, Rotation, PostRotation,
        ///   RotationPivotInverse, ScalingOffset, ScalingPivot, Scaling, ScalingPivotInverse,
        ///   GeometricTranslation, GeometricRotation, GeometricScaling
        /// </para>
        /// </remarks>
        /// <seealso href="http://download.autodesk.com/us/fbx/20112/FBX_SDK_HELP/index.html?url=WS1a9193826455f5ff1f92379812724681e696651.htm,topicNumber=d0e7429"/>
        /// <seealso href="http://area.autodesk.com/forum/autodesk-fbx/fbx-sdk/the-makeup-of-the-local-matrix-of-an-kfbxnode/"/>
        private class FbxPivot
        {
            public enum PivotType
            {
                Invalid,
                Translation,
                RotationOffset,
                RotationPivot,
                PreRotation,
                Rotation,
                PostRotation,
                RotationPivotInverse,
                ScalingOffset,
                ScalingPivot,
                Scaling,
                ScalingPivotInverse,
                GeometricTranslation,
                GeometricRotation,
                GeometricScaling,
            }

            public PivotType Type;
            public Matrix Transform;

            public static PivotType GetPivotType(string nodeName)
            {
                if (nodeName.EndsWith("_Translation"))
                    return PivotType.Translation;
                else if (nodeName.EndsWith("_RotationOffset"))
                    return PivotType.RotationOffset;
                else if (nodeName.EndsWith("_RotationPivot"))
                    return PivotType.RotationPivot;
                else if (nodeName.EndsWith("_PreRotation"))
                    return PivotType.PreRotation;
                else if (nodeName.EndsWith("_Rotation"))
                    return PivotType.Rotation;
                else if (nodeName.EndsWith("_PostRotation"))
                    return PivotType.PostRotation;
                else if (nodeName.EndsWith("_RotationPivotInverse"))
                    return PivotType.RotationPivotInverse;
                else if (nodeName.EndsWith("_ScalingOffset"))
                    return PivotType.ScalingOffset;
                else if (nodeName.EndsWith("_ScalingPivot"))
                    return PivotType.ScalingPivot;
                else if (nodeName.EndsWith("_Scaling"))
                    return PivotType.Scaling;
                else if (nodeName.EndsWith("_ScalingPivotInverse"))
                    return PivotType.ScalingPivotInverse;
                else if (nodeName.EndsWith("_GeometricTranslation"))
                    return PivotType.GeometricTranslation;
                else if (nodeName.EndsWith("_GeometricRotation"))
                    return PivotType.GeometricRotation;
                else if (nodeName.EndsWith("_GeometricScaling"))
                    return PivotType.GeometricScaling;
                else
                    return PivotType.Invalid;
            }

            public Matrix GetTransform()
            {
                if (this.Type != PivotType.Invalid)
                    return this.Transform;
                else
                    return Matrix.Identity;
            }
        }
        #endregion

        private static readonly List<VectorKey> EmptyVectorKeys = new List<VectorKey>(0);
        private static readonly List<QuaternionKey> EmptyQuaternionKeys = new List<QuaternionKey>(0);

        private readonly string _importerName;

        // XNA Content importer
        private ContentIdentity _identity;

        // Assimp scene
        private Dictionary<string, Matrix> _deformationBones;   // The names and offset matrices of all deformation bones.
        private Node _rootBone;                                 // The node that represents the root bone.
        private List<Node> _bones = new List<Node>();           // All nodes attached to the root bone.
        private Dictionary<string, FbxPivot> _pivots;           // The transformation pivots.


        /// <summary>
        /// Default constructor.
        /// </summary>
        public OpenAssetImporter() : this("OpenAssetImporter")
        {
        }

        internal OpenAssetImporter(string importerName)
        {            
            _importerName = importerName;
        }

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (context == null)
                throw new ArgumentNullException("context");

            if (CurrentPlatform.OS == OS.Linux)
            {
                string targetDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

                try
                {
                    AssimpLibrary.Instance.LoadLibrary(
                        Path.Combine(targetDir, "libassimp.so"),
                        Path.Combine(targetDir, "libassimp.so"));
                }
                catch { }
            }

            _identity = new ContentIdentity(filename, _importerName);

            using (AssimpContext importer = new AssimpContext())
            {
                // TODO: check if we can set FBXPreservePivotsConfig(false) and clean up the code.

                // FBXPreservePivotsConfig(false) can be set to remove transformation
                // pivots. However, Assimp does not automatically correct animations!
                // --> Leave default settings, handle transformation pivots explicitly.
                //importer.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));

                // Set flag to remove degenerate faces (points and lines).
                // This flag is very important when PostProcessSteps.FindDegenerates is used
                // because FindDegenerates converts degenerate triangles to points and lines!
                importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesConfig(true));
                // Set flag to not remove small triangles when PostProcessSteps.FindDegenerates is used.
                //importer.SetConfig(new Assimp.Configs.RemoveDegeneratePrimitivesCheckAreaConfig(false));

                // Note about Assimp post-processing:
                // Keep post-processing to a minimum. The ModelImporter should import
                // the model as is. We don't want to lose any information, i.e. empty
                // nodes shoud not be thrown away, meshes/materials should not be merged,
                // etc. Custom model processors may depend on this information!
                Scene aiScene = importer.ImportFile(filename,
                    //PostProcessSteps.FindDegenerates |
                    PostProcessSteps.FindInvalidData |
                    PostProcessSteps.FlipUVs |              // Required for Direct3D
                    PostProcessSteps.FlipWindingOrder |     // Required for Direct3D
                    PostProcessSteps.JoinIdenticalVertices |
                    PostProcessSteps.ImproveCacheLocality |
                    PostProcessSteps.OptimizeMeshes |
                    PostProcessSteps.Triangulate

                    // Unused: 
                    //PostProcessSteps.CalculateTangentSpace
                    //PostProcessSteps.Debone |
                    //PostProcessSteps.FindInstances |      // No effect + slow?
                    //PostProcessSteps.FixInFacingNormals |
                    //PostProcessSteps.GenerateNormals |
                    //PostProcessSteps.GenerateSmoothNormals |
                    //PostProcessSteps.GenerateUVCoords | // Might be needed... find test case
                    //PostProcessSteps.LimitBoneWeights |
                    //PostProcessSteps.MakeLeftHanded |     // Not necessary, XNA is right-handed.
                    //PostProcessSteps.OptimizeGraph |      // Will eliminate helper nodes
                    //PostProcessSteps.PreTransformVertices |
                    //PostProcessSteps.RemoveComponent |
                    //PostProcessSteps.RemoveRedundantMaterials |
                    //PostProcessSteps.SortByPrimitiveType |
                    //PostProcessSteps.SplitByBoneCount |
                    //PostProcessSteps.SplitLargeMeshes |
                    //PostProcessSteps.TransformUVCoords |
                    //PostProcessSteps.ValidateDataStructure |
                    );

                // Find _rootBone, _bones, _deformationBones.
                FindSkeleton(aiScene);

                // Create _materials.
                List<MaterialContent> materials = ImportMaterials(aiScene.Materials);

                // Create _pivots and _rootNode (incl. children).
                NodeContent rootNode = ImportNodes(context, aiScene, materials);

                // If we have a simple hierarchy and just the one mesh,
                // we can flatten it out so the mesh is the root node.
                //if ((rootNode is MeshContent) == false
                //&&  rootNode.Transform == Matrix.Identity
                //&&  rootNode.Children.Count == 1 
                //&&  rootNode.Children[0] is MeshContent)
                //{
                //    Matrix absXform = rootNode.Children[0].Transform;
                //    rootNode = rootNode.Children[0];
                //    rootNode.Identity = _identity;
                //    rootNode.Transform = absXform;
                //    rootNode.Parent = null;
                //}

                // Create skeleton (incl. animations) and add to _rootNode.
                ImportSkeleton(aiScene, rootNode);

                // If we have a simple hierarchy with no bones and just the one
                // mesh, we can flatten it out so the mesh is the root node.
                if (rootNode.Children.Count == 1 && rootNode.Children[0] is MeshContent)
                {
                    Matrix absXform = rootNode.Children[0].AbsoluteTransform;
                    rootNode = rootNode.Children[0];
                    rootNode.Identity = _identity;
                    rootNode.Transform = absXform;
                }

                ImportMetadata(aiScene, rootNode);

                MetaTransform(rootNode);

                aiScene.Clear();

                return rootNode;
            }
        }

        /// <summary>
        /// Converts all Assimp <see cref="Material"/>s to standard XNA compatible <see cref="MaterialContent"/>s.
        /// </summary>
        private List<MaterialContent> ImportMaterials(List<Material> aiMaterials)
        {
            List<MaterialContent> materials = new List<MaterialContent>();

            foreach (Material aiMaterial in aiMaterials)
            {
                // TODO: What about AlphaTestMaterialContent, DualTextureMaterialContent, 
                // EffectMaterialContent, EnvironmentMapMaterialContent, and SkinnedMaterialContent?

                BasicMaterialContent material = new BasicMaterialContent();
                material.Name = aiMaterial.Name;
                material.Identity = _identity;

                if (aiMaterial.HasTextureDiffuse)
                    material.Texture = ImportTextureContentRef(aiMaterial.TextureDiffuse);

                if (aiMaterial.HasTextureOpacity)
                    material.Textures.Add("Transparency", ImportTextureContentRef(aiMaterial.TextureOpacity));

                if (aiMaterial.HasTextureSpecular)
                    material.Textures.Add("Specular", ImportTextureContentRef(aiMaterial.TextureSpecular));

                if (aiMaterial.HasTextureHeight)
                    material.Textures.Add("Bump", ImportTextureContentRef(aiMaterial.TextureHeight));

                if (aiMaterial.HasColorDiffuse)
                    material.DiffuseColor = ToXna(aiMaterial.ColorDiffuse);

                if (aiMaterial.HasColorEmissive)
                    material.EmissiveColor = ToXna(aiMaterial.ColorEmissive);

                if (aiMaterial.HasOpacity)
                    material.Alpha = aiMaterial.Opacity;

                if (aiMaterial.HasColorSpecular)
                    material.SpecularColor = ToXna(aiMaterial.ColorSpecular);

                if (aiMaterial.HasShininessStrength)
                    material.SpecularPower = aiMaterial.ShininessStrength; // aiMaterial.Shininess; // TNC: maintain backward compatibility. Should this be (ShininessStrength*Shininess)?
                
                materials.Add(material);
            }

            return materials;
        }

        private ExternalReference<TextureContent> ImportTextureContentRef(TextureSlot textureSlot, bool ext = false)
        {
            ExternalReference<TextureContent> texture = new ExternalReference<TextureContent>(textureSlot.FilePath, _identity);
            texture.OpaqueData.Add("TextureCoordinate", String.Format("TextureCoordinate{0}", textureSlot.UVIndex));

            // ext is set by ImportMaterialsEx()
            if (ext)
            {
                texture.OpaqueData.Add("Operation", textureSlot.Operation.ToString());
                texture.OpaqueData.Add("AddressU", textureSlot.WrapModeU.ToString());
                texture.OpaqueData.Add("AddressV", textureSlot.WrapModeV.ToString());
                texture.OpaqueData.Add("Mapping", textureSlot.Mapping.ToString());
            }

            return texture;
        }            

        /// <summary>
        /// Returns all the Assimp <see cref="Material"/> features as a <see cref="MaterialContent"/>.
        /// </summary>
        private List<MaterialContent> ImportMaterialsExt(List<Material> aiMaterials)
        {
            List<MaterialContent> materials = new List<MaterialContent>();

            foreach (Material aiMaterial in aiMaterials)
            {
                MaterialContent material = new MaterialContent();
                material.Name = aiMaterial.Name;
                material.Identity = _identity;

                TextureSlot[] textureSlots = aiMaterial.GetAllMaterialTextures();
                foreach (TextureSlot textureSlot in textureSlots)
                {
                    string name;

                    // Force the XNA naming standard for diffuse textures
                    // which allows the material to work with the stock
                    // model processor.
                    if (textureSlot.TextureType == TextureType.Diffuse)
                        name = BasicMaterialContent.TextureKey;
                    else
                        name = textureSlot.TextureType.ToString();

                    // We might have multiple textures of the same type so number
                    // them starting with 2 like in DualTextureMaterialContent.
                    if (textureSlot.TextureIndex > 0)
                        name += (textureSlot.TextureIndex + 1);

                    material.Textures.Add(name, ImportTextureContentRef(textureSlot, true));
                }

                if (aiMaterial.HasBlendMode)
                    material.OpaqueData.Add("BlendMode", aiMaterial.BlendMode.ToString());
                if (aiMaterial.HasBumpScaling)
                    material.OpaqueData.Add("BumpScaling", aiMaterial.BumpScaling);
                if (aiMaterial.HasColorAmbient)
                    material.OpaqueData.Add("AmbientColor", ToXna(aiMaterial.ColorAmbient));
                if (aiMaterial.HasColorDiffuse)
                    material.OpaqueData.Add("DiffuseColor", ToXna(aiMaterial.ColorDiffuse));
                if (aiMaterial.HasColorEmissive)
                    material.OpaqueData.Add("EmissiveColor", ToXna(aiMaterial.ColorEmissive));
                if (aiMaterial.HasColorReflective)
                    material.OpaqueData.Add("ReflectiveColor", ToXna(aiMaterial.ColorReflective));
                if (aiMaterial.HasColorSpecular)
                    material.OpaqueData.Add("SpecularColor", ToXna(aiMaterial.ColorSpecular));
                if (aiMaterial.HasColorTransparent)
                    material.OpaqueData.Add("TransparentColor", ToXna(aiMaterial.ColorTransparent));
                if (aiMaterial.HasOpacity)
                    material.OpaqueData.Add("Opacity", aiMaterial.Opacity);
                if (aiMaterial.HasReflectivity)
                    material.OpaqueData.Add("Reflectivity", aiMaterial.Reflectivity);
                if (aiMaterial.HasShadingMode)
                    material.OpaqueData.Add("ShadingMode", aiMaterial.ShadingMode.ToString());
                if (aiMaterial.HasShininess)
                    material.OpaqueData.Add("Shininess", aiMaterial.Shininess);
                if (aiMaterial.HasShininessStrength)
                    material.OpaqueData.Add("ShininessStrength", aiMaterial.ShininessStrength);
                if (aiMaterial.HasTwoSided)
                    material.OpaqueData.Add("TwoSided", aiMaterial.IsTwoSided);
                if (aiMaterial.HasWireFrame)
                    material.OpaqueData.Add("WireFrame", aiMaterial.IsWireFrameEnabled);

                materials.Add(material);
            }

            return materials;
        }

        /// <summary>
        /// Converts all Assimp nodes to XNA nodes. (Nodes representing bones are excluded!)
        /// </summary>
        private NodeContent ImportNodes(ContentImporterContext context, Scene aiScene, List<MaterialContent> materials)
        {
            _pivots = new Dictionary<string, FbxPivot>();
            NodeContent rootNode = ImportNodes(context, aiScene, aiScene.RootNode, materials, null,  null);
            return rootNode;
        }

        /// <summary>
        /// Converts the specified node and all descendant nodes.
        /// </summary>
        /// <param name="aiNode">The node.</param>
        /// <param name="aiParent">The parent node. Can be <see langword="null"/>.</param>
        /// <param name="parent">The <paramref name="aiParent"/> node converted to XNA.</param>
        /// <returns>The XNA <see cref="NodeContent"/>.</returns>
        /// <remarks>
        /// It may be necessary to skip certain "preserve pivot" nodes in the hierarchy. The
        /// converted node needs to be relative to <paramref name="aiParent"/>, not <c>node.Parent</c>.
        /// </remarks>
        private NodeContent ImportNodes(ContentImporterContext context, Scene aiScene, Node aiNode, List<MaterialContent> materials, Node aiParent, NodeContent parent)
        {
            Debug.Assert(aiNode != null);

            NodeContent node = null;
            if (aiNode.HasMeshes)
            {
                MeshContent mesh = new MeshContent();
                mesh.Name = aiNode.Name;
                mesh.Identity = _identity;
                mesh.Transform = ToXna(GetRelativeTransform(aiNode, aiParent));

                foreach (int meshIndex in aiNode.MeshIndices)
                {
                    Mesh aiMesh = aiScene.Meshes[meshIndex];
                    if (!aiMesh.HasVertices)
                        continue;

                    GeometryContent geometry = CreateGeometry(context, mesh, aiMesh, materials);
                    mesh.Geometry.Add(geometry);
                }

                node = mesh;
            }
            else if (aiNode.Name.Contains("_$AssimpFbx$"))
            {
                // This is a transformation pivot.
                //   <OriginalName>_$AssimpFbx$_<TransformName>
                // where <TransformName> is one of
                //   Translation, RotationOffset, RotationPivot, PreRotation, Rotation,
                //   PostRotation, RotationPivotInverse, ScalingOffset, ScalingPivot,
                //   Scaling, ScalingPivotInverse
                string originalName = GetNodeName(aiNode.Name);
                if (!_pivots.TryGetValue(originalName, out FbxPivot pivot))
                {
                    pivot = new FbxPivot();
                    _pivots.Add(originalName, pivot);
                }
                pivot.Transform = ToXna(aiNode.Transform);
                pivot.Type = FbxPivot.GetPivotType(aiNode.Name);
                if (pivot.Type == FbxPivot.PivotType.Invalid)
                    throw new InvalidContentException(String.Format("Unknown $AssimpFbx$ node: \"{0}\"", aiNode.Name), _identity);
            }
            else if (!_bones.Contains(aiNode)) // Ignore bones.
            {
                node = new NodeContent();
                node.Name = aiNode.Name;
                node.Identity = _identity;
                node.Transform = ToXna(GetRelativeTransform(aiNode, aiParent));
            }

            if (node != null)
            {
                if (parent != null)
                    parent.Children.Add(node);

                // For the children, this is the new parent.
                aiParent = aiNode;
                parent = node;

                if (aiScene.HasAnimations)
                {
                    foreach (Animation aiAnimation in aiScene.Animations)
                    {
                        AnimationContent animationContent = ImportAnimation(aiAnimation, node.Name);
                        if (animationContent.Channels.Count > 0)
                            node.Animations.Add(animationContent.Name, animationContent);
                    }
                }
            }

            Debug.Assert(parent != null);

            foreach (Node aiChild in aiNode.Children)
                ImportNodes(context, aiScene, aiChild, materials, aiParent, parent);

            return node;
        }

        private GeometryContent CreateGeometry(ContentImporterContext context, MeshContent mesh, Mesh aiMesh, List<MaterialContent> materials)
        {
            GeometryContent geometry = new GeometryContent();
            geometry.Identity = _identity;
            geometry.Material = materials[aiMesh.MaterialIndex];

            // Vertices
            int baseVertex = mesh.Positions.Count;
            foreach (Vector3D aiVert in aiMesh.Vertices)
                mesh.Positions.Add(ToXna(aiVert));
            geometry.Vertices.AddRange(Enumerable.Range(baseVertex, aiMesh.VertexCount));
            geometry.Indices.AddRange(aiMesh.GetIndices());

            if (aiMesh.HasBones)
            {
                List<BoneWeightCollection> xnaWeights = new List<BoneWeightCollection>();
                int vertexCount = geometry.Vertices.VertexCount;
                bool missingBoneWeights = false;
                for (int i = 0; i < vertexCount; i++)
                {
                    BoneWeightCollection list = new BoneWeightCollection();
                    for (int boneIndex = 0; boneIndex < aiMesh.BoneCount; boneIndex++)
                    {
                        Bone aiBone = aiMesh.Bones[boneIndex];
                        foreach (VertexWeight aiWeight in aiBone.VertexWeights)
                        {
                            if (aiWeight.VertexID != i)
                                continue;

                            list.Add(new BoneWeight(aiBone.Name, aiWeight.Weight));
                        }
                    }

                    if (list.Count == 0)
                    {
                        // No bone weights found for vertex. Use bone 0 as fallback.
                        missingBoneWeights = true;
                        list.Add(new BoneWeight(aiMesh.Bones[0].Name, 1));
                    }

                        xnaWeights.Add(list);
                }

                if (missingBoneWeights)
                {
                    context.Logger.LogWarning(
                        string.Empty, 
                        _identity, 
                        "No bone weights found for one or more vertices of skinned mesh '{0}'.",
                        aiMesh.Name);
                }

                geometry.Vertices.Channels.Add(VertexChannelNames.Weights(0), xnaWeights);
            }

            // Individual channels go here
            if (aiMesh.HasNormals)
                geometry.Vertices.Channels.Add(VertexChannelNames.Normal(), aiMesh.Normals.Select(ToXna));

            for (int i = 0; i < aiMesh.TextureCoordinateChannelCount; i++)
                geometry.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(i), aiMesh.TextureCoordinateChannels[i].Select(ToXnaTexCoord));

            for (int i = 0; i < aiMesh.VertexColorChannelCount; i++)
                geometry.Vertices.Channels.Add(VertexChannelNames.Color(i), aiMesh.VertexColorChannels[i].Select(ToXnaColor));

            return geometry;
        }

        /// <summary>
        /// Identifies the nodes that represent bones and stores the bone offset matrices.
        /// </summary>
        private void FindSkeleton(Scene aiScene)
        {
            // See http://assimp.sourceforge.net/lib_html/data.html, section "Bones"
            // and notes above.

            // First, identify all deformation bones.
            _deformationBones = FindDeformationBones(aiScene);
            if (_deformationBones.Count == 0)
                return;

            // Walk the tree upwards to find the root bones.
            HashSet<Node> rootBones = new HashSet<Node>();
            foreach (string boneName in _deformationBones.Keys)
                rootBones.Add(FindRootBone(aiScene, boneName));

            if (rootBones.Count > 1)
                throw new InvalidContentException("Multiple skeletons found. Please ensure that the model does not contain more that one skeleton.", _identity);

            _rootBone = rootBones.First();

            // Add all nodes below root bone to skeleton.
            GetSubtree(_rootBone, _bones);
        }

        /// <summary>
        /// Finds the deformation bones (= bones attached to meshes).
        /// </summary>
        /// <param name="aiScene">The scene.</param>
        /// <returns>A dictionary of all deformation bones and their offset matrices.</returns>
        private static Dictionary<string, Matrix> FindDeformationBones(Scene aiScene)
        {
            Debug.Assert(aiScene != null);

            Dictionary<string, Matrix> deformationBones = new Dictionary<string, Matrix>();
            if (aiScene.HasMeshes)
                foreach (Mesh aiMesh in aiScene.Meshes)
                    if (aiMesh.HasBones)
                        foreach (Bone aiBone in aiMesh.Bones)
                            if (!deformationBones.ContainsKey(aiBone.Name))
                                deformationBones[aiBone.Name] = ToXna(aiBone.OffsetMatrix);

            return deformationBones;
        }

        /// <summary>
        /// Finds the root bone of a specific bone in the skeleton.
        /// </summary>
        /// <param name="aiScene">The scene.</param>
        /// <param name="boneName">The name of a bone in the skeleton.</param>
        /// <returns>The root bone.</returns>
        private static Node FindRootBone(Scene aiScene, string boneName)
        {
            Debug.Assert(aiScene != null);
            Debug.Assert(!string.IsNullOrEmpty(boneName));

            // Start with the specified bone.
            Node node = aiScene.RootNode.FindNode(boneName);
            Debug.Assert(node != null, "Node referenced by mesh not found in model.");

            // Walk all the way up to the scene root or the mesh node.
            Node rootBone = node;
            while (node != aiScene.RootNode && !node.HasMeshes)
            {
                // Only when FBXPreservePivotsConfig(true):
                // The FBX path likes to put these extra preserve pivot nodes in here.
                if (!node.Name.Contains("$AssimpFbx$"))
                    rootBone = node;

                node = node.Parent;
            }

            return rootBone;
        }

        /// <summary>
        /// Imports the skeleton including all skeletal animations.
        /// </summary>
        private void ImportSkeleton(Scene aiScene, NodeContent rootNode)
        {
            if (_rootBone == null)
                return;

            // Convert nodes to bones and attach to root node.
            BoneContent rootBoneContent = (BoneContent)ImportBones(_rootBone, _rootBone.Parent, null);
            rootNode.Children.Add(rootBoneContent);

            if (!aiScene.HasAnimations)
                return;

            // Convert animations and add to root bone.
            foreach (Animation aiAnimation in aiScene.Animations)
            {
                AnimationContent animationContent = ImportAnimation(aiAnimation);
                rootBoneContent.Animations.Add(animationContent.Name, animationContent);
            }
        }

        /// <summary>
        /// Converts the specified node and all descendant nodes to XNA bones.
        /// </summary>
        /// <param name="aiNode">The node.</param>
        /// <param name="aiParent">The parent node.</param>
        /// <param name="parent">The <paramref name="aiParent"/> node converted to XNA.</param>
        /// <returns>The XNA <see cref="NodeContent"/>.</returns>
        private NodeContent ImportBones(Node aiNode, Node aiParent, NodeContent parent)
        {
            Debug.Assert(aiNode != null);
            Debug.Assert(aiParent != null);

            NodeContent node = null;
            if (!aiNode.Name.Contains("_$AssimpFbx$")) // Ignore pivot nodes
            {
                const string mangling = "_$AssimpFbxNull$"; // Null leaf nodes are helpers

                if (aiNode.Name.Contains(mangling))
                {
                    // Null leaf node
                    node = new NodeContent();
                    node.Name = aiNode.Name.Replace(mangling, string.Empty);
                    node.Identity = _identity;
                    node.Transform = ToXna(GetRelativeTransform(aiNode, aiParent));
                }
                else if (_bones.Contains(aiNode))
                {
                    // Bone
                    node = new BoneContent();
                    node.Name = aiNode.Name;
                    node.Identity = _identity;

                    // node.Transform is irrelevant for bones. This transform is just the
                    // pose of the node at the time of the export. This could, for example,
                    // be one of the animation frames. It is not necessarily the bind pose
                    // (rest pose)!
                    // In XNA BoneContent.Transform needs to be set to the relative bind pose
                    // matrix. The relative bind pose matrix can be derived from the OffsetMatrix
                    // which is stored in aiMesh.Bones.
                    //
                    // offsetMatrix ... Offset matrix. Transforms the mesh from local space to bone space in bind pose.
                    // bindPoseRel  ... Relative bind pose matrix. Defines the transform of a bone relative to its parent bone.
                    // bindPoseAbs  ... Absolute bind pose matrix. Defines the transform of a bone relative to world space.
                    //
                    // The offset matrix is the inverse of the absolute bind pose matrix.
                    //   offsetMatrix = inverse(bindPoseAbs)
                    //
                    // bindPoseAbs = bindPoseRel * parentBindPoseAbs
                    // => bindPoseRel = bindPoseAbs * inverse(parentBindPoseAbs)
                    //                = inverse(offsetMatrix) * parentOffsetMatrix

                    Matrix offsetMatrix;
                    Matrix parentOffsetMatrix;
                    bool isOffsetMatrixValid = _deformationBones.TryGetValue(aiNode.Name, out offsetMatrix);
                    bool isParentOffsetMatrixValid = _deformationBones.TryGetValue(aiParent.Name, out parentOffsetMatrix);
                    if (isOffsetMatrixValid && isParentOffsetMatrixValid)
                    {
                        node.Transform = Matrix.Invert(offsetMatrix) * parentOffsetMatrix;
                    }
                    else if (isOffsetMatrixValid && aiNode == _rootBone)
                    {
                        // The current bone is the first in the chain.
                        // The parent offset matrix is missing. :(
                        if (_pivots.TryGetValue(node.Name, out FbxPivot pivot))
                        {
                            // --> Use transformation pivot.
                            node.Transform = pivot.GetTransform();
                        }
                        else
                        {
                            // --> Let's assume that parent's transform is Identity.
                            node.Transform = Matrix.Invert(offsetMatrix);
                        }
                    }
                    else if (isOffsetMatrixValid && aiParent == _rootBone)
                    {
                        // The current bone is the second bone in the chain.
                        // The parent offset matrix is missing. :(
                        // --> Derive matrix from parent bone, which is the root bone.
                        parentOffsetMatrix = parent.Transform;
                        node.Transform = Matrix.Invert(offsetMatrix) * Matrix.Invert(parentOffsetMatrix);
                    }
                    else
                    {
                        // Offset matrices are not provided by Assimp. :(
                        // Let's hope that the skeleton was exported in bind pose.
                        // (Otherwise we are just importing garbage.)
                        node.Transform = ToXna(GetRelativeTransform(aiNode, aiParent));
                    }
                }
            }

            if (node != null)
            {
                if (parent != null)
                    parent.Children.Add(node);

                // For the children, this is the new parent.
                aiParent = aiNode;
                parent = node;
            }

            foreach (Node aiChild in aiNode.Children)
                ImportBones(aiChild, aiParent, parent);

            return node;
        }

        /// <summary>
        /// Converts the specified animation to XNA.
        /// </summary>
        /// <param name="aiAnimation">The animation.</param>
        /// <param name="nodeName">An optional filter.</param>
        /// <returns>The animation converted to XNA.</returns>
        private AnimationContent ImportAnimation(Animation aiAnimation, string nodeName = null)
        {
            AnimationContent animation = new AnimationContent();
            animation.Name = GetAnimationName(aiAnimation.Name);
            animation.Identity = _identity;
            animation.Duration = TimeSpan.FromSeconds(aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond);

            // In Assimp animation channels may be split into separate channels.
            //   "nodeXyz" --> "nodeXyz_$AssimpFbx$_Translation",
            //                 "nodeXyz_$AssimpFbx$_Rotation",
            //                 "nodeXyz_$AssimpFbx$_Scaling"
            // Group animation channels by name (strip the "_$AssimpFbx$" part).
            IEnumerable<IGrouping<string,NodeAnimationChannel>> channelGroups;
            if (nodeName != null)
            {
                channelGroups = aiAnimation.NodeAnimationChannels
                                           .Where(channel => nodeName == GetNodeName(channel.NodeName))
                                           .GroupBy(channel => GetNodeName(channel.NodeName));
            }
            else
            {
                channelGroups = aiAnimation.NodeAnimationChannels
                                           .GroupBy(channel => GetNodeName(channel.NodeName));
            }

            foreach (IGrouping<string,NodeAnimationChannel> channelGroup in channelGroups)
            {
                string boneName = channelGroup.Key;
                AnimationChannel channel = new AnimationChannel();

                // Get transformation pivot for current bone.
                if (!_pivots.TryGetValue(boneName, out FbxPivot pivot))
                    pivot = new FbxPivot();

                List<VectorKey> scaleKeys = EmptyVectorKeys;
                List<QuaternionKey> rotationKeys = EmptyQuaternionKeys;
                List<VectorKey> translationKeys = EmptyVectorKeys;

                foreach (NodeAnimationChannel aiChannel in channelGroup)
                {
                    if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Scaling"))
                    {
                        scaleKeys = aiChannel.ScalingKeys;

                        Debug.Assert(pivot.Type == FbxPivot.PivotType.Scaling);
                        Debug.Assert(!aiChannel.HasRotationKeys || (aiChannel.RotationKeyCount == 1 && (aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(1, 0, 0, 0) || aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(0, 0, 0, 0))));
                        Debug.Assert(!aiChannel.HasPositionKeys || (aiChannel.PositionKeyCount == 1 && aiChannel.PositionKeys[0].Value == new Vector3D(0, 0, 0)));
                    }
                    else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Rotation"))
                    {
                        rotationKeys = aiChannel.RotationKeys;

                        Debug.Assert(pivot.Type == FbxPivot.PivotType.Rotation);
                        Debug.Assert(!aiChannel.HasScalingKeys || (aiChannel.ScalingKeyCount == 1 && aiChannel.ScalingKeys[0].Value == new Vector3D(1, 1, 1)));
                        Debug.Assert(!aiChannel.HasPositionKeys || (aiChannel.PositionKeyCount == 1 && aiChannel.PositionKeys[0].Value == new Vector3D(0, 0, 0)));
                    }
                    else if (aiChannel.NodeName.EndsWith("_$AssimpFbx$_Translation"))
                    {
                        translationKeys = aiChannel.PositionKeys;

                        Debug.Assert(pivot.Type == FbxPivot.PivotType.Translation);
                        Debug.Assert(!aiChannel.HasScalingKeys || (aiChannel.ScalingKeyCount == 1 && aiChannel.ScalingKeys[0].Value == new Vector3D(1, 1, 1)));
                        Debug.Assert(!aiChannel.HasRotationKeys || (aiChannel.RotationKeyCount == 1 && (aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(1, 0, 0, 0) || aiChannel.RotationKeys[0].Value == new Assimp.Quaternion(0, 0, 0, 0))));
                    }
                    else
                    {
                        scaleKeys = aiChannel.ScalingKeys;
                        rotationKeys = aiChannel.RotationKeys;
                        translationKeys = aiChannel.PositionKeys;
                    }
                }

                int scaleIndex = -1;
                int rotationIndex = -1;
                int translationIndex = -1;

                // Interpolate frames  
                int framesPerSecond = 60;
                double ticksPerFrame = aiAnimation.TicksPerSecond / framesPerSecond;
                int frames = (int)Math.Ceiling(aiAnimation.DurationInTicks / ticksPerFrame);
                for (int frame = 0; frame < frames; frame++)
                {
                    double time = frame * ticksPerFrame;

                    Vector3? scale = null;
                    if (scaleKeys.Count > 0)
                    {
                        while ((scaleIndex+1) < scaleKeys.Count &&
                               time > scaleKeys[(scaleIndex+1)].Time)
                            scaleIndex++;

                        int scaleIndexA = scaleIndex;
                        if (scaleIndexA == -1) // wrap scaleIndexA
                            scaleIndexA = scaleKeys.Count-1;
                        int scaleIndexB = Math.Min(scaleIndex+1, scaleKeys.Count-1); // (scaleIndex+1) % scaleKeys.Count;

                        double scaleTimeA = scaleKeys[scaleIndexA].Time;
                        double scaleTimeB = scaleKeys[scaleIndexB].Time;
                        if (scaleIndexA == scaleKeys.Count-1) // wrap scaleTimeA
                            scaleTimeA = Math.Min(0, scaleTimeA - aiAnimation.DurationInTicks);
                        double dt = scaleTimeB - scaleTimeA;
                        if (dt != 0)
                        {
                            float amount = (float)((time - scaleTimeA) / dt);
                            Vector3 scaleA = ToXna(scaleKeys[scaleIndexA].Value);
                            Vector3 scaleB = ToXna(scaleKeys[scaleIndexB].Value);
                            scale = Vector3.Lerp(scaleA, scaleB, amount);
                        }
                        else
                        {
                            scale = ToXna(scaleKeys[scaleIndexB].Value);
                        }
                    }

                    Quaternion? rotation = null;
                    if (rotationKeys.Count > 0)
                    {
                        while ((rotationIndex+1) < rotationKeys.Count &&
                               time > rotationKeys[(rotationIndex+1)].Time)
                            rotationIndex++;

                        int rotationIndexA = rotationIndex;
                        if (rotationIndexA == -1) // wrap rotationIndexA
                            rotationIndexA = rotationKeys.Count-1;
                        int rotationIndexB = Math.Min(rotationIndex+1, rotationKeys.Count - 1); // (rotationIndex+1) % rotationKeys.Count;

                        double rotationTimeA = rotationKeys[rotationIndexA].Time;
                        double rotationTimeB = rotationKeys[rotationIndexB].Time;
                        if (rotationIndexA == rotationKeys.Count-1) // wrap rotationTimeA
                            rotationTimeA = Math.Min(0, rotationTimeA - aiAnimation.DurationInTicks);
                        double dt = rotationTimeB - rotationTimeA;
                        if (dt != 0)
                        {
                            float amount = (float)((time - rotationTimeA) / dt);
                            Quaternion rotationA = ToXna(rotationKeys[rotationIndexA].Value);
                            Quaternion rotationB = ToXna(rotationKeys[rotationIndexB].Value);
                            rotation = Quaternion.Slerp(rotationA, rotationB, amount);
                        }
                        else
                        {
                            rotation = ToXna(rotationKeys[rotationIndexB].Value);
                        }
                    }

                    Vector3? translation = null;
                    if (translationKeys.Count > 0)
                    {
                        while ((translationIndex+1) < translationKeys.Count &&
                               time > translationKeys[(translationIndex+1)].Time)
                            translationIndex++;

                        int translationIndexA = translationIndex;
                        if (translationIndexA == -1) // wrap translationIndexA
                            translationIndexA = translationKeys.Count-1;
                        int translationIndexB = Math.Min(translationIndex+1, translationKeys.Count-1); // (translationIndex+1) % translationKeys.Count;

                        double translationTimeA = translationKeys[translationIndexA].Time;
                        double translationTimeB = translationKeys[translationIndexB].Time;
                        if (translationIndexA == translationKeys.Count-1) // wrap translationTimeA
                            translationTimeA = Math.Min(0, translationTimeA - aiAnimation.DurationInTicks);
                        double dt = translationTimeB - translationTimeA;
                        if (dt != 0)
                        {
                            float amount = (float)((time - translationTimeA) / dt);
                            Vector3 translationA = ToXna(translationKeys[translationIndexA].Value);
                            Vector3 translationB = ToXna(translationKeys[translationIndexB].Value);
                            translation = Vector3.Lerp(translationA, translationB, amount);
                        }
                        else
                        {
                            translation = ToXna(translationKeys[translationIndexB].Value);
                        }
                    }

                    // Apply transformation.
                    Matrix transform = Matrix.Identity;
                    if (scale.HasValue)
                        transform *= Matrix.CreateScale(scale.Value);
                    if (rotation.HasValue)
                        transform *= Matrix.CreateFromQuaternion(rotation.Value);
                    if (translation.HasValue)
                        transform *= Matrix.CreateTranslation(translation.Value);

                    long ticks = (long)(time * (TimeSpan.TicksPerSecond / aiAnimation.TicksPerSecond));
                    channel.Add(new AnimationKeyframe(TimeSpan.FromTicks(ticks), transform));
                }

                animation.Channels[channelGroup.Key] = channel;
            }

            return animation;
        }

        /// <summary>
        /// Copies the current node and all descendant nodes into a list.
        /// </summary>
        /// <param name="aiNode">The current node.</param>
        /// <param name="list">The list.</param>
        private static void GetSubtree(Node aiNode, List<Node> list)
        {
            Debug.Assert(aiNode != null);
            Debug.Assert(list != null);

            list.Add(aiNode);
            foreach (Node aiChild in aiNode.Children)
                GetSubtree(aiChild, list);
        }

        /// <summary>
        /// Gets the transform of node relative to a specific ancestor node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="ancestor">The ancestor node. Can be <see langword="null"/>.</param>
        /// <returns>
        /// The relative transform. If <paramref name="ancestor"/> is <see langword="null"/> the
        /// absolute transform of <paramref name="node"/> is returned.
        /// </returns>
        private static Matrix4x4 GetRelativeTransform(Node node, Node ancestor)
        {
            Debug.Assert(node != null);

            // Get transform of node relative to ancestor.
            Matrix4x4 transform = node.Transform;
            Node parent = node.Parent;
            while (parent != null && parent != ancestor)
            {
                transform = transform * parent.Transform;
                parent = parent.Parent;
            }

            if (parent == null && ancestor != null)
                throw new ArgumentException(String.Format("Node \"{0}\" is not an ancestor of \"{1}\".", ancestor.Name, node.Name));

            return transform;
        }

        /// <summary>
        /// Gets the animation name without the "AnimStack::" part.
        /// </summary>
        /// <param name="name">The mangled animation name.</param>
        /// <returns>The original animation name.</returns>
        private static string GetAnimationName(string name)
        {
            return name.Replace("AnimStack::", string.Empty);
        }

        /// <summary>
        /// Gets the node name without the "_$AssimpFbx$" part.
        /// </summary>
        /// <param name="name">The mangled node name.</param>
        /// <returns>The original node name.</returns>
        private static string GetNodeName(string name)
        {
            int index = name.IndexOf("_$AssimpFbx$", StringComparison.Ordinal);
            return (index >= 0) ? name.Remove(index) : name;
        }

        private static void ImportMetadata(Scene _scene, NodeContent rootNode)
        {
            foreach (var metaDataPair in _scene.Metadata)
            {
                string key = "metadata:" + metaDataPair.Key;
                object value = metaDataPair.Value.Data;

                if (metaDataPair.Value.DataType == MetaDataType.Vector3D)
                    value = ToXna((Vector3D)value);

                rootNode.OpaqueData.Add(key, value);
            }
        }

        private static Vector3 GetAxis(int axis)
        {
            switch (axis)
            {
                case 0: return Vector3.Right;
                case 1: return Vector3.Up;
                case 2: return Vector3.Forward;
                default: throw new InvalidOperationException("axis");
            }
        }

        private static void MetaTransform(NodeContent rootNode)
        {
            Matrix metaTransform = Matrix.Identity;

            int metaCoordAxis = rootNode.OpaqueData.GetValue<int>("metadata:CoordAxis", 0);
            int metaCoordAxisSign = rootNode.OpaqueData.GetValue<int>("metadata:CoordAxisSign", 1);
            metaTransform.Right = GetAxis(metaCoordAxis) * metaCoordAxisSign;

            int metaUpAxis = rootNode.OpaqueData.GetValue<int>("metadata:UpAxis", 1);
            int metaUpAxisSign = rootNode.OpaqueData.GetValue<int>("metadata:UpAxisSign", 1);
            metaTransform.Up = GetAxis(metaUpAxis) * metaUpAxisSign;

            int metaFrontAxis = rootNode.OpaqueData.GetValue<int>("metadata:FrontAxis", 2);
            int metaFrontAxisSign = rootNode.OpaqueData.GetValue<int>("metadata:FrontAxisSign", 1);
            metaTransform.Forward = GetAxis(metaFrontAxis) * metaFrontAxisSign;

            float metaUnitScaleFactor = rootNode.OpaqueData.GetValue<float>("metadata:UnitScaleFactor", 1f);
            metaTransform *= Matrix.CreateScale(metaUnitScaleFactor);

            MeshHelper.TransformScene(rootNode, metaTransform);
        }

        #region Conversion Helpers

        [DebuggerStepThrough]
        private static Matrix ToXna(Matrix4x4 matrix)
        {
            Matrix result;

            result.M11 = matrix.A1;
            result.M12 = matrix.B1;
            result.M13 = matrix.C1;
            result.M14 = matrix.D1;

            result.M21 = matrix.A2;
            result.M22 = matrix.B2;
            result.M23 = matrix.C2;
            result.M24 = matrix.D2;

            result.M31 = matrix.A3;
            result.M32 = matrix.B3;
            result.M33 = matrix.C3;
            result.M34 = matrix.D3;

            result.M41 = matrix.A4;
            result.M42 = matrix.B4;
            result.M43 = matrix.C4;
            result.M44 = matrix.D4;

            return result;
        }

        [DebuggerStepThrough]
        private static Vector2 ToXna(Vector2D vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [DebuggerStepThrough]
        private static Vector3 ToXna(Vector3D vector)
        {
            Vector3 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = vector.Z;
            return result;
        }

        [DebuggerStepThrough]
        private static Quaternion ToXna(Assimp.Quaternion quaternion)
        {
            Quaternion result;
            result.X = quaternion.X;
            result.Y = quaternion.Y;
            result.Z = quaternion.Z;
            result.W = quaternion.W;
            return result;
        }

        [DebuggerStepThrough]
        private static Vector3 ToXna(Color4D color)
        {
            Vector3 result;
            result.X = color.R;
            result.Y = color.G;
            result.Z = color.B;
            return result;
        }

        [DebuggerStepThrough]
        private static Vector2 ToXnaTexCoord(Vector3D vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [DebuggerStepThrough]
        private static Color ToXnaColor(Color4D color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
        #endregion
    }
}
