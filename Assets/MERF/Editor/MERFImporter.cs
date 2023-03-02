using BigGustave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static ViewDependency;
using static WebRequestAsyncUtility;

public class MERFImporter {

    private static readonly string LoadingTitle = "Loading Assets";
    private static readonly string ProcessingTitle = "Processing Assets";
    private static readonly string DownloadInfo = "Loading Assets for ";
    private static readonly string AssemblyInfo = "Assembling 3D Volume Textures for ";
    private static readonly string DownloadAllTitle = "Downloading All Assets";
    private static readonly string DownloadAllMessage = "You are about to download all the demo scenes from the MERF paper!\nDownloading/Processing might take a few minutes and quite a bit of RAM & disk space.\n\nClick 'OK', if you wish to continue.";

    [MenuItem("MERF/Asset Downloads/Download All", false, -20)]
    public static void DownloadAllAssets() {
        if (!EditorUtility.DisplayDialog(DownloadAllTitle, DownloadAllMessage, "OK")) {
            return;
        }

        foreach (var scene in (MERFScene[])Enum.GetValues(typeof(MERFScene))) {
            ImportAssetsAsync(scene);
        }
    }

    [MenuItem("MERF/Asset Downloads/Gardenvase", false, 0)]
    public static void DownloadGardenvaseAssets() {
        ImportAssetsAsync(MERFScene.Gardenvase);
    }
    [MenuItem("MERF/Asset Downloads/Bicycle", false, 0)]
    public static void DownloadBicycleAssets() {
        ImportAssetsAsync(MERFScene.Bicycle);
    }
    [MenuItem("MERF/Asset Downloads/Kitchen Lego", false, 0)]
    public static void DownloadKitchenLegoAssets() {
        ImportAssetsAsync(MERFScene.KitchenLego);
    }
    [MenuItem("MERF/Asset Downloads/Stump", false, 0)]
    public static void DownloadStumpAssets() {
        ImportAssetsAsync(MERFScene.Stump);
    }
    [MenuItem("MERF/Asset Downloads/Bonsai", false, 0)]
    public static void DownloadOfficeBonsaiAssets() {
        ImportAssetsAsync(MERFScene.OfficeBonsai);
    }
    [MenuItem("MERF/Asset Downloads/Full Living Room", false, 0)]
    public static void DownloadFullLivingRoomAssets() {
        ImportAssetsAsync(MERFScene.FullLivingRoom);
    }
    [MenuItem("MERF/Asset Downloads/Kitchen Counter", false, 0)]
    public static void DownloadKitchenCounterAssets() {
        ImportAssetsAsync(MERFScene.KitchenCounter);
    }

    private const string BASE_URL = "https://merf42.github.io/viewer/scenes/";

    private static string BASE_FOLDER = Path.Combine("Assets", "MERF Data");
    private static string BASE_LIB_FOLDER = Path.Combine("Library", "Cached MERF Data");

    private static string GetBasePath(MERFScene scene) {
        return Path.Combine(BASE_FOLDER, scene.String());
    }

    private static string GetCacheLocation(MERFScene scene) {
        return Path.Combine(BASE_LIB_FOLDER, scene.String());
    }

    internal static string GetBaseUrl(MERFScene scene) {
        return Path.Combine(BASE_URL, scene.String());
    }
    private static Uri GetMERFSourcesUrl(MERFScene scene) {
        return new Uri(Path.Combine(BASE_URL, $"{scene.String()}.json"));
    }

    private static string GetSceneParamsAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/SceneParams/{scene.String()}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPlaneRGBAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} RGB Triplane Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPlaneDensityAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Density Triplane Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPlaneFeaturesAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Features Triplane Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetOccupancyGridAssetPath(MERFScene scene, int i) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Occupancy Grid {i} Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetRGBTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} RGB Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetAlphaTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Alpha Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetFeatureTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Feature Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetAtlasTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Atlas Index Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetShaderAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Shaders/RayMarchShader_{scene}.shader";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetMaterialAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Materials/Material_{scene}.mat";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetWeightsAssetPath(MERFScene scene, int i) {
        string path = $"{GetBasePath(scene)}/SceneParams/weightsTex{i}.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static string GetOccupancyGridCachePath(MERFScene scene, int i) {
        string path = Path.Combine(GetCacheLocation(scene), $"occupancy_grid_{i}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetAtlasIndexCachePath(MERFScene scene) {
        string path = Path.Combine(GetCacheLocation(scene), "atlas_indices.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPlaneRGBCachePath(MERFScene scene, int i) {
        string path = Path.Combine(GetCacheLocation(scene), $"plane_rgb_and_density_{i}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetPlaneFeaturesCachePath(MERFScene scene, int i) {
        string path = Path.Combine(GetCacheLocation(scene), $"plane_features_{i}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetRGBVolumeCachePath(MERFScene scene, int i) {
        string path = Path.Combine(GetCacheLocation(scene), $"rgba_{i:D3}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetFeatureVolumeCachePath(MERFScene scene, int i) {
        string path = Path.Combine(GetCacheLocation(scene), $"feature_{i:D3}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetSceneUrlsCachePath(MERFScene scene) {
        string path = Path.Combine(GetCacheLocation(scene), $"{scene.String()}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }

    private static int[] occupancyGridBlockSizes = new int[] { 8, 16, 32, 64, 128 };

    private static ImportContext _context;
    private static object sweightsTexTwo;

    private static async Task<MERFSources> DownloadSceneUrlsAsync(MERFScene scene) {
        string path = GetSceneUrlsCachePath(scene);
        string sceneUrlsJsonString;

        if (File.Exists(path)) {
            // file is already downloaded
            sceneUrlsJsonString = File.ReadAllText(path);
        } else {
            Uri url = GetMERFSourcesUrl(scene);
            sceneUrlsJsonString = await WebRequestSimpleAsync.SendWebRequestAsync(url);
            File.WriteAllText(path, sceneUrlsJsonString);
        }

        MERFSources sceneUrls = JsonConvert.DeserializeObject<MERFSources>(sceneUrlsJsonString, new MERFSourcesConverter());
        return sceneUrls;
    }

    private static async Task<SceneParams> DownloadSceneParamsAsync(MERFSources sceneUrls, MERFScene scene) {
        Uri url = sceneUrls.Get("scene_params.json");
        string sceneParamsJson = await WebRequestSimpleAsync.SendWebRequestAsync(url);
        TextAsset mlpJsonTextAsset = new TextAsset(sceneParamsJson);
        CreateAsset(mlpJsonTextAsset, GetSceneParamsAssetPath(scene));

        SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
        return sceneParams;
    }

    private static async Task<byte[][]> DownloadOccupancyGridPNGsAsync(MERFSources sceneUrls, MERFScene scene) {
        List<Task<byte[]>> occupancyGridTasks = new List<Task<byte[]>>();

        for (int i = 0; i < occupancyGridBlockSizes.Length; i++) {
            Task<byte[]> t = DownloadOccupancyGridPNGAsync(sceneUrls, scene, i);
            occupancyGridTasks.Add(t);
        }

        byte[][] results = await Task.WhenAll(occupancyGridTasks);
        return results;
    }

    private static async Task<byte[]> DownloadOccupancyGridPNGAsync(MERFSources sceneUrls, MERFScene scene, int i) {
        string path = GetOccupancyGridCachePath(scene, occupancyGridBlockSizes[i]);

        if (!File.Exists(path)) {
            Uri url = sceneUrls.Get($"occupancy_grid_{occupancyGridBlockSizes[i]}.png");
            byte[] pngData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(path, pngData);
        }

        // occupancy grid .pngs have resolutions up to 16384, which Unity
        // doesn't let us decode into Texture2Ds, so we use a 3rd party lib.
        byte[] occupancyGridData;
        using (var stream = File.OpenRead(path)) {
            Png image = Png.Open(stream);
            int size = image.Width * image.Height;
            occupancyGridData = new byte[size];
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    int index = image.Width * y + x;
                    Pixel pixel = image.GetPixel(x, y);
                    occupancyGridData[index] = pixel.R;
                }
            }
        }

        return occupancyGridData;
    }

    private static async Task<Texture2D> DownloadAtlasIndexPNGAsync(MERFSources sceneUrls, MERFScene scene) {
        string path = GetAtlasIndexCachePath(scene);
        byte[] atlasIndexData;

        if (File.Exists(path)) {
            // file is already downloaded
            atlasIndexData = File.ReadAllBytes(path);
        } else {
            Uri url = sceneUrls.Get("atlas_indices.png");
            atlasIndexData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(path, atlasIndexData);
        }

        //!NOTE: Unity does NOT load this as an RGB24 texture. PNGs are always loaded as ARGB32.
        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.RGB24, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        atlasIndexImage.LoadImage(atlasIndexData);

        return atlasIndexImage;
    }


    private static async Task<Texture2D[]> DownloadPlanePNGsAsync(MERFSources sceneUrls, MERFScene scene) {
        List<Task<Texture2D>> planeTasks = new List<Task<Texture2D>>();

        for (int plane_idx = 0; plane_idx < 3; ++plane_idx) {
            Task<Texture2D> t = DownloadPlaneRGBPNGAsync(sceneUrls, scene, plane_idx);
            planeTasks.Add(t);
            t = DownloadPlaneFeaturesRGBPNGAsync(sceneUrls, scene, plane_idx);
            planeTasks.Add(t);
        }

        Texture2D[] results = await Task.WhenAll(planeTasks);
        return results;
    }

    private static async Task<Texture2D> DownloadPlaneRGBPNGAsync(MERFSources sceneUrls, MERFScene scene, int i) {
        string path = GetPlaneRGBCachePath(scene, i);
        byte[] atlasIndexData;

        if (File.Exists(path)) {
            // file is already downloaded
            atlasIndexData = File.ReadAllBytes(path);
        } else {
            Uri url = sceneUrls.Get($"plane_rgb_and_density_{i}.png");
            atlasIndexData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(path, atlasIndexData);
        }

        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        atlasIndexImage.LoadImage(atlasIndexData);

        return atlasIndexImage;
    }

    private static async Task<Texture2D> DownloadPlaneFeaturesRGBPNGAsync(MERFSources sceneUrls, MERFScene scene, int i) {
        string path = GetPlaneFeaturesCachePath(scene, i);
        byte[] atlasIndexData;

        if (File.Exists(path)) {
            // file is already downloaded
            atlasIndexData = File.ReadAllBytes(path);
        } else {
            Uri url = sceneUrls.Get($"plane_features_{i}.png");
            atlasIndexData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
            File.WriteAllBytes(path, atlasIndexData);
        }

        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        atlasIndexImage.LoadImage(atlasIndexData);

        return atlasIndexImage;
    }

    private static async Task<Texture2D[]> DownloadRGBVolumeDataAsync(MERFSources sceneUrls, MERFScene scene, SceneParams sceneParams) {
        Texture2D[] rgbVolumeArray = new Texture2D[sceneParams.NumSlices];
        for (int i = 0; i < sceneParams.NumSlices; i++) {
            string path = GetRGBVolumeCachePath(scene, i);
            byte[] rgbVolumeData;

            if (File.Exists(path)) {
                // file is already downloaded
                rgbVolumeData = File.ReadAllBytes(path);
            } else {
                Uri url = sceneUrls.GetRGBVolumeUrl(i);
                rgbVolumeData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
                File.WriteAllBytes(path, rgbVolumeData);
            }

            Texture2D rgbVolumeImage = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: true) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                alphaIsTransparency = true
            };
            rgbVolumeImage.LoadImage(rgbVolumeData);
            rgbVolumeArray[i] = rgbVolumeImage;
        }

        return rgbVolumeArray;
    }

    private static async Task<Texture2D[]> DownloadFeatureVolumeDataAsync(MERFSources sceneUrls, MERFScene scene, SceneParams sceneParams) {
        Texture2D[] featureVolumeArray = new Texture2D[sceneParams.NumSlices];

        for (int i = 0; i < sceneParams.NumSlices; i++) {
            string path = GetFeatureVolumeCachePath(scene, i);
            byte[] featureVolumeData;

            if (File.Exists(path)) {
                // file is already downloaded
                featureVolumeData = File.ReadAllBytes(path);
            } else {
                Uri url = sceneUrls.GetFeatureVolumeUrl(i);
                featureVolumeData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
                File.WriteAllBytes(path, featureVolumeData);
            }

            Texture2D featureVolumeImage = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: true) {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            featureVolumeImage.LoadImage(featureVolumeData);
            featureVolumeArray[i] = featureVolumeImage;
        }

        return featureVolumeArray;
    }

    /// <summary>
    /// Loads the indirection grid.
    /// </summary>
    private static void CreateAtlasIndexTexture(MERFScene scene, Texture2D atlasIndexImage, SceneParams sceneParams) {
        int width = (int)Mathf.Ceil(sceneParams.GridWidth / (float)sceneParams.BlockSize);
        int height = (int)Mathf.Ceil(sceneParams.GridHeight / (float)sceneParams.BlockSize);
        int depth = (int)Mathf.Ceil(sceneParams.GridDepth / (float)sceneParams.BlockSize);

        string atlasAssetPath = GetAtlasTextureAssetPath(scene);

        // already exists
        if (File.Exists(atlasAssetPath)) {
            return;
        }

        // initialize 3D texture
        Texture3D atlasIndex3DVolume = CreateVolumeTexture(width, height, depth, TextureFormat.RGB24, FilterMode.Point);
        atlasIndex3DVolume.name = Path.GetFileNameWithoutExtension(atlasAssetPath);

        // load data into 3D textures
        NativeArray<byte> rawAtlasIndexData = atlasIndexImage.GetRawTextureData<byte>();

        // we need to separate/extract RGB values manually, because Unity doesn't allow loading PNGs as RGB24 -.-
        // rawatlasIndexData is in ARGB format
        NativeArray<byte> atlasVolumeData = new NativeArray<byte>(3 * width * height * depth, Allocator.Temp);
        for (int i = 0, j = 0; i < rawAtlasIndexData.Length; i += 4, j += 3) {
            atlasVolumeData[j    ] = rawAtlasIndexData[i + 1];
            atlasVolumeData[j + 1] = rawAtlasIndexData[i + 2];
            atlasVolumeData[j + 2] = rawAtlasIndexData[i + 3];
        }

        atlasIndex3DVolume.SetPixelData(atlasVolumeData, 0);
        atlasIndex3DVolume.Apply();
        atlasVolumeData.Dispose();

        CreateAsset(atlasIndex3DVolume, atlasAssetPath);
        _context.AtlasIndex3DVolume = atlasIndex3DVolume;
    }

    /// <summary>
    /// Load triplanes.
    /// </summary>
    private static void CreateTriplaneRGBTextureArrays(MERFScene scene, Texture2D[] planeImages, SceneParams sceneParams) {
        //if (useTriplane)
        int planeWidth = sceneParams.PlaneWidth0;
        int planeHeight = sceneParams.PlaneHeight0;
        Vector2 planeSize = new Vector2(planeWidth, planeHeight);
        Texture2DArray planeRgbTexture       = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGB24);
        Texture2DArray planeDensityTexture   = CreateTextureArray(planeWidth, planeHeight, TextureFormat.R8);
        Texture2DArray planeFeaturesTexture  = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGBA32);

        NativeArray<byte> planeRgbStack      = new NativeArray<byte>(planeWidth * planeHeight * 3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<byte> planeDensityStack  = new NativeArray<byte>(planeWidth * planeHeight    , Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<byte> planeFeaturesStack = new NativeArray<byte>(planeWidth * planeHeight * 4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int plane_idx = 0; plane_idx < 3; plane_idx++) {
            Texture2D planeRgbAndDensity = planeImages[2 * plane_idx];
            Texture2D planeFeatures      = planeImages[2 * plane_idx + 1];
            // in ARGB format
            NativeArray<byte> rgbAndDensity = planeRgbAndDensity.GetRawTextureData<byte>();
            NativeArray<byte> features = planeFeatures.GetRawTextureData<byte>();

            for (int j = 0; j < planeWidth * planeHeight; j++) {
                planeRgbStack[j * 3 + 0] = rgbAndDensity[j * 4 + 1];
                planeRgbStack[j * 3 + 1] = rgbAndDensity[j * 4 + 2];
                planeRgbStack[j * 3 + 2] = rgbAndDensity[j * 4 + 3];
                planeDensityStack[j] = rgbAndDensity[j * 4];
                planeFeaturesStack[j * 4    ] = features[j * 4 + 1];
                planeFeaturesStack[j * 4 + 1] = features[j * 4 + 2];
                planeFeaturesStack[j * 4 + 2] = features[j * 4 + 3];
                planeFeaturesStack[j * 4 + 3] = features[j * 4    ];
            }

            planeRgbTexture.SetPixelData(planeRgbStack, 0, plane_idx);
            planeDensityTexture.SetPixelData(planeDensityStack, 0, plane_idx);
            planeFeaturesTexture.SetPixelData(planeFeaturesStack, 0, plane_idx);
        }

        planeRgbTexture.Apply();
        planeDensityTexture.Apply();
        planeFeaturesTexture.Apply();
        planeRgbStack.Dispose();
        planeDensityStack.Dispose();
        planeFeaturesStack.Dispose();

        CreateAsset(planeRgbTexture, GetPlaneRGBAssetPath(scene));
        CreateAsset(planeDensityTexture , GetPlaneDensityAssetPath(scene));
        CreateAsset(planeFeaturesTexture, GetPlaneFeaturesAssetPath(scene));

        _context.PlaneRgbTexture = planeRgbTexture;
        _context.PlaneDensityTexture = planeDensityTexture;
        _context.PlaneFeaturesTexture = planeFeaturesTexture;
    }

    private static Texture2DArray CreateTextureArray(int width, int height, TextureFormat format) {
        return new Texture2DArray(width, height, 3, format, mipChain: false) {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    }

    private static void CreateOccupancyGridTexture(MERFScene scene, byte[][] occupancyGrid, SceneParams sceneParams) {
        int baseGridWidth;
        double baseVoxelSize;
        if (true) {  //if (useTriplane)
            baseGridWidth = sceneParams.PlaneWidth0;
            baseVoxelSize = sceneParams.VoxelSizeTriplane;
        } else {
            baseGridWidth = sceneParams.GridWidth;
            baseVoxelSize = sceneParams.VoxelSize;
        }
        Texture3D[] occupancyGridTextures = new Texture3D[occupancyGridBlockSizes.Length];
        Vector4[] occupancyGridSizes = new Vector4[occupancyGridBlockSizes.Length];
        double[] occupancyVoxelSizes = new double[occupancyGridBlockSizes.Length];
        for (int occupancyGridIndex = 0; occupancyGridIndex < occupancyGridBlockSizes.Length; occupancyGridIndex++) {
            string occupancyAssetPath = GetOccupancyGridAssetPath(scene, occupancyGridIndex);
            int occupancyGridBlockSize = occupancyGridBlockSizes[occupancyGridIndex];
            // Assuming width = height = depth which typically holds when employing
            // scene contraction
            int w = (int)Math.Ceiling(baseGridWidth / (double)occupancyGridBlockSize);
            int h = w;
            int d = w;
            Texture3D occupancyGridTexture = CreateVolumeTexture(w, h, d, TextureFormat.R8, FilterMode.Point);
            occupancyGridTexture.name = Path.GetFileNameWithoutExtension(occupancyAssetPath);
            occupancyGridTextures[occupancyGridIndex] = occupancyGridTexture;
            occupancyGridSizes[occupancyGridIndex] = new Vector4(w, h, d, 0f);
            occupancyVoxelSizes[occupancyGridIndex] = baseVoxelSize * occupancyGridBlockSize;
            byte[] occupancyGridImageFourChannels = occupancyGrid[occupancyGridIndex];
            occupancyGridTexture.SetPixelData(occupancyGridImageFourChannels, 0);
            occupancyGridTexture.Apply();
            CreateAsset(occupancyGridTexture, occupancyAssetPath);
        }

        _context.OccupancyGridTextures = occupancyGridTextures;
        _context.OccupancyGridSizes = occupancyGridSizes;
        _context.OccupancyVoxelSizes = occupancyVoxelSizes;
    }

    private static Texture3D CreateVolumeTexture(int width, int height, int depth, TextureFormat format, FilterMode filterMode) {
        return new Texture3D(width, height, depth, format, mipChain: false) {
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
    }

    /// <summary>
    /// Creates the given asset and overwrites any existing asset with the same name
    /// </summary>
    private static void CreateAsset(UnityEngine.Object obj, string path) {
        var existing = AssetDatabase.LoadAssetAtPath(path, obj.GetType());
        if (existing != null) {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(obj, path);
    }

    /// <summary>
    /// Assemble shader code from header, on-the-fly generated view-dependency
    /// functions and body
    /// </summary>
    private static void CreateRayMarchShader(MERFScene scene, SceneParams sceneParams) {
        string shaderSource = ShaderTemplate.Template;
        string viewDependenceFunctions = CreateViewDependenceFunctions(sceneParams);
        shaderSource = new Regex("VIEWDEPENDENCESHADERFUNCTIONS").Replace(shaderSource, viewDependenceFunctions);
        shaderSource = new Regex("RAYMARCHVERTEXSHADER").Replace(shaderSource, ShaderTemplate.RayMarchVertexShader);
        shaderSource = new Regex("RAYMARCHFRAGMENTSHADER").Replace(shaderSource, ShaderTemplate.RayMarchFragmentShaderBody);

        shaderSource = new Regex("OBJECT_NAME").Replace(shaderSource, $"{scene}");
        string shaderAssetPath = GetShaderAssetPath(scene);
        File.WriteAllText(shaderAssetPath, shaderSource);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
        _context.Shader = shader;
    }

    /// <summary>
    /// Upload networks weights into textures (biases are written into as
    /// compile-time constants into the shader)
    /// </summary>
    private static void CreateWeightTextures(MERFScene scene, SceneParams sceneParams) {
        Texture2D weightsTexZero = CreateNetworkWeightTexture(sceneParams._0Weights);
        Texture2D weightsTexOne = CreateNetworkWeightTexture(sceneParams._1Weights);
        Texture2D weightsTexTwo = CreateNetworkWeightTexture(sceneParams._2Weights);
        CreateAsset(weightsTexZero, GetWeightsAssetPath(scene, 0));
        CreateAsset(weightsTexOne, GetWeightsAssetPath(scene, 1));
        CreateAsset(weightsTexTwo, GetWeightsAssetPath(scene, 2));
        AssetDatabase.SaveAssets();

        _context.WeightsTexZero = weightsTexZero;
        _context.WeightsTexOne = weightsTexOne;
        _context.WeightsTexTwo = weightsTexTwo;
    }

    private static void CreateMaterial(MERFScene scene, SceneParams sceneParams) {
        CreateRayMarchShader(scene, sceneParams);
        CreateWeightTextures(scene, sceneParams);

        string materialAssetPath = GetMaterialAssetPath(scene);
        Shader raymarchShader =_context.Shader;
        Material material = new Material(raymarchShader);

        // Now set all shader properties
        material.SetTexture("occupancyGrid_L4"     , _context.OccupancyGridTextures[0]);
        material.SetTexture("occupancyGrid_L3"     , _context.OccupancyGridTextures[1]);
        material.SetTexture("occupancyGrid_L2"     , _context.OccupancyGridTextures[2]);
        material.SetTexture("occupancyGrid_L1"     , _context.OccupancyGridTextures[3]);
        material.SetTexture("occupancyGrid_L0"     , _context.OccupancyGridTextures[4]);
        material.SetFloat  ("voxelSizeOccupancy_L4", (float)_context.OccupancyVoxelSizes[0]);
        material.SetFloat  ("voxelSizeOccupancy_L3", (float)_context.OccupancyVoxelSizes[1]);
        material.SetFloat  ("voxelSizeOccupancy_L2", (float)_context.OccupancyVoxelSizes[2]);
        material.SetFloat  ("voxelSizeOccupancy_L1", (float)_context.OccupancyVoxelSizes[3]);
        material.SetFloat  ("voxelSizeOccupancy_L0", (float)_context.OccupancyVoxelSizes[4]);
        material.SetVector ("gridSizeOccupancy_L4" , _context.OccupancyGridSizes[0]);
        material.SetVector ("gridSizeOccupancy_L3" , _context.OccupancyGridSizes[1]);
        material.SetVector ("gridSizeOccupancy_L2" , _context.OccupancyGridSizes[2]);
        material.SetVector ("gridSizeOccupancy_L1" , _context.OccupancyGridSizes[3]);
        material.SetVector ("gridSizeOccupancy_L0" , _context.OccupancyGridSizes[4]);
        material.SetTexture("weightsZero"          , _context.WeightsTexZero);
        material.SetTexture("weightsOne"           , _context.WeightsTexOne);
        material.SetTexture("weightsTwo"           , _context.WeightsTexTwo);

        material.SetVector("minPosition", new Vector4(
            (float)sceneParams.MinX,
            (float)sceneParams.MinY,
            (float)sceneParams.MinZ,
            0f)
        );
        material.SetInt("stepMult", 1);

        //if (useTriplane)
        material.SetTexture("planeRgb"         , _context.PlaneRgbTexture);
        material.SetTexture("planeDensity"     , _context.PlaneDensityTexture);
        material.SetTexture("planeFeatures"    , _context.PlaneFeaturesTexture);
        material.SetVector ("planeSize"        , new Vector4(sceneParams.PlaneWidth0, sceneParams.PlaneHeight0, 0, 0));
        material.SetFloat  ("voxelSizeTriplane", (float)sceneParams.VoxelSizeTriplane);
        LocalKeyword useTriplaneKeyword = new LocalKeyword(_context.Shader, "USE_TRIPLANE");
        material.SetKeyword(useTriplaneKeyword, true);

        //if (useSparseGrid)
        material.SetTexture("sparseGridDensity" , _context.DensityVolumeTexture);
        material.SetTexture("sparseGridRgb"     , _context.RGBVolumeTexture);
        material.SetTexture("sparseGridFeatures", _context.FeatureVolumeTexture);
        material.SetTexture("sparseGridIndex"   , _context.AtlasIndexTexture);
        material.SetFloat  ("blockSize"         ,        sceneParams.BlockSize);
        material.SetFloat  ("voxelSize"         , (float)sceneParams.VoxelSize);
        material.SetVector ("gridSize"          , new Vector4(sceneParams.GridWidth, sceneParams.GridHeight, sceneParams.GridDepth, 0));
        material.SetVector ("atlasSize"         , new Vector4(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, 0));
        LocalKeyword useSparseGridKeyword = new LocalKeyword(_context.Shader, "USE_SPARSE_GRID");
        material.SetKeyword(useSparseGridKeyword, true);

        //if (useLargerStepsWhenOccluded)
        LocalKeyword useLargerStepsKeyword = new LocalKeyword(_context.Shader, "LARGER_STEPS_WHEN_OCCLUDED");
        material.SetKeyword(useLargerStepsKeyword, true);

        CreateAsset(material, materialAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _context.Material = material;
    }

    private static async void ImportAssetsAsync(MERFScene scene) {
        _context = new ImportContext();
        string objName = scene.String();

        // first, make sure texture resources are downloaded to temp directory
        // then load them into memory 
        int progressId = Progress.Start(LoadingTitle, $"{DownloadInfo}'{objName}'...");
        MERFSources sceneUrls = await DownloadSceneUrlsAsync(scene);
        Progress.Report(progressId, 0.2f);

        SceneParams sceneParams = await DownloadSceneParamsAsync(sceneUrls, scene);

        long numTextures = occupancyGridBlockSizes.Length;

        byte[][] occupancyGrid = await DownloadOccupancyGridPNGsAsync(sceneUrls, scene);

        bool useSparseGrid = sceneParams.VoxelSize > 0;
        Task<Texture2D> atlasIndexTask = null;
        if (useSparseGrid) {
            // Load the indirection grid.
            atlasIndexTask = DownloadAtlasIndexPNGAsync(sceneUrls, scene);
            numTextures += 2 * sceneParams.NumSlices;
        }

        Texture2D[] planeImages = null;
        bool useTriplane = true; //sceneParams.ContainsKey("voxel_size_triplane");
        if (useTriplane) {
            numTextures += 6;
            planeImages = await DownloadPlanePNGsAsync(sceneUrls, scene);
        }

        var rgbVolumeTask = DownloadRGBVolumeDataAsync(sceneUrls, scene, sceneParams);
        var featureVolumeTask = DownloadFeatureVolumeDataAsync(sceneUrls, scene, sceneParams);

        Texture2D[] rgbImages = await rgbVolumeTask;
        Texture2D[] featureImages = await featureVolumeTask;

        // create 3D volumes and other assets
        Progress.Report(progressId, 0.3f, $"{AssemblyInfo}'{objName}'...");

        Texture2D atlasIndexData = await atlasIndexTask;
        CreateAtlasIndexTexture(scene, atlasIndexData, sceneParams);

        Progress.Report(progressId, 0.4f, $"{AssemblyInfo}'{objName}'...");

        CreateTriplaneRGBTextureArrays(scene, planeImages, sceneParams);

        Progress.Report(progressId, 0.5f, $"{AssemblyInfo}'{objName}'...");

        CreateOccupancyGridTexture(scene, occupancyGrid, sceneParams);

        Progress.Report(progressId, 0.6f, $"{AssemblyInfo}'{objName}'...");

        CreateMaterial(scene, sceneParams);

        /*Initialize(scene, atlasIndexData, rgbImages, featureImages, sceneParams);*/

        Progress.Remove(progressId);
    }
}