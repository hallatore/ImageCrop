using Microsoft.Maui.Graphics;

namespace ImageCrop.Core
{
    public class BoundingBoxHelper
    {
        private readonly int _imageWidth;
        private readonly int _imageHeight;
        public Size OutputSize { get; private set; }

        public BoundingBoxHelper(int imageWidth, int imageHeight, Size outputSize) 
        {
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;
            OutputSize = outputSize;
        }

        public Rect GetBoundingBox(int width, int height)
        {
            var boundingBox = new Rect(0, 0, width, height);
            boundingBox = EnsureFit(boundingBox);
            return boundingBox;
        }

        public Rect ScaleBoundingBox(Rect boundingBox, float paddingPercentage)
        {
            var result = boundingBox.Inflate(
                (int) (boundingBox.Width * paddingPercentage),
                (int) (boundingBox.Height * paddingPercentage));

            result = EnsureFit(result);

            return result;
        }

        public Rect EnsureFit(Rect boundingBox)
        {
            boundingBox = EnsureBoundingBoxAspectRatio(boundingBox);
            boundingBox = EnsureBoundingBoxSize(boundingBox);
            return boundingBox;
        }

        private Rect EnsureBoundingBoxAspectRatio(Rect boundingBox)
        {
            var osAspectRatio = (double) OutputSize.Width / OutputSize.Height;
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

        public Rect EnsureBoundingBoxSize(Rect boundingBox)
        {
            if (boundingBox.Width < OutputSize.Width)
            {
                boundingBox = boundingBox.Inflate(
                    (int) (((double) OutputSize.Width - boundingBox.Width) / 2),
                    (int) (((double) OutputSize.Height - boundingBox.Height) / 2));
            }

            // Fix if box is too large
            if (boundingBox.Width > _imageWidth)
            {
                var oldHeight = boundingBox.Height;
                boundingBox.Height = (int) ((double) boundingBox.Height / boundingBox.Width * _imageWidth);
                boundingBox.Y += (oldHeight - boundingBox.Height) / 2;
                boundingBox.Width = _imageWidth;
            }

            if (boundingBox.Height > _imageHeight)
            {
                var oldWidth = boundingBox.Width;
                boundingBox.Width = (int) ((double) boundingBox.Width / boundingBox.Height * _imageHeight);
                boundingBox.X += (oldWidth - boundingBox.Width) / 2;
                boundingBox.Height = _imageHeight;
            }

            // Fix is box is outside image

            if (boundingBox.Bottom > _imageHeight)
            {
                boundingBox = boundingBox.Offset(0, _imageHeight - boundingBox.Bottom);
            }

            if (boundingBox.Top < 0)
            {
                boundingBox = boundingBox.Offset(0, boundingBox.Top * -1);
            }

            if (boundingBox.Right > _imageWidth)
            {
                boundingBox = boundingBox.Offset(_imageWidth - boundingBox.Right, 0);
            }

            if (boundingBox.Left < 0)
            {
                boundingBox = boundingBox.Offset(boundingBox.Left * -1, 0);
            }

            return boundingBox;
        }
    }
}
