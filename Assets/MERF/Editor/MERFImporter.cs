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
    public static async void DownloadAllAssets() {
        if (!EditorUtility.DisplayDialog(DownloadAllTitle, DownloadAllMessage, "OK")) {
            return;
        }

        foreach (var scene in (MERFScene[])Enum.GetValues(typeof(MERFScene))) {
            await ImportAssetsAsync(scene);
        }
    }

    [MenuItem("MERF/Asset Downloads/Gardenvase", false, 0)]
    public static async void DownloadGardenvaseAssets() {
        await ImportAssetsAsync(MERFScene.Gardenvase);
    }
    [MenuItem("MERF/Asset Downloads/Bicycle", false, 0)]
    public static async void DownloadBicycleAssets() {
        await ImportAssetsAsync(MERFScene.Bicycle);
    }
    [MenuItem("MERF/Asset Downloads/Kitchen Lego", false, 0)]
    public static async void DownloadKitchenLegoAssets() {
        await ImportAssetsAsync(MERFScene.KitchenLego);
    }
    [MenuItem("MERF/Asset Downloads/Stump", false, 0)]
    public static async void DownloadStumpAssets() {
        await ImportAssetsAsync(MERFScene.Stump);
    }
    [MenuItem("MERF/Asset Downloads/Bonsai", false, 0)]
    public static async void DownloadOfficeBonsaiAssets() {
        await ImportAssetsAsync(MERFScene.OfficeBonsai);
    }
    [MenuItem("MERF/Asset Downloads/Full Living Room", false, 0)]
    public static async void DownloadFullLivingRoomAssets() {
        await ImportAssetsAsync(MERFScene.FullLivingRoom);
    }
    [MenuItem("MERF/Asset Downloads/Kitchen Counter", false, 0)]
    public static async void DownloadKitchenCounterAssets() {
        await ImportAssetsAsync(MERFScene.KitchenCounter);
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
    private static string GetRGBVolumeTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} RGB Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetDensityVolumeTextureAssetPath(MERFScene scene) {
        string path = $"{GetBasePath(scene)}/Textures/{scene.String()} Density Volume Texture.asset";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;
    }
    private static string GetFeatureVolumeTextureAssetPath(MERFScene scene) {
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
                    occupancyGridData[index] = (byte)(pixel.R == 1 ?  255 : 0);
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

        // !!! Unity's LoadImage() does NOT respect the texture format specified in the input texture!
        // It always loads this as ARGB32, no matter the format specified here.
        // Ideally we'd directly load an RGB24 texture.
        Texture2D atlasIndexImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
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

        // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
        Texture2D planeRGBImages = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        planeRGBImages.LoadImage(atlasIndexData);

        return planeRGBImages;
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

        // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
        Texture2D planeFeaturesImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        planeFeaturesImage.LoadImage(atlasIndexData);

        return planeFeaturesImage;
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

            // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
            Texture2D rgbVolumeImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
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

            // Unity's LoadImage() always loads this as ARGB32, no matter the format specified here
            Texture2D featureVolumeImage = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false, linear: true) {
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

        // initialize 3D texture
        Texture3D atlasIndex3DVolume = CreateVolumeTexture(width, height, depth, TextureFormat.RGB24, FilterMode.Point);

        // load data into 3D textures
        NativeArray<byte> rawAtlasIndexData = atlasIndexImage.GetRawTextureData<byte>();

        // we need to separate/extract RGB values manually, because Unity doesn't allow loading PNGs as RGB24 -.-
        // rawatlasIndexData is in ARGB format
        NativeArray<byte> atlasVolumeData = atlasIndex3DVolume.GetPixelData<byte>(0);
        for (int i = 0, j = 0; i < rawAtlasIndexData.Length; i += 4, j += 3) {
            atlasVolumeData[j    ] = rawAtlasIndexData[i + 1];
            atlasVolumeData[j + 1] = rawAtlasIndexData[i + 2];
            atlasVolumeData[j + 2] = rawAtlasIndexData[i + 3];
        }

        atlasIndex3DVolume.SetPixelData(atlasVolumeData, 0);

        // flip the y axis for each depth slice
        FlipY<Color24>(atlasIndex3DVolume);
        atlasIndex3DVolume.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        CreateAsset(atlasIndex3DVolume, atlasAssetPath);
        _context.AtlasIndexTexture = atlasIndex3DVolume;
    }

    /// <summary>
    /// Load triplanes.
    /// </summary>
    private static void CreateTriplaneRGBTextureArrays(MERFScene scene, Texture2D[] planeImages, SceneParams sceneParams) {
        //if (useTriplane)
        int planeWidth = sceneParams.PlaneWidth0;
        int planeHeight = sceneParams.PlaneHeight0;

        Texture2DArray planeRgbTexture       = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGB24);
        Texture2DArray planeDensityTexture   = CreateTextureArray(planeWidth, planeHeight, TextureFormat.R8);
        Texture2DArray planeFeaturesTexture  = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGBA32);

        for (int plane_idx = 0; plane_idx < 3; plane_idx++) {
            NativeArray<byte> planeRgbSlice = planeRgbTexture.GetPixelData<byte>(0, plane_idx);
            NativeArray<byte> planeDensitySlice = planeDensityTexture.GetPixelData<byte>(0, plane_idx);
            NativeArray<byte> planeFeaturesSlice = planeFeaturesTexture.GetPixelData<byte>(0, plane_idx);

            Texture2D planeRgbAndDensity = planeImages[2 * plane_idx];
            Texture2D planeFeatures      = planeImages[2 * plane_idx + 1];
            // in ARGB format
            NativeArray<byte> rgbAndDensity = planeRgbAndDensity.GetRawTextureData<byte>();
            NativeArray<byte> features = planeFeatures.GetRawTextureData<byte>();

            for (int j = 0; j < planeWidth * planeHeight; j++) {
                planeRgbSlice[j * 3 + 0] = rgbAndDensity[j * 4 + 1];
                planeRgbSlice[j * 3 + 1] = rgbAndDensity[j * 4 + 2];
                planeRgbSlice[j * 3 + 2] = rgbAndDensity[j * 4 + 3];
                planeDensitySlice[j] = rgbAndDensity[j * 4];
                planeFeaturesSlice[j * 4    ] = features[j * 4 + 1];
                planeFeaturesSlice[j * 4 + 1] = features[j * 4 + 2];
                planeFeaturesSlice[j * 4 + 2] = features[j * 4 + 3];
                planeFeaturesSlice[j * 4 + 3] = features[j * 4    ];
            }
        }

        FlipY<Color24>(planeRgbTexture);
        FlipZ<Color24>(planeRgbTexture);
        FlipY<byte>(planeDensityTexture);
        FlipZ<byte>(planeDensityTexture);
        FlipY<Color32>(planeFeaturesTexture);
        FlipZ<Color32>(planeFeaturesTexture);

        planeRgbTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        planeDensityTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        planeFeaturesTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

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
            occupancyGridTextures[occupancyGridIndex] = occupancyGridTexture;
            occupancyGridSizes[occupancyGridIndex] = new Vector4(w, h, d, 0f);
            occupancyVoxelSizes[occupancyGridIndex] = baseVoxelSize * occupancyGridBlockSize;
            byte[] occupancyGridImageFourChannels = occupancyGrid[occupancyGridIndex];
            occupancyGridTexture.SetPixelData(occupancyGridImageFourChannels, 0);
            occupancyGridTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            CreateAsset(occupancyGridTexture, occupancyAssetPath);
        }

        _context.OccupancyGridTextures = occupancyGridTextures;
        _context.OccupancyGridSizes = occupancyGridSizes;
        _context.OccupancyVoxelSizes = occupancyVoxelSizes;
    }

    private static void CreateRGBAndDensityVolumeTexture(MERFScene scene, Texture2D[] rgbImages, SceneParams sceneParams) {
        Debug.Assert(rgbImages.Length == sceneParams.NumSlices);
        int volumeWidth = sceneParams.AtlasWidth;
        int volumeHeight = sceneParams.AtlasHeight;
        int volumeDepth = sceneParams.AtlasDepth;

        int sliceDepth = sceneParams.SliceDepth;                    // slices packed into one atlased texture
        int numSlices  = sceneParams.NumSlices;                     // number of slice atlases
        int ppAtlas    = volumeWidth * volumeHeight * sliceDepth;   // pixels per atlased texture
        int ppSlice    = volumeWidth * volumeHeight;                // pixels per volume slice

        Texture3D rgbVolumeTexture      = CreateVolumeTexture(volumeWidth, volumeHeight, volumeDepth, TextureFormat.RGB24, FilterMode.Bilinear);
        Texture3D densityVolumeTexture  = CreateVolumeTexture(volumeWidth, volumeHeight, volumeDepth, TextureFormat.R8, FilterMode.Bilinear);
        NativeArray<byte> rgbPixels     = rgbVolumeTexture    .GetPixelData<byte>(0);
        NativeArray<byte> densityPixels = densityVolumeTexture.GetPixelData<byte>(0);

        for (int i = 0; i < numSlices; i++) {
            // rgbaImage is in ARGB format!
            NativeArray<byte> rgbaImage = rgbImages[i].GetRawTextureData<byte>();

            // The png's RGB channels hold RGB and the png's alpha channel holds
            // density. We split apart RGB and density and upload to two distinct
            // textures, so we can separately query these quantities.
            for (int s_r = sliceDepth - 1, s = 0; s_r >= 0; s_r--, s++) {

                int baseIndexRGB = (i * ppAtlas + s * ppSlice) * 3;
                int baseIndexAlpha = (i * ppAtlas + s * ppSlice);
                for (int j = 0; j < ppSlice; j++) {
                    rgbPixels[baseIndexRGB + (j * 3)] = rgbaImage[((s_r * ppSlice + j) * 4) + 1];
                    rgbPixels[baseIndexRGB + (j * 3) + 1] = rgbaImage[((s_r * ppSlice + j) * 4) + 2];
                    rgbPixels[baseIndexRGB + (j * 3) + 2] = rgbaImage[((s_r * ppSlice + j) * 4) + 3];
                    densityPixels[baseIndexAlpha + j] = rgbaImage[((s_r * ppSlice + j) * 4)];
                }
            }
        }

        FlipY<Color24>(rgbVolumeTexture);
        FlipZ<Color24>(rgbVolumeTexture, sceneParams.AtlasBlocksZ);
        FlipY<byte>(densityVolumeTexture);
        FlipZ<byte>(densityVolumeTexture, sceneParams.AtlasBlocksZ);

        rgbVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        densityVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        string rgbVolumeAssetPath = GetRGBVolumeTextureAssetPath(scene);
        string densityVolumeAssetPath = GetDensityVolumeTextureAssetPath(scene);
        CreateAsset(rgbVolumeTexture, rgbVolumeAssetPath);
        CreateAsset(densityVolumeTexture, densityVolumeAssetPath);

        _context.RGBVolumeTexture = rgbVolumeTexture;
        _context.DensityVolumeTexture = densityVolumeTexture;
    }

    private static void CreateFeatureVolumeTexture(MERFScene scene, Texture2D[] featureImages, SceneParams sceneParams) {
        Debug.Assert(featureImages.Length == sceneParams.NumSlices);
        int volumeWidth = sceneParams.AtlasWidth;
        int volumeHeight = sceneParams.AtlasHeight;
        int volumeDepth = sceneParams.AtlasDepth;

        int sliceDepth = sceneParams.SliceDepth;                // slices packed into one atlased texture
        int numSlices = sceneParams.NumSlices;                  // number of slice atlases
        int ppAtlas = volumeWidth * volumeHeight * sliceDepth;  // pixels per atlased feature texture
        int ppSlice = volumeWidth * volumeHeight;               // pixels per volume slice

        Texture3D featureVolumeTexture = CreateVolumeTexture(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, FilterMode.Bilinear);
        NativeArray<Color32> featurePixels = featureVolumeTexture.GetPixelData<Color32>(0);

        for (int i = 0; i < numSlices; i++) {
            NativeArray<Color32> _featureImageFourSlices = featureImages[i].GetRawTextureData<Color32>();

            for (int s_r = sliceDepth - 1, s = 0; s_r >= 0; s_r--, s++) {
                int targetIndex = (i * ppAtlas) + (s * ppSlice);
                NativeSlice<Color32> dst = new NativeSlice<Color32>(featurePixels, targetIndex, ppSlice);
                NativeSlice<Color32> src = new NativeSlice<Color32>(_featureImageFourSlices, s_r * ppSlice, ppSlice);

                dst.CopyFrom(src);
            }
        }
        FlipY<Color32>(featureVolumeTexture);
        FlipZ<Color32>(featureVolumeTexture, sceneParams.AtlasBlocksZ);

        featureVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        string featureVolumeAssetPath = GetFeatureVolumeTextureAssetPath(scene);
        CreateAsset(featureVolumeTexture, featureVolumeAssetPath);

        _context.FeatureVolumeTexture = featureVolumeTexture;
    }

    private static Texture3D CreateVolumeTexture(int width, int height, int depth, TextureFormat format, FilterMode filterMode) {
        return new Texture3D(width, height, depth, format, mipChain: false) {
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
    }

    /// <summary>
    /// Vertically flips each depth slice in the given 3D texture.
    /// </summary>
    private static void FlipY<T>(Texture3D texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;
        NativeArray<T> data = texture.GetPixelData<T>(0);
        for (int z = 0; z < depth; z++) {
            for (int y = 0; y < height / 2; y++) {
                for (int x = 0; x < width; x++) {
                    int flippedY = height - y - 1;
                    int source = z * (width * height) + (flippedY * width) + x;
                    int target = z * (width * height) + (y * width) + x;
                    (data[target], data[source]) = (data[source], data[target]);
                }
            }
        }
    }

    private static void FlipY<T>(Texture2DArray texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;
        for (int z = 0; z < depth; z++) {
            NativeArray<T> data = texture.GetPixelData<T>(0, z);
            for (int y = 0; y < height / 2; y++) {
                for (int x = 0; x < width; x++) {
                    int flippedY = height - y - 1;
                    int source = (flippedY * width) + x;
                    int target = (y * width) + x;
                    (data[target], data[source]) = (data[source], data[target]);
                }
            }
        }
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
    /// Flips z - Reverses the order of the depth slices of the given 3D texture.
    /// Atlases might be divided in macro blocks that are treated individually here.
    /// I.e. depth slices are only reversed within a block.
    /// </summary>
    private static void FlipZ<T>(Texture3D texture, int atlasBlocksZ) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;
        int stride = depth / atlasBlocksZ;
        int blockSize = width * height * stride;
        int sliceSize = width * height;

        NativeArray<T> data = texture.GetPixelData<T>(0);
        NativeArray<T> tmp = new NativeArray<T>(sliceSize, Allocator.Temp);

        for (int z = 0; z < atlasBlocksZ; z++) {
            for (int s = 0; s < stride / 2; s++) {
                int atlasBlock = z * blockSize;
                int slice1Index = atlasBlock + s * sliceSize;
                int slice2Index = atlasBlock + ((stride - s - 1) * sliceSize);

                NativeSlice<T> slice1 = new NativeSlice<T>(data, slice1Index, sliceSize);
                NativeSlice<T> slice2 = new NativeSlice<T>(data, slice2Index, sliceSize);

                slice1.CopyTo(tmp);
                slice1.CopyFrom(slice2);
                slice2.CopyFrom(tmp);
            }
        }

        tmp.Dispose();
    }

    /// <summary>
    /// Reverse the order of the depth slices of the 3D texture
    /// </summary>
    private static void FlipZ<T>(Texture3D texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;
        int sliceSize = width * height;

        NativeArray<T> data = texture.GetPixelData<T>(0);
        NativeArray<T> tmp = new NativeArray<T>(sliceSize, Allocator.Temp);

        for (int z = 0; z < depth / 2; z++) {
            int slice1Index = z * sliceSize;
            int slice2Index = (depth - z - 1) * sliceSize;

            NativeSlice<T> slice1 = new NativeSlice<T>(data, slice1Index, sliceSize);
            NativeSlice<T> slice2 = new NativeSlice<T>(data, slice2Index, sliceSize);

            slice1.CopyTo(tmp);
            slice1.CopyFrom(slice2);
            slice2.CopyFrom(tmp);
        }

        tmp.Dispose();
    }

    private static void FlipZ<T>(Texture2DArray texture) where T : struct {
        int width = texture.width;
        int height = texture.height;
        int depth = texture.depth;

        NativeArray<T> tmp = new NativeArray<T>(width * height, Allocator.Temp);

        for (int z = 0; z < depth / 2; z++) {
            int slice1Index = z;
            int slice2Index = (depth - z - 1);

            NativeArray<T> slice1 = texture.GetPixelData<T>(0, slice1Index);
            NativeArray<T> slice2 = texture.GetPixelData<T>(0, slice2Index);

            slice1.CopyTo(tmp);
            slice1.CopyFrom(slice2);
            slice2.CopyFrom(tmp);
        }

        tmp.Dispose();
    }

    private struct Color24 {
        public byte r;
        public byte g;
        public byte b;
    }

    /// <summary>
    /// Creates the given asset and overwrites any existing asset with the same name
    /// </summary>
    private static void CreateAsset(UnityEngine.Object obj, string path) {
        var existing = AssetDatabase.LoadAssetAtPath(path, obj.GetType());
        if (existing != null) {
            AssetDatabase.DeleteAsset(path);
        }
        obj.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(obj, path);
    }

    /// <summary>
    /// Assemble shader code from header, on-the-fly generated view-dependency
    /// functions and body
    /// </summary>
    private static void CreateRayMarchShader(MERFScene scene, SceneParams sceneParams) {
        string shaderSource = RaymarchShader.Template;
        shaderSource = new Regex("VIEWDEPENDENCESHADERFUNCTIONS").Replace(shaderSource, CreateViewDependenceFunctions(sceneParams));
        shaderSource = new Regex("RAYMARCHVERTEXSHADER"         ).Replace(shaderSource, RaymarchShader.RayMarchVertexShader);
        shaderSource = new Regex("RAYMARCHFRAGMENTSHADER"       ).Replace(shaderSource, RaymarchShader.RayMarchFragmentShaderBody);

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
        material.SetTexture("_OccupancyGrid_L4"     , _context.OccupancyGridTextures[0]);
        material.SetTexture("_OccupancyGrid_L3"     , _context.OccupancyGridTextures[1]);
        material.SetTexture("_OccupancyGrid_L2"     , _context.OccupancyGridTextures[2]);
        material.SetTexture("_OccupancyGrid_L1"     , _context.OccupancyGridTextures[3]);
        material.SetTexture("_OccupancyGrid_L0"     , _context.OccupancyGridTextures[4]);
        material.SetFloat  ("_VoxelSizeOccupancy_L4", (float)_context.OccupancyVoxelSizes[0]);
        material.SetFloat  ("_VoxelSizeOccupancy_L3", (float)_context.OccupancyVoxelSizes[1]);
        material.SetFloat  ("_VoxelSizeOccupancy_L2", (float)_context.OccupancyVoxelSizes[2]);
        material.SetFloat  ("_VoxelSizeOccupancy_L1", (float)_context.OccupancyVoxelSizes[3]);
        material.SetFloat  ("_VoxelSizeOccupancy_L0", (float)_context.OccupancyVoxelSizes[4]);
        material.SetVector ("_GridSizeOccupancy_L4" , _context.OccupancyGridSizes[0]);
        material.SetVector ("_GridSizeOccupancy_L3" , _context.OccupancyGridSizes[1]);
        material.SetVector ("_GridSizeOccupancy_L2" , _context.OccupancyGridSizes[2]);
        material.SetVector ("_GridSizeOccupancy_L1" , _context.OccupancyGridSizes[3]);
        material.SetVector ("_GridSizeOccupancy_L0" , _context.OccupancyGridSizes[4]);
        material.SetInteger("_DisplayMode"          , 0);
        material.SetTexture("_WeightsZero"          , _context.WeightsTexZero);
        material.SetTexture("_WeightsOne"           , _context.WeightsTexOne);
        material.SetTexture("_WeightsTwo"           , _context.WeightsTexTwo);

        float[][] m = sceneParams.WorldspaceTOpengl;
        Matrix4x4 worldspaceTOpengl = new Matrix4x4 {
            m00 = m[0][0],
            m01 = m[0][1],
            m02 = m[0][2],
            m10 = m[1][0],
            m11 = m[1][1],
            m12 = m[1][2],
            m20 = m[2][0],
            m21 = m[2][1],
            m22 = m[2][2]
        };
        material.SetMatrix("_Worldspace_T_opengl", worldspaceTOpengl);

        material.SetVector("_MinPosition", new Vector4(
            (float)sceneParams.MinX,
            (float)sceneParams.MinY,
            (float)sceneParams.MinZ,
            0f)
        );
        material.SetInteger("_StepMult", 1);

        //if (useTriplane)
        material.SetTexture("_PlaneRgb"         , _context.PlaneRgbTexture);
        material.SetTexture("_PlaneDensity"     , _context.PlaneDensityTexture);
        material.SetTexture("_PlaneFeatures"    , _context.PlaneFeaturesTexture);
        material.SetVector ("_PlaneSize"        , new Vector4(sceneParams.PlaneWidth0, sceneParams.PlaneHeight0, 0, 0));
        material.SetFloat  ("_VoxelSizeTriplane", (float)sceneParams.VoxelSizeTriplane);
        LocalKeyword useTriplaneKeyword = new LocalKeyword(_context.Shader, "USE_TRIPLANE");
        material.SetKeyword(useTriplaneKeyword, true);

        //if (useSparseGrid)
        material.SetTexture("_SparseGridDensity" , _context.DensityVolumeTexture);
        material.SetTexture("_SparseGridRgb"     , _context.RGBVolumeTexture);
        material.SetTexture("_SparseGridFeatures", _context.FeatureVolumeTexture);
        material.SetTexture("_SparseGridIndex"   , _context.AtlasIndexTexture);
        material.SetFloat  ("_BlockSize"         ,        sceneParams.BlockSize);
        material.SetFloat  ("_VoxelSize"         , (float)sceneParams.VoxelSize);
        material.SetVector ("_GridSize"          , new Vector4(sceneParams.GridWidth, sceneParams.GridHeight, sceneParams.GridDepth, 0));
        material.SetVector ("_AtlasSize"         , new Vector4(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, 0));
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

    private static async Task ImportAssetsAsync(MERFScene scene) {
        _context = new ImportContext();
        string objName = scene.String();

        // first, make sure texture resources are downloaded to temp directory
        // then load them into memory 
        int progressId = Progress.Start(LoadingTitle, $"{DownloadInfo}'{objName}'...");
        MERFSources sceneUrls = await DownloadSceneUrlsAsync(scene);
        Progress.Report(progressId, 0.2f);

        SceneParams sceneParams = await DownloadSceneParamsAsync(sceneUrls, scene);

        byte[][] occupancyGrid = await DownloadOccupancyGridPNGsAsync(sceneUrls, scene);

        bool useSparseGrid = sceneParams.VoxelSize > 0;
        Task<Texture2D> atlasIndexTask = null;
        if (useSparseGrid) {
            // Load the indirection grid.
            atlasIndexTask = DownloadAtlasIndexPNGAsync(sceneUrls, scene);
        }

        Texture2D[] planeImages = null;
        bool useTriplane = true; //sceneParams.ContainsKey("voxel_size_triplane");
        if (useTriplane) {
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

        if (useSparseGrid) {
            CreateRGBAndDensityVolumeTexture(scene, rgbImages, sceneParams);
            CreateFeatureVolumeTexture(scene, featureImages, sceneParams);
        }

        Progress.Report(progressId, 0.7f, $"{AssemblyInfo}'{objName}'...");

        CreateMaterial(scene, sceneParams);

        /*Initialize(scene, atlasIndexData, rgbImages, featureImages, sceneParams);*/

        Progress.Remove(progressId);
    }
}