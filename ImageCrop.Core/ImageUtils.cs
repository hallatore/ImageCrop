
using Microsoft.Maui.Graphics;
using SkiaSharp;

namespace ImageCrop.Core;

public class ImageUtils
{
    public static SKBitmap LoadFromStream(Stream imageStream)
    {
        imageStream.Seek(0, SeekOrigin.Begin);
        using var image = SKImage.FromEncodedData(imageStream);
        return SKBitmap.FromImage(image);
    }

    public static SKBitmap CropAndResize(
        SKBitmap image,
        Rect boundingBox,
        Size outputSize)
    {
        using var pixmap = new SKPixmap(image.Info, image.GetPixels());
        var rect = new SKRect((int)boundingBox.Left, (int) boundingBox.Top, (int) boundingBox.Right, (int) boundingBox.Bottom);

        image = CropBitmap(rect, image);
        //var subset = pixmap.ExtractSubset(rectI);
        //var bitmap = new SKBitmap(subset.Info);
        //pixmap.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes);

        image = image.Resize(new SKSizeI((int) outputSize.Width, (int) outputSize.Height), SKFilterQuality.High);

        return image;
    }

    private static SKBitmap CropBitmap(SKRect cropRect, SKBitmap bitmap)
    {
        var croppedBitmap = new SKBitmap((int) cropRect.Width,
                                                (int) cropRect.Height);
        var dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
        var source = new SKRect(cropRect.Left, cropRect.Top,
                                    cropRect.Right, cropRect.Bottom);

        using (var canvas = new SKCanvas(croppedBitmap))
        {
            canvas.DrawBitmap(bitmap, source, dest);
        }

        return croppedBitmap;
    }

    public static Stream SaveToStream(SKBitmap image)
    {
        var ms = new MemoryStream();
        image.Encode(ms, SKEncodedImageFormat.Webp, 100);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    //public static float GetOverlapPercentage(Rectangle a, Rectangle b)
    //{
    //    var intersecting_area =
    //        (float) Math.Max(0, Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left)) *
    //        Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));

    //    var percent_coverage = intersecting_area /
    //                           ((float) a.Height * a.Width + (float) b.Height * b.Width - intersecting_area);

    //    return percent_coverage;
    //}
}