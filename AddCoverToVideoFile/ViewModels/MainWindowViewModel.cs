using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using DynamicData;
using Avalonia.Platform;
using Avalonia;
using System.Windows.Input;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using System.Reactive.Disposables;

namespace AddCoverToVideoFile.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "AddCoverToVideoFile v1.0.0.1";

        private string statusBarMessage;

        public string StatusBarMessage
        {
            get => statusBarMessage;
            set => this.RaiseAndSetIfChanged(ref statusBarMessage, value);
        }

        private string statusBarErrorMessage;

        public string StatusBarErrorMessage
        {
            get => statusBarErrorMessage;
            set => this.RaiseAndSetIfChanged(ref statusBarErrorMessage, value);
        }

        private string _defaultTextForPicture = "Drop a picture to add";

        public string DefaultTextForPicture
        {
            get => _defaultTextForPicture;
            set => this.RaiseAndSetIfChanged(ref _defaultTextForPicture, value);
        }

        private string _defaultTextForVideo = "Drop a video file";

        public string DefaultTextForVideo
        {
            get => _defaultTextForVideo;
            set => this.RaiseAndSetIfChanged(ref _defaultTextForVideo, value);
        }

        private string _videoFilePath;
        public string VideoFilePath
        {
            get => _videoFilePath;
            set => this.RaiseAndSetIfChanged(ref _videoFilePath, value);
        }

        private string _pictureFilePath;
        public string PictureFilePath
        {
            get => _pictureFilePath;
            set => this.RaiseAndSetIfChanged(ref _pictureFilePath, value);
        }

        private string _videoFileName;
        public string VideoFileName
        {
            get => _videoFileName;
            set => this.RaiseAndSetIfChanged(ref _videoFileName, value);
        }

        private string _pictureFileName;
        public string PictureFileName
        {
            get => _pictureFileName;
            set => this.RaiseAndSetIfChanged(ref _pictureFileName, value);
        }

        private Bitmap _albumArt;
        public Bitmap AlbumArt
        {
            get => _albumArt;
            set => this.RaiseAndSetIfChanged(ref _albumArt, value);
        }

        private Bitmap _newAlbumArt;
        public Bitmap NewAlbumArt
        {
            get => _newAlbumArt;
            private set => this.RaiseAndSetIfChanged(ref _newAlbumArt, value);
        }

        private Bitmap _defaultDropImageForPicture;
        public Bitmap DefaultDropImageForPicture
        {
            get => _defaultDropImageForPicture;
            private set => this.RaiseAndSetIfChanged(ref _defaultDropImageForPicture, value);
        }

        private Bitmap _defaultDropImageForVideo;
        public Bitmap DefaultDropImageForVideo
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

            ApplyAndSaveCommand = ReactiveCommand.Create(async () =>
            {
                await OnSave();
            });
        }

        public async void OnFileDrop(IEnumerable<string> filepaths)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            if (filepaths.Count() > 0)
            {
                StatusBarErrorMessage = "";
                StatusBarMessage = "";

                foreach (var filePath in filepaths)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string fileExt = System.IO.Path.GetExtension(fileName);

                    if ((fileExt.ToLower() == ".mp4") || (fileExt.ToLower() == ".mkv") || (fileExt.ToLower() == ".avi"))
                    {
                        VideoFilePath = filePath;
                        VideoFileName = fileName;
                        AlbumArt = null;
                        DefaultTextForVideo = "";

                        TagLib.File file = TagLib.File.Create(VideoFilePath);

                        if (file.Tag.Pictures.Length > 0)
                        {
                            using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                            {
                                AlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));
                            }
                        }
                        else
                        {
                            DefaultDropImageForVideo = new Bitmap(assets?.Open(new Uri("avares://AddCoverToVideoFile/Assets/video2.png")));
                        }
                    }
                    else if ((fileExt.ToLower() == ".jpg") || (fileExt.ToLower() == ".png"))
                    {
                        PictureFilePath = filePath;
                        PictureFileName = System.IO.Path.GetFileName(filePath);
                        DefaultTextForPicture = "";
                        DefaultDropImageForPicture = null;
                        await LoadCover(filePath);
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

                //Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        // TODO save as .tmp then replace it.
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

                    //var custom = (TagLib.IFD.IFDTag)file.GetTag(TagLib.TagTypes.Xiph);
                    //var tag = file.Tag as .;


                    /*
                    string title = file.Tag.Title;
                    TimeSpan duration = file.Properties.Duration;
                    Debug.WriteLine("Title: {0}, duration: {1}", title, duration);
                    file.Tag.Title = "my new title";
                    */
                    /*
                    file.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(@"C:\Users\torum\Desktop\test.jpg") };

                    file.Save();

                    */
                    TagLib.Picture picture = new TagLib.Picture(PictureFilePath);
                    //picture.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    //picture.Type = TagLib.PictureType.FrontCover;
                    /*
                    TagLib.Id3v2.AttachmentFrame cover = new TagLib.Id3v2.AttachmentFrame
                    {
                        Type = TagLib.PictureType.FrontCover,
                        Description = "Cover",
                        MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                        //Data = imageBytes,
                        TextEncoding = TagLib.StringType.UTF16


                    };
                    */
                    file.Tag.Pictures = new TagLib.IPicture[] { picture };

                    try
                    {
                        var tmpFile = Path.ChangeExtension(VideoFilePath, ".bak");
                        System.IO.File.Copy(VideoFilePath, tmpFile);

                        file.Save();

                        await Task.Delay(30);

                        System.IO.File.Delete(tmpFile);

                        Dispatcher.UIThread.Post(async () => 
                        {
                            using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                            {
                                AlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(stream, 400));
                            }
                            IsButtonEnabled = true;
                            StatusBarMessage = "Done";
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
            await using (var imageStream = await LoadCoverBitmapAsync(path))
            {
                NewAlbumArt = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }

        public async Task<Stream> LoadCoverBitmapAsync(string path)
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