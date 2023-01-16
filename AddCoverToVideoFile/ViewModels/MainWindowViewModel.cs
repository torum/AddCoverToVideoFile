using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AddCoverToVideoFile.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static string? Greeting => "AddCoverToVideoFile v1.0.0.5";

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

        private bool _isButtonEnabled = false;
        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            private set => this.RaiseAndSetIfChanged(ref _isButtonEnabled, value);
        }

        public ICommand ApplyAndSaveCommand { get; }

        public MainWindowViewModel()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            DefaultDropImageForPicture = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));
            DefaultDropImageForVideo = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/drop2.png")));

            ApplyAndSaveCommand = ReactiveCommand.Create( () =>
            {
                //await OnSave();
                Task.Run(() => OnSave());
            });
        }

        public async void OnFileDrop(IEnumerable<string>? filepaths)
        {
            if (filepaths == null)
            {
                return;
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (filepaths.Any())
            {
                StatusBarErrorMessage = "";
                StatusBarMessage = "";

                foreach (var filePath in filepaths)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string fileExt = System.IO.Path.GetExtension(fileName);

                    if ((fileExt.ToLower() == ".mp4") || (fileExt.ToLower() == ".mkv"))// || (fileExt.ToLower() == ".avi"))
                    {
                        VideoFilePath = filePath;
                        VideoFileName = fileName;
                        AlbumArt = null;
                        DefaultTextForVideo = "";

                        TagLib.File file = TagLib.File.Create(VideoFilePath);

                        if (file.Tag.Pictures.Length > 0)
                        {
                            // for Avalonia UI
                            using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                            {
                                AlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));
                            }

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
                            DefaultDropImageForVideo = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/video2.png")));
                        }
                    }
                    else if ((fileExt.ToLower() == ".jpg") || (fileExt.ToLower() == ".jpeg") || (fileExt.ToLower() == ".png"))
                    {
                        PictureFilePath = filePath;
                        PictureFileName = System.IO.Path.GetFileName(filePath);
                        DefaultTextForPicture = "";
                        DefaultDropImageForPicture = null;

                        // for Avalonia UI
                        await LoadCover(filePath);

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

                    TagLib.Picture picture = new TagLib.Picture(PictureFilePath);
                    
                    var ext = Path.GetExtension(PictureFilePath);
                    if (!string.IsNullOrEmpty(ext))
                    {
                        if (ext.ToLower() == ".jpg" || ext.ToLower() == ".jpeg")
                        { 
                            picture.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        }
                        else if (ext.ToLower() == ".png" )
                        {
                            picture.MimeType = "image/png";
                        }
                    }

                    picture.Type = TagLib.PictureType.FrontCover;
                    
                    // Preserving other pictures
                    if (file.Tag.Pictures.Count() > 0)
                    {
                        file.Tag.Pictures[0] = picture;
                    }
                    else
                    {
                        file.Tag.Pictures = new TagLib.IPicture[] { picture };
                    }

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
            NewAlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
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