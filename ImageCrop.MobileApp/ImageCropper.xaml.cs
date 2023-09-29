using ImageCrop.Core;

namespace ImageCrop.MobileApp;

public partial class ImageCropper : ContentView
{
    private Rect _boundingBox;
    private readonly ImageSource _imageSource;
    private readonly Size _imageSize;
    private Size _outputSize;

    public ImageCropper(ImageSource imageSource, Size imageSize, Size outputSize)
    {
        InitializeComponent();
        _imageSource = imageSource;
        _imageSize = imageSize;
        _outputSize = outputSize;
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
        _outputSize = outputSize;
        AdjustSize();
        AdjustImage();
    }

    public void SetBoundingBox(Rect boundingBox)
    {
        _boundingBox = boundingBox;
        AdjustImage();
    }

    public Rect GetBoundingBox()
    {
        return _boundingBox;
    }

    public void Zoom(float scaleFactor)
    {
        _boundingBox = ImageSet.ScaleBoundingBox(_boundingBox, scaleFactor);
        _boundingBox = ImageSet.EnsureBoundingBoxSize(_boundingBox, _imageSize, _outputSize);
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
            _boundingBox = ImageSet.EnsureBoundingBoxSize(_boundingBox, _imageSize, _outputSize);
            AdjustImage();

            tempPanOffsetX = e.TotalX;
            tempPanOffsetY = e.TotalY;
        }
    }

    private void AdjustSize()
    {
        var ratio = GetViewSizeRatio();

        var newWidth = _outputSize.Width * ratio;
        var newHeight = _outputSize.Height * ratio;

        if (newHeight <= 0 || newWidth <= 0)
        {
            return;
        }

        AbsoluteLayoutContainer.WidthRequest = newWidth;
        AbsoluteLayoutContainer.HeightRequest = newHeight;
    }

    private double GetViewSizeRatio()
    {
        var ratioX = Width / _outputSize.Width;
        var ratioY = Height / _outputSize.Height;
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