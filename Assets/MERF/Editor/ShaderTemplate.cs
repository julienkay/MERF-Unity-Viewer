public static class ShaderTemplate {

    public const string Template = @"Shader ""MERF/RayMarchShader_OBJECT_NAME"" {
    Properties {
        mapAlpha(""Alpha Map"", 3D) = """" {}
        mapColor(""Color Map"", 3D) = """" {}
        mapFeatures(""Feature Map"", 3D) = """" {}
        mapIndex(""Index Map"", 3D) = """" {}

        weightsZero (""Weights Zero"", 2D) = ""white"" {}
        weightsOne (""Weights One"", 2D) = ""white"" {}
        weightsTwo (""Weights Two"", 2D) = ""white"" {}

	    minPosition (""Min Position"", Vector) = (0, 0, 0, 0)
        gridSize (""Grid Size"", Vector) = (0, 0, 0, 0)
        atlasSize (""Atlas Size"", Vector) = (0, 0, 0, 0)
	    voxelSize (""Voxel Size"", Float) = 0.0
	    blockSize (""Block Size"", Float) = 0.0

        maxStep (""Max Step"", Float) = 0.0
    }
    SubShader {
        Cull Front
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            #pragma shader_feature_local USE_TRIPLANE
            #pragma shader_feature_local USE_SPARSE_GRID
            #pragma shader_feature_local LARGER_STEPS_WHEN_OCCLUDED

            #pragma vertex vert
            #pragma fragment frag

            #include ""UnityCG.cginc""

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

    private const string VIEWDEPENDENCY = @"float indexToPosEnc(vec3 dir, int index) {
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

float indexToInputValue(vec3 color, vec4 features, vec3 viewdir, int j) {
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

vec4 relu(vec4 x) {
  return max(x, 0.0);
}

vec3 evaluateNetwork(
    vec3 color, vec4 features, vec3 viewdir) {

  vec4 intermediate_one[NUM_CHANNELS_ONE/4] = vec4[](
    BIAS_LIST_0
  );

  vec4 inp;
  mat4 w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 0),
      indexToInputValue(color, features, viewdir, 1),
      indexToInputValue(color, features, viewdir, 2),
      indexToInputValue(color, features, viewdir, 3));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 0), 0),
        texelFetch(weightsZero, ivec2(0, 1), 0),
        texelFetch(weightsZero, ivec2(0, 2), 0),
        texelFetch(weightsZero, ivec2(0, 3), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 4), 0),
        texelFetch(weightsZero, ivec2(0, 5), 0),
        texelFetch(weightsZero, ivec2(0, 6), 0),
        texelFetch(weightsZero, ivec2(0, 7), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 8), 0),
        texelFetch(weightsZero, ivec2(0, 9), 0),
        texelFetch(weightsZero, ivec2(0, 10), 0),
        texelFetch(weightsZero, ivec2(0, 11), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 12), 0),
        texelFetch(weightsZero, ivec2(0, 13), 0),
        texelFetch(weightsZero, ivec2(0, 14), 0),
        texelFetch(weightsZero, ivec2(0, 15), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 4),
      indexToInputValue(color, features, viewdir, 5),
      indexToInputValue(color, features, viewdir, 6),
      indexToInputValue(color, features, viewdir, 7));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 16), 0),
        texelFetch(weightsZero, ivec2(0, 17), 0),
        texelFetch(weightsZero, ivec2(0, 18), 0),
        texelFetch(weightsZero, ivec2(0, 19), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 20), 0),
        texelFetch(weightsZero, ivec2(0, 21), 0),
        texelFetch(weightsZero, ivec2(0, 22), 0),
        texelFetch(weightsZero, ivec2(0, 23), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 24), 0),
        texelFetch(weightsZero, ivec2(0, 25), 0),
        texelFetch(weightsZero, ivec2(0, 26), 0),
        texelFetch(weightsZero, ivec2(0, 27), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 28), 0),
        texelFetch(weightsZero, ivec2(0, 29), 0),
        texelFetch(weightsZero, ivec2(0, 30), 0),
        texelFetch(weightsZero, ivec2(0, 31), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 8),
      indexToInputValue(color, features, viewdir, 9),
      indexToInputValue(color, features, viewdir, 10),
      indexToInputValue(color, features, viewdir, 11));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 32), 0),
        texelFetch(weightsZero, ivec2(0, 33), 0),
        texelFetch(weightsZero, ivec2(0, 34), 0),
        texelFetch(weightsZero, ivec2(0, 35), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 36), 0),
        texelFetch(weightsZero, ivec2(0, 37), 0),
        texelFetch(weightsZero, ivec2(0, 38), 0),
        texelFetch(weightsZero, ivec2(0, 39), 0)
      );
  intermediate_one[1] += inp * w;
  

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 40), 0),
        texelFetch(weightsZero, ivec2(0, 41), 0),
        texelFetch(weightsZero, ivec2(0, 42), 0),
        texelFetch(weightsZero, ivec2(0, 43), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 44), 0),
        texelFetch(weightsZero, ivec2(0, 45), 0),
        texelFetch(weightsZero, ivec2(0, 46), 0),
        texelFetch(weightsZero, ivec2(0, 47), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 12),
      indexToInputValue(color, features, viewdir, 13),
      indexToInputValue(color, features, viewdir, 14),
      indexToInputValue(color, features, viewdir, 15));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 48), 0),
        texelFetch(weightsZero, ivec2(0, 49), 0),
        texelFetch(weightsZero, ivec2(0, 50), 0),
        texelFetch(weightsZero, ivec2(0, 51), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 52), 0),
        texelFetch(weightsZero, ivec2(0, 53), 0),
        texelFetch(weightsZero, ivec2(0, 54), 0),
        texelFetch(weightsZero, ivec2(0, 55), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 56), 0),
        texelFetch(weightsZero, ivec2(0, 57), 0),
        texelFetch(weightsZero, ivec2(0, 58), 0),
        texelFetch(weightsZero, ivec2(0, 59), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 60), 0),
        texelFetch(weightsZero, ivec2(0, 61), 0),
        texelFetch(weightsZero, ivec2(0, 62), 0),
        texelFetch(weightsZero, ivec2(0, 63), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 16),
      indexToInputValue(color, features, viewdir, 17),
      indexToInputValue(color, features, viewdir, 18),
      indexToInputValue(color, features, viewdir, 19));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 64), 0),
        texelFetch(weightsZero, ivec2(0, 65), 0),
        texelFetch(weightsZero, ivec2(0, 66), 0),
        texelFetch(weightsZero, ivec2(0, 67), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 68), 0),
        texelFetch(weightsZero, ivec2(0, 69), 0),
        texelFetch(weightsZero, ivec2(0, 70), 0),
        texelFetch(weightsZero, ivec2(0, 71), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 72), 0),
        texelFetch(weightsZero, ivec2(0, 73), 0),
        texelFetch(weightsZero, ivec2(0, 74), 0),
        texelFetch(weightsZero, ivec2(0, 75), 0)
      );
      intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 76), 0),
        texelFetch(weightsZero, ivec2(0, 77), 0),
        texelFetch(weightsZero, ivec2(0, 78), 0),
        texelFetch(weightsZero, ivec2(0, 79), 0)
      );
      intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 20),
      indexToInputValue(color, features, viewdir, 21),
      indexToInputValue(color, features, viewdir, 22),
      indexToInputValue(color, features, viewdir, 23));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 80), 0),
        texelFetch(weightsZero, ivec2(0, 81), 0),
        texelFetch(weightsZero, ivec2(0, 82), 0),
        texelFetch(weightsZero, ivec2(0, 83), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 84), 0),
        texelFetch(weightsZero, ivec2(0, 85), 0),
        texelFetch(weightsZero, ivec2(0, 86), 0),
        texelFetch(weightsZero, ivec2(0, 87), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 88), 0),
        texelFetch(weightsZero, ivec2(0, 89), 0),
        texelFetch(weightsZero, ivec2(0, 90), 0),
        texelFetch(weightsZero, ivec2(0, 91), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 92), 0),
        texelFetch(weightsZero, ivec2(0, 93), 0),
        texelFetch(weightsZero, ivec2(0, 94), 0),
        texelFetch(weightsZero, ivec2(0, 95), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 24),
      indexToInputValue(color, features, viewdir, 25),
      indexToInputValue(color, features, viewdir, 26),
      indexToInputValue(color, features, viewdir, 27));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 96), 0),
        texelFetch(weightsZero, ivec2(0, 97), 0),
        texelFetch(weightsZero, ivec2(0, 98), 0),
        texelFetch(weightsZero, ivec2(0, 99), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 100), 0),
        texelFetch(weightsZero, ivec2(0, 101), 0),
        texelFetch(weightsZero, ivec2(0, 102), 0),
        texelFetch(weightsZero, ivec2(0, 103), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 104), 0),
        texelFetch(weightsZero, ivec2(0, 105), 0),
        texelFetch(weightsZero, ivec2(0, 106), 0),
        texelFetch(weightsZero, ivec2(0, 107), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 108), 0),
        texelFetch(weightsZero, ivec2(0, 109), 0),
        texelFetch(weightsZero, ivec2(0, 110), 0),
        texelFetch(weightsZero, ivec2(0, 111), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 28),
      indexToInputValue(color, features, viewdir, 29),
      indexToInputValue(color, features, viewdir, 30),
      indexToInputValue(color, features, viewdir, 31));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 112), 0),
        texelFetch(weightsZero, ivec2(0, 113), 0),
        texelFetch(weightsZero, ivec2(0, 114), 0),
        texelFetch(weightsZero, ivec2(0, 115), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 116), 0),
        texelFetch(weightsZero, ivec2(0, 117), 0),
        texelFetch(weightsZero, ivec2(0, 118), 0),
        texelFetch(weightsZero, ivec2(0, 119), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 120), 0),
        texelFetch(weightsZero, ivec2(0, 121), 0),
        texelFetch(weightsZero, ivec2(0, 122), 0),
        texelFetch(weightsZero, ivec2(0, 123), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 124), 0),
        texelFetch(weightsZero, ivec2(0, 125), 0),
        texelFetch(weightsZero, ivec2(0, 126), 0),
        texelFetch(weightsZero, ivec2(0, 127), 0)
      );
  intermediate_one[3] += inp * w;

  inp = vec4(
      indexToInputValue(color, features, viewdir, 32),
      indexToInputValue(color, features, viewdir, 33),
      indexToInputValue(color, features, viewdir, 34),
      indexToInputValue(color, features, viewdir, 35));

  w = mat4(
        texelFetch(weightsZero, ivec2(0, 128), 0),
        texelFetch(weightsZero, ivec2(0, 129), 0),
        texelFetch(weightsZero, ivec2(0, 130), 0),
        texelFetch(weightsZero, ivec2(0, 131), 0)
      );
  intermediate_one[0] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 132), 0),
        texelFetch(weightsZero, ivec2(0, 133), 0),
        texelFetch(weightsZero, ivec2(0, 134), 0),
        texelFetch(weightsZero, ivec2(0, 135), 0)
      );
  intermediate_one[1] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 136), 0),
        texelFetch(weightsZero, ivec2(0, 137), 0),
        texelFetch(weightsZero, ivec2(0, 138), 0),
        texelFetch(weightsZero, ivec2(0, 139), 0)
      );
  intermediate_one[2] += inp * w;


  w = mat4(
        texelFetch(weightsZero, ivec2(0, 140), 0),
        texelFetch(weightsZero, ivec2(0, 141), 0),
        texelFetch(weightsZero, ivec2(0, 142), 0),
        texelFetch(weightsZero, ivec2(0, 143), 0)
      );
  intermediate_one[3] += inp * w;


  vec4 intermediate_two[NUM_CHANNELS_TWO/4] = vec4[](
    BIAS_LIST_1
  );
  for (int j = 0; j < NUM_CHANNELS_ONE/4; ++j) {
    inp = relu(intermediate_one[j]);
    for (int i = 0; i < NUM_CHANNELS_TWO; i += 4) {
      w = mat4(
        texelFetch(weightsOne, ivec2(0, j * NUM_CHANNELS_TWO + i), 0),
        texelFetch(weightsOne, ivec2(0, j * NUM_CHANNELS_TWO + (i+1)), 0),
        texelFetch(weightsOne, ivec2(0, j * NUM_CHANNELS_TWO + (i+2)), 0),
        texelFetch(weightsOne, ivec2(0, j * NUM_CHANNELS_TWO + (i+3)), 0)
      );
      intermediate_two[i/4] += inp * w;
    }
  }

  vec4 result = BIAS_LIST_2;
  for (int j = 0; j < NUM_CHANNELS_TWO/4; ++j) {
    inp = relu(intermediate_two[j]);
    w = mat4(
      texelFetch(weightsTwo, ivec2(0, j * NUM_CHANNELS_THREE), 0),
      texelFetch(weightsTwo, ivec2(0, j * NUM_CHANNELS_THREE + 1), 0),
      texelFetch(weightsTwo, ivec2(0, j * NUM_CHANNELS_THREE + 2), 0),
      texelFetch(weightsTwo, ivec2(0, j * NUM_CHANNELS_THREE + 3), 0)
    );
    result.xyz += (inp * w).xyz;
  }
  return 1.0 / (1.0 + exp(-result.xyz)); // Sigmoid
}
";
    
    private const string FRAGMENT = @"vec2 rayAabbIntersection(vec3 aabbMin, vec3 aabbMax, vec3 origin,
                         vec3 invDirection) {
  vec3 t1 = (aabbMin - origin) * invDirection;
  vec3 t2 = (aabbMax - origin) * invDirection;
  vec3 tMin = min(t1, t2);
  vec3 tMax = max(t1, t2);
  return vec2(max(tMin.x, max(tMin.y, tMin.z)),
              min(tMax.x, min(tMax.y, tMax.z)));
}

#define SIGMOID(DTYPE) DTYPE sigmoid(DTYPE x) { return 1.0 / (1.0 + exp(-x)); }
SIGMOID(vec3)
SIGMOID(vec4)

#define DENORMALIZE(DTYPE)\
DTYPE denormalize(DTYPE x, float min, float max) {\
    return min + x * (max - min);\
}
DENORMALIZE(float)
DENORMALIZE(vec3)
DENORMALIZE(vec4)

float densityActivation(float x) { return exp(x - 1.0f); }

float densityToAlpha(float x, float stepSize) {
  return 1.0 - exp(-x * stepSize);
}

// Component-wise maximum
float max3 (vec3 v) {
  return max (max (v.x, v.y), v.z);
}

// Projective contraction
vec3 contract(vec3 x) {
  vec3 xAbs = abs(x);
  float xMax = max3(xAbs);
  if (xMax <= 1.0) {
    return x;
  }
  float scale = 1.0 / xMax;
  vec3 z = scale * x;
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
vec3 inverseContract(vec3 z) {
  vec3 zAbs = abs(z);
  float zMax = max3(zAbs);
  if (zMax <= 1.0) {
    return z;
  }
  float eps = 1e-6;
  float invZMax = max(eps, 2.0 - zMax);
  float scale = 1.0 / invZMax;
  vec3 x = scale * z;
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
void sort5(inout float[5] array, int arrayLength) {
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
float getTMax(vec3 o, vec3 d, vec3 t0, vec3 t1,
  vec3 aabbMin, vec3 aabbMax) {
  float eps = 1e-3;
  vec3 q;

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
vec3 h(vec3 o, vec3 d, vec3 p) {
  return (p - o) / d;
}

vec3 h0(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = (1.0 / (2.0 - p.x) - o.x) / d.x;
  t.y = (o.y - p.y * o.x) / (p.y * d.x - d.y);
  t.z = (o.z - p.z * o.x) / (p.z * d.x - d.z);
  return t;
}

vec3 h1(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = (o.x - p.x * o.y) / (p.x * d.y - d.x);
  t.y = (1.0 / (2.0 - p.y) - o.y) / d.y;
  t.z = (o.z - p.z * o.y) / (p.z * d.y - d.z);
  return t;
}

vec3 h2(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = (o.x - p.x * o.z) / (p.x * d.z - d.x);
  t.y = (o.y - p.y * o.z) / (p.y * d.z - d.y);
  t.z = (1.0 / (2.0 - p.z) - o.z) / d.z;
  return t;
}

vec3 h3(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = (1.0 / (-p.x - 2.0) - o.x) / d.x;
  t.y = -(o.x*p.y + o.y) / (d.x*p.y + d.y);
  t.z = -(o.x*p.z + o.z) / (d.x*p.z + d.z);
  return t;
}

vec3 h4(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = -(o.y*p.x + o.x) / (d.y*p.x + d.x);
  t.y = (1.0 / (-p.y - 2.0) - o.y) / d.y;
  t.z = -(o.y*p.z + o.z) / (d.y*p.z + d.z);
  return t;
}

vec3 h5(vec3 o, vec3 d, vec3 p) {
  vec3 t;
  t.x = -(o.z*p.x + o.x) / (d.z*p.x + d.x);
  t.y = -(o.z*p.y + o.y) / (d.z*p.y + d.y);
  t.z = (1.0 / (-p.z - 2.0) - o.z) / d.z;
  return t;
}

// Intersects ray with all seven quadrants to obtain t-values at which the ray
// exits a quadrant. We need to know these t-values since whenever we 
// enter a new quadrant the origin and direction of the ray in contracted space
// needs to be recomputed.
float[5] findTraversedQuadrants(vec3 o, vec3 d, float near) {
  float[5] listQuadrantTMax = float[](INF, INF, INF, INF, INF); // Rays traverse up to 5 quadrants
  int numQuadrantsTraversed = 0;
  float c1 = 1.0 - 1e-5;
  float c2 = 2.0 - 1e-4;
  vec3 aabbMin;
  vec3 aabbMax;
  vec3 t0;
  vec3 t1;
  float tMax;

  // core region
  aabbMin = vec3(-1.0, -1.0, -1.0);
  aabbMax = vec3(1.0, 1.0, 1.0);
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
  aabbMin = vec3( c1, -c1, -c1);
  aabbMax = vec3( c2,  c1,  c1);
  t0 = h0(o, d, aabbMin);
  t1 = h0(o, d, aabbMax);
  tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
  if (tMax >= near) {
    listQuadrantTMax[numQuadrantsTraversed] = tMax;
    numQuadrantsTraversed++;
  }

  // argmax(|o+t*d|) = 1, o[1]+t*d[1] >= 0
  aabbMin = vec3(-c1, c1, -c1);
  aabbMax = vec3(c1, c2, c1);
  t0 = h1(o, d, aabbMin);
  t1 = h1(o, d, aabbMax);
  tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
  if (tMax >= near) {
    listQuadrantTMax[numQuadrantsTraversed] = tMax;
    numQuadrantsTraversed++;
  }

  // argmax(|o+t*d|) = 2, o[2]+t*d[2] >= 0
  aabbMin = vec3(-c1, -c1, c1);
  aabbMax = vec3(c1, c1, c2);
  t0 = h2(o, d, aabbMin);
  t1 = h2(o, d, aabbMax);
  tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
  if (tMax >= near) {
    listQuadrantTMax[numQuadrantsTraversed] = tMax;
    numQuadrantsTraversed++;
  }

  // argmax(|o+t*d|) = 0, o[0]+t*d[0] < 0
  aabbMin = vec3(-c2, -c1, -c1);
  aabbMax = vec3(-c1, c1, c1);
  t0 = h3(o, d, aabbMin);
  t1 = h3(o, d, aabbMax);
  tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
  if (tMax >= near) {
    listQuadrantTMax[numQuadrantsTraversed] = tMax;
    numQuadrantsTraversed++;
  }

  // argmax(|o+t*d|) = 1, o[1]+t*d[1] < 0
  aabbMin = vec3(-c1, -c2, -c1);
  aabbMax = vec3(c1, -c1, c1);
  t0 = h4(o, d, aabbMin);
  t1 = h4(o, d, aabbMax);
  tMax = getTMax(o, d, t0, t1, aabbMin, aabbMax);
  if (tMax >= near) {
    listQuadrantTMax[numQuadrantsTraversed] = tMax;
    numQuadrantsTraversed++;
  }

  // argmax(|o+t*d|) = 2, o[2]+t*d[2] < 0
  aabbMin = vec3(-c1, -c1, -c2);
  aabbMax = vec3(c1, c1, -c1);
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
  vec3 oContracted; // ray origin in contracted space
  vec3 dContracted; // ray direction in contracted space
  vec2 quadrantTMinMaxContracted; // contraction-space t-values at which the ray
  // enters or exits the current quadrant
};

// This function is called whenever we enter a new quadrant. We compute
// origin and direction of the ray in contracted space and compute for which
// t-value (in contracted space) the ray enters/exits the quadrant
// tP and tQ are two world-space t-values that must lie within th entered
// quadrant, i.e. contract(o+tP*d) and  contract(o+tQ*d) must lie within the
// entered quadrant.
QuadrantSetupResults quadrantSetup(vec3 o, vec3 d, float tP, float tQ) {
  QuadrantSetupResults r;

  // Which quadrant did we enter?
  vec3 xP = o + tP * d;
  vec3 xAbs = abs(xP);
  float xMax = max3(xAbs);

  // Get the AABB of the quadrant the point x is in
  // Non-squash case, central quadrant:
  vec3 aabbMin = vec3(-1.0, -1.0, -1.0);
  vec3 aabbMax = vec3(1.0, 1.0, 1.0);
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
  vec3 zQ = contract(o + tQ * d);
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
    vec3 z, vec3 minPosition, vec3 oContracted,
    vec3 invDContracted, highp sampler3D occupancyGrid,
    float voxelSizeOccupancy, vec3 gridSizeOccupancy) {
  OccupancyQueryResults r;
  vec3 posOccupancy;
  vec3 blockMin;
  vec3 blockMax;
  float occupancy;
  posOccupancy = (z - minPosition) / voxelSizeOccupancy;
  blockMin = floor(posOccupancy);
  blockMax = floor(posOccupancy) + 1.0;
  occupancy = texture(
    occupancyGrid,
    (blockMin + blockMax) * 0.5 / gridSizeOccupancy
  ).r;
  blockMin = blockMin * voxelSizeOccupancy + minPosition;
  blockMax = blockMax * voxelSizeOccupancy + minPosition;
  r.inEmptySpace = occupancy == 0.0;
  r.tBlockMax = rayAabbIntersection(blockMin, blockMax, oContracted, invDContracted).y;
  return r;
}


#define QUERY_OCCUPANCY_GRID(tBlockMax_L, occupancyGrid, voxelSizeOccupancy, gridSizeOccupancy)\
if (tContracted > tBlockMax_L) {\
  occupancyQueryResults =\
    queryOccupancyGrid(z, minPosition, r.oContracted, invDContracted,\
                        occupancyGrid, voxelSizeOccupancy, gridSizeOccupancy);\
  tBlockMax_L = occupancyQueryResults.tBlockMax;\
  if (occupancyQueryResults.inEmptySpace) {\
    tContracted = max(tContracted, tBlockMax_L) + 0.5 * stepSizeContracted;\
    continue;\
  }\
}

void main() {
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
  float nearWorld = nearPlane;
  vec3 originWorld = vOrigin;
  vec3 directionWorld = normalize(vDirection);

#ifdef USE_SPARSE_GRID
  ivec3 iGridSize = ivec3(round(gridSize));
  int iBlockSize = int(round(blockSize));
  ivec3 iBlockGridBlocks = (iGridSize + iBlockSize - 1) / iBlockSize;
  ivec3 iBlockGridSize = iBlockGridBlocks * iBlockSize;
  vec3 blockGridSize = vec3(iBlockGridSize);
#endif

  float[5] listQuadrantTMax = findTraversedQuadrants(originWorld,
      directionWorld, nearWorld);

  float tP = nearWorld;
  float tQ = mix(nearWorld, listQuadrantTMax[0], 0.5);

  QuadrantSetupResults r = quadrantSetup(originWorld, directionWorld, tP, tQ);
  float tContracted = 0.0;
  int quadrantIndex = 1;

  float tBlockMax_L0 = -INF;
  float tBlockMax_L1 = -INF;
  float tBlockMax_L2 = -INF;
  float tBlockMax_L3 = -INF;
  float tBlockMax_L4 = -INF;

  float visibility = 1.0;
  vec3 accumulatedColor = vec3(0.0, 0.0, 0.0);
  vec4 accumulatedFeatures = vec4(0.0, 0.0, 0.0, 0.0);
  int step = 0;

#ifdef USE_TRIPLANE
  #define GRID_SIZE planeSize
  #define VOXEL_SIZE voxelSizeTriplane
#else
  #define GRID_SIZE gridSize
  #define VOXEL_SIZE voxelSize
#endif
  int maxStep = stepMult * int(ceil(length(GRID_SIZE)));
  float origStepSizeContracted = VOXEL_SIZE / float(stepMult);

  while (step < maxStep && visibility > 1.0 / 255.0) {
    step++;
#ifdef LARGER_STEPS_WHEN_OCCLUDED
    float stepSizeContracted = origStepSizeContracted *
        mix(8.0, 1.0, min(1.0, visibility / 0.66));
#else
    float stepSizeContracted = origStepSizeContracted;
#endif

    // check if the ray is exiting the current quadrant
    if (tContracted > r.quadrantTMinMaxContracted.y) {
      vec3 z = r.oContracted + r.quadrantTMinMaxContracted.y * r.dContracted;

      // Check if we hit an outer wall
      // If so, we can terminate the ray as the ray won't enter another quadrant
      if (max3(abs(z)) >= 2.0 - 1e-3) break;

      // sStup ray in the new quadrant
      // By using the precomputed t-values we can find two points that are guranteed
      // to lie within the new quadrant.
      tP = mix(listQuadrantTMax[quadrantIndex - 1], listQuadrantTMax[quadrantIndex], 0.1);
      tQ = mix(listQuadrantTMax[quadrantIndex - 1], listQuadrantTMax[quadrantIndex], 0.9);
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
    vec3 z = r.oContracted + tContracted * r.dContracted;

    // Hierarchical empty space skipping
    vec3 invDContracted = 1.0 / r.dContracted;
    OccupancyQueryResults occupancyQueryResults;
    QUERY_OCCUPANCY_GRID(tBlockMax_L0, occupancyGrid_L0, voxelSizeOccupancy_L0,
                         gridSizeOccupancy_L0)
    QUERY_OCCUPANCY_GRID(tBlockMax_L1, occupancyGrid_L1, voxelSizeOccupancy_L1,
                         gridSizeOccupancy_L1)
    QUERY_OCCUPANCY_GRID(tBlockMax_L2, occupancyGrid_L2, voxelSizeOccupancy_L2,
                         gridSizeOccupancy_L2)             
    QUERY_OCCUPANCY_GRID(tBlockMax_L3, occupancyGrid_L3, voxelSizeOccupancy_L3,
                         gridSizeOccupancy_L3)
    QUERY_OCCUPANCY_GRID(tBlockMax_L4, occupancyGrid_L4, voxelSizeOccupancy_L4,
                         gridSizeOccupancy_L4)
 
    // We are in occupied space
    // compute grid positions for the sparse 3D grid and on the triplane planes
#ifdef USE_SPARSE_GRID
    vec3 posSparseGrid = (z - minPosition) / voxelSize - 0.5;
#endif
#ifdef USE_TRIPLANE
    vec3 posTriplaneGrid = (z - minPosition) / voxelSizeTriplane;
#endif

    // Calculate where the next sample would land in order to compute the
    // step size in world space (required for density-to-alpha conversion)
    // make sure not to shoot ouf the current quadrant
    float tContractedNext = min(tContracted + stepSizeContracted, r.quadrantTMinMaxContracted.y); 
    // Position of the next sample in contracted space
    vec3 zNext = r.oContracted + tContractedNext * r.dContracted; 
    float stepSizeWorld = distance(inverseContract(zNext), inverseContract(z));

#ifdef USE_SPARSE_GRID
    vec3 atlasBlockMin =
        floor(posSparseGrid / blockSize) * blockSize;
    vec3 atlasBlockMax = atlasBlockMin + blockSize;
    vec3 atlasBlockIndex =
        255.0 * texture(sparseGridIndex, (atlasBlockMin + atlasBlockMax) /
                                      (2.0 * blockGridSize)).xyz;
    if (atlasBlockIndex.x <= 254.0) {
    vec3 posAtlas = clamp(posSparseGrid - atlasBlockMin, 0.0, blockSize);

    posAtlas += atlasBlockIndex * (blockSize + 1.0);
    posAtlas += 0.5;
    vec3 atlasUvw = posAtlas / atlasSize;

    if (displayMode == DISPLAY_COARSE_GRID) {
      // Half-pixel apron
      accumulatedColor = atlasBlockIndex * (blockSize + 1.0) / atlasSize;
      accumulatedFeatures.rgb = atlasBlockIndex * (blockSize + 1.0) / atlasSize;
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
    float density = texture(sparseGridDensity, atlasUvw).x;
    density = denormalize(density, quantizeMinDensity, quantizeMaxDensity);
#else
    float density = 0.0;
#endif
#ifdef USE_TRIPLANE
    vec3[3] planeUv;
    planeUv[0] = vec3(posTriplaneGrid.yz / planeSize, 0.0);
    planeUv[1] = vec3(posTriplaneGrid.xz / planeSize, 1.0);
    planeUv[2] = vec3(posTriplaneGrid.xy / planeSize, 2.0);

    float densityTemp;
    densityTemp = texture(planeDensity, planeUv[0]).x;
    densityTemp = denormalize(densityTemp, quantizeMinDensity,
                               quantizeMaxDensity);
    density += densityTemp;

    densityTemp = texture(planeDensity, planeUv[1]).x;
    densityTemp = denormalize(densityTemp, quantizeMinDensity,
                               quantizeMaxDensity);
    density += densityTemp;

    densityTemp = texture(planeDensity, planeUv[2]).x;
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
      vec3 rgb = texture(sparseGridRgb, atlasUvw).rgb;
      rgb = denormalize(rgb, quantizeMinFeatures, quantizeMaxFeatures);
#else
      vec3 rgb = vec3(0.0, 0.0, 0.0);
#endif
#ifdef USE_TRIPLANE
      vec3 rgbTemp;
      rgbTemp = texture(planeRgb, planeUv[0]).rgb;
      rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                              quantizeMaxFeatures);
      rgb += rgbTemp;

      rgbTemp = texture(planeRgb, planeUv[1]).rgb;
      rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                              quantizeMaxFeatures);
      rgb += rgbTemp;

      rgbTemp = texture(planeRgb, planeUv[2]).rgb;
      rgbTemp = denormalize(rgbTemp.rgb, quantizeMinFeatures,
                              quantizeMaxFeatures);
      rgb += rgbTemp;
#endif

      rgb = sigmoid(rgb); // Apply activation function

      if (displayMode != DISPLAY_DIFFUSE) {
        vec4 features = vec4(0.0, 0.0, 0.0, 0.0);
#ifdef USE_SPARSE_GRID
        features = texture(sparseGridFeatures, atlasUvw);
        features = denormalize(features, quantizeMinFeatures,
                               quantizeMaxFeatures);
#endif
#ifdef USE_TRIPLANE
        vec4 featuresTemp;
        featuresTemp = texture(planeFeatures, planeUv[0]);
        features +=
            denormalize(featuresTemp,
                        quantizeMinFeatures, quantizeMaxFeatures);

        featuresTemp = texture(planeFeatures, planeUv[1]);
        features +=
            denormalize(featuresTemp,
                        quantizeMinFeatures, quantizeMaxFeatures);

        featuresTemp = texture(planeFeatures, planeUv[2]);
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

  if (displayMode == DISPLAY_VIEW_DEPENDENT) {
    accumulatedColor = vec3(0.0, 0.0, 0.0) * visibility;
  } else if (displayMode == DISPLAY_FEATURES) {
    accumulatedColor = accumulatedFeatures.rgb;
  }

  // Composite on white background
  accumulatedColor = vec3(1.0, 1.0, 1.0) * visibility + accumulatedColor;

  // Run view-dependency network
  if ((displayMode == DISPLAY_NORMAL ||
       displayMode == DISPLAY_VIEW_DEPENDENT)) {
    accumulatedColor += evaluateNetwork(accumulatedColor, accumulatedFeatures,
                             worldspaceROpengl * normalize(vDirection));
  }
  gl_FragColor = vec4(accumulatedColor, 1.0);
}
";
    private const string VERTEX = @"varying vec3 vOrigin;
varying vec3 vDirection;
uniform mat4 world_T_clip;

void main() {
  vec4 positionClip = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
  gl_Position = positionClip;

  positionClip /= positionClip.w;
  vec4 nearPoint = world_T_clip * vec4(positionClip.x, positionClip.y, -1.0, 1.0);
  vec4 farPoint = world_T_clip * vec4(positionClip.x, positionClip.y, 1.0, 1.0);

  vOrigin = nearPoint.xyz / nearPoint.w;
  vDirection = normalize(farPoint.xyz / farPoint.w - vOrigin);
}
";
}
