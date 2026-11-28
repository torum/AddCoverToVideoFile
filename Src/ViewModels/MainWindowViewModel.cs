using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AddCoverToVideoFile.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        DefaultDropImageForPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
        DefaultDropImageForVideo = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
    }

    public bool IsBusy
    {
        get;
        private set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
            ApplyAndSaveCommand.NotifyCanExecuteChanged();
            FileDropCommand.NotifyCanExecuteChanged();
        }
    }

    [ObservableProperty]
    public partial Bitmap? DefaultDropImageForPicture { get; set; }

    [ObservableProperty]
    public partial Bitmap? DefaultDropImageForVideo { get; set; }

    [ObservableProperty]
    public partial string? DefaultTextForPicture { get; set; } = "Drop a picture to add";

    [ObservableProperty]
    public partial string? DefaultTextForVideo { get; set; } = "Drop a video file";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyAndSaveCommand))]
    public partial string? VideoFilePath { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyAndSaveCommand))]
    public partial string? PictureFilePath { get; set; }

    [ObservableProperty]
    public partial string? VideoFileName { get; set; }

    [ObservableProperty]
    public partial string? PictureFileName { get; set; }

    [ObservableProperty]
    public partial Bitmap? AlbumArt { get; set; }

    [ObservableProperty]
    public partial Bitmap? NewAlbumArt { get; set; }

    [ObservableProperty]
    public partial string? StatusBarMessage { get; set; }

    [ObservableProperty]
    public partial string? StatusBarErrorMessage { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsButtonEnabled { get; set; }


    [RelayCommand(CanExecute = nameof(CanApplyAndSave))]
    public async Task ApplyAndSave()
    {
        if (!CanApplyAndSave())
        {
            return;
        }

        var videoFilePath = VideoFilePath!;
        var pictureFilePath = PictureFilePath!;
        var title = Title;

        IsBusy = true;
        IsButtonEnabled = false;
        StatusBarErrorMessage = "";
        StatusBarMessage = "Saving...";

        await Task.Run(() => OnSave(videoFilePath, pictureFilePath, title));
    }

    private bool CanApplyAndSave()
    {
        return !string.IsNullOrEmpty(VideoFilePath)
            && !string.IsNullOrEmpty(PictureFilePath)
            && !IsBusy;
    }

    private bool CanProcessFiles() => !IsBusy;

    private async Task<bool> OnSave(string videoFilePath, string pictureFilePath, string title)
    {
        // https://github.com/mono/taglib-sharp/blob/master/examples/SetPictures/SetPictures.cs

        try
        {
            using TagLib.File file = TagLib.File.Create(videoFilePath);

            // Sets title.
            file.Tag.Title = title;

            TagLib.Picture picture = new(pictureFilePath);

            var ext = System.IO.Path.GetExtension(pictureFilePath);
            if (!string.IsNullOrEmpty(ext))
            {
                if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    picture.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                }
                else if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    picture.MimeType = System.Net.Mime.MediaTypeNames.Image.Png;//"image/png";
                }
            }

            picture.Type = TagLib.PictureType.FrontCover;
            // Preserving other pictures
            if (file.Tag.Pictures.Length > 0)
            {
                //file.Tag.Pictures[0] = picture;
                var tmp = file.Tag.Pictures;
                tmp[0] = picture;
                file.Tag.Pictures = tmp;
            }
            else
            {
                //file.Tag.Pictures = new TagLib.IPicture[] { picture };
                file.Tag.Pictures = [picture];
            }
            //file.Tag.Pictures = [picture];

            try
            {
                // TODO: save as .tmp then replace it.
                //var tmpFile = Path.ChangeExtension(VideoFilePath, ".bak");
                //System.IO.File.Copy(VideoFilePath, tmpFile);

                file.Save();

                await Task.Delay(30);

                // TODO: delete tmp file.
                //System.IO.File.Delete(tmpFile);

                var albumArtData = file.Tag.Pictures[0].Data.Data;
                var albumArt = await Task.Run(() =>
                {
                    using var stream = new MemoryStream(albumArtData);
                    return Bitmap.DecodeToWidth(stream, 400);
                });

                Dispatcher.UIThread.Post(() =>
                {
                    AlbumArt = albumArt;
                    IsButtonEnabled = true;
                    StatusBarMessage = "Done";
                    StatusBarErrorMessage = "";
                    IsBusy = false;
                }, DispatcherPriority.Background);

                return true;
            }
            catch (Exception e)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    IsButtonEnabled = true;
                    StatusBarErrorMessage = string.Format("Error trying to save file:  {0}", e.Message);
                    StatusBarMessage = "Error";
                    IsBusy = false;
                }, DispatcherPriority.Background);

                return false;
            }
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsButtonEnabled = true;
                StatusBarErrorMessage = string.Format("Error trying to open file:  {0}", e.Message);
                StatusBarMessage = "Error";
                IsBusy = false;
            }, DispatcherPriority.Background);
            return false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanProcessFiles))]
    public async Task OnFileDropAsync(IEnumerable<string>? filepaths)//IReadOnlyList<IStorageFile>? filepaths
    {
        if (filepaths == null || IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            //var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            var paths = filepaths.ToList();
            if (paths.Count == 0)
            {
                return;
            }

            IsButtonEnabled = false;
            StatusBarErrorMessage = "";
            StatusBarMessage = "";

            foreach (var filePath in paths)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string fileExt = System.IO.Path.GetExtension(fileName);

                if ((fileExt.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".mkv", StringComparison.OrdinalIgnoreCase)))// || (fileExt.ToLower() == ".avi"))
                {
                    VideoFilePath = filePath;
                    VideoFileName = fileName;
                    AlbumArt = null;
                    DefaultTextForVideo = "";

                    using TagLib.File file = TagLib.File.Create(VideoFilePath);

                    // Reads title
                    Title = file.Tag.Title;

                    if (file.Tag.Pictures.Length > 0)
                    {
                        // for Avalonia UI
                        using var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                        AlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));

                        /* for WPF
                        using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            AlbumArt = bitmap;
                        }
                        */

                        /* for WinUI3
                        using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
                        {
                            using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                            {
                                writer.WriteBytes(file.Tag.Pictures[0].Data.Data);
                                writer.StoreAsync().GetResults();
                            }
                            var bitmap = new BitmapImage();
                            bitmap.SetSource(ms);

                            AlbumArt = bitmap;
                        }
                        */

                    }
                    else
                    {
                        //DefaultDropImageForVideo = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/video2.png")));
                        DefaultDropImageForVideo = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/video2.png")));
                    }
                }
                else if ((fileExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".png", StringComparison.OrdinalIgnoreCase)))
                {
                    PictureFilePath = filePath;
                    PictureFileName = fileName; //filePath.Name;
                    DefaultTextForPicture = "";
                    DefaultDropImageForPicture = null;

                    // for Avalonia UI
                    await LoadCover(PictureFilePath);

                    // for WPF
                    /*
                    ImageLoader imgLoader = new ImageLoader();
                    imgLoader.BmpImg = imgLoader.GetBitmapImage(PictureFilePath);
                    NewAlbumArt = imgLoader.BmpImg;
                    */

                    /* for WinUI3
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                    NewAlbumArt = bitmapImage;
                    */
                }
                else
                {
                    StatusBarErrorMessage = string.Format("File type {0} not recognized.", fileExt);
                }
            }

            if (!string.IsNullOrEmpty(VideoFilePath) && !string.IsNullOrEmpty(PictureFilePath))
            {
                IsButtonEnabled = true;
                StatusBarMessage = "Ready";
            }
            else if (string.IsNullOrEmpty(PictureFilePath))
            {
                StatusBarMessage = "Drop a picture.";
            }
            else if (string.IsNullOrEmpty(VideoFilePath))
            {
                StatusBarMessage = "Drop a video.";
            }
        }
        catch (Exception e)
        {
            StatusBarErrorMessage = string.Format("Error processing dropped file: {0}", e.Message);
            StatusBarMessage = "Error";
            IsButtonEnabled = !string.IsNullOrEmpty(VideoFilePath) && !string.IsNullOrEmpty(PictureFilePath);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadCover(string path)
    {
        await using var imageStream = await LoadCoverBitmapAsync(path);
        if (imageStream == null)
        {
            return;
        }

        NewAlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
    }

    public static async Task<Stream?> LoadCoverBitmapAsync(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return await Task.FromResult(File.OpenRead(path));
    }
}
