﻿namespace BigGustave
{
    /// <summary>
    /// The method used to compress the image data.
    /// </summary>
    public enum CompressionMethod : byte
    {
        /// <summary>
        /// Deflate/inflate compression with a sliding window of at most 32768 bytes.
        /// </summary>
        DeflateWithSlidingWindow = 0
    }
}