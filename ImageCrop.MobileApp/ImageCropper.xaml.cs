using ImageCrop.Core;

namespace ImageCrop.MobileApp;

public partial class ImageCropper : ContentView
{
    private Rect _boundingBox;
    private readonly ImageSource _imageSource;
    private readonly Size _imageSize;

    private BoundingBoxHelper _boundingBoxHelper;

    public ImageCropper(Size originalImageSize, ImageSource previewImageSource)
    {
        InitializeComponent();
        _imageSource = previewImageSource;
        _imageSize = originalImageSize;
        _boundingBoxHelper = new BoundingBoxHelper((int) _imageSize.Width, (int) _imageSize.Height, new Size(1024, 1920));
        _boundingBox = _boundingBoxHelper.GetBoundingBox((int) _imageSize.Width, (int) _imageSize.Height);

        Loaded += ImageCropper_Loaded;
    }

    private void ImageCropper_Loaded(object sender, EventArgs e)
    {
        PreviewImage.Source = _imageSource;
        AdjustSize();
        AdjustImage();
    }

    private void ContentView_Changed(object sender, EventArgs e)
    {
        AdjustSize();
        AdjustImage();
    }

    public void UpdateOutputSize(Size outputSize)
    {
        AdjustSize();
        AdjustImage();
    }

    public Rect GetBoundingBox()
    {
        return _boundingBox;
    }

    public Size GetOutputSize()
    {
        return _boundingBoxHelper.OutputSize;
    }

    public void Zoom(float scaleFactor)
    {
        _boundingBox = _boundingBoxHelper.ScaleBoundingBox(_boundingBox, scaleFactor);
        AdjustSize();
        AdjustImage();
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        Zoom(-0.05f);
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        Zoom(0.05f);
    }

    private double tempPanOffsetX;
    private double tempPanOffsetY;

    public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
        {
            tempPanOffsetX = 0;
            tempPanOffsetY = 0;
        }

        if (e.StatusType == GestureStatus.Running || e.StatusType == GestureStatus.Started)
        {
            var updatedX = e.TotalX - tempPanOffsetX;
            var updatedY = e.TotalY - tempPanOffsetY;

            var ratio = AbsoluteLayoutContainer.Width / _boundingBox.Width;
            var tempX = updatedX / ratio;
            var tempY = updatedY / ratio;

            _boundingBox = _boundingBox.Offset((int)tempX * -1, (int)tempY * -1);
            _boundingBox = _boundingBoxHelper.EnsureFit(_boundingBox);
            AdjustImage();

            tempPanOffsetX = e.TotalX;
            tempPanOffsetY = e.TotalY;
        }
    }

    private void AdjustSize()
    {
        var ratio = GetViewSizeRatio();

        var newWidth = _boundingBoxHelper.OutputSize.Width * ratio;
        var newHeight = _boundingBoxHelper.OutputSize.Height * ratio;

        if (newHeight <= 0 || newWidth <= 0)
        {
            return;
        }

        AbsoluteLayoutContainer.WidthRequest = newWidth;
        AbsoluteLayoutContainer.HeightRequest = newHeight;
    }

    private double GetViewSizeRatio()
    {
        var ratioX = Width / _boundingBoxHelper.OutputSize.Width;
        var ratioY = Height / _boundingBoxHelper.OutputSize.Height;
        var ratio = ratioX < ratioY ? ratioX : ratioY;
        return ratio;
    }

    private void AdjustImage()
    {
        if (_boundingBox == Rect.Zero)
        {
            return;
        }

        var rect = new Rect(
            1.0 / (_imageSize.Width - _boundingBox.Width) * _boundingBox.Left,
            1.0 / (_imageSize.Height - _boundingBox.Height) * _boundingBox.Top,
            (double)_imageSize.Width / _boundingBox.Width * AbsoluteLayoutContainer.Width,
            (double)_imageSize.Height / _boundingBox.Height * AbsoluteLayoutContainer.Height);

        if (double.IsNaN(rect.X))
        {
            rect.X = 0;
        }

        if (double.IsNaN(rect.Y))
        {
            rect.Y = 0;
        }

        AbsoluteLayout.SetLayoutBounds(PreviewImage, rect);
    }
}