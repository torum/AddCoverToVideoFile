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
using System.Web;

namespace AddCoverToVideoFile.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public bool IsBusy
    {
        get;
        private set
        {
            if (field == value)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial Bitmap? DefaultDropImageForPicture { get; set; }

    [ObservableProperty]
    public partial Bitmap? DefaultDropImageForVideo { get; set; }

    [ObservableProperty]
    public partial string? DefaultTextForPicture { get; set; } = "Drop a picture to add";

    [ObservableProperty]
    public partial string? DefaultTextForVideo {  get; set; } = "Drop a video file";

    [ObservableProperty]
    public partial string? VideoFilePath { get; set; }

    [ObservableProperty]
    public partial string? PictureFilePath {  get; set; }

    [ObservableProperty]
    public partial string? VideoFileName { get; set;  }

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

    public MainWindowViewModel()
    {
        DefaultDropImageForPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
        DefaultDropImageForVideo = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
        /*
        ApplyAndSaveCommand = ReactiveCommand.Create(() =>
        {
            //await OnSave();
            Task.Run(() => OnSave());
        });
        */
    }

    [RelayCommand(CanExecute = nameof(CanApplyAndSave))]
    public void ApplyAndSave()
    {
        //await OnSave();
        Task.Run(() => OnSave());
    }

    private bool CanApplyAndSave()
    {
        return true;
    }

    private async Task<bool> OnSave()
    {
        // https://github.com/mono/taglib-sharp/blob/master/examples/SetPictures/SetPictures.cs

        Dispatcher.UIThread.Post(() => { StatusBarMessage = "Saving..."; }, DispatcherPriority.Send);

        if (!string.IsNullOrEmpty(VideoFilePath) && !string.IsNullOrEmpty(PictureFilePath))
        {
            Dispatcher.UIThread.Post(() => { IsBusy = true; IsButtonEnabled = false; }, DispatcherPriority.Send);

            try
            {
                TagLib.File file = TagLib.File.Create(VideoFilePath);

                // Sets title.
                file.Tag.Title = Title;

                TagLib.Picture picture = new(PictureFilePath);

                var ext = System.IO.Path.GetExtension(PictureFilePath);
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

                    Dispatcher.UIThread.Post(async () =>
                    {
                        // Load pic for visula confirmation.
                        using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                        {
                            AlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));
                        }
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
                finally
                {
                    //
                    file.Dispose();
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
        else
        {
            return false;
        }
    }

    public async void OnFileDrop(IEnumerable<string>? filepaths)//IReadOnlyList<IStorageFile>? filepaths
    {
        if (filepaths == null)
        {
            return;
        }

        //var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        if (filepaths.Any())
        {
            StatusBarErrorMessage = "";
            StatusBarMessage = "";

            foreach (var filePath in filepaths)
            {
                string fileName = HttpUtility.UrlDecode(System.IO.Path.GetFileName(filePath));//filePath.Name;//
                string fileExt = System.IO.Path.GetExtension(fileName);

                if ((fileExt.Equals(".mp4", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".mkv", StringComparison.OrdinalIgnoreCase)))// || (fileExt.ToLower() == ".avi"))
                {
                    VideoFilePath = HttpUtility.UrlDecode(filePath);//filePath;.Path.AbsolutePath;
                    VideoFileName = fileName;
                    AlbumArt = null;
                    DefaultTextForVideo = "";

                    TagLib.File file = TagLib.File.Create(VideoFilePath);

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

                    file.Dispose();
                }
                else if ((fileExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) || (fileExt.Equals(".png", StringComparison.OrdinalIgnoreCase)))
                {
                    PictureFilePath = HttpUtility.UrlDecode(filePath);// filePath;//.Path.AbsolutePath;
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
    }

    public async Task LoadCover(string path)
    {
        await using var imageStream = await LoadCoverBitmapAsync(path);
        if (imageStream != null)
        {
            NewAlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
        }
    }

    public static async Task<Stream?> LoadCoverBitmapAsync(string path)
    {
        if (File.Exists(path))
        {
            return await Task.FromResult(File.OpenRead(path));
        }
        else
        {
            return null;
        }
    }
}
