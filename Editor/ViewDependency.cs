using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;
using System.Text;

namespace MERF.Editor {

    public static class ViewDependency {

        /// <summary>
        /// Creates a float32 data texture containing MLP weights.
        /// </summary>
        public static Texture2D CreateNetworkWeightTexture(double[][] network_weights) {
            int width = network_weights.Length;
            int height = network_weights[0].Length;

            NativeArray<float> weightsData = new NativeArray<float>(width * height, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int co = 0; co < height; co++) {
                for (int ci = 0; ci < width; ci++) {
                    int index = co * width + ci;
                    double weight = network_weights[ci][co];
                    weightsData[index] = (float)weight;
                }
            }

            NativeArray<float> weightsDataNew = new NativeArray<float>(width * height, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int j = 0; j < width; j += 4) {
                for (int i = 0; i < height; i++) {
                    for (int c = 0; c < 4; c++) {
                        weightsDataNew[(j / 4) * height * 4 + i * 4 + c] = weightsData[(j / 4) * 4 + i * ((width / 4) * 4) + c];
                    }
                }
            }

            Texture2D texture = new Texture2D(1, width * height / 4, TextureFormat.RGBA32, mipChain: false, linear: true) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixelData(weightsDataNew, 0);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            weightsData.Dispose();
            weightsDataNew.Dispose();

            return texture;
        }

        /// <summary>
        /// Creates shader code for the view-dependence MLP.
        /// 
        /// This populates the shader code in viewDependenceNetworkShaderFunctions with
        /// network weights and sizes as compile-time constants. The result is returned
        /// as a string.
        /// </summary>
        public static string CreateViewDependenceFunctions(SceneParams sceneParams) {
            // For mat4mul, we need to make sure that widths/heights of matrices
            // are multiples of 4
            for (int layerIndex = 0; layerIndex < 3; layerIndex++) {
                double[][] weights = sceneParams.GetWeights(layerIndex);
                double[] bias = sceneParams.GetBias(layerIndex);
                int width = weights.Length;
                int height = weights[0].Length;
                int new_width = MakeMultipleOf(width, 4);
                int new_height = MakeMultipleOf(height, 4);
                double[][] new_weights = new double[new_width][];
                for (int i = 0; i < new_weights.Length; i++) {
                    new_weights[i] = new double[new_height];
                }
                double[] new_bias = new double[new_height];
                for (int j = 0; j < new_width; j++) {
                    for (int i = 0; i < new_height; i++) {
                        if (j < width && i < height) {
                            new_weights[j][i] = weights[j][i];
                            new_bias[i] = bias[i];
                        } else {
                            new_weights[j][i] = 0.0;
                            new_bias[i] = 0.0;
                        }
                    }
                }
                sceneParams.SetWeights(layerIndex, new_weights);
                sceneParams.SetBias(layerIndex, new_bias);
            }

            SceneParams network_weights = sceneParams;

            // Write bias values as compile-time constants.
            string fragmentShaderSource = RaymarchShader.ViewDependenceNetworkShaderFunctions;

            for (int layerIndex = 0; layerIndex < 3; layerIndex++) {
                StringBuilder biasList = ToBiasList(network_weights.GetBias(layerIndex));
                fragmentShaderSource = new Regex("BIAS_LIST_" + layerIndex).Replace(fragmentShaderSource, $"{biasList}");
            }

            int channelsZero = network_weights._0Weights.Length;
            int channelsOne = network_weights._0Bias.Length;
            int channelsTwo = network_weights._1Bias.Length;
            int channelsThree = network_weights._2Bias.Length;
            int posEncScales = 4;

            fragmentShaderSource = new Regex("NUM_CHANNELS_ZERO").Replace(fragmentShaderSource, $"{channelsZero}");
            fragmentShaderSource = new Regex("NUM_POSENC_SCALES").Replace(fragmentShaderSource, $"{posEncScales}");
            fragmentShaderSource = new Regex("NUM_CHANNELS_ONE").Replace(fragmentShaderSource, $"{channelsOne}");
            fragmentShaderSource = new Regex("NUM_CHANNELS_TWO").Replace(fragmentShaderSource, $"{channelsTwo}");
            fragmentShaderSource = new Regex("NUM_CHANNELS_THREE").Replace(fragmentShaderSource, $"{channelsThree}");

            return fragmentShaderSource;
        }

        private static StringBuilder ToBiasList(double[] biases) {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
            int width = biases.Length;
            StringBuilder biasList = new StringBuilder(width * 12);
            biasList.Append("float4(");
            for (int i = 0; i < width; i++) {
                double bias = biases[i];
                if (i % 4 == 0 && i != 0 && i != width - 1) {
                    biasList.Append("), float4(");
                }
                biasList.Append(bias.ToString("F7", culture));
                if (i + 1 < width && (i + 1) % 4 != 0) {
                    biasList.Append(", ");
                }
            }
            biasList.Append(")");
            return biasList;
        }

        private static int MakeMultipleOf(int x, int y) {
            if (x % y == 0) {
                return x;
            } else {
                return x + y - x % y;
            }
        }
    }
}