// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    [DebuggerDisplay("{DebugDisplayString}")]
    public class EffectParameter
    {
        ShaderProfileType _profile;

        public string Name { get; private set; }

        public string Semantic { get; private set; }

        public EffectParameterClass ParameterClass { get; private set; }

        public EffectParameterType ParameterType { get; private set; }

        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        public EffectParameterCollection Elements { get; private set; }

        public EffectParameterCollection StructureMembers { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }
        
        internal object Data { get; private set; }

        /// <summary>
        /// The next state key used when an effect parameter
        /// is updated by any of the 'set' methods.
        /// </summary>
        internal static ulong NextStateKey { get; private set; }

        /// <summary>
        /// The current state key which is used to detect
        /// if the parameter value has been changed.
        /// </summary>
        internal ulong StateKey { get; private set; }


        internal EffectParameter(EffectParameterClass class_,
                                    EffectParameterType type,
                                    string name,
                                    int rowCount,
                                    int columnCount,
                                    string semantic,
                                    EffectAnnotationCollection annotations,
                                    EffectParameterCollection elements,
                                    EffectParameterCollection structMembers,
                                    object data,
                                    ShaderProfileType profile)
        {
            ParameterClass = class_;
            ParameterType = type;

            Name = name;
            Semantic = semantic;
            Annotations = annotations;

            RowCount = rowCount;
            ColumnCount = columnCount;

            Elements = elements;
            StructureMembers = structMembers;

            Data = data;
            _profile = profile;

            StateKey = unchecked(NextStateKey++);
        }

        internal EffectParameter(EffectParameter cloneSource)
        {
            // Share all the immutable types.
            ParameterClass = cloneSource.ParameterClass;
            ParameterType = cloneSource.ParameterType;
            Name = cloneSource.Name;
            Semantic = cloneSource.Semantic;
            Annotations = cloneSource.Annotations;
            RowCount = cloneSource.RowCount;
            ColumnCount = cloneSource.ColumnCount;

            // Clone the mutable types.
            Elements = cloneSource.Elements.Clone();
            StructureMembers = cloneSource.StructureMembers.Clone();

            // The data is mutable, so we have to clone it.
            Array array = cloneSource.Data as Array;
            if (array != null)
                Data = array.Clone();
            _profile = cloneSource._profile;

            StateKey = unchecked(NextStateKey++);
        }


        public bool GetValueBoolean()
        {
            if (ParameterClass == EffectParameterClass.Scalar && ParameterType == EffectParameterType.Bool)
            {
                if (_profile == ShaderProfileType.OpenGL_Mojo)
                    return ((float[])Data)[0] != 0.0f; // MojoShader encodes booleans into a float.
                else
                    return ((int[])Data)[0] != 0;
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(bool value)
        {
            if (ParameterClass == EffectParameterClass.Scalar && ParameterType == EffectParameterType.Bool)
            {
                if (_profile == ShaderProfileType.OpenGL_Mojo)
                    ((float[])Data)[0] = value ? 1 : 0; // MojoShader encodes booleans into a float.
                else
                    ((int[])Data)[0] = value ? 1 : 0;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public bool[] GetValueBooleanArray()
        {
            throw new NotImplementedException();
        }

        public void SetValue(bool[] value)
        {
            throw new NotImplementedException();
        }


        public int GetValueInt32()
        {
            if (ParameterClass == EffectParameterClass.Scalar && ParameterType == EffectParameterType.Int32)
            {
                if (_profile == ShaderProfileType.OpenGL_Mojo)
                    return (int)((float[])Data)[0]; // MojoShader encodes integers into a float.
                else
                    return ((int[])Data)[0];
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(int value)
        {
            if (ParameterType == EffectParameterType.Single)
            {
                ((float[])Data)[0] = value;

                StateKey = unchecked(NextStateKey++);
                return;
            }

            if (ParameterClass == EffectParameterClass.Scalar && ParameterType == EffectParameterType.Int32)
            {
                if (_profile == ShaderProfileType.OpenGL_Mojo)
                    ((float[])Data)[0] = value; // MojoShader encodes integers into a float.
                else
                    ((int[])Data)[0] = value;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public int[] GetValueInt32Array()
        {
            if (Elements != null && Elements.Count > 0)
            {
                int[] ret = new int[RowCount * ColumnCount * Elements.Count];
                for (int i = 0; i < Elements.Count; i++)
                {
                    int[] elmArray = Elements[i].GetValueInt32Array();
                    for (int j = 0; j < elmArray.Length; j++)
                        ret[RowCount * ColumnCount * i + j] = elmArray[j];
                }
                return ret;
            }

            switch (ParameterClass)
            {
                case EffectParameterClass.Scalar:
                    return new int[] { GetValueInt32() };
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetValue(int[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);

            StateKey = unchecked(NextStateKey++);
        }


        public Single GetValueSingle()
        {
            if (ParameterClass == EffectParameterClass.Scalar && ParameterType == EffectParameterType.Single)
            {
                return ((float[])Data)[0];
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Single value)
        {
            if (ParameterType == EffectParameterType.Single)
            {
                ((float[])Data)[0] = value;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public Single[] GetValueSingleArray()
        {
            if (Elements != null && Elements.Count > 0)
            {
                Single[] ret = new Single[RowCount * ColumnCount * Elements.Count];
                for (int i = 0; i < Elements.Count; i++)
                {
                    Single[] elmArray = Elements[i].GetValueSingleArray();
                    for (int j = 0; j < elmArray.Length; j++)
                        ret[RowCount * ColumnCount * i + j] = elmArray[j];
                }
                return ret;
            }

            switch (ParameterClass)
            {
                case EffectParameterClass.Scalar:
                    return new Single[] { GetValueSingle() };
                case EffectParameterClass.Vector:
                case EffectParameterClass.Matrix:
                    if (Data is Matrix)
                        return Matrix.ToFloatArray((Matrix)Data);
                    else
                        return (float[])Data;
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetValue(Single[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);

            StateKey = unchecked(NextStateKey++);
        }


        public Vector2 GetValueVector2()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] vecInfo = (float[])Data;
                return new Vector2(vecInfo[0], vecInfo[1]);
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Vector2 value)
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] fData = (float[])Data;
                fData[0] = value.X;
                fData[1] = value.Y;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public Vector2[] GetValueVector2Array()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                if (Elements != null && Elements.Count > 0)
                {
                    Vector2[] result = new Vector2[Elements.Count];
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        float[] v = Elements[i].GetValueSingleArray();
                        result[i] = new Vector2(v[0], v[1]);
                    }
                    return result;
                }

                return null;
            }
            else 
                throw new InvalidCastException();
        }

        public void SetValue(Vector2[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);

            StateKey = unchecked(NextStateKey++);
        }


        public Vector3 GetValueVector3()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] vecInfo = (float[])Data;
                return new Vector3(vecInfo[0], vecInfo[1], vecInfo[2]);
            }
            else
                throw new InvalidCastException();
            }

        public void SetValue(Vector3 value)
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] fData = (float[])Data;
                fData[0] = value.X;
                fData[1] = value.Y;
                fData[2] = value.Z;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public Vector3[] GetValueVector3Array()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                if (Elements != null && Elements.Count > 0)
                {
                    Vector3[] result = new Vector3[Elements.Count];
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        float[] v = Elements[i].GetValueSingleArray();
                        result[i] = new Vector3(v[0], v[1], v[2]);
                    }
                    return result;
                }
                return null;
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Vector3[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);

            StateKey = unchecked(NextStateKey++);
        }


        public Vector4 GetValueVector4()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] vecInfo = (float[])Data;
                return new Vector4(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Vector4 value)
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] fData = (float[])Data;
                fData[0] = value.X;
                fData[1] = value.Y;
                fData[2] = value.Z;
                fData[3] = value.W;

                StateKey = unchecked(NextStateKey++);                
            }
            else
                throw new InvalidCastException();
        }

        public Vector4[] GetValueVector4Array()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                if (Elements != null && Elements.Count > 0)
                {
                    Vector4[] result = new Vector4[Elements.Count];
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        float[] v = Elements[i].GetValueSingleArray();
                        result[i] = new Vector4(v[0], v[1], v[2], v[3]);
                    }
                    return result;
                }
                return null;
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Vector4[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);

            StateKey = unchecked(NextStateKey++);
        }


        public Quaternion GetValueQuaternion()
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] vecInfo = (float[])Data;
                return new Quaternion(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Quaternion value)
        {
            if (ParameterClass == EffectParameterClass.Vector && ParameterType == EffectParameterType.Single)
            {
                float[] fData = (float[])Data;
                fData[0] = value.X;
                fData[1] = value.Y;
                fData[2] = value.Z;
                fData[3] = value.W;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }

        public Quaternion[] GetValueQuaternionArray()
        {
            throw new NotImplementedException();
        }

        public void SetValue(Quaternion[] value)
        {
            throw new NotImplementedException();
        }


        public Matrix GetValueMatrix()
        {
            if (ParameterClass == EffectParameterClass.Matrix && ParameterType == EffectParameterType.Single)
            {
                float[] fData = (float[])Data;

                if (RowCount == 4 && ColumnCount == 4)
                {
                    return new Matrix(fData[0], fData[4], fData[ 8], fData[12],
                                      fData[1], fData[5], fData[ 9], fData[13],
                                      fData[2], fData[6], fData[10], fData[14],
                                      fData[3], fData[7], fData[11], fData[15]);
                }
                else
                    throw new InvalidCastException();
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Matrix value)
        {
            if (ParameterClass == EffectParameterClass.Matrix && ParameterType == EffectParameterType.Single)
            {
                // HLSL expects matrices to be transposed by default.
                // These unrolled loops do the transpose during assignment.
                if (RowCount == 4)
                {
                    if (ColumnCount == 4)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;
                        fData[3] = value.M41;

                        fData[4] = value.M12;
                        fData[5] = value.M22;
                        fData[6] = value.M32;
                        fData[7] = value.M42;

                        fData[8] = value.M13;
                        fData[9] = value.M23;
                        fData[10] = value.M33;
                        fData[11] = value.M43;

                        fData[12] = value.M14;
                        fData[13] = value.M24;
                        fData[14] = value.M34;
                        fData[15] = value.M44;
                    }
                    else if (ColumnCount == 3)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;
                        fData[3] = value.M41;

                        fData[4] = value.M12;
                        fData[5] = value.M22;
                        fData[6] = value.M32;
                        fData[7] = value.M42;

                        fData[8] = value.M13;
                        fData[9] = value.M23;
                        fData[10] = value.M33;
                        fData[11] = value.M43;
                    }
                    else if (ColumnCount == 2)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;
                        fData[3] = value.M41;

                        fData[4] = value.M12;
                        fData[5] = value.M22;
                        fData[6] = value.M32;
                        fData[7] = value.M42;
                    }
                    else
                        throw new InvalidCastException();
                }
                else if (RowCount == 3)
                {
                    if (ColumnCount == 4)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;

                        fData[3] = value.M12;
                        fData[4] = value.M22;
                        fData[5] = value.M32;

                        fData[6] = value.M13;
                        fData[7] = value.M23;
                        fData[8] = value.M33;

                        fData[9] = value.M14;
                        fData[10] = value.M24;
                        fData[11] = value.M34;
                    }
                    else if (ColumnCount == 3)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;

                        fData[3] = value.M12;
                        fData[4] = value.M22;
                        fData[5] = value.M32;

                        fData[6] = value.M13;
                        fData[7] = value.M23;
                        fData[8] = value.M33;
                    }
                    else if (ColumnCount == 2)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;

                        fData[3] = value.M12;
                        fData[4] = value.M22;
                        fData[5] = value.M32;
                    }
                    else
                        throw new InvalidCastException();
                }
                else
                    throw new InvalidCastException();

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();

        }

        public Matrix[] GetValueMatrixArray(int count)
        {
            if (ParameterClass == EffectParameterClass.Matrix && ParameterType == EffectParameterType.Single)
            {
                Matrix[] ret = new Matrix[count];
                for (int i = 0; i < count; i++)
                    ret[i] = Elements[i].GetValueMatrix();

                return ret;
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Matrix[] value)
        {
            if (ParameterClass == EffectParameterClass.Matrix && ParameterType == EffectParameterType.Single)
            {
                if (RowCount == 4)
                {
                    if (ColumnCount == 4)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;
                            fData[3] = value[i].M41;

                            fData[4] = value[i].M12;
                            fData[5] = value[i].M22;
                            fData[6] = value[i].M32;
                            fData[7] = value[i].M42;

                            fData[8] = value[i].M13;
                            fData[9] = value[i].M23;
                            fData[10] = value[i].M33;
                            fData[11] = value[i].M43;

                            fData[12] = value[i].M14;
                            fData[13] = value[i].M24;
                            fData[14] = value[i].M34;
                            fData[15] = value[i].M44;
                        }
                    }
                    else if (ColumnCount == 3)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;
                            fData[3] = value[i].M41;

                            fData[4] = value[i].M12;
                            fData[5] = value[i].M22;
                            fData[6] = value[i].M32;
                            fData[7] = value[i].M42;

                            fData[8] = value[i].M13;
                            fData[9] = value[i].M23;
                            fData[10] = value[i].M33;
                            fData[11] = value[i].M43;
                        }
                    }
                    else if (ColumnCount == 2)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;
                            fData[3] = value[i].M41;

                            fData[4] = value[i].M12;
                            fData[5] = value[i].M22;
                            fData[6] = value[i].M32;
                            fData[7] = value[i].M42;
                        }
                    }
                    else
                        throw new InvalidCastException();
                }
                else if (RowCount == 3)
                {
                    if (ColumnCount == 4)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;

                            fData[3] = value[i].M12;
                            fData[4] = value[i].M22;
                            fData[5] = value[i].M32;

                            fData[6] = value[i].M13;
                            fData[7] = value[i].M23;
                            fData[8] = value[i].M33;

                            fData[9] = value[i].M14;
                            fData[10] = value[i].M24;
                            fData[11] = value[i].M34;
                        }
                    }
                    else if (ColumnCount == 3)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;

                            fData[3] = value[i].M12;
                            fData[4] = value[i].M22;
                            fData[5] = value[i].M32;

                            fData[6] = value[i].M13;
                            fData[7] = value[i].M23;
                            fData[8] = value[i].M33;
                        }
                    }
                    else if (ColumnCount == 2)
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            float[] fData = (float[])Elements[i].Data;

                            fData[0] = value[i].M11;
                            fData[1] = value[i].M21;
                            fData[2] = value[i].M31;

                            fData[3] = value[i].M12;
                            fData[4] = value[i].M22;
                            fData[5] = value[i].M32;
                        }
                    }
                    else
                        throw new InvalidCastException();                    
                }
                else
                    throw new InvalidCastException();

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }


        public void SetValueTranspose(Matrix value)
        {
            if (ParameterClass == EffectParameterClass.Matrix && ParameterType == EffectParameterType.Single)
            {
                // HLSL expects matrices to be transposed by default, so copying them straight
                // from the in-memory version effectively transposes them back to row-major.
            
                if (RowCount == 4)
                {
                    if (ColumnCount == 4)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M12;
                        fData[2] = value.M13;
                        fData[3] = value.M14;

                        fData[4] = value.M21;
                        fData[5] = value.M22;
                        fData[6] = value.M23;
                        fData[7] = value.M24;

                        fData[8] = value.M31;
                        fData[9] = value.M32;
                        fData[10] = value.M33;
                        fData[11] = value.M34;

                        fData[12] = value.M41;
                        fData[13] = value.M42;
                        fData[14] = value.M43;
                        fData[15] = value.M44;
                    }
                    else if (ColumnCount == 3)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M12;
                        fData[2] = value.M13;
                        fData[3] = value.M14;

                        fData[4] = value.M21;
                        fData[5] = value.M22;
                        fData[6] = value.M23;
                        fData[7] = value.M24;

                        fData[8] = value.M31;
                        fData[9] = value.M32;
                        fData[10] = value.M33;
                        fData[11] = value.M34;
                    }
                    else if (ColumnCount == 2)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M21;
                        fData[2] = value.M31;
                        fData[3] = value.M41;

                        fData[4] = value.M12;
                        fData[5] = value.M22;
                        fData[6] = value.M32;
                        fData[7] = value.M42;
                    }
                    else
                        throw new InvalidCastException();
                }
                else if (RowCount == 3)
                {
                    if (ColumnCount == 4)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M12;
                        fData[2] = value.M13;

                        fData[3] = value.M21;
                        fData[4] = value.M22;
                        fData[5] = value.M23;

                        fData[6] = value.M31;
                        fData[7] = value.M32;
                        fData[8] = value.M33;

                        fData[9] = value.M41;
                        fData[10] = value.M42;
                        fData[11] = value.M43;
                    }
                    else if (ColumnCount == 3)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M12;
                        fData[2] = value.M13;

                        fData[3] = value.M21;
                        fData[4] = value.M22;
                        fData[5] = value.M23;

                        fData[6] = value.M31;
                        fData[7] = value.M32;
                        fData[8] = value.M33;
                    }
                    else if (ColumnCount == 2)
                    {
                        float[] fData = (float[])Data;

                        fData[0] = value.M11;
                        fData[1] = value.M12;
                        fData[2] = value.M13;

                        fData[3] = value.M21;
                        fData[4] = value.M22;
                        fData[5] = value.M23;
                    }
                    else
                        throw new InvalidCastException();
                }
                else
                    throw new InvalidCastException();

                StateKey = unchecked(NextStateKey++);            
            }
            else
                throw new InvalidCastException();
        }


        public string GetValueString()
        {
            if (ParameterClass == EffectParameterClass.Object && ParameterType == EffectParameterType.String)
            {
                return ((string[])Data)[0];
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(string value)
        {
            throw new NotImplementedException();
        }


        public Texture2D GetValueTexture2D()
        {
            if (ParameterClass == EffectParameterClass.Object && ParameterType == EffectParameterType.Texture2D)
            {
                return (Texture2D)Data;
            }
            else
                throw new InvalidCastException();
        }

        public Texture3D GetValueTexture3D()
        {
            if (ParameterClass == EffectParameterClass.Object && ParameterType == EffectParameterType.Texture3D)
            {
                return (Texture3D)Data;
            }
            else
                throw new InvalidCastException();
        }

        public TextureCube GetValueTextureCube()
        {
            if (ParameterClass == EffectParameterClass.Object && ParameterType == EffectParameterType.TextureCube)
            {
                return (TextureCube)Data;
            }
            else
                throw new InvalidCastException();
        }

        public void SetValue(Texture value)
        {
            if (this.ParameterType == EffectParameterType.Texture ||
                this.ParameterType == EffectParameterType.Texture1D ||
                this.ParameterType == EffectParameterType.Texture2D ||
                this.ParameterType == EffectParameterType.Texture3D ||
                this.ParameterType == EffectParameterType.TextureCube)
            {
                Data = value;

                StateKey = unchecked(NextStateKey++);
            }
            else
                throw new InvalidCastException();
        }


        /// <summary>
        /// Property referenced by the DebuggerDisplayAttribute.
        /// </summary>
        private string DebugDisplayString
        {
            get
            {
                string semanticStr = string.Empty;
                if (!string.IsNullOrEmpty(Semantic))
                    semanticStr = string.Concat(" <", Semantic, ">");

                return string.Concat("[", ParameterClass, " ", ParameterType, "]", semanticStr, " ", Name, " : ", GetDebugDataValueString());
            }
        }

        private string GetDebugDataValueString()
        {
            string valueStr;

            if (Data == null)
            {
                if (Elements == null)
                    valueStr = "(null)";
                else
                    valueStr = string.Join(", ", Elements.Select(e => e.GetDebugDataValueString()));
            }
            else
            {
                switch (ParameterClass)
                {
                    // Object types are stored directly in the Data property.
                    // Display Data's string value.
                    case EffectParameterClass.Object:
                        valueStr = Data.ToString();
                        break;

                    // Matrix types are stored in a float[16] which we don't really have room for.
                    // Display "...".
                    case EffectParameterClass.Matrix:
                        valueStr = "...";
                        break;

                    // Scalar types are stored as a float[1].
                    // Display the first (and only) element's string value.                    
                    case EffectParameterClass.Scalar:
                        valueStr = (Data as Array).GetValue(0).ToString();
                        break;

                    // Vector types are stored as an Array<Type>.
                    // Display the string value of each array element.
                    case EffectParameterClass.Vector:
                        Array array = Data as Array;
                        string[] arrayStr = new string[array.Length];
                        int idx = 0;
                        foreach (object e in array)
                        {
                            arrayStr[idx] = array.GetValue(idx).ToString();
                            idx++;
                        }

                        valueStr = string.Join(" ", arrayStr);
                        break;

                    // Handle additional cases here...
                    default:
                        valueStr = Data.ToString();
                        break;
                }
            }

            return string.Concat("{", valueStr, "}");
        }
    }
}
