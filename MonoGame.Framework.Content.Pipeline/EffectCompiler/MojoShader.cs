// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal class MojoShader
    {
#if OLD_CONTENT_PROCESSORS
        const string mojoshader_dll = "libmojoshader_32.dll";
#else
        const string mojoshader_dll = "libmojoshader_64.dll";
#endif

        public partial class NativeConstants
        {
            /// MOJOSHADER_VERSION -> 1111
            public const int MOJOSHADER_VERSION = 1111;

            /// MOJOSHADER_CHANGESET -> "hg-1111:91a6af79b5e4"
            public const string MOJOSHADER_CHANGESET = "hg-1111:91a6af79b5e4";

            /// POSITION_NONE -> (-3)
            public const int POSITION_NONE = -3;

            /// POSITION_BEFORE -> (-2)
            public const int POSITION_BEFORE = -2;

            /// POSITION_AFTER -> (-1)
            public const int POSITION_AFTER = -1;

            /// PROFILE_D3D -> "d3d"
            public const string PROFILE_D3D = "d3d";

            /// PROFILE_BYTECODE -> "bytecode"
            public const string PROFILE_BYTECODE = "bytecode";

            /// PROFILE_GLSL -> "glsl"
            public const string PROFILE_GLSL = "glsl";

            /// PROFILE_GLSL120 -> "glsl120"
            public const string PROFILE_GLSL120 = "glsl120";

            /// PROFILE_ARB1 -> "arb1"
            public const string PROFILE_ARB1 = "arb1";

            /// PROFILE_NV2 -> "nv2"
            public const string PROFILE_NV2 = "nv2";

            /// PROFILE_NV3 -> "nv3"
            public const string PROFILE_NV3 = "nv3";

            /// PROFILE_NV4 -> "nv4"
            public const string PROFILE_NV4 = "nv4";

            /// SRC_PROFILE_HLSL_VS_1_1 -> "hlsl_vs_1_1"
            public const string SRC_PROFILE_HLSL_VS_1_1 = "hlsl_vs_1_1";

            /// SRC_PROFILE_HLSL_VS_2_0 -> "hlsl_vs_2_0"
            public const string SRC_PROFILE_HLSL_VS_2_0 = "hlsl_vs_2_0";

            /// SRC_PROFILE_HLSL_VS_3_0 -> "hlsl_vs_3_0"
            public const string SRC_PROFILE_HLSL_VS_3_0 = "hlsl_vs_3_0";

            /// SRC_PROFILE_HLSL_PS_1_1 -> "hlsl_ps_1_1"
            public const string SRC_PROFILE_HLSL_PS_1_1 = "hlsl_ps_1_1";

            /// SRC_PROFILE_HLSL_PS_1_2 -> "hlsl_ps_1_2"
            public const string SRC_PROFILE_HLSL_PS_1_2 = "hlsl_ps_1_2";

            /// SRC_PROFILE_HLSL_PS_1_3 -> "hlsl_ps_1_3"
            public const string SRC_PROFILE_HLSL_PS_1_3 = "hlsl_ps_1_3";

            /// SRC_PROFILE_HLSL_PS_1_4 -> "hlsl_ps_1_4"
            public const string SRC_PROFILE_HLSL_PS_1_4 = "hlsl_ps_1_4";

            /// SRC_PROFILE_HLSL_PS_2_0 -> "hlsl_ps_2_0"
            public const string SRC_PROFILE_HLSL_PS_2_0 = "hlsl_ps_2_0";

            /// SRC_PROFILE_HLSL_PS_3_0 -> "hlsl_ps_3_0"
            public const string SRC_PROFILE_HLSL_PS_3_0 = "hlsl_ps_3_0";

            // CONST -> (1 << 31)
            public const int CONST = (1) << (31);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Uniform
        {
            /// UniformType->Anonymous_cf91fd71_65e4_4c31_a6d5_9488d7f3d32a
            public UniformType type;

            /// int
            public int index;

            /// int
            public int array_count;

            /// int
            public int constant;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Constant
        {
            /// UniformType->Anonymous_cf91fd71_65e4_4c31_a6d5_9488d7f3d32a
            public UniformType type;

            /// int
            public int index;

            /// Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be
            public Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Sampler
        {
            /// SamplerType->Anonymous_a752a39b_b479_42b0_9502_e39ba7d86100
            public SamplerType type;

            /// int
            public int index;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// int
            public int texbem;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SamplerMap
        {
            /// int
            public int index;

            /// SamplerType->Anonymous_a752a39b_b479_42b0_9502_e39ba7d86100
            public SamplerType type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Attribute
        {
            /// Usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
            public Usage usage;

            /// int
            public int index;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Swizzle
        {
            /// Usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
            public Usage usage;

            /// unsigned int
            public uint index;

            /// unsigned char[4]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string swizzles;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SymbolTypeInfo
        {
            /// SymbolClass->Anonymous_681c4b26_94f7_4142_a8e9_b970fe0b60df
            public SymbolClass parameter_class;

            /// SymbolType->Anonymous_d8534f21_7f44_465d_8843_40a435dbb54a
            public SymbolType parameter_type;

            /// unsigned int
            public uint rows;

            /// unsigned int
            public uint columns;

            /// unsigned int
            public uint elements;

            /// unsigned int
            public uint member_count;

            /// SymbolStructMember*
            public IntPtr members;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SymbolStructMember
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// SymbolTypeInfo
            public SymbolTypeInfo info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Symbol
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// SymbolRegisterSet->Anonymous_9ff7ac54_131c_43b0_a295_9830d24ac76b
            public SymbolRegisterSet register_set;

            /// unsigned int
            public uint register_index;

            /// unsigned int
            public uint register_count;

            /// SymbolTypeInfo
            public SymbolTypeInfo info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Error
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string error;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string filename;

            /// int
            public int error_position;
        }

        public enum PreshaderOpcode
        {
            NOP,
            MOV,
            NEG,
            RCP,
            FRC,
            EXP,
            LOG,
            RSQ,
            SIN,
            COS,
            ASIN,
            ACOS,
            ATAN,
            MIN,
            MAX,
            LT,
            GE,
            ADD,
            MUL,
            ATAN2,
            DIV,
            CMP,
            MOVC,
            DOT,
            NOISE,
            SCALAR_OPS,

            /// MIN_SCALAR -> SCALAR_OPS
            MIN_SCALAR = PreshaderOpcode.SCALAR_OPS,

            MAX_SCALAR,
            LT_SCALAR,
            GE_SCALAR,
            ADD_SCALAR,
            MUL_SCALAR,
            ATAN2_SCALAR,
            DIV_SCALAR,
            DOT_SCALAR,
            NOISE_SCALAR,
        }

        public enum PreshaderOperandType
        {
            /// LITERAL -> 1
            LITERAL = 1,
            /// INPUT -> 2
            INPUT = 2,
            /// OUTPUT -> 4
            OUTPUT = 4,
            /// TEMP -> 7
            TEMP = 7,
            /// UNKN -> 0xff
            UNKN = 255,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PreshaderOperand
        {

            /// PreshaderOperandType
            public PreshaderOperandType type;

            /// unsigned int
            public uint index;

            /// int
            public int indexingType;

            /// unsigned int
            public uint indexingIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PreshaderInstruction
        {

            /// PreshaderOpcode
            public PreshaderOpcode opcode;

            /// unsigned int
            public uint element_count;

            /// unsigned int
            public uint operand_count;

            /// PreshaderOperand[4]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.Struct)]
            public PreshaderOperand[] operands;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Preshader
        {
            /// unsigned int
            public uint literal_count;

            /// double*
            public IntPtr literals;

            /// unsigned int
            public uint temp_count;

            /// unsigned int
            public uint symbol_count;

            /// Symbol*
            public IntPtr symbols;

            /// unsigned int
            public uint instruction_count;

            /// PreshaderInstruction*
            public IntPtr instructions;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParseData
        {
            /// int
            public int error_count;

            /// Error*
            public IntPtr errors;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string profile;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string output;

            /// int
            public int output_len;

            /// int
            public int instruction_count;

            /// ShaderType->Anonymous_96517ad6_cc69_4542_8537_054e63919d54
            public ShaderType shader_type;

            /// int
            public int major_ver;

            /// int
            public int minor_ver;

            /// int
            public int uniform_count;

            /// Uniform*
            public IntPtr uniforms;

            /// int
            public int constant_count;

            /// Constant*
            public IntPtr constants;

            /// int
            public int sampler_count;

            /// Sampler*
            public IntPtr samplers;

            /// int
            public int attribute_count;

            /// Attribute*
            public IntPtr attributes;

            /// int
            public int output_count;

            /// Attribute*
            public IntPtr outputs;

            /// int
            public int swizzle_count;

            /// Swizzle*
            public IntPtr swizzles;

            /// int
            public int symbol_count;

            /// Symbol*
            public IntPtr symbols;

            /// Preshader*
            public IntPtr preshader;

            /// Malloc
            public IntPtr malloc;

            /// Free
            public IntPtr free;

            /// void*
            public IntPtr malloc_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectParam
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string semantic;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectState
        {
            /// unsigned int
            public uint type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectPass
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// unsigned int
            public uint state_count;

            /// EffectState*
            public IntPtr states;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectTechnique
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;

            /// unsigned int
            public uint pass_count;

            /// EffectPass*
            public IntPtr passes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectTexture
        {

            /// unsigned int
            public uint param;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EffectShader
        {
            /// unsigned int
            public uint technique;

            /// unsigned int
            public uint pass;

            /// ParseData*
            public IntPtr shader;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Effect
        {
            /// int
            public int error_count;

            /// Error*
            public IntPtr errors;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string profile;

            /// int
            public int param_count;

            /// EffectParam*
            public IntPtr @params;

            /// int
            public int technique_count;

            /// EffectTechnique*
            public IntPtr techniques;

            /// int
            public int texture_count;

            /// EffectTexture*
            public IntPtr textures;

            /// int
            public int shader_count;

            /// EffectShader*
            public IntPtr shaders;

            /// Malloc
            public IntPtr malloc;

            /// Free
            public IntPtr free;

            /// void*
            public IntPtr malloc_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PreprocessorDefine
        {
            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string identifier;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string definition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PreprocessData
        {
            /// int
            public int error_count;

            /// Error*
            public IntPtr errors;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string output;

            /// int
            public int output_len;

            /// Malloc
            public IntPtr malloc;

            /// Free
            public IntPtr free;

            /// void*
            public IntPtr malloc_data;
        }

        /// Return Type: int
        ///inctype: IncludeType->Anonymous_f1eed39d_7d1b_46d4_972e_a3229d15c26e
        ///fname: char*
        ///parent: char*
        ///outdata: char**
        ///outbytes: unsigned int*
        ///m: Malloc
        ///f: Free
        ///d: void*
        public delegate int IncludeOpen(IncludeType inctype, [In()] [MarshalAs(UnmanagedType.LPStr)] string fname, [In()] [MarshalAs(UnmanagedType.LPStr)] string parent, ref IntPtr outdata, ref uint outbytes, IntPtr m, IntPtr f, IntPtr d);

        /// Return Type: void
        ///data: char*
        ///m: Malloc
        ///f: Free
        ///d: void*
        public delegate void IncludeClose([In()] [MarshalAs(UnmanagedType.LPStr)] string data, IntPtr m, IntPtr f, IntPtr d);

        public enum AstDataTypeType
        {
            NONE,
            BOOL,
            INT,
            UINT,
            FLOAT,
            FLOAT_SNORM,
            FLOAT_UNORM,
            HALF,
            DOUBLE,
            STRING,
            SAMPLER_1D,
            SAMPLER_2D,
            SAMPLER_3D,
            SAMPLER_CUBE,
            SAMPLER_STATE,
            SAMPLER_COMPARISON_STATE,
            STRUCT,
            ARRAY,
            VECTOR,
            MATRIX,
            BUFFER,
            FUNCTION,
            USER,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeStructMember
        {

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string identifier;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeStruct
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataTypeStructMember*
            public IntPtr members;

            /// int
            public int member_count;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeArray
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataType*
            public IntPtr @base;

            /// int
            public int elements;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeMatrix
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataType*
            public IntPtr @base;

            /// int
            public int rows;

            /// int
            public int columns;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeBuffer
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataType*
            public IntPtr @base;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeFunction
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataType*
            public IntPtr retval;

            /// AstDataType**
            public IntPtr @params;

            /// int
            public int num_params;

            /// int
            public int intrinsic;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstDataTypeUser
        {

            /// AstDataTypeType
            public AstDataTypeType type;

            /// AstDataType*
            public IntPtr details;

            /// char*
            public IntPtr name;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct AstDataType
        {

            /// AstDataTypeType
            [FieldOffset(0)]
            public AstDataTypeType type;

            /// AstDataTypeArray
            [FieldOffset(0)]
            public AstDataTypeArray array;

            /// AstDataTypeStruct
            [FieldOffset(0)]
            public AstDataTypeStruct structure;

            /// AstDataTypeVector->AstDataTypeArray
            [FieldOffset(0)]
            public AstDataTypeArray vector;

            /// AstDataTypeMatrix
            [FieldOffset(0)]
            public AstDataTypeMatrix matrix;

            /// AstDataTypeBuffer
            [FieldOffset(0)]
            public AstDataTypeBuffer buffer;

            /// AstDataTypeUser
            [FieldOffset(0)]
            public AstDataTypeUser user;

            /// AstDataTypeFunction
            [FieldOffset(0)]
            public AstDataTypeFunction function;
        }

        public enum AstNodeType
        {
            OP_START_RANGE,
            OP_START_RANGE_UNARY,
            OP_PREINCREMENT,
            OP_PREDECREMENT,
            OP_NEGATE,
            OP_COMPLEMENT,
            OP_NOT,
            OP_POSTINCREMENT,
            OP_POSTDECREMENT,
            OP_CAST,
            OP_END_RANGE_UNARY,
            OP_START_RANGE_BINARY,
            OP_COMMA,
            OP_MULTIPLY,
            OP_DIVIDE,
            OP_MODULO,
            OP_ADD,
            OP_SUBTRACT,
            OP_LSHIFT,
            OP_RSHIFT,
            OP_LESSTHAN,
            OP_GREATERTHAN,
            OP_LESSTHANOREQUAL,
            OP_GREATERTHANOREQUAL,
            OP_EQUAL,
            OP_NOTEQUAL,
            OP_BINARYAND,
            OP_BINARYXOR,
            OP_BINARYOR,
            OP_LOGICALAND,
            OP_LOGICALOR,
            OP_ASSIGN,
            OP_MULASSIGN,
            OP_DIVASSIGN,
            OP_MODASSIGN,
            OP_ADDASSIGN,
            OP_SUBASSIGN,
            OP_LSHIFTASSIGN,
            OP_RSHIFTASSIGN,
            OP_ANDASSIGN,
            OP_XORASSIGN,
            OP_ORASSIGN,
            OP_DEREF_ARRAY,
            OP_END_RANGE_BINARY,
            OP_START_RANGE_TERNARY,
            OP_CONDITIONAL,
            OP_END_RANGE_TERNARY,
            OP_START_RANGE_DATA,
            OP_IDENTIFIER,
            OP_INT_LITERAL,
            OP_FLOAT_LITERAL,
            OP_STRING_LITERAL,
            OP_BOOLEAN_LITERAL,
            OP_END_RANGE_DATA,
            OP_START_RANGE_MISC,
            OP_DEREF_STRUCT,
            OP_CALLFUNC,
            OP_CONSTRUCTOR,
            OP_END_RANGE_MISC,
            OP_END_RANGE,

            COMPUNIT_START_RANGE,
            COMPUNIT_FUNCTION,
            COMPUNIT_TYPEDEF,
            COMPUNIT_STRUCT,
            COMPUNIT_VARIABLE,
            COMPUNIT_END_RANGE,

            STATEMENT_START_RANGE,
            STATEMENT_EMPTY,
            STATEMENT_BREAK,
            STATEMENT_CONTINUE,
            STATEMENT_DISCARD,
            STATEMENT_BLOCK,
            STATEMENT_EXPRESSION,
            STATEMENT_IF,
            STATEMENT_SWITCH,
            STATEMENT_FOR,
            STATEMENT_DO,
            STATEMENT_WHILE,
            STATEMENT_RETURN,
            STATEMENT_TYPEDEF,
            STATEMENT_STRUCT,
            STATEMENT_VARDECL,
            STATEMENT_END_RANGE,

            MISC_START_RANGE,
            FUNCTION_PARAMS,
            FUNCTION_SIGNATURE,
            SCALAR_OR_ARRAY,
            TYPEDEF,
            PACK_OFFSET,
            VARIABLE_LOWLEVEL,
            ANNOTATION,
            VARIABLE_DECLARATION,
            STRUCT_DECLARATION,
            STRUCT_MEMBER,
            SWITCH_CASE,
            ARGUMENTS,
            MISC_END_RANGE,
            END_RANGE,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstNodeInfo
        {

            /// AstNodeType
            public AstNodeType type;

            /// char*
            public IntPtr filename;

            /// unsigned int
            public uint line;
        }

        public enum AstVariableAttributes
        {
            // EXTERN -> (1<<0)
            EXTERN = (1) << (0),
            // NOINTERPOLATION -> (1<<1)
            NOINTERPOLATION = (1) << (1),
            // SHARED -> (1<<2)
            SHARED = (1) << (2),
            // STATIC -> (1<<3)
            STATIC = (1) << (3),
            // UNIFORM -> (1<<4)
            UNIFORM = (1) << (4),
            // VOLATILE -> (1<<5)
            VOLATILE = (1) << (5),
            // CONST -> (1<<6)
            CONST = (1) << (6),
            // ROWMAJOR -> (1<<7)
            ROWMAJOR = (1) << (7),
            // COLUMNMAJOR -> (1<<8)
            COLUMNMAJOR = (1) << (8),
        }

        public enum AstIfAttributes
        {
            NONE,
            BRANCH,
            FLATTEN,
            IFALL,
            IFANY,
            PREDICATE,
            PREDICATEBLOCK,
        }

        public enum AstSwitchAttributes
        {
            NONE,
            FLATTEN,
            BRANCH,
            FORCECASE,
            CALL,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstGeneric
        {

            /// AstNodeInfo
            public AstNodeInfo ast;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpression
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstArguments
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstExpression*
            public IntPtr argument;

            /// AstArguments*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionUnary
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr operand;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionBinary
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr left;

            /// AstExpression*
            public IntPtr right;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionTernary
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr left;

            /// AstExpression*
            public IntPtr center;

            /// AstExpression*
            public IntPtr right;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionIdentifier
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            public IntPtr identifier;

            /// int
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionIntLiteral
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// int
            public int value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionFloatLiteral
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// double
            public double value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionStringLiteral
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            public IntPtr @string;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionBooleanLiteral
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// int
            public int value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionConstructor
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstArguments*
            public IntPtr args;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionDerefStruct
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr identifier;

            /// char*
            public IntPtr member;

            /// int
            public int isswizzle;

            /// int
            public int member_index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionCallFunction
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpressionIdentifier*
            public IntPtr identifier;

            /// AstArguments*
            public IntPtr args;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionCast
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr operand;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstCompilationUnit
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstCompilationUnit*
            public IntPtr next;
        }

        public enum AstFunctionStorageClass
        {
            NONE,
            INLINE,
        }

        public enum AstInputModifier
        {
            NONE,
            IN,
            OUT,
            INOUT,
            UNIFORM,
        }

        public enum AstInterpolationModifier
        {
            NONE,
            LINEAR,
            CENTROID,
            NOINTERPOLATION,
            NOPERSPECTIVE,
            SAMPLE,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstFunctionParameters
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstInputModifier
            public AstInputModifier input_modifier;

            /// char*
            public IntPtr identifier;

            /// char*
            public IntPtr semantic;

            /// AstInterpolationModifier
            public AstInterpolationModifier interpolation_modifier;

            /// AstExpression*
            public IntPtr initializer;

            /// AstFunctionParameters*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstFunctionSignature
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            public IntPtr identifier;

            /// AstFunctionParameters*
            public IntPtr @params;

            /// AstFunctionStorageClass
            public AstFunctionStorageClass storage_class;

            /// char*
            public IntPtr semantic;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstScalarOrArray
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// char*
            public IntPtr identifier;

            /// int
            public int isarray;

            /// AstExpression*
            public IntPtr dimension;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstAnnotations
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// AstExpression*
            public IntPtr initializer;

            /// AstAnnotations*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstPackOffset
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// char*
            public IntPtr ident1;

            /// char*
            public IntPtr ident2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstVariableLowLevel
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstPackOffset*
            public IntPtr packoffset;

            /// char*
            public IntPtr register_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstStructMembers
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            public IntPtr semantic;

            /// AstScalarOrArray*
            public IntPtr details;

            /// AstInterpolationModifier
            public AstInterpolationModifier interpolation_mod;

            /// AstStructMembers*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstStructDeclaration
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// char*
            public IntPtr name;

            /// AstStructMembers*
            public IntPtr members;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstVariableDeclaration
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// int
            public int attributes;

            /// AstDataType*
            public IntPtr datatype;

            /// AstStructDeclaration*
            public IntPtr anonymous_datatype;

            /// AstScalarOrArray*
            public IntPtr details;

            /// char*
            public IntPtr semantic;

            /// AstAnnotations*
            public IntPtr annotations;

            /// AstExpression*
            public IntPtr initializer;

            /// AstVariableLowLevel*
            public IntPtr lowlevel;

            /// AstVariableDeclaration*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstBlockStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstStatement*
            public IntPtr statements;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstReturnStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstExpression*
            public IntPtr expr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstExpressionStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstExpression*
            public IntPtr expr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstIfStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// int
            public int attributes;

            /// AstExpression*
            public IntPtr expr;

            /// AstStatement*
            public IntPtr statement;

            /// AstStatement*
            public IntPtr else_statement;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstSwitchCases
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstExpression*
            public IntPtr expr;

            /// AstStatement*
            public IntPtr statement;

            /// AstSwitchCases*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstSwitchStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// int
            public int attributes;

            /// AstExpression*
            public IntPtr expr;

            /// AstSwitchCases*
            public IntPtr cases;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstWhileStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// int
            public int unroll;

            /// AstExpression*
            public IntPtr expr;

            /// AstStatement*
            public IntPtr statement;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstForStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// int
            public int unroll;

            /// AstVariableDeclaration*
            public IntPtr var_decl;

            /// AstExpression*
            public IntPtr initializer;

            /// AstExpression*
            public IntPtr looptest;

            /// AstExpression*
            public IntPtr counter;

            /// AstStatement*
            public IntPtr statement;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstTypedef
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstDataType*
            public IntPtr datatype;

            /// int
            public int isconst;

            /// AstScalarOrArray*
            public IntPtr details;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstTypedefStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstTypedef*
            public IntPtr type_info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstVarDeclStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstVariableDeclaration*
            public IntPtr declaration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstStructStatement
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstStatement*
            public IntPtr next;

            /// AstStructDeclaration*
            public IntPtr struct_info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstCompilationUnitFunction
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstCompilationUnit*
            public IntPtr next;

            /// AstFunctionSignature*
            public IntPtr declaration;

            /// AstStatement*
            public IntPtr definition;

            /// int
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstCompilationUnitTypedef
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstCompilationUnit*
            public IntPtr next;

            /// AstTypedef*
            public IntPtr type_info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstCompilationUnitStruct
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstCompilationUnit*
            public IntPtr next;

            /// AstStructDeclaration*
            public IntPtr struct_info;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstCompilationUnitVariable
        {

            /// AstNodeInfo
            public AstNodeInfo ast;

            /// AstCompilationUnit*
            public IntPtr next;

            /// AstVariableDeclaration*
            public IntPtr declaration;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct AstNode
        {

            /// AstNodeInfo
            [FieldOffset(0)]
            public AstNodeInfo ast;

            /// AstGeneric
            [FieldOffset(0)]
            public AstGeneric generic;

            /// AstExpression
            [FieldOffset(0)]
            public AstExpression expression;

            /// AstArguments
            [FieldOffset(0)]
            public AstArguments arguments;

            /// AstExpressionUnary
            [FieldOffset(0)]
            public AstExpressionUnary unary;

            /// AstExpressionBinary
            [FieldOffset(0)]
            public AstExpressionBinary binary;

            /// AstExpressionTernary
            [FieldOffset(0)]
            public AstExpressionTernary ternary;

            /// AstExpressionIdentifier
            [FieldOffset(0)]
            public AstExpressionIdentifier identifier;

            /// AstExpressionIntLiteral
            [FieldOffset(0)]
            public AstExpressionIntLiteral intliteral;

            /// AstExpressionFloatLiteral
            [FieldOffset(0)]
            public AstExpressionFloatLiteral floatliteral;

            /// AstExpressionStringLiteral
            [FieldOffset(0)]
            public AstExpressionStringLiteral stringliteral;

            /// AstExpressionBooleanLiteral
            [FieldOffset(0)]
            public AstExpressionBooleanLiteral boolliteral;

            /// AstExpressionConstructor
            [FieldOffset(0)]
            public AstExpressionConstructor constructor;

            /// AstExpressionDerefStruct
            [FieldOffset(0)]
            public AstExpressionDerefStruct derefstruct;

            /// AstExpressionCallFunction
            [FieldOffset(0)]
            public AstExpressionCallFunction callfunc;

            /// AstExpressionCast
            [FieldOffset(0)]
            public AstExpressionCast cast;

            /// AstCompilationUnit
            [FieldOffset(0)]
            public AstCompilationUnit compunit;

            /// AstFunctionParameters
            [FieldOffset(0)]
            public AstFunctionParameters @params;

            /// AstFunctionSignature
            [FieldOffset(0)]
            public AstFunctionSignature funcsig;

            /// AstScalarOrArray
            [FieldOffset(0)]
            public AstScalarOrArray soa;

            /// AstAnnotations
            [FieldOffset(0)]
            public AstAnnotations annotations;

            /// AstPackOffset
            [FieldOffset(0)]
            public AstPackOffset packoffset;

            /// AstVariableLowLevel
            [FieldOffset(0)]
            public AstVariableLowLevel varlowlevel;

            /// AstStructMembers
            [FieldOffset(0)]
            public AstStructMembers structmembers;

            /// AstStructDeclaration
            [FieldOffset(0)]
            public AstStructDeclaration structdecl;

            /// AstVariableDeclaration
            [FieldOffset(0)]
            public AstVariableDeclaration vardecl;

            /// AstStatement
            [FieldOffset(0)]
            public AstStatement stmt;

            /// AstEmptyStatement->AstStatement
            [FieldOffset(0)]
            public AstStatement emptystmt;

            /// AstBreakStatement->AstStatement
            [FieldOffset(0)]
            public AstStatement breakstmt;

            /// AstContinueStatement->AstStatement
            [FieldOffset(0)]
            public AstStatement contstmt;

            /// AstDiscardStatement->AstStatement
            [FieldOffset(0)]
            public AstStatement discardstmt;

            /// AstBlockStatement
            [FieldOffset(0)]
            public AstBlockStatement blockstmt;

            /// AstReturnStatement
            [FieldOffset(0)]
            public AstReturnStatement returnstmt;

            /// AstExpressionStatement
            [FieldOffset(0)]
            public AstExpressionStatement exprstmt;

            /// AstIfStatement
            [FieldOffset(0)]
            public AstIfStatement ifstmt;

            /// AstSwitchCases
            [FieldOffset(0)]
            public AstSwitchCases cases;

            /// AstSwitchStatement
            [FieldOffset(0)]
            public AstSwitchStatement switchstmt;

            /// AstWhileStatement
            [FieldOffset(0)]
            public AstWhileStatement whilestmt;

            /// AstDoStatement->AstWhileStatement
            [FieldOffset(0)]
            public AstWhileStatement dostmt;

            /// AstForStatement
            [FieldOffset(0)]
            public AstForStatement forstmt;

            /// AstTypedef
            [FieldOffset(0)]
            public AstTypedef typdef;

            /// AstTypedefStatement
            [FieldOffset(0)]
            public AstTypedefStatement typedefstmt;

            /// AstVarDeclStatement
            [FieldOffset(0)]
            public AstVarDeclStatement vardeclstmt;

            /// AstStructStatement
            [FieldOffset(0)]
            public AstStructStatement structstmt;

            /// AstCompilationUnitFunction
            [FieldOffset(0)]
            public AstCompilationUnitFunction funcunit;

            /// AstCompilationUnitTypedef
            [FieldOffset(0)]
            public AstCompilationUnitTypedef typedefunit;

            /// AstCompilationUnitStruct
            [FieldOffset(0)]
            public AstCompilationUnitStruct structunit;

            /// AstCompilationUnitVariable
            [FieldOffset(0)]
            public AstCompilationUnitVariable varunit;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AstData
        {
            /// int
            public int error_count;

            /// Error*
            public IntPtr errors;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string source_profile;

            /// AstNode*
            public IntPtr ast;

            /// malloc
            public IntPtr Malloc;

            /// Free
            public IntPtr free;

            /// void*
            public IntPtr malloc_data;

            /// void*
            public IntPtr opaque;
        }

        public enum irNodeType
        {
            START_RANGE_EXPR,
            CONSTANT,
            TEMP,
            BINOP,
            MEMORY,
            CALL,
            ESEQ,
            ARRAY,
            CONVERT,
            SWIZZLE,
            CONSTRUCT,
            END_RANGE_EXPR,
            START_RANGE_STMT,
            MOVE,
            EXPR_STMT,
            JUMP,
            CJUMP,
            SEQ,
            LABEL,
            DISCARD,
            END_RANGE_STMT,
            START_RANGE_MISC,
            EXPRLIST,
            END_RANGE_MISC,
            END_RANGE,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irNodeInfo
        {
            /// irNodeType
            public irNodeType type;

            /// char*
            public IntPtr filename;

            /// unsigned int
            public uint line;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irGeneric
        {
            /// irNodeInfo
            public irNodeInfo ir;
        }

        public enum irBinOpType
        {
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE,
            MODULO,
            AND,
            OR,
            XOR,
            LSHIFT,
            RSHIFT,
            UNKNOWN,
        }

        public enum irConditionType
        {
            EQL,
            NEQ,
            LT,
            GT,
            LEQ,
            GEQ,
            UNKNOWN,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irExprInfo
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// AstDataTypeType
            public AstDataTypeType type;

            /// int
            public int elements;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irConstant
        {
            /// irExprInfo
            public irExprInfo info;

            /// Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111
            public Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111 value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irTemp
        {
            /// irExprInfo
            public irExprInfo info;

            /// int
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irBinOp
        {
            /// irExprInfo
            public irExprInfo info;

            /// irBinOpType
            public irBinOpType op;

            /// irExpression*
            public IntPtr left;

            /// irExpression*
            public IntPtr right;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irMemory
        {
            /// irExprInfo
            public irExprInfo info;

            /// int
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irCall
        {
            /// irExprInfo
            public irExprInfo info;

            /// int
            public int index;

            /// irExprList*
            public IntPtr args;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irESeq
        {
            /// irExprInfo
            public irExprInfo info;

            /// irStatement*
            public IntPtr stmt;

            /// irExpression*
            public IntPtr expr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irArray
        {
            /// irExprInfo
            public irExprInfo info;

            /// irExpression*
            public IntPtr array;

            /// irExpression*
            public IntPtr element;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irConvert
        {
            /// irExprInfo
            public irExprInfo info;

            /// irExpression*
            public IntPtr expr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct irSwizzle
        {
            /// irExprInfo
            public irExprInfo info;

            /// irExpression*
            public IntPtr expr;

            /// char[4]
            public IntPtr channels;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irConstruct
        {
            /// irExprInfo
            public irExprInfo info;

            /// irExprList*
            public IntPtr args;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct irExpression
        {
            /// irNodeInfo
            [FieldOffset(0)]
            public irNodeInfo ir;

            /// irExprInfo
            [FieldOffset(0)]
            public irExprInfo info;

            /// irConstant
            [FieldOffset(0)]
            public irConstant constant;

            /// irTemp
            [FieldOffset(0)]
            public irTemp temp;

            /// irBinOp
            [FieldOffset(0)]
            public irBinOp binop;

            /// irMemory
            [FieldOffset(0)]
            public irMemory memory;

            /// irCall
            [FieldOffset(0)]
            public irCall call;

            /// irESeq
            [FieldOffset(0)]
            public irESeq eseq;

            /// irArray
            [FieldOffset(0)]
            public irArray array;

            /// irConvert
            [FieldOffset(0)]
            public irConvert convert;

            /// irSwizzle
            [FieldOffset(0)]
            public irSwizzle swizzle;

            /// irConstruct
            [FieldOffset(0)]
            public irConstruct construct;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irMove
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// irExpression*
            public IntPtr dst;

            /// irExpression*
            public IntPtr src;

            /// int
            public int writemask;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irExprStmt
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// irExpression*
            public IntPtr expr;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irJump
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// int
            public int label;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irCJump
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// irConditionType
            public irConditionType cond;

            /// irExpression*
            public IntPtr left;

            /// irExpression*
            public IntPtr right;

            /// int
            public int iftrue;

            /// int
            public int iffalse;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irSeq
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// irStatement*
            public IntPtr first;

            /// irStatement*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irLabel
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// int
            public int index;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct irStatement
        {
            /// irNodeInfo
            [FieldOffset(0)]
            public irNodeInfo ir;

            /// irGeneric
            [FieldOffset(0)]
            public irGeneric generic;

            /// irMove
            [FieldOffset(0)]
            public irMove move;

            /// irExprStmt
            [FieldOffset(0)]
            public irExprStmt expr;

            /// irJump
            [FieldOffset(0)]
            public irJump jump;

            /// irCJump
            [FieldOffset(0)]
            public irCJump cjump;

            /// irSeq
            [FieldOffset(0)]
            public irSeq seq;

            /// irLabel
            [FieldOffset(0)]
            public irLabel label;

            /// irDiscard->irGeneric
            [FieldOffset(0)]
            public irGeneric discard;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct irExprList
        {
            /// irNodeInfo
            public irNodeInfo ir;

            /// irExpression*
            public IntPtr expr;

            /// irExprList*
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct irMisc
        {
            /// irNodeInfo
            [FieldOffset(0)]
            public irNodeInfo ir;

            /// irGeneric
            [FieldOffset(0)]
            public irGeneric generic;

            /// irExprList
            [FieldOffset(0)]
            public irExprList exprlist;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct irNode
        {
            /// irNodeInfo
            [FieldOffset(0)]
            public irNodeInfo ir;

            /// irGeneric
            [FieldOffset(0)]
            public irGeneric generic;

            /// irExpression
            [FieldOffset(0)]
            public irExpression expr;

            /// irStatement
            [FieldOffset(0)]
            public irStatement stmt;

            /// irMisc
            [FieldOffset(0)]
            public irMisc misc;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CompileData
        {
            /// int
            public int error_count;

            /// Error*
            public IntPtr errors;

            /// int
            public int warning_count;

            /// Error*
            public IntPtr warnings;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string source_profile;

            /// char*
            [MarshalAs(UnmanagedType.LPStr)]
            public string output;

            /// int
            public int output_len;

            /// int
            public int symbol_count;

            /// Symbol*
            public IntPtr symbols;

            /// Malloc
            public IntPtr malloc;

            /// Free
            public IntPtr free;

            /// void*
            public IntPtr malloc_data;
        }

        /// Return Type: void*
        ///fnname: char*
        ///data: void*
        public delegate IntPtr glGetProcAddress([In()] [MarshalAs(UnmanagedType.LPStr)] string fnname, IntPtr data);

        [StructLayout(LayoutKind.Explicit)]
        public struct Anonymous_5371dd6a_e42a_47c1_91d1_a2af9a8283be
        {
            /// float[4]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.R4)]
            [FieldOffset(0)]
            public IntPtr f;

            /// int[4]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
            [FieldOffset(0)]
            public IntPtr i;

            /// int
            [FieldOffset(0)]
            public int b;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Anonymous_3a13e6d2_72d8_4c86_b5bf_9aff36c73111
        {
            /// int[16]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I4)]
            [FieldOffset(0)]
            public IntPtr ival;

            /// float[16]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.R4)]
            [FieldOffset(0)]
            public IntPtr fval;
        }

        public enum UniformType
        {
            /// UNKNOWN -> -1
            UNKNOWN = -1,
            FLOAT,
            INT,
            BOOL,
        }

        public enum SamplerType
        {
            /// UNKNOWN -> -1
            SAMPLER_UNKNOWN = -1,
            SAMPLER_2D = 0,
            SAMPLER_CUBE = 1,
            SAMPLER_VOLUME = 2,
            SAMPLER_1D = 3,
        }

        public enum Usage
        {
            /// UNKNOWN -> -1
            UNKNOWN = -1,
            POSITION,
            BLENDWEIGHT,
            BLENDINDICES,
            NORMAL,
            POINTSIZE,
            TEXCOORD,
            TANGENT,
            BINORMAL,
            TESSFACTOR,
            POSITIONT,
            COLOR,
            FOG,
            DEPTH,
            SAMPLE,
            TOTAL,
        }

        public enum SymbolClass
        {
            SCALAR,
            VECTOR,
            MATRIX_ROWS,
            MATRIX_COLUMNS,
            OBJECT,
            STRUCT,
        }

        public enum SymbolType
        {
            VOID,
            BOOL,
            INT,
            FLOAT,
            STRING,
            TEXTURE,
            TEXTURE1D,
            TEXTURE2D,
            TEXTURE3D,
            TEXTURECUBE,
            SAMPLER,
            SAMPLER1D,
            SAMPLER2D,
            SAMPLER3D,
            SAMPLERCUBE,
            PIXELSHADER,
            VERTEXSHADER,
            PIXELFRAGMENT,
            VERTEXFRAGMENT,
            UNSUPPORTED,
        }

        public enum SymbolRegisterSet
        {
            BOOL,
            INT4,
            FLOAT4,
            SAMPLER,
        }

        public enum ShaderType
        {

            // UNKNOWN -> 0
            UNKNOWN = 0,
            // PIXEL -> (1<<0)
            PIXEL = (1) << (0),
            // VERTEX -> (1<<1)
            VERTEX = (1) << (1),
            // GEOMETRY -> (1<<2)
            GEOMETRY = (1) << (2),
            // ANY -> 0xFFFFFFFF
            ANY = -1,
        }

        public enum IncludeType
        {
            LOCAL,
            SYSTEM,
        }

        public enum AttributeType
        {
            // UNKNOWN -> -1
            UNKNOWN = -1,
            BYTE,
            UBYTE,
            SHORT,
            USHORT,
            INT,
            UINT,
            FLOAT,
            DOUBLE,
            HALF_FLOAT,
        }

        public partial class NativeMethods
        {

            /// Return Type: int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_version")]
            public static extern int Version();


            /// Return Type: char*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_changeset")]
            public static extern IntPtr Changeset();


            /// Return Type: int
            ///profile: char*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_maxShaderModel")]
            public static extern int MaxShaderModel([In()] [MarshalAs(UnmanagedType.LPStr)] string profile);


            /// Return Type: ParseData*
            ///tokenbuf: char*
            ///bufsize: int
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_parseExpression")]
            public static extern IntPtr ParseExpression([In()] byte[] tokenbuf, int bufsize, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///param0: Preshader*
            ///param1: float*
            ///param2: float*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_runPreshader")]
            public static extern void RunPreshader(ref Preshader param0, ref float param1, ref float param2);


            /// Return Type: ParseData*
            ///profile: char*
            ///tokenbuf: char*
            ///bufsize: int
            ///swiz: Swizzle*
            ///swizcount: int
            ///smap: SamplerMap*
            ///smapcount: int
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_parse")]
            public static extern IntPtr Parse([In()] [MarshalAs(UnmanagedType.LPStr)] string profile, [In()] byte[] tokenbuf, int bufsize, IntPtr swiz, int swizcount, IntPtr smap, int smapcount, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///data: ParseData*
            [DllImport(mojoshader_dll, EntryPoint = "FreeParseData")]
            public static extern void FreeParseData(ref ParseData data);


            /// Return Type: Effect*
            ///profile: char*
            ///buf: char*
            ///_len: int
            ///swiz: Swizzle*
            ///swizcount: int
            ///smap: SamplerMap*
            ///smapcount: int
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_parseEffect")]
            public static extern IntPtr ParseEffect([In()] [MarshalAs(UnmanagedType.LPStr)] string profile, [In()] [MarshalAs(UnmanagedType.LPStr)] string buf, int _len, ref Swizzle swiz, int swizcount, ref SamplerMap smap, int smapcount, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///effect: Effect*
            [DllImport(mojoshader_dll, EntryPoint = "FreeEffect")]
            public static extern void FreeEffect(ref Effect effect);


            /// Return Type: PreprocessData*
            ///filename: char*
            ///source: char*
            ///sourcelen: unsigned int
            ///defines: PreprocessorDefine*
            ///define_count: unsigned int
            ///include_open: IncludeOpen
            ///include_close: IncludeClose
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_preprocess")]
            public static extern IntPtr Preprocess([In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref PreprocessorDefine defines, uint define_count, IncludeOpen include_open, IncludeClose include_close, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///data: PreprocessData*
            [DllImport(mojoshader_dll, EntryPoint = "FreePreprocessData")]
            public static extern void FreePreprocessData(ref PreprocessData data);


            /// Return Type: ParseData*
            ///filename: char*
            ///source: char*
            ///sourcelen: unsigned int
            ///comments: char**
            ///comment_count: unsigned int
            ///symbols: Symbol*
            ///symbol_count: unsigned int
            ///defines: PreprocessorDefine*
            ///define_count: unsigned int
            ///include_open: IncludeOpen
            ///include_close: IncludeClose
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_assemble")]
            public static extern IntPtr Assemble([In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref IntPtr comments, uint comment_count, ref Symbol symbols, uint symbol_count, ref PreprocessorDefine defines, uint define_count, IncludeOpen include_open, IncludeClose include_close, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: AstData*
            ///srcprofile: char*
            ///filename: char*
            ///source: char*
            ///sourcelen: unsigned int
            ///defs: PreprocessorDefine*
            ///define_count: unsigned int
            ///include_open: IncludeOpen
            ///include_close: IncludeClose
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_parseAst")]
            public static extern IntPtr ParseAst([In()] [MarshalAs(UnmanagedType.LPStr)] string srcprofile, [In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref PreprocessorDefine defs, uint define_count, IncludeOpen include_open, IncludeClose include_close, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///data: AstData*
            [DllImport(mojoshader_dll, EntryPoint = "FreeAstData")]
            public static extern void FreeAstData(ref AstData data);


            /// Return Type: CompileData*
            ///srcprofile: char*
            ///filename: char*
            ///source: char*
            ///sourcelen: unsigned int
            ///defs: PreprocessorDefine*
            ///define_count: unsigned int
            ///include_open: IncludeOpen
            ///include_close: IncludeClose
            ///m: Malloc
            ///f: Free
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_compile")]
            public static extern IntPtr Compile([In()] [MarshalAs(UnmanagedType.LPStr)] string srcprofile, [In()] [MarshalAs(UnmanagedType.LPStr)] string filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string source, uint sourcelen, ref PreprocessorDefine defs, uint define_count, IncludeOpen include_open, IncludeClose include_close, IntPtr m, IntPtr f, IntPtr d);


            /// Return Type: void
            ///data: CompileData*
            [DllImport(mojoshader_dll, EntryPoint = "FreeCompileData")]
            public static extern void FreeCompileData(ref CompileData data);


            /// Return Type: int
            ///lookup: glGetProcAddress
            ///d: void*
            ///profs: char**
            ///size: int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glAvailableProfiles")]
            public static extern int glAvailableProfiles(glGetProcAddress lookup, IntPtr d, ref IntPtr profs, int size);


            /// Return Type: char*
            ///lookup: glGetProcAddress
            ///d: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glBestProfile")]
            public static extern IntPtr glBestProfile(glGetProcAddress lookup, IntPtr d);


            /// Return Type: char*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetError")]
            public static extern IntPtr glGetError();


            /// Return Type: int
            ///shader_type: ShaderType->Anonymous_96517ad6_cc69_4542_8537_054e63919d54
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glMaxUniforms")]
            public static extern int glMaxUniforms(ShaderType shader_type);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetVertexShaderUniformF")]
            public static extern void glSetVertexShaderUniformF(uint idx, ref float data, uint vec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetVertexShaderUniformF")]
            public static extern void glGetVertexShaderUniformF(uint idx, ref float data, uint vec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///ivec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetVertexShaderUniformI")]
            public static extern void glSetVertexShaderUniformI(uint idx, ref int data, uint ivec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///ivec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetVertexShaderUniformI")]
            public static extern void glGetVertexShaderUniformI(uint idx, ref int data, uint ivec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///bcount: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetVertexShaderUniformB")]
            public static extern void glSetVertexShaderUniformB(uint idx, ref int data, uint bcount);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///bcount: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetVertexShaderUniformB")]
            public static extern void glGetVertexShaderUniformB(uint idx, ref int data, uint bcount);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetPixelShaderUniformF")]
            public static extern void glSetPixelShaderUniformF(uint idx, ref float data, uint vec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetPixelShaderUniformF")]
            public static extern void glGetPixelShaderUniformF(uint idx, ref float data, uint vec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///ivec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetPixelShaderUniformI")]
            public static extern void glSetPixelShaderUniformI(uint idx, ref int data, uint ivec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///ivec4count: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetPixelShaderUniformI")]
            public static extern void glGetPixelShaderUniformI(uint idx, ref int data, uint ivec4count);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///bcount: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetPixelShaderUniformB")]
            public static extern void glSetPixelShaderUniformB(uint idx, ref int data, uint bcount);


            /// Return Type: void
            ///idx: unsigned int
            ///data: int*
            ///bcount: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetPixelShaderUniformB")]
            public static extern void glGetPixelShaderUniformB(uint idx, ref int data, uint bcount);


            /// Return Type: void
            ///sampler: unsigned int
            ///mat00: float
            ///mat01: float
            ///mat10: float
            ///mat11: float
            ///lscale: float
            ///loffset: float
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetLegacyBumpMapEnv")]
            public static extern void glSetLegacyBumpMapEnv(uint sampler, float mat00, float mat01, float mat10, float mat11, float lscale, float loffset);


            /// Return Type: void
            ///usage: Usage->Anonymous_9c01433d_7bb5_4c50_bf77_e65cef0661b5
            ///index: int
            ///size: unsigned int
            ///type: AttributeType->Anonymous_2f2591e6_1657_418c_9f54_80f3acd43cbe
            ///normalized: int
            ///stride: unsigned int
            ///ptr: void*
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetVertexAttribute")]
            public static extern void glSetVertexAttribute(Usage usage, int index, uint size, AttributeType type, int normalized, uint stride, IntPtr ptr);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4n: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetVertexPreshaderUniformF")]
            public static extern void glSetVertexPreshaderUniformF(uint idx, ref float data, uint vec4n);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4n: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetVertexPreshaderUniformF")]
            public static extern void glGetVertexPreshaderUniformF(uint idx, ref float data, uint vec4n);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4n: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glSetPixelPreshaderUniformF")]
            public static extern void glSetPixelPreshaderUniformF(uint idx, ref float data, uint vec4n);


            /// Return Type: void
            ///idx: unsigned int
            ///data: float*
            ///vec4n: unsigned int
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glGetPixelPreshaderUniformF")]
            public static extern void glGetPixelPreshaderUniformF(uint idx, ref float data, uint vec4n);


            /// Return Type: void
            [DllImport(mojoshader_dll, EntryPoint = "MOJOSHADER_glProgramReady")]
            public static extern void glProgramReady();

        }


    }
}

