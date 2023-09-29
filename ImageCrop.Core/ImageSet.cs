using Microsoft.Maui.Graphics;

namespace ImageCrop.Core;

public class ImageSet : IDisposable
{
    public MemoryStream LoadedImage { get; set; }
    public Size LoadedImageSize { get; set; }
    public string FileName { get; set; }
    public List<Rect> BoundingBoxes { get; set; }
    public Size OutputSize { get; set; }

    public ImageSet(
        Stream imageStream,
        Size size,
        string fileName,
        Size outputSize)
    {
        LoadedImage = new MemoryStream();
        imageStream.Seek(0, SeekOrigin.Begin);
        imageStream.CopyTo(LoadedImage);
        LoadedImageSize = size;
        FileName = fileName;
        OutputSize = outputSize;
        BoundingBoxes = new List<Rect>();
    }

    public void GenerateBoundingBoxes()
    {
        BoundingBoxes = new List<Rect>();

        var originalImageBoundingBox = new Rect(0, 0, LoadedImageSize.Width, LoadedImageSize.Height);
        originalImageBoundingBox = EnsureBoundingBoxAspectRatio(originalImageBoundingBox, OutputSize);
        originalImageBoundingBox = EnsureBoundingBoxSize(originalImageBoundingBox, LoadedImageSize, OutputSize);
        BoundingBoxes.Add(originalImageBoundingBox);

        //BoundingBoxes.AddRange(GenerateBoundingBoxesFromEntropy(LoadedImage, OutputSize));
        //BoundingBoxes.AddRange(GenerateBoundingBoxesFromEdges(LoadedImage, OutputSize));
        //BoundingBoxes.AddRange(GenerateBoundingBoxesFromSmartcrop(LoadedImage, OutputSize));
    }

    //private static List<Rect> GenerateBoundingBoxesFromSmartcrop(Image<Rgba32> image, Size outputSize)
    //{
    //    var downSampleSize = 300;
    //    var boundingBoxes = new List<Rect>();

    //    var widthIsLargerThanHeight = image.Width > image.Height;
    //    var downSampledImage = image.Clone(
    //        img => img.Resize(
    //            widthIsLargerThanHeight ? downSampleSize : 0,
    //            !widthIsLargerThanHeight ? downSampleSize : 0));

    //    var stream = ImageUtils.SaveAsJpegStream(downSampledImage);

    //    var smartCrop = new Smartcrop.ImageCrop(outputSize.Width, outputSize.Height);
    //    smartCrop.Options.Prescale = false;
    //    var result = smartCrop.Crop(stream);
    //    boundingBoxes.Add(new Rectangle(result.Area.X, result.Area.Y, result.Area.Width, result.Area.Height));

    //    var rectangleScaleFactor = 1.0 / Math.Max(image.Width, image.Height) * downSampleSize;
    //    boundingBoxes = ResizeRectangles(boundingBoxes, rectangleScaleFactor);

    //    return boundingBoxes;
    //}

    //private static List<Rect> GenerateBoundingBoxesFromEntropy(Image<Rgba32> image, Size outputSize)
    //{
    //    var downSampleSize = 300;
    //    var maxNumberOfEdges = 10;
    //    var boundingBoxPaddingPercentage = 0.2f;
    //    var maxBoundingBoxOverlapPercentage = 0.7f;
    //    var boundingBoxes = new List<Rectangle>();

    //    var widthIsLargerThanHeight = image.Width > image.Height;
    //    var downSampledImage = image.Clone(
    //        img => img.Resize(
    //            widthIsLargerThanHeight ? downSampleSize : 0,
    //            !widthIsLargerThanHeight ? downSampleSize : 0));

    //    using (var temp = downSampledImage.Clone())
    //    {
    //        var configuration = image.GetConfiguration();

    //        // Detect the edges.
    //        new EdgeDetector2DProcessor(KnownEdgeDetectorKernels.Sobel, true)
    //            .CreatePixelSpecificProcessor(configuration, temp, temp.Bounds())
    //            .Execute();

    //        // Apply threshold binarization filter.
    //        new BinaryThresholdProcessor(0.5f)
    //            .CreatePixelSpecificProcessor(configuration, temp, temp.Bounds())
    //            .Execute();

    //        // Search for the first white pixels
    //        var rectangle = GetFilteredBoundingRectangle(temp.Frames.RootFrame, 0);

    //        boundingBoxes.Add(rectangle);
    //    }

    //    var rectangleScaleFactor = 1.0 / Math.Max(image.Width, image.Height) * downSampleSize;
    //    boundingBoxes = ResizeRectangles(boundingBoxes, rectangleScaleFactor);

    //    return boundingBoxes;
    //}

    /// <summary>
    /// Finds the bounding rectangle based on the first instance of any color component other
    /// than the given one.
    /// </summary>
    /// <param name="bitmap">The <see cref="Image{TPixel}"/> to search within.</param>
    /// <param name="componentValue">The color component value to remove.</param>
    /// <param name="channel">The <see cref="RgbaComponent"/> channel to test against.</param>
    /// <returns>
    /// The <see cref="Rectangle"/>.
    /// </returns>
    //private static Rect GetFilteredBoundingRectangle(
    //    ImageFrame<Rgba32> bitmap,
    //    float componentValue,
    //    int channel = 3)
    //{
    //    var epsilon = 0.001F;
    //    var width = bitmap.Width;
    //    var height = bitmap.Height;
    //    Point topLeft = default;
    //    Point bottomRight = default;

    //    Func<ImageFrame<Rgba32>, int, int, float, bool> delegateFunc;

    //    // Determine which channel to check against
    //    switch (channel)
    //    {
    //        case 1:
    //            delegateFunc = (
    //                pixels,
    //                x,
    //                y,
    //                b) => MathF.Abs(pixels[x, y].ToVector4().X - b) > epsilon;

    //            break;

    //        case 2:
    //            delegateFunc = (
    //                pixels,
    //                x,
    //                y,
    //                b) => MathF.Abs(pixels[x, y].ToVector4().Y - b) > epsilon;

    //            break;

    //        case 3:
    //            delegateFunc = (
    //                pixels,
    //                x,
    //                y,
    //                b) => MathF.Abs(pixels[x, y].ToVector4().Z - b) > epsilon;

    //            break;

    //        default:
    //            delegateFunc = (
    //                pixels,
    //                x,
    //                y,
    //                b) => MathF.Abs(pixels[x, y].ToVector4().W - b) > epsilon;

    //            break;
    //    }

    //    int GetMinY(ImageFrame<Rgba32> pixels)
    //    {
    //        for (var y = 0; y < height; y++)
    //        {
    //            for (var x = 0; x < width; x++)
    //            {
    //                if (delegateFunc(pixels, x, y, componentValue))
    //                {
    //                    return y;
    //                }
    //            }
    //        }

    //        return 0;
    //    }

    //    int GetMaxY(ImageFrame<Rgba32> pixels)
    //    {
    //        for (var y = height - 1; y > -1; y--)
    //        {
    //            for (var x = 0; x < width; x++)
    //            {
    //                if (delegateFunc(pixels, x, y, componentValue))
    //                {
    //                    return y;
    //                }
    //            }
    //        }

    //        return height;
    //    }

    //    int GetMinX(ImageFrame<Rgba32> pixels)
    //    {
    //        for (var x = 0; x < width; x++)
    //        {
    //            for (var y = 0; y < height; y++)
    //            {
    //                if (delegateFunc(pixels, x, y, componentValue))
    //                {
    //                    return x;
    //                }
    //            }
    //        }

    //        return 0;
    //    }

    //    int GetMaxX(ImageFrame<Rgba32> pixels)
    //    {
    //        for (var x = width - 1; x > -1; x--)
    //        {
    //            for (var y = 0; y < height; y++)
    //            {
    //                if (delegateFunc(pixels, x, y, componentValue))
    //                {
    //                    return x;
    //                }
    //            }
    //        }

    //        return width;
    //    }

    //    topLeft.Y = GetMinY(bitmap);
    //    topLeft.X = GetMinX(bitmap);
    //    bottomRight.Y = Clamp(GetMaxY(bitmap) + 1, 0, height);
    //    bottomRight.X = Clamp(GetMaxX(bitmap) + 1, 0, width);

    //    return GetBoundingRectangle(topLeft, bottomRight);
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static int Clamp(
    //    int value,
    //    int min,
    //    int max)
    //{
    //    if (value > max)
    //    {
    //        return max;
    //    }

    //    if (value < min)
    //    {
    //        return min;
    //    }

    //    return value;
    //}

    //private static Rect GetBoundingRectangle(Point topLeft, Point bottomRight)
    //{
    //    return new Rect(
    //        topLeft.X,
    //        topLeft.Y,
    //        bottomRight.X - topLeft.X,
    //        bottomRight.Y - topLeft.Y);
    //}

    //private static List<Rect> GenerateBoundingBoxesFromEdges(Image<Rgba32> image, Size outputSize)
    //{
    //    var downSampleSize = 300;
    //    var maxNumberOfEdges = 10;
    //    var boundingBoxPaddingPercentage = 0.2f;
    //    var maxBoundingBoxOverlapPercentage = 0.7f;
    //    var boundingBoxes = new List<Rect>();

    //    var widthIsLargerThanHeight = image.Width > image.Height;
    //    var downSampledImage = image.Clone(
    //        img => img.Resize(
    //            widthIsLargerThanHeight ? downSampleSize : 0,
    //            !widthIsLargerThanHeight ? downSampleSize : 0));

    //    var edgesImage = downSampledImage.Clone(
    //        img => img
    //            .DetectEdges(EdgeDetector2DKernel.SobelKernel, true)
    //            .Brightness(0.5f)
    //            .Contrast(10000.0f)
    //    );

    //    edgesImage = FilterOutEdges(edgesImage, maxNumberOfEdges * 5, out _, 2);
    //    edgesImage = FilterOutEdges(edgesImage, maxNumberOfEdges, out var edgeGroups, 10);

    //    var random = new Random(42);

    //    if (!edgeGroups.Any())
    //    {
    //        return boundingBoxes;
    //    }

    //    var rectangleScaleFactor = 1.0 / Math.Max(image.Width, image.Height) * downSampleSize;
    //    edgeGroups = ResizeRectangles(edgeGroups, rectangleScaleFactor);

    //    for (var i = 0; i < edgeGroups.Count; i++)
    //    for (var j = 0; j < edgeGroups.Count; j++)
    //    {
    //        var rectangleCombo = new List<Rect>
    //        {
    //            edgeGroups[i],
    //            edgeGroups[j]
    //        };

    //        var boundingBox = GetBoundingBox(image.Size(), rectangleCombo, outputSize, boundingBoxPaddingPercentage);

    //        if (boundingBox == Rectangle.Empty ||
    //            boundingBoxes.Contains(boundingBox) ||
    //            boundingBoxes.Any(
    //                box => ImageUtils.GetOverlapPercentage(boundingBox, box) > maxBoundingBoxOverlapPercentage))
    //        {
    //            continue;
    //        }

    //        boundingBoxes.Add(boundingBox);
    //    }

    //    for (var i = 0; i < 100; i++)
    //    {
    //        var boundingBox = GetRandomBoundingBox(
    //            image.Size(),
    //            edgeGroups,
    //            outputSize,
    //            random,
    //            boundingBoxPaddingPercentage);

    //        if (boundingBox == Rect.Zero ||
    //            boundingBoxes.Contains(boundingBox) ||
    //            boundingBoxes.Any(
    //                box => ImageUtils.GetOverlapPercentage(boundingBox, box) > maxBoundingBoxOverlapPercentage))
    //        {
    //            continue;
    //        }

    //        boundingBoxes.Add(boundingBox);
    //    }

    //    return boundingBoxes;
    //}

    //private static List<Rect> ResizeRectangles(List<Rect> rectangles, double scaleFactor)
    //{
    //    return rectangles
    //        .Select(
    //            r => new Rect(
    //                (int) (r.X / scaleFactor),
    //                (int) (r.Y / scaleFactor),
    //                (int) (r.Width / scaleFactor),
    //                (int) (r.Height / scaleFactor)
    //            ))
    //        .ToList();
    //}

    //private static Rect GetBoundingBox(
    //    Size imageSize,
    //    List<Rect> rectangles,
    //    Size outputSize,
    //    float paddingPercentage)
    //{
    //    var boundingBox = CombineBoundingBoxes(rectangles);
    //    ScaleBoundingBox(ref boundingBox, paddingPercentage);
    //    EnsureBoundingBoxAspectRatio(ref boundingBox, outputSize);
    //    EnsureBoundingBoxSize(ref boundingBox, imageSize, outputSize);
    //    return boundingBox;
    //}

    public static Rect ScaleBoundingBox(Rect boundingBox, float paddingPercentage)
    {
        return boundingBox.Inflate(
            (int) (boundingBox.Width * paddingPercentage),
            (int) (boundingBox.Height * paddingPercentage));
    }

    //private static Rect GetRandomBoundingBox(
    //    Size imageSize,
    //    List<Rect> rectangles,
    //    Size outputSize,
    //    Random random,
    //    float paddingPercentage)
    //{
    //    var rectangleChances = random.Next();
    //    var filteredRectangles = rectangles
    //        .Where(r => random.Next() >= rectangleChances)
    //        .ToList();

    //    if (!filteredRectangles.Any())
    //    {
    //        return Rect.Zero;
    //    }

    //    var boundingBox = CombineBoundingBoxes(filteredRectangles);
    //    boundingBox.Inflate(
    //        (int) (boundingBox.Width * paddingPercentage),
    //        (int) (boundingBox.Height * paddingPercentage));

    //    EnsureBoundingBoxAspectRatio(ref boundingBox, outputSize);
    //    EnsureBoundingBoxSize(ref boundingBox, imageSize, outputSize);
    //    return boundingBox;
    //}

    private static Rect EnsureBoundingBoxAspectRatio(Rect boundingBox, Size outputSize)
    {
        var osAspectRatio = (double) outputSize.Width / outputSize.Height;
        var bbAspectRatio = (double) boundingBox.Width / boundingBox.Height;

        // Fix aspect ratio
        if (bbAspectRatio > osAspectRatio)
        {
            var height = boundingBox.Width / osAspectRatio;
            return boundingBox.Inflate(0, (int) ((height - boundingBox.Height) / 2));
        }
        else
        {
            var width = boundingBox.Height * osAspectRatio;
            return boundingBox.Inflate((int) ((width - boundingBox.Width) / 2), 0);
        }
    }

    public static Rect EnsureBoundingBoxSize(
        Rect boundingBox,
        Size imageSize,
        Size outputSize)
    {
        if (boundingBox.Width < outputSize.Width)
        {
            boundingBox = boundingBox.Inflate(
                (int) (((double) outputSize.Width - boundingBox.Width) / 2),
                (int) (((double) outputSize.Height - boundingBox.Height) / 2));
        }

        // Fix if box is too large
        if (boundingBox.Width > imageSize.Width)
        {
            var oldHeight = boundingBox.Height;
            boundingBox.Height = (int) ((double) boundingBox.Height / boundingBox.Width * imageSize.Width);
            boundingBox.Y += (oldHeight - boundingBox.Height) / 2;
            boundingBox.Width = imageSize.Width;
        }

        if (boundingBox.Height > imageSize.Height)
        {
            var oldWidth = boundingBox.Width;
            boundingBox.Width = (int) ((double) boundingBox.Width / boundingBox.Height * imageSize.Height);
            boundingBox.X += (oldWidth - boundingBox.Width) / 2;
            boundingBox.Height = imageSize.Height;
        }

        // Fix is box is outside image

        if (boundingBox.Bottom > imageSize.Height)
        {
            boundingBox = boundingBox.Offset(0, imageSize.Height - boundingBox.Bottom);
        }

        if (boundingBox.Top < 0)
        {
            boundingBox = boundingBox.Offset(0, boundingBox.Top * -1);
        }

        if (boundingBox.Right > imageSize.Width)
        {
            boundingBox = boundingBox.Offset(imageSize.Width - boundingBox.Right, 0);
        }

        if (boundingBox.Left < 0)
        {
            boundingBox = boundingBox.Offset(boundingBox.Left * -1, 0);
        }

        return boundingBox;
    }

    //private static Rect CombineBoundingBoxes(List<Rect> rectangles)
    //{
    //    var yMin = rectangles.Min(p => p.Top);
    //    var yMax = rectangles.Max(p => p.Bottom);
    //    var xMin = rectangles.Min(p => p.Left);
    //    var xMax = rectangles.Max(p => p.Right);
    //    return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    //}

    //private static Image<Rgba32> FilterOutEdges(
    //    Image<Rgba32> image,
    //    int maxNumberOfEdges,
    //    out List<Rectangle> rectangles,
    //    int pixelGroupSize)
    //{
    //    var groupCount = 0;
    //    var pixelGroups = new Dictionary<int, List<(int y, int x)>>();
    //    var pixels = new Dictionary<(int y, int x), int>();

    //    image.ProcessPixelRows(
    //        accessor =>
    //        {
    //            for (var y = 0; y < accessor.Height; y++)
    //            {
    //                var pixelRow = accessor.GetRowSpan(y);

    //                for (var x = 0; x < pixelRow.Length; x++)
    //                {
    //                    ref var pixel = ref pixelRow[x];

    //                    if (!(pixel.R > 100 || pixel.G > 100 || pixel.B > 100))
    //                    {
    //                        continue;
    //                    }

    //                    var pixelGroup = TryGetPixelGroup(pixelGroupSize, pixels, y, x);

    //                    if (pixelGroup == null)
    //                    {
    //                        pixelGroup = ++groupCount;
    //                        pixelGroups.Add(pixelGroup.Value, new List<(int y, int x)>());
    //                    }

    //                    pixelGroups[pixelGroup.Value].Add((y, x));
    //                    pixels[(y, x)] = pixelGroup.Value;
    //                }
    //            }
    //        });

    //    var resultImage = new Image<Rgba32>(image.Width, image.Height);

    //    var selectedGroups = pixelGroups
    //        .OrderByDescending(pg => pg.Value.Count)
    //        .Take(maxNumberOfEdges)
    //        .ToList();

    //    var filteredPixels = selectedGroups
    //        .SelectMany(pg => pg.Value)
    //        .ToHashSet();

    //    rectangles = selectedGroups
    //        .Select(
    //            pg =>
    //            {
    //                var yMin = pg.Value.Min(p => p.y);
    //                var yMax = pg.Value.Max(p => p.y);
    //                var xMin = pg.Value.Min(p => p.x);
    //                var xMax = pg.Value.Max(p => p.x);
    //                return new Rectangle(xMin, yMin, xMax - xMin, yMax - yMin);
    //            })
    //        .ToList();

    //    resultImage.ProcessPixelRows(
    //        accessor =>
    //        {
    //            for (var y = 0; y < accessor.Height; y++)
    //            {
    //                var pixelRow = accessor.GetRowSpan(y);

    //                for (var x = 0; x < pixelRow.Length; x++)
    //                {
    //                    ref var pixel = ref pixelRow[x];

    //                    if (filteredPixels.Contains((y, x)))
    //                    {
    //                        var pixelGroupId = pixels[(y, x)];
    //                        pixel.Rgb = GetColor(pixelGroupId);
    //                    }
    //                }
    //            }
    //        });

    //    return resultImage;
    //}

    //private static Rgb24 GetColor(int pixelGroupId)
    //{
    //    switch (pixelGroupId % 7)
    //    {
    //        case 1:
    //            return new Rgb24(0, 255, 255);
    //        case 2:
    //            return new Rgb24(255, 0, 255);
    //        case 3:
    //            return new Rgb24(255, 255, 0);
    //        case 4:
    //            return new Rgb24(255, 0, 0);
    //        case 5:
    //            return new Rgb24(0, 255, 0);
    //        case 6:
    //            return new Rgb24(0, 0, 255);
    //        default:
    //            return new Rgb24(255, 255, 255);
    //    }
    //}

    //private static int? TryGetPixelGroup(
    //    int maxOffset,
    //    Dictionary<(int y, int x), int> pixels,
    //    int y,
    //    int x)
    //{
    //    var groups = new List<(int y, int x)>();

    //    for (var yOffset = maxOffset * -1; yOffset <= 0; yOffset++)
    //    for (var xOffset = maxOffset * -1; xOffset <= maxOffset; xOffset++)
    //    {
    //        if (yOffset == 0 && xOffset >= 0)
    //        {
    //            continue;
    //        }

    //        groups.Add((yOffset, xOffset));
    //    }

    //    groups = groups
    //        .OrderBy(item => Math.Abs(item.y) + Math.Abs(item.x))
    //        .ToList();

    //    foreach (var (yOffset, xOffset) in groups)
    //    {
    //        if (pixels.ContainsKey((y + yOffset, x + xOffset)))
    //        {
    //            return pixels[(y + yOffset, x + xOffset)];
    //        }
    //    }

    //    return null;
    //}

    public void Dispose()
    {
        LoadedImage.Dispose();
    }
}