using Newtonsoft.Json;

public class SceneParams {
    [JsonProperty("voxel_size")]
    public double VoxelSize { get; set; }

    [JsonProperty("block_size")]
    public long BlockSize { get; set; }

    [JsonProperty("grid_width")]
    public long GridWidth { get; set; }

    [JsonProperty("grid_height")]
    public long GridHeight { get; set; }

    [JsonProperty("grid_depth")]
    public long GridDepth { get; set; }

    [JsonProperty("atlas_width")]
    public long AtlasWidth { get; set; }

    [JsonProperty("atlas_height")]
    public long AtlasHeight { get; set; }

    [JsonProperty("atlas_depth")]
    public long AtlasDepth { get; set; }

    [JsonProperty("num_slices")]
    public long NumSlices { get; set; }

    [JsonProperty("slice_depth")]
    public long SliceDepth { get; set; }

    [JsonProperty("atlas_blocks_x")]
    public long AtlasBlocksX { get; set; }

    [JsonProperty("atlas_blocks_y")]
    public long AtlasBlocksY { get; set; }

    [JsonProperty("atlas_blocks_z")]
    public long AtlasBlocksZ { get; set; }

    [JsonProperty("min_x")]
    public long MinX { get; set; }

    [JsonProperty("min_y")]
    public long MinY { get; set; }

    [JsonProperty("min_z")]
    public long MinZ { get; set; }

    [JsonProperty("worldspace_T_opengl")]
    public long[][] WorldspaceTOpengl { get; set; }

    [JsonProperty("0_weights")]
    public double[][] The0_Weights { get; set; }

    [JsonProperty("1_weights")]
    public double[][] The1_Weights { get; set; }

    [JsonProperty("2_weights")]
    public double[][] The2_Weights { get; set; }

    [JsonProperty("0_bias")]
    public double[] The0_Bias { get; set; }

    [JsonProperty("1_bias")]
    public double[] The1_Bias { get; set; }

    [JsonProperty("2_bias")]
    public double[] The2_Bias { get; set; }

    [JsonProperty("voxel_size_triplane")]
    public double VoxelSizeTriplane { get; set; }

    [JsonProperty("plane_width_0")]
    public long PlaneWidth0 { get; set; }

    [JsonProperty("plane_height_0")]
    public long PlaneHeight0 { get; set; }

    [JsonProperty("plane_width_1")]
    public long PlaneWidth1 { get; set; }

    [JsonProperty("plane_height_1")]
    public long PlaneHeight1 { get; set; }

    [JsonProperty("plane_width_2")]
    public long PlaneWidth2 { get; set; }

    [JsonProperty("plane_height_2")]
    public long PlaneHeight2 { get; set; }

    [JsonProperty("format")]
    public string Format { get; set; }
}