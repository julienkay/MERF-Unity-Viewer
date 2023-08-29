using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;
using System.Text;
using System;

namespace MERF.Editor {

    public static class ViewDependency {

        private static float[] GetNetworkWeights(double[][] network_weights) {
            int width = network_weights.Length;
            int height = network_weights[0].Length;

            float[] weightsData = new float[width * height];
            for (int co = 0; co < height; co++) {
                for (int ci = 0; ci < width; ci++) {
                    int index = co * width + ci;
                    double weight = network_weights[ci][co];
                    weightsData[index] = (float)weight;
                }
            }

            float[] weightsDataNew = new float[width * height];
            for (int j = 0; j < width; j += 4) {
                for (int i = 0; i < height; i++) {
                    for (int c = 0; c < 4; c++) {
                        weightsDataNew[(j / 4) * height * 4 + i * 4 + c] = weightsData[(j / 4) * 4 + i * ((width / 4) * 4) + c];
                    }
                }
            }

            return weightsDataNew;
        }

        private static void FlipY<T>(Texture2D texture) where T : struct {
            int width = texture.width;
            int height = texture.height;
            NativeArray<T> data = texture.GetPixelData<T>(0);
            for (int y = 0; y < height / 2; y++) {
                for (int x = 0; x < width; x++) {
                    int flippedY = height - y - 1;
                    int source = (flippedY * width) + x;
                    int target = (y * width) + x;
                    (data[target], data[source]) = (data[source], data[target]);
                }
            }
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
            string fragShader = RaymarchShader.ViewDependenceNetworkShaderFunctions;

            for (int layerIndex = 0; layerIndex < 3; layerIndex++) {
                StringBuilder biasList = toConstructorList(network_weights.GetBias(layerIndex));
                fragShader = new Regex("BIAS_LIST_" + layerIndex).Replace(fragShader, $"{biasList}");
            }

            float[] weights0 = GetNetworkWeights(sceneParams._0Weights);
            float[] weights1 = GetNetworkWeights(sceneParams._1Weights);
            float[] weights2 = GetNetworkWeights(sceneParams._2Weights);

            for (int i = 0; i < sceneParams._0Weights.Length; i++) {
                int stride = sceneParams._0Weights[0].Length;
                float[] subArray = weights0.SubArray(i * stride, stride);
                fragShader = new Regex($"__W0_{i}__").Replace(fragShader, $"{toConstructorList(subArray)}");
            }
            for (int i = 0; i < sceneParams._1Weights.Length; i++) {
                int stride = sceneParams._1Weights[0].Length;
                float[] subArray = weights1.SubArray(i * stride, stride);
                fragShader = new Regex($"__W1_{i}__").Replace(fragShader, $"{toConstructorList(subArray)}");
            }
            for (int i = 0; i < sceneParams._2Weights.Length; i++) {
                int stride = sceneParams._2Weights[0].Length;
                float[] subArray = weights2.SubArray(i * stride, stride);
                fragShader = new Regex($"__W2_{i}__").Replace(fragShader, $"{toConstructorList(subArray)}");
            }

            return fragShader;
        }

        private static StringBuilder toConstructorList(double[] values) {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
            int width = values.Length;
            StringBuilder output = new StringBuilder(width * 12);
            for (int i = 0; i < width; i++) {
                double value = values[i];
                output.Append(value.ToString("F7", culture));
                if (i + 1 < width) {
                    output.Append(", ");
                }
            }
            return output;
        }

        private static StringBuilder toConstructorList(float[] values) {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
            int width = values.Length;
            StringBuilder output = new StringBuilder(width * 12);
            for (int i = 0; i < width; i++) {
                double value = values[i];
                output.Append(value.ToString("F7", culture));
                if (i + 1 < width) {
                    output.Append(", ");
                }
            }
            return output;
        }

        private static int MakeMultipleOf(int x, int y) {
            if (x % y == 0) {
                return x;
            } else {
                return x + y - x % y;
            }
        }
        private static T[] SubArray<T>(this T[] array, int offset, int length) {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
    }
}