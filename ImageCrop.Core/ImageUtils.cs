
using Microsoft.Maui.Graphics;
using SkiaSharp;

namespace ImageCrop.Core;

public static class ImageUtils
{
    public static SKBitmap LoadImageFromStream(this Stream imageStream)
    {
        imageStream.Seek(0, SeekOrigin.Begin);
        using var image = SKImage.FromEncodedData(imageStream);
        return SKBitmap.FromImage(image);
    }

    public static SKBitmap CropAndResize(
        this SKBitmap image,
        Rect boundingBox,
        Size outputSize)
    {
        using var pixmap = new SKPixmap(image.Info, image.GetPixels());
        var rect = new SKRect((int)boundingBox.Left, (int) boundingBox.Top, (int) boundingBox.Right, (int) boundingBox.Bottom);

        image = CropBitmap(rect, image);
        image = image.Resize(new SKSizeI((int) outputSize.Width, (int) outputSize.Height), SKFilterQuality.High);
        return image;
    }

    private static SKBitmap CropBitmap(SKRect cropRect, SKBitmap bitmap)
    {
        var croppedBitmap = new SKBitmap((int) cropRect.Width, (int) cropRect.Height);
        var dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
        var source = new SKRect(cropRect.Left, cropRect.Top, cropRect.Right, cropRect.Bottom);

        using var canvas = new SKCanvas(croppedBitmap);
        canvas.DrawBitmap(bitmap, source, dest);

        return croppedBitmap;
    }

    public static Stream SaveToStream(this SKBitmap image, SKEncodedImageFormat imageFormat = SKEncodedImageFormat.Webp, int quality = 100)
    {
        var ms = new MemoryStream();
        image.Encode(ms, imageFormat, quality);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}