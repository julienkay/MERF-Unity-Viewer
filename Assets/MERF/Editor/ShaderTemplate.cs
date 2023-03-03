public static class ShaderTemplate {

    public const string Template = @"Shader ""MERF/RayMarchShader_OBJECT_NAME"" {
    Properties {
        _OccupancyGrid_L4      (""OccupancyGrid_L4""     , 3D     ) = """" {}
        _OccupancyGrid_L3      (""OccupancyGrid_L3""     , 3D     ) = """" {}
        _OccupancyGrid_L2      (""OccupancyGrid_L2""     , 3D     ) = """" {}
        _OccupancyGrid_L1      (""OccupancyGrid_L1""     , 3D     ) = """" {}
        _OccupancyGrid_L0      (""OccupancyGrid_L0""     , 3D     ) = """" {}
        _VoxelSizeOccupancy_L4 (""VoxelSizeOccupancy_L4"", Float  ) = 0.0
        _VoxelSizeOccupancy_L3 (""VoxelSizeOccupancy_L3"", Float  ) = 0.0
        _VoxelSizeOccupancy_L2 (""VoxelSizeOccupancy_L2"", Float  ) = 0.0
        _VoxelSizeOccupancy_L1 (""VoxelSizeOccupancy_L1"", Float  ) = 0.0
        _VoxelSizeOccupancy_L0 (""VoxelSizeOccupancy_L0"", Float  ) = 0.0
        _GridSizeOccupancy_L4  (""_GridSizeOccupancy_L4"", Vector ) = (0, 0, 0, 0)
        _GridSizeOccupancy_L3  (""_GridSizeOccupancy_L3"", Vector ) = (0, 0, 0, 0)
        _GridSizeOccupancy_L2  (""_GridSizeOccupancy_L2"", Vector ) = (0, 0, 0, 0)
        _GridSizeOccupancy_L1  (""_GridSizeOccupancy_L1"", Vector ) = (0, 0, 0, 0)
        _GridSizeOccupancy_L0  (""_GridSizeOccupancy_L0"", Vector ) = (0, 0, 0, 0)
        _DisplayMode           (""_DisplayMode""          , Integer) = 0
        _WeightsZero           (""Weights Zero""         , 2D     ) = ""white"" {}
        _WeightsOne            (""Weights One""          , 2D     ) = ""white"" {}
        _WeightsTwo            (""Weights Two""          , 2D     ) = ""white"" {}
	    _MinPosition           (""Min Position""         , Vector ) = (0, 0, 0, 0)
        _StepMult              (""StepMult""             , Integer) = 1
        
        _PlaneRgb              (""PlaneRgb""             , 2DArray) = """" {}
        _PlaneDensity          (""PlaneDensity""         , 2DArray) = """" {}
        _PlaneFeatures         (""PlaneFeatures""        , 2DArray) = """" {}
	    _PlaneSize             (""PlaneSize""            , Vector ) = (0, 0, 0, 0)
        _VoxelSizeTriplane     (""VoxelSizeTriplane""    , Float  ) = 0.0

        _SparseGridDensity     (""SparseGridDensity""    , 3D     ) = ""white"" {}
        _SparseGridRgb         (""SparseGridRgb""        , 3D     ) = ""white"" {}
        _SparseGridFeatures    (""SparseGridFeatures""   , 3D     ) = ""white"" {}
        _SparseGridIndex       (""SparseGridIndex""      , 3D     ) = ""white"" {}
	    _BlockSize             (""Block Size""           , Float  ) = 0.0
	    _VoxelSize             (""Voxel Size""           , Float  ) = 0.0
        _GridSize              (""Grid Size""            , Vector ) = (0, 0, 0, 0)
        _AtlasSize             (""Atlas Size""           , Vector ) = (0, 0, 0, 0)
    }
    SubShader {
        Cull Front
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            #define UNITY_SHADER_NO_UPGRADE 
            #pragma shader_feature_local USE_TRIPLANE
            #pragma shader_feature_local USE_SPARSE_GRID
            #pragma shader_feature_local LARGER_STEPS_WHEN_OCCLUDED

            #pragma vertex vert
            #pragma fragment frag

            #include ""UnityCG.cginc""

            float3 vOrigin;
            float3 vDirection;

            float3x3 _WorldspaceROpengl;
            int _DisplayMode;
            float4 _MinPosition;

            sampler3D OccupancyGrid_L0;
            sampler3D OccupancyGrid_L1;
            sampler3D OccupancyGrid_L2;
            sampler3D OccupancyGrid_L3;
            sampler3D OccupancyGrid_L4;

            float VoxelSizeOccupancy_L0;
            float VoxelSizeOccupancy_L1;
            float VoxelSizeOccupancy_L2;
            float VoxelSizeOccupancy_L3;
            float VoxelSizeOccupancy_L4;

            float4 _GridSizeOccupancy_L0;
            float4 _GridSizeOccupancy_L1;
            float4 _GridSizeOccupancy_L2;
            float4 _GridSizeOccupancy_L3;
            float4 _GridSizeOccupancy_L4;

            UNITY_DECLARE_TEX2D(_WeightsZero);
            UNITY_DECLARE_TEX2D(_WeightsOne);
            UNITY_DECLARE_TEX2D(_WeightsTwo);

            int _StepMult;
            float3 _GridSize;
            float _VoxelSize;

            #ifdef USE_SPARSE_GRID
            float3 _AtlasSize;
            float _BlockSize;
            sampler3D _SparseGridDensity;
            sampler3D _SparseGridRgb;
            sampler3D _SparseGridFeatures;
            sampler3D _SparseGridIndex;
            #endif

            #ifdef USE_TRIPLANE
            // need to use texture arrays, otherwise we exceed max texture unit limit
            float sampler2DArray _PlaneRgb;
            float sampler2DArray _PlaneDensity;
            float sampler2DArray _PlaneFeatures;
            float2 _PlaneSize;
            float _VoxelSizeTriplane;
            #endif

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 origin : TEXCOORD1;
                float3 direction : TEXCOORD2;
            };

            VIEWDEPENDENCESHADERFUNCTIONS

            RAYMARCHVERTEXSHADER

            RAYMARCHFRAGMENTSHADER

            ENDCG
        }
    }
}
";
    /// <summary>
    /// The ray marching shader is built programmatically.
    /// This string contains the header for the shader.
    /// </summary>
    public static string RayMarchFragmentShaderHeader {
        get {
            return HEADER;
        }
    }

    public static string RayMarchVertexShader {
        get {
            return VERTEX;
        }
    }

    public static string RayMarchFragmentShaderBody {
        get {
            return FRAGMENT;
        }
    }

    public static string ViewDependenceNetworkShaderFunctions {
        get {
            return VIEWDEPENDENCY;
        }
    }
    
    private const string HEADER = @"";

    private const string VIEWDEPENDENCY = @"float indexToPosEnc(float3 dir, int index) {
                float coordinate =
                    (index % 3 == 0) ? dir.x : (
                    (index % 3 == 1) ? dir.y : dir.z);
                if (index < 3) {
                    return coordinate;
                }
                int scaleExponent = ((index - 3) % (3 * 4)) / 3;
                coordinate *= pow(2.0, float(scaleExponent));
                if ((index - 3) >= 3 * 4) {
                    const float kHalfPi = 1.57079632679489661923;
                    coordinate += kHalfPi;
                }
                return sin(coordinate);
            }

            float indexToInputValue(float3 color, float4 features, float3 viewdir, int j) {
                float input_value = 0.0;
                if (j < 3) {
                    input_value =
                        (j == 0) ? color.r : (
                        (j == 1) ? color.g : color.b);
                } else if (j < 7) {
                    input_value =
                        (j == 3) ? features.r : (
                        (j == 4) ? features.g : (
                        (j == 5) ? features.b : features.a));
                } else {
                    input_value = indexToPosEnc(viewdir, j - 7);
                }
                if (abs(input_value) < 0.1 / 255.0) {
                    input_value = 0.0;
                }
                return input_value;
            }

            float4 relu(float4 x) {
                return max(x, 0.0);
            }

            float3 evaluateNetwork(float3 color, float4 features, float3 viewdir) {
                float4 intermediate_one[16/4] = {
                    float4(0.0000000, 0.0000000, 0.0000000, 0.0000000),
                    float4(0.0000000, 0.0000000, 0.0000000, 0.0000000),
                    float4(0.0000000, 0.0000000, 0.0000000, 0.0000000),
                    float4(0.0000000, 0.0000000, 0.0000000, 0.0000000)
                };

                float4 inp;
                float4x4 w;

                inp = float4(
                    indexToInputValue(color, features, viewdir, 0),
                    indexToInputValue(color, features, viewdir, 1),
                    indexToInputValue(color, features, viewdir, 2),
                    indexToInputValue(color, features, viewdir, 3));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 0, 0)),
                    _WeightsZero.Load(int3(0, 1, 0)),
                    _WeightsZero.Load(int3(0, 2, 0)),
                    _WeightsZero.Load(int3(0, 3, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 4, 0)),
                    _WeightsZero.Load(int3(0, 5, 0)),
                    _WeightsZero.Load(int3(0, 6, 0)),
                    _WeightsZero.Load(int3(0, 7, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 8, 0)),
                    _WeightsZero.Load(int3(0, 9, 0)),
                    _WeightsZero.Load(int3(0, 10, 0)),
                    _WeightsZero.Load(int3(0, 11, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 12, 0)),
                    _WeightsZero.Load(int3(0, 13, 0)),
                    _WeightsZero.Load(int3(0, 14, 0)),
                    _WeightsZero.Load(int3(0, 15, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 4),
                    indexToInputValue(color, features, viewdir, 5),
                    indexToInputValue(color, features, viewdir, 6),
                    indexToInputValue(color, features, viewdir, 7));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 16, 0)),
                    _WeightsZero.Load(int3(0, 17, 0)),
                    _WeightsZero.Load(int3(0, 18, 0)),
                    _WeightsZero.Load(int3(0, 19, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 20, 0)),
                    _WeightsZero.Load(int3(0, 21, 0)),
                    _WeightsZero.Load(int3(0, 22, 0)),
                    _WeightsZero.Load(int3(0, 23, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 24, 0)),
                    _WeightsZero.Load(int3(0, 25, 0)),
                    _WeightsZero.Load(int3(0, 26, 0)),
                    _WeightsZero.Load(int3(0, 27, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 28, 0)),
                    _WeightsZero.Load(int3(0, 29, 0)),
                    _WeightsZero.Load(int3(0, 30, 0)),
                    _WeightsZero.Load(int3(0, 31, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 8),
                    indexToInputValue(color, features, viewdir, 9),
                    indexToInputValue(color, features, viewdir, 10),
                    indexToInputValue(color, features, viewdir, 11));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 32, 0)),
                    _WeightsZero.Load(int3(0, 33, 0)),
                    _WeightsZero.Load(int3(0, 34, 0)),
                    _WeightsZero.Load(int3(0, 35, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 36, 0)),
                    _WeightsZero.Load(int3(0, 37, 0)),
                    _WeightsZero.Load(int3(0, 38, 0)),
                    _WeightsZero.Load(int3(0, 39, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 40, 0)),
                    _WeightsZero.Load(int3(0, 41, 0)),
                    _WeightsZero.Load(int3(0, 42, 0)),
                    _WeightsZero.Load(int3(0, 43, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 44, 0)),
                    _WeightsZero.Load(int3(0, 45, 0)),
                    _WeightsZero.Load(int3(0, 46, 0)),
                    _WeightsZero.Load(int3(0, 47, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 12),
                    indexToInputValue(color, features, viewdir, 13),
                    indexToInputValue(color, features, viewdir, 14),
                    indexToInputValue(color, features, viewdir, 15));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 48, 0)),
                    _WeightsZero.Load(int3(0, 49, 0)),
                    _WeightsZero.Load(int3(0, 50, 0)),
                    _WeightsZero.Load(int3(0, 51, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 52, 0)),
                    _WeightsZero.Load(int3(0, 53, 0)),
                    _WeightsZero.Load(int3(0, 54, 0)),
                    _WeightsZero.Load(int3(0, 55, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 56, 0)),
                    _WeightsZero.Load(int3(0, 57, 0)),
                    _WeightsZero.Load(int3(0, 58, 0)),
                    _WeightsZero.Load(int3(0, 59, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 60, 0)),
                    _WeightsZero.Load(int3(0, 61, 0)),
                    _WeightsZero.Load(int3(0, 62, 0)),
                    _WeightsZero.Load(int3(0, 63, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 16),
                    indexToInputValue(color, features, viewdir, 17),
                    indexToInputValue(color, features, viewdir, 18),
                    indexToInputValue(color, features, viewdir, 19));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 64, 0)),
                    _WeightsZero.Load(int3(0, 65, 0)),
                    _WeightsZero.Load(int3(0, 66, 0)),
                    _WeightsZero.Load(int3(0, 67, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 68, 0)),
                    _WeightsZero.Load(int3(0, 69, 0)),
                    _WeightsZero.Load(int3(0, 70, 0)),
                    _WeightsZero.Load(int3(0, 71, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 72, 0)),
                    _WeightsZero.Load(int3(0, 73, 0)),
                    _WeightsZero.Load(int3(0, 74, 0)),
                    _WeightsZero.Load(int3(0, 75, 0))
                    );
                    intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 76, 0)),
                    _WeightsZero.Load(int3(0, 77, 0)),
                    _WeightsZero.Load(int3(0, 78, 0)),
                    _WeightsZero.Load(int3(0, 79, 0))
                    );
                    intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 20),
                    indexToInputValue(color, features, viewdir, 21),
                    indexToInputValue(color, features, viewdir, 22),
                    indexToInputValue(color, features, viewdir, 23));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 80, 0)),
                    _WeightsZero.Load(int3(0, 81, 0)),
                    _WeightsZero.Load(int3(0, 82, 0)),
                    _WeightsZero.Load(int3(0, 83, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 84, 0)),
                    _WeightsZero.Load(int3(0, 85, 0)),
                    _WeightsZero.Load(int3(0, 86, 0)),
                    _WeightsZero.Load(int3(0, 87, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 88, 0)),
                    _WeightsZero.Load(int3(0, 89, 0)),
                    _WeightsZero.Load(int3(0, 90, 0)),
                    _WeightsZero.Load(int3(0, 91, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 92, 0)),
                    _WeightsZero.Load(int3(0, 93, 0)),
                    _WeightsZero.Load(int3(0, 94, 0)),
                    _WeightsZero.Load(int3(0, 95, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 24),
                    indexToInputValue(color, features, viewdir, 25),
                    indexToInputValue(color, features, viewdir, 26),
                    indexToInputValue(color, features, viewdir, 27));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 96, 0)),
                    _WeightsZero.Load(int3(0, 97, 0)),
                    _WeightsZero.Load(int3(0, 98, 0)),
                    _WeightsZero.Load(int3(0, 99, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 100, 0)),
                    _WeightsZero.Load(int3(0, 101, 0)),
                    _WeightsZero.Load(int3(0, 102, 0)),
                    _WeightsZero.Load(int3(0, 103, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 104, 0)),
                    _WeightsZero.Load(int3(0, 105, 0)),
                    _WeightsZero.Load(int3(0, 106, 0)),
                    _WeightsZero.Load(int3(0, 107, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 108, 0)),
                    _WeightsZero.Load(int3(0, 109, 0)),
                    _WeightsZero.Load(int3(0, 110, 0)),
                    _WeightsZero.Load(int3(0, 111, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 28),
                    indexToInputValue(color, features, viewdir, 29),
                    indexToInputValue(color, features, viewdir, 30),
                    indexToInputValue(color, features, viewdir, 31));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 112, 0)),
                    _WeightsZero.Load(int3(0, 113, 0)),
                    _WeightsZero.Load(int3(0, 114, 0)),
                    _WeightsZero.Load(int3(0, 115, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 116, 0)),
                    _WeightsZero.Load(int3(0, 117, 0)),
                    _WeightsZero.Load(int3(0, 118, 0)),
                    _WeightsZero.Load(int3(0, 119, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 120, 0)),
                    _WeightsZero.Load(int3(0, 121, 0)),
                    _WeightsZero.Load(int3(0, 122, 0)),
                    _WeightsZero.Load(int3(0, 123, 0))
                    );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 124, 0)),
                    _WeightsZero.Load(int3(0, 125, 0)),
                    _WeightsZero.Load(int3(0, 126, 0)),
                    _WeightsZero.Load(int3(0, 127, 0))
                    );
                intermediate_one[3] += mul(inp, w);

                inp = float4(
                    indexToInputValue(color, features, viewdir, 32),
                    indexToInputValue(color, features, viewdir, 33),
                    indexToInputValue(color, features, viewdir, 34),
                    indexToInputValue(color, features, viewdir, 35));

                w = float4x4(
                    _WeightsZero.Load(int3(0, 128, 0)),
                    _WeightsZero.Load(int3(0, 129, 0)),
                    _WeightsZero.Load(int3(0, 130, 0)),
                    _WeightsZero.Load(int3(0, 131, 0))
                    );
                intermediate_one[0] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 132, 0)),
                    _WeightsZero.Load(int3(0, 133, 0)),
                    _WeightsZero.Load(int3(0, 134, 0)),
                    _WeightsZero.Load(int3(0, 135, 0))
                    );
                intermediate_one[1] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 136, 0)),
                    _WeightsZero.Load(int3(0, 137, 0)),
                    _WeightsZero.Load(int3(0, 138, 0)),
                    _WeightsZero.Load(int3(0, 139, 0))
                );
                intermediate_one[2] += mul(inp, w);


                w = float4x4(
                    _WeightsZero.Load(int3(0, 140, 0)),
                    _WeightsZero.Load(int3(0, 141, 0)),
                    _WeightsZero.Load(int3(0, 142, 0)),
                    _WeightsZero.Load(int3(0, 143, 0))
                );
                intermediate_one[3] += mul(inp, w);


                float4 intermediate_two[16/4] = {
                    float4(0.2806560, -0.2163800, 0.1149169, 0.1220414),
                    float4(0.1760421, -0.2192539, 0.0500887, -0.2273668),
                    float4(0.0488495, 0.2099760, 0.0931385, 0.1399519),
                    float4(0.0994009, 0.2953304, 0.1486122, 0.0342005)
                };
                int j = 0;
                for (j = 0; j < 16/4; ++j) {
                    inp = relu(intermediate_one[j]);
                    for (int i = 0; i < 16; i += 4) {
                    w = float4x4(
                        _WeightsOne.Load(int3(0, j * 16 + i, 0)),
                        _WeightsOne.Load(int3(0, j * 16 + (i+1), 0)),
                        _WeightsOne.Load(int3(0, j * 16 + (i+2), 0)),
                        _WeightsOne.Load(int3(0, j * 16 + (i+3), 0))
                    );
                    intermediate_two[i/4] += mul(inp, w);
                    }
                }

                float4 result = float4(-0.2284832, -0.0142633, -0.2382168, 0.0000000);
                for (j = 0; j < 16/4; ++j) {
                    inp = relu(intermediate_two[j]);
                    w = float4x4(
                        _WeightsTwo.Load(int3(0, j * 4, 0)),
                        _WeightsTwo.Load(int3(0, j * 4 + 1, 0)),
                        _WeightsTwo.Load(int3(0, j * 4 + 2, 0)),
                        _WeightsTwo.Load(int3(0, j * 4 + 3, 0))
                    );
                    result.xyz += (mul(inp, w)).xyz;
                }
                return 1.0 / (1.0 + exp(-result.xyz)); // Sigmoid
            }
";
    
    private const string FRAGMENT = @"float2 rayAabbIntersection(float3 aabbMin, float3 aabbMax, float3 origin, float3 invDirection) {
              float3 t1 = (aabbMin - origin) * invDirection;
              float3 t2 = (aabbMax - origin) * invDirection;
              float3 tMin = min(t1, t2);
              float3 tMax = max(t1, t2);
              return float2(max(tMin.x, max(tMin.y, tMin.z)),
                          min(tMax.x, min(tMax.y, tMax.z)));
            }
            
            #define SIGMOID(DTYPE) DTYPE sigmoid(DTYPE x) { return 1.0 / (1.0 + exp(-x)); }
            SIGMOID(float3)
            SIGMOID(float4)
            
            #define DENORMALIZE(DTYPE)\
            DTYPE denormalize(DTYPE x, float min, float max) {\
                return min + x * (max - min);\
            }
            DENORMALIZE(float)
            DENORMALIZE(float3)
            DENORMALIZE(float4)
            
            float densityActivation(float x) { return exp(x - 1.0f); }
            
            float densityToAlpha(float x, float stepSize) {
              return 1.0 - exp(-x * stepSize);
            }
            
            // Component-wise maximum
            float max3 (float3 v) {
              return max (max (v.x, v.y), v.z);
            }
            
            // Projective contraction
            float3 contract(float3 x) {
              float3 xAbs = abs(x);
              float xMax = max3(xAbs);
              if (xMax <= 1.0) {
                return x;
              }
              float scale = 1.0 / xMax;
              float3 z = scale * x;
              // note that z.a = sign(z.a) where a is the the argmax component
              if (xAbs.x >= xAbs.y && xAbs.x >= xAbs.z) {
                z.x *= (2.0 - scale); // argmax = 0
              } else if (xAbs.y >= xAbs.x && xAbs.y >= xAbs.z) {
                z.y *= (2.0 - scale); // argmax = 1
              } else {
                z.z *= (2.0 - scale); // argmax = 2
              }
              return z;
            }
            
            // Inverse projective contraction
            float3 inverseContract(float3 z) {
              float3 zAbs = abs(z);
              float zMax = max3(zAbs);
              if (zMax <= 1.0) {
                return z;
              }
              float eps = 1e-6;
              float invZMax = max(eps, 2.0 - zMax);
              float scale = 1.0 / invZMax;
              float3 x = scale * z;
              if (zAbs.x >= zAbs.y && zAbs.x >= zAbs.z) {
                x.x = sign(x.x) * scale; // argmax = 0
              } else if (zAbs.y >= zAbs.x && zAbs.y >= zAbs.z) {
                x.y = sign(x.y) * scale; // argmax = 1
              } else {
                x.z = sign(x.z) * scale; // argmax = 2
              }
              return x;
            }
            
            // Sorts an array of length 5 in-place. This is hardcoded to 5 since a ray
            // traverses up to 5 quadrants.
            void sort5(inout float array[5], int arrayLength) {
              float t;
              for (int i = 0; i < arrayLength; ++i) {
                for (int j = i+1; j < arrayLength; ++j) {
                  if (array[j] < array[i]) {
                    t = array[i];
                    array[i] = array[j];
                    array[j] = t;
                  }
                }
              }
            }
            
            float  lt(float a, float b){ return a < b ? 1.0 : 0.0;}
            float  lessThan(float  a,float b){ return lt(a,b);}
            float2 lessThan(float2 a,float2 b){ return float2(lt(a.x,b.x),lt(a.y,b.y));}
            float3 lessThan(float3 a,float3 b){ return float3(lt(a.x,b.x),lt(a.y,b.y),lt(a.z,b.z));}
            float4 lessThan(float4 a,float4 b){ return float4(lt(a.x,b.x),lt(a.y,b.y),lt(a.z,b.z),lt(a.w,b.w));}
            float  gt(float a, float b){ return a > b ? 1.0 : 0.0;}
            float  greaterThan(float  a, float b){ return gt(a,b);}
            float2 greaterThan(float2 a, float2 b){ return float2(gt(a.x,b.x),gt(a.y,b.y));}
            float3 greaterThan(float3 a, float3 b){ return float3(gt(a.x,b.x),gt(a.y,b.y),gt(a.z,b.z));}
            float4 greaterThan(float4 a, float4 b){ return float4(gt(a.x,b.x),gt(a.y,b.y),gt(a.z,b.z),gt(a.w,b.w));}

            // A solution is invalid if it does not lie on the plane or is outside of 
            // the bounding box
            #define INF 1e25
            #define SOLUTION_CHECK(T, P, AXIS)\
            q = contract(o + T.AXIS * d);\
            if (abs(q.AXIS - P.AXIS) > eps || any(lessThan(q, aabbMin - eps)) ||\
                any(greaterThan(q, aabbMax + eps))) {\
              T.AXIS = -INF;\
            }
            
            // First checks wether the computed cancidate solutions are actually lieing on
            // the bounding box. Then of all the valid intersections we return the one with
            // the highest t-value (tMax).
            // o: origin
            // d: direction
            // t0: candiate solutions for intersections with minimum YZ, XZ, XY planes
            // t1: candiate solutions for intersections with maximum YZ, XZ, XY planes
            // aabbMin: minimum of axis-aligned bounding bound
            // aabbMin: maximum of axis-aligned bounding bound
            float getTMax(float3 o, float3 d, float3 t0, float3 t1,
              float3 aabbMin, float3 aabbMax) {
              float eps = 1e-3;
              float3 q;
            
              // Invalid solutions are set to -INF and therefore ignored.
              SOLUTION_CHECK(t0, aabbMin, x)
              SOLUTION_CHECK(t0, aabbMin, y)
              SOLUTION_CHECK(t0, aabbMin, z)
              SOLUTION_CHECK(t1, aabbMax, x)
              SOLUTION_CHECK(t1, aabbMax, y)
              SOLUTION_CHECK(t1, aabbMax, z)
              return max(max3(t0), max3(t1));
            }
            
            // The following functions compute intersections between rays and axis-aligned
            // planes in contracted space.
            // The seven functions correspond to seven cases assiociated with the seven
            // quadrants present in projective contraction. The functions are derived 
            // by solving contract(o+t*d) for t.
            // o: origin
            // d: direction
            // p: x, y and z components define axis-aligned planes that the ray (o, d) is
            //    intersected against
            //    (x -> YZ-plane, y -> XZ-plane, z -> XY-plane)
            float3 h(float3 o, float3 d, float3 p) {
              return (p - o) / d;
            }
            
            float3 h0(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = (1.0 / (2.0 - p.x) - o.x) / d.x;
              t.y = (o.y - p.y * o.x) / (p.y * d.x - d.y);
              t.z = (o.z - p.z * o.x) / (p.z * d.x - d.z);
              return t;
            }
            
            float3 h1(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = (o.x - p.x * o.y) / (p.x * d.y - d.x);
              t.y = (1.0 / (2.0 - p.y) - o.y) / d.y;
              t.z = (o.z - p.z * o.y) / (p.z * d.y - d.z);
              return t;
            }
            
            float3 h2(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = (o.x - p.x * o.z) / (p.x * d.z - d.x);
              t.y = (o.y - p.y * o.z) / (p.y * d.z - d.y);
              t.z = (1.0 / (2.0 - p.z) - o.z) / d.z;
              return t;
            }
            
            float3 h3(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = (1.0 / (-p.x - 2.0) - o.x) / d.x;
              t.y = -(o.x*p.y + o.y) / (d.x*p.y + d.y);
              t.z = -(o.x*p.z + o.z) / (d.x*p.z + d.z);
              return t;
            }
            
            float3 h4(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = -(o.y*p.x + o.x) / (d.y*p.x + d.x);
              t.y = (1.0 / (-p.y - 2.0) - o.y) / d.y;
              t.z = -(o.y*p.z + o.z) / (d.y*p.z + d.z);
              return t;
            }
            
            float3 h5(float3 o, float3 d, float3 p) {
              float3 t;
              t.x = -(o.z*p.x + o.x) / (d.z*p.x + d.x);
              t.y = -(o.z*p.y + o.y) / (d.z*p.y + d.y);
              t.z = (1.0 / (-p.z - 2.0) - o.z) / d.z;
              return t;
            }
            
            struct Quadrants {
                float array[5];
            };

            // Intersects ray with all seven quadrants to obtain t-values at which the ray
            // exits a quadrant. We need to know these t-values since whenever we 
            // enter a new quadrant the origin and direction of the ray in contracted space
            // needs to be recomputed.
            Quadrants findTraversedQuadrants(float3 o, float3 d, float near) {
              Quadrants q;
              float listQuadrantTMax[5] = { INF, INF, INF, INF, INF }; // Rays traverse up to 5 quadrants
              q.array = listQuadrantTMax;
              int numQuadrantsTraversed = 0;
              float c1 = 1.0 - 1e-5;
              float c2 = 2.0 - 1e-4;
              float3 aabbMin;
              float3 aabbMax;
              float3 t0;
              float3 t1;
              float tMax;
            
              // core region
              aabbMin = float3(-1.0, -1.0, -1.0);
              aabbMax = float3(1.0, 1.0, 1.0);
              t0 = h(o, d, aabbMin);
              t1 = h(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
            
              // We discard intersections with quadrants that lie behind the camera
              // (tMax < near). When a quadrant is not traversed, getTMax returns -INF
              // and therefore this check also discards these values.
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 0, o[0]+t*d[0] >= 0
              aabbMin = float3( c1, -c1, -c1);
              aabbMax = float3( c2,  c1,  c1);
              t0 = h0(o, d, aabbMin);
              t1 = h0(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 1, o[1]+t*d[1] >= 0
              aabbMin = float3(-c1, c1, -c1);
              aabbMax = float3(c1, c2, c1);
              t0 = h1(o, d, aabbMin);
              t1 = h1(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 2, o[2]+t*d[2] >= 0
              aabbMin = float3(-c1, -c1, c1);
              aabbMax = float3(c1, c1, c2);
              t0 = h2(o, d, aabbMin);
              t1 = h2(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 0, o[0]+t*d[0] < 0
              aabbMin = float3(-c2, -c1, -c1);
              aabbMax = float3(-c1, c1, c1);
              t0 = h3(o, d, aabbMin);
              t1 = h3(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 1, o[1]+t*d[1] < 0
              aabbMin = float3(-c1, -c2, -c1);
              aabbMax = float3(c1, -c1, c1);
              t0 = h4(o, d, aabbMin);
              t1 = h4(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              // argmax(|o+t*d|) = 2, o[2]+t*d[2] < 0
              aabbMin = float3(-c1, -c1, -c2);
              aabbMax = float3(c1, c1, -c1);
              t0 = h5(o, d, aabbMin);
              t1 = h5(o, d, aabbMax);
              tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
              if (tMax >= near) {
                listQuadrantTMax[numQuadrantsTraversed] = tMax;
                numQuadrantsTraversed++;
              }
            
              sort5(listQuadrantTMax, numQuadrantsTraversed);
              return listQuadrantTMax;
            }
            
            struct QuadrantSetupResults {
              float3 oContracted; // ray origin in contracted space
              float3 dContracted; // ray direction in contracted space
              float2 quadrantTMinMaxContracted; // contraction-space t-values at which the ray
              // enters or exits the current quadrant
            };
            
            // This function is called whenever we enter a new quadrant. We compute
            // origin and direction of the ray in contracted space and compute for which
            // t-value (in contracted space) the ray enters/exits the quadrant
            // tP and tQ are two world-space t-values that must lie within th entered
            // quadrant, i.e. contract(o+tP*d) and  contract(o+tQ*d) must lie within the
            // entered quadrant.
            QuadrantSetupResults quadrantSetup(float3 o, float3 d, float tP, float tQ) {
              QuadrantSetupResults r;
            
              // Which quadrant did we enter?
              float3 xP = o + tP * d;
              float3 xAbs = abs(xP);
              float xMax = max3(xAbs);
            
              // Get the AABB of the quadrant the point x is in
              // Non-squash case, central quadrant:
              float3 aabbMin = float3(-1.0, -1.0, -1.0);
              float3 aabbMax = float3(1.0, 1.0, 1.0);
              if (xMax > 1.0) {
                // The point is inside in one of the outer quadrants (""squash zone"")
                if (xAbs.x >= xAbs.y && xAbs.x >= xAbs.z) {
                  aabbMin.x = xP.x > 0.0 ? 1.0 : -2.0; // argmax = 0
                  aabbMax.x = xP.x > 0.0 ? 2.0 : -1.0;
                } else if (xAbs.y >= xAbs.x && xAbs.y >= xAbs.z) {
                  aabbMin.y = xP.y > 0.0 ? 1.0 : -2.0; // argmax = 1
                  aabbMax.y = xP.y > 0.0 ? 2.0 : -1.0;
                } else {
                  aabbMin.z = xP.z > 0.0 ? 1.0 : -2.0; // argmax = 2
                  aabbMax.z = xP.z > 0.0 ? 2.0 : -1.0;
                }
              }
            
              // Estimate the direction of the ray in contracted space by computing the
              // vector difference with two different t-values that are guanteed to
              // correspond to points within the current quadrant
              r.oContracted = contract(xP);
              float3 zQ = contract(o + tQ * d);
              r.dContracted = normalize(zQ - r.oContracted);
            
              // When is the ray exiting the current quadrant? We need this value in
              // order to know when we enter a new quadrant or when to terminate ray marching.
              // Note that im findTraversedQuadrants word-space t-values are computed, while
              // we compute here contraction-space t-values. The world-space t-values are
              // needed to robustly obtain two points (tP and tQ) that are guranteed to lie
              // within a quadrant. With help of these values we can generate two points
              // in contracted space from which we can estimate the ray origin and direction
              // in contracted space. However, once we raymarch in contracted space we need
              // the contraction-space t-value to conveniently check whether we are still 
              // in the same quadrant. Alternatively, one could convert the contraction-
              // space point to a world-space point and estimate a world space t-value, but
              // this has been found to be numerically unstable.
              r.quadrantTMinMaxContracted =
                  rayAabbIntersection(aabbMin, aabbMax, r.oContracted, 1.0 / r.dContracted);
              return r;
            }
            
            
            struct OccupancyQueryResults {
              bool inEmptySpace;
              float tBlockMax;
            };
            
            OccupancyQueryResults queryOccupancyGrid(
                float3 z, float3 _MinPosition, float3 oContracted,
                float3 invDContracted, sampler3D occupancyGrid,
                float _VoxelSizeOccupancy, float3 _GridSizeOccupancy) {
              OccupancyQueryResults r;
              float3 posOccupancy;
              float3 blockMin;
              float3 blockMax;
              float occupancy;
              posOccupancy = (z - _MinPosition) / _VoxelSizeOccupancy;
              blockMin = floor(posOccupancy);
              blockMax = floor(posOccupancy) + 1.0;
              occupancy = tex3D(
                occupancyGrid,
                (blockMin + blockMax) * 0.5 / _GridSizeOccupancy
              ).r;
              blockMin = blockMin * _VoxelSizeOccupancy + _MinPosition;
              blockMax = blockMax * _VoxelSizeOccupancy + _MinPosition;
              r.inEmptySpace = occupancy == 0.0;
              r.tBlockMax = rayAabbIntersection(blockMin, blockMax, oContracted, invDContracted).y;
              return r;
            }
            
            
            #define QUERY_OCCUPANCY_GRID(tBlockMax_L, occupancyGrid, _VoxelSizeOccupancy, _GridSizeOccupancy)\
            if (tContracted > tBlockMax_L) {\
              occupancyQueryResults =\
                queryOccupancyGrid(z, _MinPosition, r.oContracted, invDContracted,\
                                    occupancyGrid, _VoxelSizeOccupancy, _GridSizeOccupancy);\
              tBlockMax_L = occupancyQueryResults.tBlockMax;\
              if (occupancyQueryResults.inEmptySpace) {\
                tContracted = max(tContracted, tBlockMax_L) + 0.5 * stepSizeContracted;\
                continue;\
              }\
            }
            
            fixed4 frag (v2f i) : SV_Target {
              // See the DisplayMode enum at the top of this file.
              // Runs the full model with view dependence.
              const int DISPLAY_NORMAL = 0;
              // Disables the view-dependence network.
              const int DISPLAY_DIFFUSE = 1;
              // Only shows the latent features.
              const int DISPLAY_FEATURES = 2;
              // Only shows the view dependent component.
              const int DISPLAY_VIEW_DEPENDENT = 3;
              // Only shows the coarse block grid.
              const int DISPLAY_COARSE_GRID = 4;
            
              // Set up the ray parameters in world space..
              float nearWorld = _ProjectionParams.y;
              half3 originWorld = i.origin.xyz;
              half3 directionWorld = normalize(i.direction);
            
            #ifdef USE_SPARSE_GRID
              int3 iGridSize = int3(round(_GridSize));
              int iBlockSize = int(round(_BlockSize));
              int3 iBlockGridBlocks = (iGridSize + iBlockSize - 1) / iBlockSize;
              int3 iBlockGridSize = iBlockGridBlocks * iBlockSize;
              float3 blockGridSize = float3(iBlockGridSize);
            #endif
            
              float listQuadrantTMax[5] = findTraversedQuadrants(originWorld,
                  directionWorld, nearWorld);
            
              float tP = nearWorld;
              float tQ = lerp(nearWorld, listQuadrantTMax[0], 0.5);
            
              QuadrantSetupResults r = quadrantSetup(originWorld, directionWorld, tP, tQ);
              float tContracted = 0.0;
              int quadrantIndex = 1;
            
              float tBlockMax_L0 = -INF;
              float tBlockMax_L1 = -INF;
              float tBlockMax_L2 = -INF;
              float tBlockMax_L3 = -INF;
              float tBlockMax_L4 = -INF;
            
              float visibility = 1.0;
              float3 accumulatedColor = float3(0.0, 0.0, 0.0);
              float4 accumulatedFeatures = float4(0.0, 0.0, 0.0, 0.0);
              int step = 0;
            
            #ifdef USE_TRIPLANE
              #define GRID_SIZE _PlaneSize
              #define VOXEL_SIZE _VoxelSizeTriplane
            #else
              #define GRID_SIZE _GridSize
              #define VOXEL_SIZE _VoxelSize
            #endif
            #ifdef USE_TRIPLANE
              float2 gridSize = GRID_SIZE;
            #else
              float3 gridSize = GRID_SIZE;
            #endif
              int maxStep = _StepMult * int(ceil(length(gridSize)));
              float origStepSizeContracted = VOXEL_SIZE / float(_StepMult);
            
              [loop]
              while (step < maxStep && visibility > 1.0 / 255.0) {
                step++;
            #ifdef LARGER_STEPS_WHEN_OCCLUDED
                float stepSizeContracted = origStepSizeContracted *
                    lerp(8.0, 1.0, min(1.0, visibility / 0.66));
            #else
                float stepSizeContracted = origStepSizeContracted;
            #endif
            
                // check if the ray is exiting the current quadrant
                if (tContracted > r.quadrantTMinMaxContracted.y) {
                  float3 z = r.oContracted + r.quadrantTMinMaxContracted.y * r.dContracted;
            
                  // Check if we hit an outer wall
                  // If so, we can terminate the ray as the ray won't enter another quadrant
                  if (max3(abs(z)) >= 2.0 - 1e-3) break;
            
                  // sStup ray in the new quadrant
                  // By using the precomputed t-values we can find two points that are guranteed
                  // to lie within the new quadrant.
                  tP = lerp(listQuadrantTMax[quadrantIndex - 1], listQuadrantTMax[quadrantIndex], 0.1);
                  tQ = lerp(listQuadrantTMax[quadrantIndex - 1], listQuadrantTMax[quadrantIndex], 0.9);
                  r = quadrantSetup(originWorld, directionWorld, tP, tQ);
                  tContracted = r.quadrantTMinMaxContracted.x;
                  quadrantIndex++;
            
                  // Reset all tMax values to force occupancy queries
                  tBlockMax_L0 = -INF;
                  tBlockMax_L1 = -INF;
                  tBlockMax_L2 = -INF;
                  tBlockMax_L3 = -INF;
                  tBlockMax_L4 = -INF;
                }
            
                // Position of current sample in contracted space
                float3 z = r.oContracted + tContracted * r.dContracted;
            
                // Hierarchical empty space skipping
                float3 invDContracted = 1.0 / r.dContracted;
                OccupancyQueryResults occupancyQueryResults;
                QUERY_OCCUPANCY_GRID(tBlockMax_L0, OccupancyGrid_L0, VoxelSizeOccupancy_L0,
                                     _GridSizeOccupancy_L0)
                QUERY_OCCUPANCY_GRID(tBlockMax_L1, OccupancyGrid_L1, VoxelSizeOccupancy_L1,
                                     _GridSizeOccupancy_L1)
                QUERY_OCCUPANCY_GRID(tBlockMax_L2, OccupancyGrid_L2, VoxelSizeOccupancy_L2,
                                     _GridSizeOccupancy_L2)             
                QUERY_OCCUPANCY_GRID(tBlockMax_L3, OccupancyGrid_L3, VoxelSizeOccupancy_L3,
                                     _GridSizeOccupancy_L3)
                QUERY_OCCUPANCY_GRID(tBlockMax_L4, OccupancyGrid_L4, VoxelSizeOccupancy_L4,
                                     _GridSizeOccupancy_L4)
             
                // We are in occupied space
                // compute grid positions for the sparse 3D grid and on the triplane planes
            #ifdef USE_SPARSE_GRID
                float3 posSparseGrid = (z - _MinPosition) / _VoxelSize - 0.5;
            #endif
            #ifdef USE_TRIPLANE
                float3 posTriplaneGrid = (z - _MinPosition) / _VoxelSizeTriplane;
            #endif
            
                // Calculate where the next sample would land in order to compute the
                // step size in world space (required for density-to-alpha conversion)
                // make sure not to shoot ouf the current quadrant
                float tContractedNext = min(tContracted + stepSizeContracted, r.quadrantTMinMaxContracted.y); 
                // Position of the next sample in contracted space
                float3 zNext = r.oContracted + tContractedNext * r.dContracted; 
                float stepSizeWorld = distance(inverseContract(zNext), inverseContract(z));
            
            #ifdef USE_SPARSE_GRID
                float3 atlasBlockMin =
                    floor(posSparseGrid / _BlockSize) * _BlockSize;
                float3 atlasBlockMax = atlasBlockMin + _BlockSize;
                float3 atlasBlockIndex =
                    255.0 * tex3D(_SparseGridIndex, (atlasBlockMin + atlasBlockMax) /
                                                  (2.0 * blockGridSize)).xyz;
                if (atlasBlockIndex.x <= 254.0) {
                float3 posAtlas = clamp(posSparseGrid - atlasBlockMin, 0.0, _BlockSize);
            
                posAtlas += atlasBlockIndex * (_BlockSize + 1.0);
                posAtlas += 0.5;
                float3 atlasUvw = posAtlas / _AtlasSize;
            
                if (_DisplayMode == DISPLAY_COARSE_GRID) {
                  // Half-pixel apron
                  accumulatedColor = atlasBlockIndex * (_BlockSize + 1.0) / _AtlasSize;
                  accumulatedFeatures.rgb = atlasBlockIndex * (_BlockSize + 1.0) / _AtlasSize;
                  accumulatedFeatures.a = 1.0;
                  visibility = 0.0;
                  continue;
                }
            #endif
            
                // Value ranges used for quantization
                float quantizeMinFeatures = -7.0;
                float quantizeMaxFeatures = 7.0;
                float quantizeMinDensity = -14.0;
                float quantizeMaxDensity = 14.0;
            
                // First fetch all densities
            #ifdef USE_SPARSE_GRID
                float density = tex3D(_SparseGridDensity, atlasUvw).x;
                density = denormalize(density, quantizeMinDensity, quantizeMaxDensity);
            #else
                float density = 0.0;
            #endif
            #ifdef USE_TRIPLANE
                float3[3] planeUv;
                planeUv[0] = float3(posTriplaneGrid.yz / _PlaneSize, 0.0);
                planeUv[1] = float3(posTriplaneGrid.xz / _PlaneSize, 1.0);
                planeUv[2] = float3(posTriplaneGrid.xy / _PlaneSize, 2.0);
            
                float densityTemp;
                densityTemp = texture(_PlaneDensity, planeUv[0]).x;
                densityTemp = denormalize(densityTemp, quantizeMinDensity,
                                           quantizeMaxDensity);
                density += densityTemp;
            
                densityTemp = texture(_PlaneDensity, planeUv[1]).x;
                densityTemp = denormalize(densityTemp, quantizeMinDensity,
                                           quantizeMaxDensity);
                density += densityTemp;
            
                densityTemp = texture(_PlaneDensity, planeUv[2]).x;
                densityTemp = denormalize(densityTemp, quantizeMinDensity,
                                           quantizeMaxDensity);
                density += densityTemp;
            #endif
            
                // Activate density and convert density to alpha.
                density = densityActivation(density);
                float alpha = densityToAlpha(density, stepSizeWorld);
            
                // Only fetch RGBFFFF (7 bytes) if alpha is non-negligible to save bandwidth
                if (alpha > 0.5 / 255.0) {
            #ifdef USE_SPARSE_GRID
                  float3 rgb = tex3D(_SparseGridRgb, atlasUvw).rgb;
                  rgb = denormalize(rgb, quantizeMinFeatures, quantizeMaxFeatures);
            #else
                  float3 rgb = float3(0.0, 0.0, 0.0);
            #endif
            #ifdef USE_TRIPLANE
                  float3 rgbTemp;
                  rgbTemp = texture(_PlaneRgb, planeUv[0]).rgb;
                  rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                                          quantizeMaxFeatures);
                  rgb += rgbTemp;
            
                  rgbTemp = texture(_PlaneRgb, planeUv[1]).rgb;
                  rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                                          quantizeMaxFeatures);
                  rgb += rgbTemp;
            
                  rgbTemp = texture(_PlaneRgb, planeUv[2]).rgb;
                  rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                                          quantizeMaxFeatures);
                  rgb += rgbTemp;
            #endif
            
                  rgb = sigmoid(rgb); // Apply activation function
            
                  if (_DisplayMode != DISPLAY_DIFFUSE) {
                    float4 features = float4(0.0, 0.0, 0.0, 0.0);
            #ifdef USE_SPARSE_GRID
                    features = tex3D(_SparseGridFeatures, atlasUvw);
                    features = denormalize(features, quantizeMinFeatures,
                                           quantizeMaxFeatures);
            #endif
            #ifdef USE_TRIPLANE
                    float4 featuresTemp;
                    featuresTemp = texture(_PlaneFeatures, planeUv[0]);
                    features +=
                        denormalize(featuresTemp,
                                    quantizeMinFeatures, quantizeMaxFeatures);
            
                    featuresTemp = texture(_PlaneFeatures, planeUv[1]);
                    features +=
                        denormalize(featuresTemp,
                                    quantizeMinFeatures, quantizeMaxFeatures);
            
                    featuresTemp = texture(_PlaneFeatures, planeUv[2]);
                    features +=
                        denormalize(featuresTemp,
                                    quantizeMinFeatures, quantizeMaxFeatures);
            #endif
            
                    features = sigmoid(features);
                    accumulatedFeatures += visibility * alpha * features;
                  }
                  accumulatedColor += visibility * alpha * rgb;
                  visibility *= 1.0 - alpha;
                }
            #ifdef USE_SPARSE_GRID
                } // end of check: atlasBlockIndex.x <= 254.0
            #endif
                tContracted += stepSizeContracted;
              }
            
              if (_DisplayMode == DISPLAY_VIEW_DEPENDENT) {
                accumulatedColor = float3(0.0, 0.0, 0.0) * visibility;
              } else if (_DisplayMode == DISPLAY_FEATURES) {
                accumulatedColor = accumulatedFeatures.rgb;
              }
            
              // Composite on white background
              accumulatedColor = float3(1.0, 1.0, 1.0) * visibility + accumulatedColor;
            
              // Run view-dependency network
              if ((_DisplayMode == DISPLAY_NORMAL ||
                   _DisplayMode == DISPLAY_VIEW_DEPENDENT)) {
                accumulatedColor += evaluateNetwork(accumulatedColor, accumulatedFeatures,
                                         mul(_WorldspaceROpengl, normalize(vDirection)));
              }
              return fixed4(accumulatedColor, 1.0);
            }
";
    private const string VERTEX = @"v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float4 screenPos = ComputeScreenPos(o.vertex);
                o.origin = screenPos.xyz / screenPos.w;
                o.direction = -WorldSpaceViewDir(v.vertex);

                return o;
            }
";
}
