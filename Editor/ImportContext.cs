using System.IO;
using UnityEngine;

namespace MERF.Editor {

    /// <summary>
    /// Carries information for all the assets created during import of a MERF scene.
    /// </summary>
    public class ImportContext {

        /// <summary>
        /// True if we are we currently importing a custom scene,
        /// false if it is one of the demo scenes.
        /// </summary>
        public bool CustomScene;

        /// <summary>
        /// The demo scene being imported.
        /// </summary>
        public MERFScene Scene;

        /// <summary>
        /// The path to the source files for custom scene imports.
        /// </summary>
        public string CustomScenePath;

        public string SceneName {
            get {
                if (CustomScene) {
                    return new DirectoryInfo(CustomScenePath).Name.ToLower();
                } else {
                    return Scene.LowerCaseName();
                }

            }
        }

        public string SceneNameUpperCase {
            get {
                if (CustomScene) {
                    return new DirectoryInfo(CustomScenePath).Name;
                } else {
                    return Scene.Name();
                }

            }
        }

        public Texture2DArray PlaneRgbTexture;
        public Texture2DArray PlaneDensityTexture;
        public Texture2DArray PlaneFeaturesTexture;
        public Texture3D[] OccupancyGridTextures;
        public Vector4[] OccupancyGridSizes;
        public double[] OccupancyVoxelSizes;
        public Shader Shader;
        public Material Material;
        public Texture3D DensityVolumeTexture;
        public Texture3D RGBVolumeTexture;
        public Texture3D FeatureVolumeTexture;
        public Texture3D AtlasIndexTexture;
    }
}