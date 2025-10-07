using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

namespace AddCoverToVideoFile.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static string? Greeting => "AddCoverToVideoFile v1.0.0.7";

        private string? _statusBarMessage;

        public string? StatusBarMessage
        {
            get => _statusBarMessage;
            set => this.RaiseAndSetIfChanged(ref _statusBarMessage, value);
        }

        private string? _statusBarErrorMessage;

        public string? StatusBarErrorMessage
        {
            get => _statusBarErrorMessage;
            set => this.RaiseAndSetIfChanged(ref _statusBarErrorMessage, value);
        }

        private string? _defaultTextForPicture = "Drop a picture to add";

        public string? DefaultTextForPicture
        {
            get => _defaultTextForPicture;
            set => this.RaiseAndSetIfChanged(ref _defaultTextForPicture, value);
        }

        private string? _defaultTextForVideo = "Drop a video file";

        public string? DefaultTextForVideo
        {
            get => _defaultTextForVideo;
            set => this.RaiseAndSetIfChanged(ref _defaultTextForVideo, value);
        }

        private string? _videoFilePath;
        public string? VideoFilePath
        {
            get => _videoFilePath;
            set => this.RaiseAndSetIfChanged(ref _videoFilePath, value);
        }

        private string? _pictureFilePath;
        public string? PictureFilePath
        {
            get => _pictureFilePath;
            set => this.RaiseAndSetIfChanged(ref _pictureFilePath, value);
        }

        private string? _videoFileName;
        public string? VideoFileName
        {
            get => _videoFileName;
            set => this.RaiseAndSetIfChanged(ref _videoFileName, value);
        }

        private string? _pictureFileName;
        public string? PictureFileName
        {
            get => _pictureFileName;
            set => this.RaiseAndSetIfChanged(ref _pictureFileName, value);
        }

        private Bitmap? _albumArt;
        public Bitmap? AlbumArt
        {
            get => _albumArt;
            set => this.RaiseAndSetIfChanged(ref _albumArt, value);
        }

        private Bitmap? _newAlbumArt;
        public Bitmap? NewAlbumArt
        {
            get => _newAlbumArt;
            private set => this.RaiseAndSetIfChanged(ref _newAlbumArt, value);
        }

        private Bitmap? _defaultDropImageForPicture;
        public Bitmap? DefaultDropImageForPicture
        {
            get => _defaultDropImageForPicture;
            private set => this.RaiseAndSetIfChanged(ref _defaultDropImageForPicture, value);
        }

        private Bitmap? _defaultDropImageForVideo;
        public Bitmap? DefaultDropImageForVideo
        {
            get => _defaultDropImageForVideo;
            private set => this.RaiseAndSetIfChanged(ref _defaultDropImageForVideo, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private bool _isButtonEnabled = false;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            private set => this.RaiseAndSetIfChanged(ref _isButtonEnabled, value);
        }

        public ICommand ApplyAndSaveCommand { get; }

        public MainWindowViewModel()
        {
            //var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            //DefaultDropImageForPicture = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
            DefaultDropImageForPicture = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
            //DefaultDropImageForVideo = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
            DefaultDropImageForVideo = new Bitmap(AssetLoader.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));

            ApplyAndSaveCommand = ReactiveCommand.Create( () =>
            {
                //await OnSave();
                Task.Run(() => OnSave());
            });
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
                    
                    var ext = Path.GetExtension(PictureFilePath);
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

}