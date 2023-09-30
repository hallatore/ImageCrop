using ImageCrop.Core;
using SkiaSharp;

namespace ImageCrop.MobileApp;

public partial class MainPage : ContentPage
{
    private bool _isLoading;
    private ImageCropper _currentImageCropper;
    private SKBitmap _bitmap;
    private string _fileName;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenClicked(object sender, EventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        await LoadPhoto();
    }

    private void OnAspectRatioClicked(object sender, EventArgs e)
    {
        _currentImageCropper.UpdateOutputSize( new Size(1024, 1024));
    }

    private async Task LoadPhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported && !_isLoading)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                _isLoading = true;
                PreviewImage.IsVisible = false;
                ActivityIndicatorElement.IsRunning = true;

                if (_currentImageCropper != null)
                {
                    Container.Children.Remove(_currentImageCropper);
                }

                await await Task.Factory.StartNew(
                    async () =>
                    {
                        _fileName = Path.GetFileNameWithoutExtension(photo.FileName);
                        await using var sourceStream = await photo.OpenReadAsync();

                        //var previewPath = Path.Combine(FileSystem.Current.CacheDirectory, photo.FileName);
                        //using var fileStream = File.OpenWrite(previewPath);
                        //await sourceStream.CopyToAsync(fileStream);
                        //fileStream.Close();

                        var previewStream = new MemoryStream();
                        sourceStream.Seek(0, SeekOrigin.Begin);
                        await sourceStream.CopyToAsync(previewStream);
                        previewStream.Seek(0, SeekOrigin.Begin);

                        //var imageSource = ImageSource.FromFile(previewPath);
                        var previewImageSource = ImageSource.FromStream(() => previewStream);

                        //loadedPhoto = new MemoryStream();
                        //sourceStream.Seek(0, SeekOrigin.Begin);
                        //sourceStream.CopyTo(loadedPhoto);

                        sourceStream.Seek(0, SeekOrigin.Begin);
                        _bitmap = ImageUtils.LoadImageFromStream(sourceStream);
                        _currentImageCropper = new ImageCropper(new Size(_bitmap.Width, _bitmap.Height), previewImageSource);
                    },
                    TaskCreationOptions.LongRunning);

                Container.Children.Insert(0, _currentImageCropper);

                _isLoading = false;
                ActivityIndicatorElement.IsRunning = false;
            }
        }
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (_isLoading || _bitmap == null || _currentImageCropper == null)
        {
            return;
        }

        _isLoading = true;
        ActivityIndicatorElement.IsRunning = true;
        var previewImagePath = Path.Combine(FileSystem.Current.CacheDirectory, _fileName + ".webp");

        await await Task.Factory.StartNew(
            async () =>
            {
                using var previewImage = ImageUtils.CropAndResize(
                    _bitmap,
                    _currentImageCropper.GetBoundingBox(),
                    _currentImageCropper.GetOutputSize());

                await using var previewImageStream = ImageUtils.SaveToStream(previewImage);
                await using var fileStream = File.OpenWrite(previewImagePath);
                await previewImageStream.CopyToAsync(fileStream);
            },
            TaskCreationOptions.LongRunning);

        ActivityIndicatorElement.IsRunning = false;
        _isLoading = false;

        await Share.Default.RequestAsync(
            new ShareFileRequest
            {
                Title = "Share photo",
                File = new ShareFile(previewImagePath)
            });
    }
}