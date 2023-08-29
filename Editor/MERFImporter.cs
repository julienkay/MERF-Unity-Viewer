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
using static MERF.Editor.ViewDependency;
using static MERF.Editor.WebRequestAsyncUtility;

namespace MERF.Editor {

    public class MERFImporter {

        private static readonly string LoadingTitle = "Loading Assets";
        private static readonly string DownloadInfo = "Loading Assets for ";
        private static readonly string AssemblyInfo = "Assembling 3D Volume Textures for ";
        private static readonly string FolderTitle = "Select folder with MERF source files";
        private static readonly string FolderExistsTitle = "Folder already exists";
        private static readonly string FolderExistsMsg = "A folder for this asset already exists in the Unity project. Overwrite?";
        private static readonly string OK = "OK";
        private static readonly string ImportErrorTitle = "Error importing MERF assets";

        [MenuItem("MERF/Import from disk", false, 0)]
        public async static void ImportAssetsFromDisk() {
            // select folder with custom data
            string path = EditorUtility.OpenFolderPanel(FolderTitle, "", "");
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
                return;
            }

            // ask whether to overwrite existing folder
            string objName = new DirectoryInfo(path).Name;
            if (Directory.Exists($"{BASE_FOLDER}{objName}")) {
                if (!EditorUtility.DisplayDialog(FolderExistsTitle, FolderExistsMsg, OK)) {
                    return;
                }
            }

            await ImportCustomScene(path);
        }

        [MenuItem("MERF/Asset Downloads/Gardenvase", false, 0)]
        public static async void DownloadGardenvaseAssets() {
            await ImportDemoSceneAsync(MERFScene.Gardenvase);
        }
        [MenuItem("MERF/Asset Downloads/Bicycle", false, 0)]
        public static async void DownloadBicycleAssets() {
            await ImportDemoSceneAsync(MERFScene.Bicycle);
        }
        [MenuItem("MERF/Asset Downloads/Kitchen Lego", false, 0)]
        public static async void DownloadKitchenLegoAssets() {
            await ImportDemoSceneAsync(MERFScene.KitchenLego);
        }
        [MenuItem("MERF/Asset Downloads/Stump", false, 0)]
        public static async void DownloadStumpAssets() {
            await ImportDemoSceneAsync(MERFScene.Stump);
        }
        [MenuItem("MERF/Asset Downloads/Bonsai", false, 0)]
        public static async void DownloadOfficeBonsaiAssets() {
            await ImportDemoSceneAsync(MERFScene.OfficeBonsai);
        }
        [MenuItem("MERF/Asset Downloads/Full Living Room", false, 0)]
        public static async void DownloadFullLivingRoomAssets() {
            await ImportDemoSceneAsync(MERFScene.FullLivingRoom);
        }
        [MenuItem("MERF/Asset Downloads/Kitchen Counter", false, 0)]
        public static async void DownloadKitchenCounterAssets() {
            await ImportDemoSceneAsync(MERFScene.KitchenCounter);
        }

        private const string BASE_URL = "https://creiser.github.io/assets/scenes/";
        private static string BASE_FOLDER = Path.Combine("Assets", "MERF Data");
        private static string BASE_LIB_FOLDER = Path.Combine("Library", "Cached MERF Data");

        private static string BasePath => Path.Combine(BASE_FOLDER, _context.SceneName);

        private static Uri GetMERFSourcesUrl(MERFScene scene) {
            return new Uri(Path.Combine(BASE_URL, $"{scene.LowerCaseName()}.json"));
        }

        private static string SceneParamsAssetPath => GetAssetPath("SceneParams", $"{_context.SceneName}.asset");
        private static string PlaneRGBAssetPath => GetAssetPath("Textures", $"{_context.SceneName} RGB Triplane Texture.asset");
        private static string PlaneDensityAssetPath => GetAssetPath("Textures", $"{_context.SceneName} Density Triplane Texture.asset");
        private static string PlaneFeaturesAssetPath => GetAssetPath("Textures", $"{_context.SceneName} Features Triplane Texture.asset");
        private static string OccupancyGridAssetPath(int i) => GetAssetPath("Textures", $"{_context.SceneName} Occupancy Grid {i} Texture.asset");
        private static string RGBVolumeTextureAssetPath => GetAssetPath("Textures", $"{_context.SceneName} RGB Volume Texture.asset");
        private static string DensityVolumeTextureAssetPath => GetAssetPath("Textures", $"{_context.SceneName} Density Volume Texture.asset");
        private static string FeatureVolumeTextureAssetPath => GetAssetPath("Textures", $"{_context.SceneName} Feature Volume Texture.asset");
        private static string AtlasTextureAssetPath => GetAssetPath("Textures", $"{_context.SceneName} Atlas Index Texture.asset");
        private static string ShaderAssetPath => GetAssetPath("Shaders", $"RayMarchShader_{_context.SceneName}.shader");
        private static string MaterialAssetPath => GetAssetPath("Materials", $"Material_{_context.SceneName}.mat");
        private static string PrefabAssetPath => GetAssetPath("", $"{_context.SceneName}.prefab");

        /// <summary>
        /// This returns a path in the asset directory to store the specific asset into.
        /// </summary>
        private static string GetAssetPath(string subFolder, string assetName) {
            string path = Path.Combine(BasePath, subFolder, assetName);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        private static string OccupancyGridCachePath(int i) => GetCachePath($"occupancy_grid_{i}.png");
        private static string AtlasIndexCachePath => GetCachePath("atlas_indices.png");
        private static string PlaneRGBCachePath(int i) => GetCachePath($"plane_rgb_and_density_{i}.png");
        private static string PlaneFeaturesCachePath(int i) => GetCachePath($"plane_features_{i}.png");
        private static string RGBVolumeCachePath(int i) => GetCachePath($"rgba_{i:D3}.png");
        private static string FeatureVolumeCachePath(int i) => GetCachePath($"feature_{i:D3}.png");
        private static string SceneUrlsCachePath => GetCachePath($"{_context.SceneName}.json");
        private static string SceneParamsCachePath => GetCachePath($"{_context.SceneName}_scene_params.json");


        /// <summary>
        /// This is either the location where demo scenes are first downloaded to,
        /// or the path to the source files for custom scene imports.
        /// </summary>
        private static string GetCachePath(string assetName) {
            string path;
            if (_context.CustomScene) {
                path = Path.Combine(_context.CustomScenePath, assetName);
            } else {
                path = Path.Combine(Path.Combine(BASE_LIB_FOLDER, _context.SceneName, assetName));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            return path;
        }

        private static int[] occupancyGridBlockSizes = new int[] { 8, 16, 32, 64, 128 };

        private static ImportContext _context;

        /// <summary>
        /// Creates Unity assets for the given MERF assets on disk.
        /// </summary>
        /// <param name="path">The path to the folder with the MERF assets (PNGs & mlp.json)</param>
        private static async Task ImportCustomScene(string path) {
            _context = new ImportContext() {
                CustomScene = true,
                CustomScenePath = path,
                Scene = MERFScene.Custom
            };
            string objName = new DirectoryInfo(path).Name;

            SceneParams sceneParams = CopySceneParamsFromPath(path);
            if (sceneParams == null) {
                return;
            }

            //TODO: validate if all expected assets are there

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            await ProcessAssets(objName, null);
        }

        private static async Task ImportDemoSceneAsync(MERFScene scene) {
            _context = new ImportContext() {
                CustomScene = false,
                Scene = scene
            };

            MERFSources sceneUrls = await DownloadSceneUrlsAsync();
            await DownloadSceneParamsAsync(sceneUrls);

            await ProcessAssets(scene.Name(), sceneUrls);
        }

        private static async Task ProcessAssets(string sceneName, MERFSources sceneUrls) {
            var sceneParams = GetSceneParams();

            int progressId = Progress.Start(LoadingTitle, $"{DownloadInfo}'{sceneName}'...");
            byte[][] occupancyGrid = await DownloadOccupancyGridPNGsAsync(sceneUrls);

            bool useSparseGrid = sceneParams.VoxelSize > 0;
            Task<Texture2D> atlasIndexTask = null;
            if (useSparseGrid) {
                // Load the indirection grid.
                atlasIndexTask = DownloadAtlasIndexPNGAsync(sceneUrls);
            }

            Texture2D[] planeImages = null;
            bool useTriplane = true; //sceneParams.ContainsKey("voxel_size_triplane");
            if (useTriplane) {
                planeImages = await DownloadPlanePNGsAsync(sceneUrls);
            }

            var rgbVolumeTask = DownloadRGBVolumeDataAsync(sceneUrls, sceneParams);
            var featureVolumeTask = DownloadFeatureVolumeDataAsync(sceneUrls, sceneParams);

            Texture2D[] rgbImages = await rgbVolumeTask;
            Texture2D[] featureImages = await featureVolumeTask;

            // create 3D volumes and other assets
            Progress.Report(progressId, 0.3f, $"{AssemblyInfo}'{sceneName}'...");

            Texture2D atlasIndexData = await atlasIndexTask;
            CreateAtlasIndexTexture(atlasIndexData, sceneParams);

            Progress.Report(progressId, 0.4f, $"{AssemblyInfo}'{sceneName}'...");

            CreateTriplaneTextureArrays(planeImages, sceneParams);

            Progress.Report(progressId, 0.5f, $"{AssemblyInfo}'{sceneName}'...");

            CreateOccupancyGridTexture(occupancyGrid, sceneParams);

            Progress.Report(progressId, 0.6f, $"{AssemblyInfo}'{sceneName}'...");

            if (useSparseGrid) {
                CreateRGBAndDensityVolumeTexture(rgbImages, sceneParams);
                CreateFeatureVolumeTexture(featureImages, sceneParams);
            }

            Progress.Report(progressId, 0.7f, $"{AssemblyInfo}'{sceneName}'...");

            CreateMaterial(sceneParams);

            CreatePrefab(sceneParams);

            Progress.Remove(progressId);
        }

        private static async Task<MERFSources> DownloadSceneUrlsAsync() {
            string path = SceneUrlsCachePath;
            string sceneUrlsJsonString;

            if (File.Exists(path)) {
                // file is already downloaded
                sceneUrlsJsonString = File.ReadAllText(path);
            } else {
                Uri url = GetMERFSourcesUrl(_context.Scene);
                sceneUrlsJsonString = await WebRequestSimpleAsync.SendWebRequestAsync(url);
                File.WriteAllText(path, sceneUrlsJsonString);
            }

            MERFSources sceneUrls = JsonConvert.DeserializeObject<MERFSources>(sceneUrlsJsonString, new MERFSourcesConverter());
            return sceneUrls;
        }

        /// <summary>
        /// Looks for a scene_params.json at <paramref name="path"/> and imports it.
        /// </summary>
        private static SceneParams CopySceneParamsFromPath(string path) {
            string[] sceneParamsPaths = Directory.GetFiles(path, "scene_params.json", SearchOption.AllDirectories);
            if (sceneParamsPaths.Length > 1) {
                EditorUtility.DisplayDialog(ImportErrorTitle, "Multiple scene_params.json files found", OK);
                return null;
            }
            if (sceneParamsPaths.Length <= 0) {
                EditorUtility.DisplayDialog(ImportErrorTitle, "No scene_params.json files found", OK);
                return null;
            }

            string sceneParamsJson = File.ReadAllText(sceneParamsPaths[0]);
            TextAsset sceneParamsTextAsset = new TextAsset(sceneParamsJson);
            AssetDatabase.CreateAsset(sceneParamsTextAsset, SceneParamsAssetPath);
            SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
            return sceneParams;
        }

        private static async Task<SceneParams> DownloadSceneParamsAsync(MERFSources sceneUrls) {
            Uri url = sceneUrls.Get("scene_params.json");
            string sceneParamsJson = await WebRequestSimpleAsync.SendWebRequestAsync(url);
            File.WriteAllText(SceneParamsCachePath, sceneParamsJson);
            TextAsset mlpJsonTextAsset = new TextAsset(sceneParamsJson);
            CreateAsset(mlpJsonTextAsset, SceneParamsAssetPath);

            SceneParams sceneParams = JsonConvert.DeserializeObject<SceneParams>(sceneParamsJson);
            return sceneParams;
        }

        private static SceneParams GetSceneParams() {
            string mlpJson = AssetDatabase.LoadAssetAtPath<TextAsset>(SceneParamsAssetPath).text;
            return JsonConvert.DeserializeObject<SceneParams>(mlpJson);
        }

        private static async Task<byte[][]> DownloadOccupancyGridPNGsAsync(MERFSources sceneUrls) {
            List<Task<byte[]>> occupancyGridTasks = new List<Task<byte[]>>();

            for (int i = 0; i < occupancyGridBlockSizes.Length; i++) {
                Task<byte[]> t = DownloadOccupancyGridPNGAsync(sceneUrls, i);
                occupancyGridTasks.Add(t);
            }

            byte[][] results = await Task.WhenAll(occupancyGridTasks);
            return results;
        }

        private static async Task<byte[]> DownloadOccupancyGridPNGAsync(MERFSources sceneUrls, int i) {
            string path = OccupancyGridCachePath(occupancyGridBlockSizes[i]);

            if (!File.Exists(path)) {
                Uri url = sceneUrls.Get($"occupancy_grid_{occupancyGridBlockSizes[i]}.png");
                byte[] pngData = await WebRequestBinaryAsync.SendWebRequestAsync(url);
                File.WriteAllBytes(path, pngData);
            }

            // occupancy grid .pngs have resolutions up to 16384, which Unity
            // doesn't let us decode into Texture2Ds, so we use a 3rd party lib.
            // unlike Unity's ImageConversion.LoadImage() this library decodes the
            // data as laid out in the file: starting top left.
            byte[] occupancyGridData;
            using (var stream = File.OpenRead(path)) {
                Png image = Png.Open(stream);
                int size = image.Width * image.Height;
                occupancyGridData = new byte[size];
                for (int y = 0; y < image.Height; y++) {
                    for (int x = 0; x < image.Width; x++) {
                        int index = image.Width * y + x;
                        Pixel pixel = image.GetPixel(x, y);
                        occupancyGridData[index] = (byte)(pixel.R == 1 ? 255 : 0);
                    }
                }
            }

            return occupancyGridData;
        }

        private static async Task<Texture2D> DownloadAtlasIndexPNGAsync(MERFSources sceneUrls) {
            string path = AtlasIndexCachePath;
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


        private static async Task<Texture2D[]> DownloadPlanePNGsAsync(MERFSources sceneUrls) {
            List<Task<Texture2D>> planeTasks = new List<Task<Texture2D>>();

            for (int plane_idx = 0; plane_idx < 3; ++plane_idx) {
                Task<Texture2D> t_rgb = DownloadPlaneRGBPNGAsync(sceneUrls, plane_idx);
                planeTasks.Add(t_rgb);
                Task<Texture2D> t_f = DownloadPlaneFeaturesRGBPNGAsync(sceneUrls, plane_idx);
                planeTasks.Add(t_f);
            }

            Texture2D[] results = await Task.WhenAll(planeTasks);
            return results;
        }

        private static async Task<Texture2D> DownloadPlaneRGBPNGAsync(MERFSources sceneUrls, int i) {
            string path = PlaneRGBCachePath(i);
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
            planeRGBImages.name = $"plane_rgb_and_density_{i}";

            return planeRGBImages;
        }

        private static async Task<Texture2D> DownloadPlaneFeaturesRGBPNGAsync(MERFSources sceneUrls, int i) {
            string path = PlaneFeaturesCachePath(i);
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
            planeFeaturesImage.name = $"plane_features_{i}";

            return planeFeaturesImage;
        }

        private static async Task<Texture2D[]> DownloadRGBVolumeDataAsync(MERFSources sceneUrls, SceneParams sceneParams) {
            Texture2D[] rgbVolumeArray = new Texture2D[sceneParams.NumSlices];
            for (int i = 0; i < sceneParams.NumSlices; i++) {
                string path = RGBVolumeCachePath(i);
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

        private static async Task<Texture2D[]> DownloadFeatureVolumeDataAsync(MERFSources sceneUrls, SceneParams sceneParams) {
            Texture2D[] featureVolumeArray = new Texture2D[sceneParams.NumSlices];

            for (int i = 0; i < sceneParams.NumSlices; i++) {
                string path = FeatureVolumeCachePath(i);
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
        private static void CreateAtlasIndexTexture(Texture2D atlasIndexImage, SceneParams sceneParams) {
            int width = (int)Mathf.Ceil(sceneParams.GridWidth / (float)sceneParams.BlockSize);
            int height = (int)Mathf.Ceil(sceneParams.GridHeight / (float)sceneParams.BlockSize);
            int depth = (int)Mathf.Ceil(sceneParams.GridDepth / (float)sceneParams.BlockSize);

            string atlasAssetPath = AtlasTextureAssetPath;

            // initialize 3D texture
            Texture3D atlasIndex3DVolume = CreateVolumeTexture(width, height, depth, TextureFormat.RGB24, FilterMode.Point);

            // load data into 3D textures
            NativeArray<byte> rawAtlasIndexData = atlasIndexImage.GetRawTextureData<byte>();

            // we need to separate/extract RGB values manually, because Unity doesn't allow loading PNGs as RGB24 -.-
            // rawatlasIndexData is in ARGB format
            NativeArray<byte> atlasVolumeData = atlasIndex3DVolume.GetPixelData<byte>(0);
            for (int i = 0, j = 0; i < rawAtlasIndexData.Length; i += 4, j += 3) {
                atlasVolumeData[j] = rawAtlasIndexData[i + 1];
                atlasVolumeData[j + 1] = rawAtlasIndexData[i + 2];
                atlasVolumeData[j + 2] = rawAtlasIndexData[i + 3];
            }

            atlasIndex3DVolume.SetPixelData(atlasVolumeData, 0);

            // flip the y axis for each depth slice
            //FlipX<Color24>(atlasIndex3DVolume);
            FlipY<Color24>(atlasIndex3DVolume);
            FlipZ<Color24>(atlasIndex3DVolume);
            atlasIndex3DVolume.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            CreateAsset(atlasIndex3DVolume, atlasAssetPath);
            _context.AtlasIndexTexture = atlasIndex3DVolume;
        }

        /// <summary>
        /// Load triplanes.
        /// </summary>
        private static void CreateTriplaneTextureArrays(Texture2D[] planeImages, SceneParams sceneParams) {
            //if (useTriplane)
            int planeWidth = sceneParams.PlaneWidth0;
            int planeHeight = sceneParams.PlaneHeight0;

            Texture2DArray planeRgbTexture = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGB24);
            Texture2DArray planeDensityTexture = CreateTextureArray(planeWidth, planeHeight, TextureFormat.R8);
            Texture2DArray planeFeaturesTexture = CreateTextureArray(planeWidth, planeHeight, TextureFormat.RGBA32);

            for (int plane_idx = 0; plane_idx < 3; plane_idx++) {
                NativeArray<byte> planeRgbSlice = planeRgbTexture.GetPixelData<byte>(0, plane_idx);
                NativeArray<byte> planeDensitySlice = planeDensityTexture.GetPixelData<byte>(0, plane_idx);
                NativeArray<byte> planeFeaturesSlice = planeFeaturesTexture.GetPixelData<byte>(0, plane_idx);

                Texture2D planeRgbAndDensity = planeImages[2 * plane_idx];
                Texture2D planeFeatures = planeImages[2 * plane_idx + 1];
                // both in ARGB format!
                NativeArray<byte> rgbAndDensity = planeRgbAndDensity.GetRawTextureData<byte>();
                NativeArray<byte> features = planeFeatures.GetRawTextureData<byte>();

                for (int j = 0; j < planeWidth * planeHeight; j++) {
                    planeRgbSlice[j * 3 + 0] = rgbAndDensity[j * 4 + 1];
                    planeRgbSlice[j * 3 + 1] = rgbAndDensity[j * 4 + 2];
                    planeRgbSlice[j * 3 + 2] = rgbAndDensity[j * 4 + 3];
                    planeDensitySlice[j] = rgbAndDensity[j * 4];
                    planeFeaturesSlice[j * 4] = features[j * 4 + 1];
                    planeFeaturesSlice[j * 4 + 1] = features[j * 4 + 2];
                    planeFeaturesSlice[j * 4 + 2] = features[j * 4 + 3];
                    planeFeaturesSlice[j * 4 + 3] = features[j * 4];
                }
            }

            //FlipX<Color24>(planeRgbTexture);
            FlipY<Color24>(planeRgbTexture);
            //FlipZ<Color24>(planeRgbTexture);
            //FlipX<byte>(planeDensityTexture);
            FlipY<byte>(planeDensityTexture);
            //FlipZ<byte>(planeDensityTexture);
            //FlipX<Color32>(planeFeaturesTexture);
            FlipY<Color32>(planeFeaturesTexture);
            //FlipZ<Color32>(planeFeaturesTexture);

            planeRgbTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            planeDensityTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            planeFeaturesTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            CreateAsset(planeRgbTexture, PlaneRGBAssetPath);
            CreateAsset(planeDensityTexture, PlaneDensityAssetPath);
            CreateAsset(planeFeaturesTexture, PlaneFeaturesAssetPath);

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

        private static void CreateOccupancyGridTexture(byte[][] occupancyGrid, SceneParams sceneParams) {
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
                string occupancyAssetPath = OccupancyGridAssetPath(occupancyGridIndex);
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

        private static void CreateRGBAndDensityVolumeTexture(Texture2D[] rgbImages, SceneParams sceneParams) {
            Debug.Assert(rgbImages.Length == sceneParams.NumSlices);
            int volumeWidth  = sceneParams.AtlasWidth;
            int volumeHeight = sceneParams.AtlasHeight;
            int volumeDepth  = sceneParams.AtlasDepth;

            int sliceDepth = sceneParams.SliceDepth;                  // slices packed into one atlased texture
            int numSlices  = sceneParams.NumSlices;                   // number of slice atlases
            int ppAtlas    = volumeWidth * volumeHeight * sliceDepth; // pixels per atlased texture
            int ppSlice    = volumeWidth * volumeHeight;              // pixels per volume slice

            Texture3D rgbVolumeTexture     = CreateVolumeTexture(volumeWidth, volumeHeight, volumeDepth, TextureFormat.RGB24, FilterMode.Bilinear);
            Texture3D densityVolumeTexture = CreateVolumeTexture(volumeWidth, volumeHeight, volumeDepth, TextureFormat.R8, FilterMode.Bilinear);
            NativeArray<byte> rgbPixels    = rgbVolumeTexture.GetPixelData<byte>(0);
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
                        rgbPixels[baseIndexRGB + (j * 3)]     = rgbaImage[((s_r * ppSlice + j) * 4) + 1];
                        rgbPixels[baseIndexRGB + (j * 3) + 1] = rgbaImage[((s_r * ppSlice + j) * 4) + 2];
                        rgbPixels[baseIndexRGB + (j * 3) + 2] = rgbaImage[((s_r * ppSlice + j) * 4) + 3];
                        densityPixels[baseIndexAlpha + j]     = rgbaImage[((s_r * ppSlice + j) * 4)];
                    }
                }
            }


            //FlipX<Color24>(rgbVolumeTexture);
            FlipY<Color24>(rgbVolumeTexture);
            //FlipZ<Color24>(rgbVolumeTexture, sceneParams.AtlasBlocksZ);
            //FlipX<byte>(densityVolumeTexture);
            FlipY<byte>(densityVolumeTexture);
            //FlipZ<byte>(densityVolumeTexture, sceneParams.AtlasBlocksZ);

            rgbVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            densityVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            string rgbVolumeAssetPath = RGBVolumeTextureAssetPath;
            string densityVolumeAssetPath = DensityVolumeTextureAssetPath;
            CreateAsset(rgbVolumeTexture, rgbVolumeAssetPath);
            CreateAsset(densityVolumeTexture, densityVolumeAssetPath);

            _context.RGBVolumeTexture = rgbVolumeTexture;
            _context.DensityVolumeTexture = densityVolumeTexture;
        }

        private static void CreateFeatureVolumeTexture(Texture2D[] featureImages, SceneParams sceneParams) {
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

            //FlipX<Color32>(featureVolumeTexture);
            FlipY<Color32>(featureVolumeTexture);
            //FlipZ<Color32>(featureVolumeTexture, sceneParams.AtlasBlocksZ);

            featureVolumeTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            string featureVolumeAssetPath = FeatureVolumeTextureAssetPath;
            CreateAsset(featureVolumeTexture, featureVolumeAssetPath);

            _context.FeatureVolumeTexture = featureVolumeTexture;
        }

        private static Texture3D CreateVolumeTexture(int width, int height, int depth, TextureFormat format, FilterMode filterMode) {
            return new Texture3D(width, height, depth, format, mipChain: false) {
                filterMode = filterMode,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        private static void FlipX<T>(Texture3D texture) where T : struct {
            int width = texture.width;
            int height = texture.height;
            int depth = texture.depth;
            NativeArray<T> data = texture.GetPixelData<T>(0);
            for (int z = 0; z < depth; z++) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width / 2; x++) {
                        int flippedX = width - x - 1;
                        int source = z * (width * height) + (y * width) + flippedX;
                        int target = z * (width * height) + (y * width) + x;
                        (data[target], data[source]) = (data[source], data[target]);
                    }
                }
            }
        }

        private static void FlipX<T>(Texture2DArray texture) where T : struct {
            int width = texture.width;
            int height = texture.height;
            int depth = texture.depth;
            for (int z = 0; z < depth; z++) {
                NativeArray<T> data = texture.GetPixelData<T>(0, z);
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width / 2; x++) {
                        int flippedX = width - x - 1;
                        int source = (y * width) + flippedX;
                        int target = (y * width) + x;
                        (data[target], data[source]) = (data[source], data[target]);
                    }
                }
            }
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
        private static void CreateRayMarchShader(SceneParams sceneParams) {
            string shaderSource = RaymarchShader.Template;
            shaderSource = new Regex("VIEWDEPENDENCESHADERFUNCTIONS").Replace(shaderSource, CreateViewDependenceFunctions(sceneParams));
            shaderSource = new Regex("RAYMARCHVERTEXSHADER").Replace(shaderSource, RaymarchShader.RayMarchVertexShader);
            shaderSource = new Regex("RAYMARCHFRAGMENTSHADER").Replace(shaderSource, RaymarchShader.RayMarchFragmentShaderBody);

            shaderSource = new Regex("OBJECT_NAME").Replace(shaderSource, $"{_context.SceneNameUpperCase}");
            string shaderAssetPath = ShaderAssetPath;
            File.WriteAllText(shaderAssetPath, shaderSource);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderAssetPath);
            _context.Shader = shader;
        }

        private static void CreateMaterial(SceneParams sceneParams) {
            CreateRayMarchShader(sceneParams);

            string materialAssetPath = MaterialAssetPath;
            Shader raymarchShader = _context.Shader;
            Material material = new Material(raymarchShader);

            // Now set all shader properties
            material.SetTexture("_OccupancyGrid_L4", _context.OccupancyGridTextures[0]);
            material.SetTexture("_OccupancyGrid_L3", _context.OccupancyGridTextures[1]);
            material.SetTexture("_OccupancyGrid_L2", _context.OccupancyGridTextures[2]);
            material.SetTexture("_OccupancyGrid_L1", _context.OccupancyGridTextures[3]);
            material.SetTexture("_OccupancyGrid_L0", _context.OccupancyGridTextures[4]);
            material.SetFloat("_VoxelSizeOccupancy_L4", (float)_context.OccupancyVoxelSizes[0]);
            material.SetFloat("_VoxelSizeOccupancy_L3", (float)_context.OccupancyVoxelSizes[1]);
            material.SetFloat("_VoxelSizeOccupancy_L2", (float)_context.OccupancyVoxelSizes[2]);
            material.SetFloat("_VoxelSizeOccupancy_L1", (float)_context.OccupancyVoxelSizes[3]);
            material.SetFloat("_VoxelSizeOccupancy_L0", (float)_context.OccupancyVoxelSizes[4]);
            material.SetVector("_GridSizeOccupancy_L4", _context.OccupancyGridSizes[0]);
            material.SetVector("_GridSizeOccupancy_L3", _context.OccupancyGridSizes[1]);
            material.SetVector("_GridSizeOccupancy_L2", _context.OccupancyGridSizes[2]);
            material.SetVector("_GridSizeOccupancy_L1", _context.OccupancyGridSizes[3]);
            material.SetVector("_GridSizeOccupancy_L0", _context.OccupancyGridSizes[4]);
            material.SetInteger("_DisplayMode", 1); // make diffuse default for now, view-dependent not working yet
            material.SetVector("_MinPosition", new Vector4(
                (float)sceneParams.MinX,
                (float)sceneParams.MinY,
                (float)sceneParams.MinZ,
                0f)
            );
            material.SetInteger("_StepMult", 1);

            //if (useTriplane)
            material.SetTexture("_PlaneRgb", _context.PlaneRgbTexture);
            material.SetTexture("_PlaneDensity", _context.PlaneDensityTexture);
            material.SetTexture("_PlaneFeatures", _context.PlaneFeaturesTexture);
            material.SetVector("_PlaneSize", new Vector4(sceneParams.PlaneWidth0, sceneParams.PlaneHeight0, 0, 0));
            material.SetFloat("_VoxelSizeTriplane", (float)sceneParams.VoxelSizeTriplane);
            LocalKeyword useTriplaneKeyword = new LocalKeyword(_context.Shader, "USE_TRIPLANE");
            material.SetKeyword(useTriplaneKeyword, true);

            //if (useSparseGrid)
            material.SetTexture("_SparseGridDensity", _context.DensityVolumeTexture);
            material.SetTexture("_SparseGridRgb", _context.RGBVolumeTexture);
            material.SetTexture("_SparseGridFeatures", _context.FeatureVolumeTexture);
            material.SetTexture("_SparseGridIndex", _context.AtlasIndexTexture);
            material.SetFloat("_BlockSize", sceneParams.BlockSize);
            material.SetFloat("_VoxelSize", (float)sceneParams.VoxelSize);
            material.SetVector("_GridSize", new Vector4(sceneParams.GridWidth, sceneParams.GridHeight, sceneParams.GridDepth, 0));
            material.SetVector("_AtlasSize", new Vector4(sceneParams.AtlasWidth, sceneParams.AtlasHeight, sceneParams.AtlasDepth, 0));
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

        /// <summary>
        /// Creates a convenient prefab for the MERF scene..
        /// </summary>
        private static void CreatePrefab(SceneParams sceneParams) {
            GameObject prefabObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefabObject.transform.localScale = Vector3.one * 1000;
            GameObject.DestroyImmediate(prefabObject.GetComponent<Collider>());
            prefabObject.name = _context.SceneName;
            MeshRenderer renderer = prefabObject.GetComponent<MeshRenderer>();
            string materialAssetPath = MaterialAssetPath;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialAssetPath);
            renderer.material = material;
            PrefabUtility.SaveAsPrefabAsset(prefabObject, PrefabAssetPath);
            GameObject.DestroyImmediate(prefabObject);
        }
    }
}