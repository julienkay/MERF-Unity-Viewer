using Newtonsoft.Json;

public class SceneParams {
    [JsonProperty("voxel_size")]
    public double VoxelSize { get; set; }

    [JsonProperty("block_size")]
    public int BlockSize { get; set; }

    [JsonProperty("grid_width")]
    public int GridWidth { get; set; }

    [JsonProperty("grid_height")]
    public int GridHeight { get; set; }

    [JsonProperty("grid_depth")]
    public int GridDepth { get; set; }

    [JsonProperty("atlas_width")]
    public int AtlasWidth { get; set; }

    [JsonProperty("atlas_height")]
    public int AtlasHeight { get; set; }

    [JsonProperty("atlas_depth")]
    public int AtlasDepth { get; set; }

    [JsonProperty("num_slices")]
    public int NumSlices { get; set; }

    [JsonProperty("slice_depth")]
    public int SliceDepth { get; set; }

    [JsonProperty("atlas_blocks_x")]
    public int AtlasBlocksX { get; set; }

    [JsonProperty("atlas_blocks_y")]
    public int AtlasBlocksY { get; set; }

    [JsonProperty("atlas_blocks_z")]
    public int AtlasBlocksZ { get; set; }

    [JsonProperty("min_x")]
    public float MinX { get; set; }

    [JsonProperty("min_y")]
    public float MinY { get; set; }

    [JsonProperty("min_z")]
    public float MinZ { get; set; }

    [JsonProperty("worldspace_T_opengl")]
    public float[][] WorldspaceTOpengl { get; set; }

    [JsonProperty("0_weights")]
    public double[][] _0Weights { get; set; }

    [JsonProperty("1_weights")]
    public double[][] _1Weights { get; set; }

    [JsonProperty("2_weights")]
    public double[][] _2Weights { get; set; }

    [JsonProperty("0_bias")]
    public double[] _0Bias { get; set; }

    [JsonProperty("1_bias")]
    public double[] _1Bias { get; set; }

    [JsonProperty("2_bias")]
    public double[] _2Bias { get; set; }

    [JsonProperty("voxel_size_triplane")]
    public double VoxelSizeTriplane { get; set; }

    [JsonProperty("plane_width_0")]
    public int PlaneWidth0 { get; set; }

    [JsonProperty("plane_height_0")]
    public int PlaneHeight0 { get; set; }

    [JsonProperty("plane_width_1")]
    public int PlaneWidth1 { get; set; }

    [JsonProperty("plane_height_1")]
    public int PlaneHeight1 { get; set; }

    [JsonProperty("plane_width_2")]
    public int PlaneWidth2 { get; set; }

    [JsonProperty("plane_height_2")]
    public int PlaneHeight2 { get; set; }

    [JsonProperty("format")]
    public string Format { get; set; }
}