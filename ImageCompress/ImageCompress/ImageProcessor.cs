using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace ImageCompress;

/// <summary>
/// Image Handler
/// </summary>
public class ImageProcessor
{
    /// <summary>
    /// Input image for compressing
    /// </summary>
    /// <param name="file">File path with name</param>
    /// <param name="resultSize">Expected result size (KB)</param>
    /// <returns>Image data in bytes</returns>
    public static byte[] Compress(string file, int resultSize)
    {
        var data = File.ReadAllBytes(file);

        if (data.Length / 1024 < resultSize)
            return data;

        var compressedData = Array.Empty<byte>();
        var encoder = GetEncoder(ImageFormat.Jpeg);
        var quality = Encoder.Quality;
        var encoderParams = new EncoderParameters(1);

        using (var memoryStream1 = new MemoryStream(data))
        {
            var image = Image.FromStream(memoryStream1);

            if (encoder == null)
                using (var memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, ImageFormat.Jpeg);

                    return memoryStream2.ToArray();
                }

            var minQuality = 0;
            var maxQuality = 50;

            while (minQuality <= maxQuality)
            {
                var currentQuality = (minQuality + maxQuality) / 2;
                encoderParams.Param[0] = new EncoderParameter(quality, currentQuality);

                using (var memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, encoder, encoderParams);
                    compressedData = memoryStream2.ToArray();

                    if (compressedData.Length / 1024 < resultSize)
                        minQuality = currentQuality + 1;
                    else if (compressedData.Length / 1024 > resultSize)
                        maxQuality = currentQuality - 1;
                    else
                        return compressedData;
                }
            }

            return compressedData;
        }
    }

    /// <summary>
    /// Get image format encoding
    /// </summary>
    /// <param name="format">image format</param>
    /// <returns>image codec info</returns>
    private static ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        return ImageCodecInfo.GetImageDecoders()
                             .FirstOrDefault((Func<ImageCodecInfo?, bool>) (codec => codec?.FormatID == format.Guid));
    }

    /// <summary>
    /// Resize image
    /// </summary>
    /// <param name="file">File path with name</param>
    /// <param name="width">Expected width</param>
    /// <param name="height">Expected heidht</param>
    /// <returns>Image data in bytes</returns>
    public static byte[] ResizeImage(string file, int width, int height)
    {
        var image = Image.FromFile(file);
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }
        var converter = new ImageConverter();

        return (byte[])converter.ConvertTo(destImage, typeof(byte[]))!;
    }
}