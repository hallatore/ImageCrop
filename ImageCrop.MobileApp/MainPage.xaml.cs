using Android.Content;
using Android.Provider;
using ImageCrop.Core;
using SkiaSharp;

namespace ImageCrop.MobileApp;

public partial class MainPage : ContentPage
{
    private SKBitmap _bitmap;
    private ImageCropper _currentImageCropper;
    private string _fileName;
    private bool _isLoading;

    public MainPage()
    {
        InitializeComponent();
        Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        if (App.ImageStream != null)
        {
            _ = LoadFromStream(App.ImageStream);
            App.ImageStream = null;
        }
        else
        {
            OnOpenClicked(sender, e);
        }
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
        _currentImageCropper.UpdateOutputSize(new Size(1024, 1024));
    }

    private async Task LoadPhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported && !_isLoading)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            if (photo != null)
            {
                _fileName = Path.GetFileNameWithoutExtension(photo.FileName);
                await using var sourceStream = await photo.OpenReadAsync();
                await LoadFromStream(sourceStream);
            }
        }
    }

    public async Task LoadFromStream(Stream stream)
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
                stream.Seek(0, SeekOrigin.Begin);
                var previewStream = new MemoryStream();
                await stream.CopyToAsync(previewStream);
                previewStream.Seek(0, SeekOrigin.Begin);

                var previewImageSource = ImageSource.FromStream(() => previewStream);

                _bitmap = stream.LoadImageFromStream();
                _currentImageCropper = new ImageCropper(
                    new Size(_bitmap.Width, _bitmap.Height),
                    previewImageSource);
            },
            TaskCreationOptions.LongRunning);

        Container.Children.Insert(0, _currentImageCropper);

        _isLoading = false;
        ActivityIndicatorElement.IsRunning = false;
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
                using var previewImage = _bitmap.CropAndResize(
                    _currentImageCropper.GetBoundingBox(),
                    _currentImageCropper.GetOutputSize());

                await using var previewImageStream = previewImage.SaveToStream();
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

    private async void OnSaveClicked(object sender, EventArgs e)
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
                using var previewImage = _bitmap.CropAndResize(
                    _currentImageCropper.GetBoundingBox(),
                    _currentImageCropper.GetOutputSize());

                await using var previewImageStream = previewImage.SaveToStream();

                var context = Platform.CurrentActivity;

                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    var resolver = context.ContentResolver;
                    var contentValues = new ContentValues();
                    contentValues.Put(MediaStore.IMediaColumns.DisplayName, _fileName + ".webp");
                    contentValues.Put(MediaStore.IMediaColumns.MimeType, "image/wepb");
                    contentValues.Put(
                        MediaStore.IMediaColumns.RelativePath,
                        "DCIM/" + _fileName + ".webp");

                    var imageUri = resolver.Insert(
                        MediaStore.Images.Media.ExternalContentUri,
                        contentValues);

                    using var os = resolver.OpenOutputStream(imageUri);
                    await previewImageStream.CopyToAsync(os);
                }
            },
            TaskCreationOptions.LongRunning);

        ActivityIndicatorElement.IsRunning = false;
        _isLoading = false;
    }
}