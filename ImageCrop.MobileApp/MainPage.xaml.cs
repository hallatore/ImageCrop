using ImageCrop.Core;
using SkiaSharp;

namespace ImageCrop.MobileApp;

public partial class MainPage : ContentPage
{
    private ImageSet _imageSet;
    private int _currentImageSetIndex;
    private bool _isLoading;
    private ImageCropper _currentImageCropper;
    private MemoryStream loadedPhoto;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (_isLoading || _imageSet == null || _currentImageCropper == null)
        {
            return;
        }

        _isLoading = true;
        ActivityIndicatorElement.IsRunning = true;
        var previewImagePath = Path.Combine(
            FileSystem.Current.CacheDirectory,
            $"{_currentImageSetIndex}_{_imageSet.FileName}");

        await await Task.Factory.StartNew(
            async () =>
            {
                using var image = ImageUtils.LoadFromStream(_imageSet.LoadedImage);
                using var previewImage = ImageUtils.CropAndResize(
                    image,
                    _currentImageCropper.GetBoundingBox(),
                    _imageSet.OutputSize);

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
        _imageSet.OutputSize = new Size(1024, 1024);
    }

    //private async void OnPrevClicked(object sender, EventArgs e)
    //{
    //    if (_isLoading)
    //    {
    //        return;
    //    }

    //    ShowPreviewImage(_currentImageSetIndex - 1);
    //}

    //private async void OnNextClicked(object sender, EventArgs e)
    //{
    //    if (_isLoading)
    //    {
    //        return;
    //    }

    //    ShowPreviewImage(_currentImageSetIndex + 1);
    //}

    //private void LoadPreviewImageFromStream(Stream stream, string fileName)
    //{
    //    //if (_currentPreviewImagePath != null && File.Exists(_currentPreviewImagePath))
    //    //{
    //    //    File.Delete(_currentPreviewImagePath);
    //    //}

    //    _currentPreviewImagePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
    //    var fileStream = File.OpenWrite(_currentPreviewImagePath);
    //    stream.CopyTo(fileStream);
    //    fileStream.Close();
    //    var imageSource = ImageSource.FromFile(_currentPreviewImagePath);

    //    Dispatcher.Dispatch(
    //        () => { PreviewImage.Source = imageSource; });
    //}

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
                        var outputSize = new Size(1080, 1920);
                        await using var sourceStream = await photo.OpenReadAsync();

                        var previewPath = Path.Combine(FileSystem.Current.CacheDirectory, photo.FileName);
                        var fileStream = File.OpenWrite(previewPath);
                        await sourceStream.CopyToAsync(fileStream);
                        fileStream.Close();

                        loadedPhoto = new MemoryStream();
                        sourceStream.Seek(0, SeekOrigin.Begin);
                        sourceStream.CopyTo(loadedPhoto);

                        var imageSource = ImageSource.FromFile(previewPath);

                        sourceStream.Seek(0, SeekOrigin.Begin);
                        
                        //var imageInfo = await Image.IdentifyAsync(sourceStream);
                        //var size = new Size(imageInfo.Width, imageInfo.Height);
                        //var previewPathStream = File.OpenRead(previewPath);
                        
                        using var bitmap = ImageUtils.LoadFromStream(loadedPhoto);
                        var size = new Size(bitmap.Width, bitmap.Height);
                        //var image = await ImageUtils.LoadFromStream(sourceStream, photo.FileName);
                        _imageSet = new ImageSet(loadedPhoto, size, photo.FileName, outputSize);
                        _imageSet.GenerateBoundingBoxes();

                        _currentImageCropper = new ImageCropper(imageSource, size, outputSize);
                    },
                    TaskCreationOptions.LongRunning);

                Container.Children.Insert(0, _currentImageCropper);

                _isLoading = false;
                ActivityIndicatorElement.IsRunning = false;

                ShowPreviewImage(0);
            }
        }
    }

    //private async Task LoadPhoto()
    //{
    //    if (MediaPicker.Default.IsCaptureSupported && !_isLoading)
    //    {
    //        var photo = await MediaPicker.Default.PickPhotoAsync();

    //        if (photo != null)
    //        {
    //            _isLoading = true;
    //            PreviewImage.IsVisible = false;
    //            ActivityIndicatorElement.IsRunning = true;

    //            await await Task.Factory.StartNew(
    //                async () =>
    //                {
    //                    await using var sourceStream = await photo.OpenReadAsync();
    //                    var image = await ImageUtils.LoadFromStream(sourceStream, photo.FileName);
    //                    _imageSet = new ImageSet(image, photo.FileName, new Size(1080, 1920));
    //                    _imageSet.GenerateBoundingBoxes();
    //                },
    //                TaskCreationOptions.LongRunning);


    //            _isLoading = false;
    //            ActivityIndicatorElement.IsRunning = false;

    //            await ShowPreviewImage(0);
    //        }
    //    }
    //}

    private void ShowPreviewImage(int boundingBoxIndex)
    {
        if (_imageSet == null ||
            _currentImageCropper == null ||
            boundingBoxIndex < 0 ||
            boundingBoxIndex >= _imageSet.BoundingBoxes.Count)
        {
            return;
        }

        _currentImageSetIndex = boundingBoxIndex;
        var boundingBox = _imageSet.BoundingBoxes[boundingBoxIndex];
        _currentImageCropper.SetBoundingBox(boundingBox);
        //NextButton.Text = $"Next ({_currentImageSetIndex + 1}/{_imageSet.BoundingBoxes.Count})";
    }

    //private async Task ShowPreviewImage(int boundingBoxIndex)
    //{
    //    if (_imageSet == null || boundingBoxIndex < 0 || boundingBoxIndex >= _imageSet.BoundingBoxes.Count)
    //    {
    //        return;
    //    }

    //    PreviewImage.IsVisible = false;
    //    _currentImageSetIndex = boundingBoxIndex;
    //    var previewFileName = $"{boundingBoxIndex}_{_imageSet.FileName}";
    //    var currentPreviewImagePath = Path.Combine(FileSystem.Current.CacheDirectory, previewFileName);

    //    if (File.Exists(currentPreviewImagePath))
    //    {
    //        _currentPreviewImagePath = currentPreviewImagePath;
    //        var imageSource = ImageSource.FromFile(currentPreviewImagePath);
    //        PreviewImage.Source = imageSource;
    //    }
    //    else
    //    {
    //        var boundingBox = _imageSet.BoundingBoxes[boundingBoxIndex];
    //        _isLoading = true;
    //        ActivityIndicatorElement.IsRunning = true;

    //        await Task.Factory.StartNew(
    //            () =>
    //            {
    //                var previewImage = ImageUtils.CropAndResize(_imageSet.LoadedImage, boundingBox, _imageSet.OutputSize);
    //                var previewImageStream = ImageUtils.SaveAsJpegStream(previewImage);
    //                LoadPreviewImageFromStream(previewImageStream, previewFileName);
    //            },
    //            TaskCreationOptions.LongRunning);
    //    }

    //    NextButton.Text = $"Next ({_currentImageSetIndex + 1}/{_imageSet.BoundingBoxes.Count})";
    //    PreviewImage.IsVisible = true;
    //    _isLoading = false;
    //    ActivityIndicatorElement.IsRunning = false;
    //}
}