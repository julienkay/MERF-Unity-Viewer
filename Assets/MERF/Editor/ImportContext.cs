using UnityEngine;

/// <summary>
/// Carries information for all the assets created during import of a MERF scene.
/// </summary>
public class ImportContext {
    public Texture2DArray PlaneRgbTexture;
    public Texture2DArray PlaneDensityTexture;
    public Texture2DArray PlaneFeaturesTexture;
    public Texture3D[] OccupancyGridTextures;
    public Vector4[] OccupancyGridSizes;
    public double[] OccupancyVoxelSizes;
    public Shader Shader;
    public Texture2D WeightsTexZero;
    public Texture2D WeightsTexOne;
    public Texture2D WeightsTexTwo;
    public Material Material;
    public Texture3D DensityVolumeTexture;
    public Texture3D RGBVolumeTexture;
    public Texture3D FeatureVolumeTexture;
    public Texture3D AtlasIndexTexture;
}
