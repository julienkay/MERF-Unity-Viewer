using BigGustave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
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
        AssetDatabase.CreateAsset(mlpJsonTextAsset, GetSceneParamsAssetPath(scene));

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

        // occupancy grid .pngs have resolutions up to 16384 which Unity
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
                    occupancyGridData[x] = pixel.R;
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

    private static async Task<Texture2D[]> DownloadPlaneRGBPNGsAsync(MERFSources sceneUrls, MERFScene scene) {
        List<Task<Texture2D>> planeTasks = new List<Task<Texture2D>>();

        for (int plane_idx = 0; plane_idx < 3; ++plane_idx) {
            Task<Texture2D> t = DownloadPlaneRGBPNGAsync(sceneUrls, scene, plane_idx);
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

        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.RGB24, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        atlasIndexImage.LoadImage(atlasIndexData);

        return atlasIndexImage;
    }
    private static async Task<Texture2D[]> DownloadPlaneFeaturesPNGsAsync(MERFSources sceneUrls, MERFScene scene) {
        List<Task<Texture2D>> planeTasks = new List<Task<Texture2D>>();

        for (int plane_idx = 0; plane_idx < 3; ++plane_idx) {
            Task<Texture2D> t = DownloadPlaneRGBPNGAsync(sceneUrls, scene, plane_idx);
            planeTasks.Add(t);
        }

        Texture2D[] results = await Task.WhenAll(planeTasks);
        return results;
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

        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.RGB24, mipChain: false, linear: true) {
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
        Texture3D atlasIndex3DVolume = new Texture3D(width, height, depth, TextureFormat.RGB24, mipChain: false) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = Path.GetFileNameWithoutExtension(atlasAssetPath),
        };

        // load data into 3D textures
        NativeArray<byte> rawAtlasIndexData = atlasIndexImage.GetRawTextureData<byte>();
        Debug.Log(atlasIndexImage.format);
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

        AssetDatabase.CreateAsset(atlasIndex3DVolume, atlasAssetPath);

        string materialAssetPath = GetMaterialAssetPath(scene);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
        material.SetTexture("mapIndex", atlasIndex3DVolume);
        AssetDatabase.SaveAssets();
    }

    private static async void ImportAssetsAsync(MERFScene scene) {
        string objName = scene.String();

        int progressId = Progress.Start(LoadingTitle, $"{DownloadInfo}'{objName}'...");
        MERFSources sceneUrls = await DownloadSceneUrlsAsync(scene);
        Progress.Report(progressId, 0.2f);

        SceneParams sceneParams = await DownloadSceneParamsAsync(sceneUrls, scene);

        long numTextures = occupancyGridBlockSizes.Length;

        await DownloadOccupancyGridPNGsAsync(sceneUrls, scene);

        bool useSparseGrid = sceneParams.VoxelSize > 0;
        Task<Texture2D> atlasIndexTask = null;
        if (useSparseGrid) {
            // Load the indirection grid.
            atlasIndexTask = DownloadAtlasIndexPNGAsync(sceneUrls, scene);
            numTextures += 2 * sceneParams.NumSlices;
        }

        List<Task> planeTasks = new List<Task>();
        bool useTriplane = true; //sceneParams.ContainsKey("voxel_size_triplane");
        if (useTriplane) {
            numTextures += 6;
            planeTasks.Add(DownloadPlaneRGBPNGsAsync(sceneUrls, scene));
            planeTasks.Add(DownloadPlaneFeaturesPNGsAsync(sceneUrls, scene));
        }

        // downloads 3D slices to temp directory
        var rgbVolumeTask = DownloadRGBVolumeDataAsync(sceneUrls, scene, sceneParams);
        var featureVolumeTask = DownloadFeatureVolumeDataAsync(sceneUrls, scene, sceneParams);

        Texture2D[] rgbImages = await rgbVolumeTask;
        Texture2D[] featureImages = await featureVolumeTask;

        Progress.Report(progressId, 0.3f, $"{AssemblyInfo}'{objName}'...");

        Texture2D atlasIndexData = await atlasIndexTask;
        CreateAtlasIndexTexture(scene, atlasIndexData, sceneParams);

        /*Initialize(scene, atlasIndexData, rgbImages, featureImages, sceneParams);*/

        Progress.Remove(progressId);
    }
}