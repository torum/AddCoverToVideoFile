using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Media;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using VideoCoverIt.Common;
using TagLib;
using VideoCoverIt.Helper;
using System.Windows.Media.Imaging;

namespace VideoCoverIt.ViewModels
{

    public class MainViewModel : ViewModelBase, IFileDragDropTarget
    {
        // Application name
        const string _appName = "VideoCoverIt";

        // Application version
        const string _appVer = "0.0.0";
        public string AppVer
        {
            get
            {
                return _appVer;
            }
        }

        // Application config file folder
        const string _appDeveloper = "torum";

        // Application Window Title
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }

        private string _statusBarMessage;
        public string StatusBarMessage
        {
            get
            {
                return _statusBarMessage;
            }
            set
            {
                _statusBarMessage = value;
                NotifyPropertyChanged("StatusBarMessage");
            }
        }

        private string _statusBarErrorMessage;
        public string StatusBarErrorMessage
        {
            get
            {
                return _statusBarErrorMessage;
            }
            set
            {
                _statusBarErrorMessage = value;
                NotifyPropertyChanged("StatusBarErrorMessage");
            }
        }

        private string _videoFilePath;
        public string VideoFilePath
        {
            get
            {
                return _videoFilePath;
            }
            set
            {
                _videoFilePath = value;
                NotifyPropertyChanged("VideoFilePath");
            }
        }

        private string _pictureFilePath;
        public string PictureFilePath
        {
            get
            {
                return _pictureFilePath;
            }
            set
            {
                _pictureFilePath = value;
                NotifyPropertyChanged("PictureFilePath");
            }
        }

        private string _videoFileName;
        public string VideoFileName
        {
            get
            {
                return _videoFileName;
            }
            set
            {
                _videoFileName = value;
                NotifyPropertyChanged("VideoFileName");
            }
        }

        private ImageSource _albumArt;
        public ImageSource AlbumArt
        {
            get
            {
                return _albumArt;
            }
            set
            {
                if (_albumArt == value)
                    return;

                _albumArt = value;
                NotifyPropertyChanged("AlbumArt");
            }
        }

        private ImageSource _newAlbumArt;
        public ImageSource NewAlbumArt
        {
            get
            {
                return _newAlbumArt;
            }
            set
            {
                if (_newAlbumArt == value)
                    return;

                _newAlbumArt = value;
                NotifyPropertyChanged("NewAlbumArt");
            }
        }

        private string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string _appDataFolder;
        private string _appConfigFilePath;

        public MainViewModel()
        {
            // データ保存フォルダの取得
            _appDataFolder = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 設定ファイルのパス
            _appConfigFilePath = _appDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(_appDataFolder);



            PlayCommand = new RelayCommand(PlayCommand_Execute, PlayCommand_CanExecute);


            
        }

        #region == Apps load and close ==

         // 起動時の処理
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region == アプリ設定のロード  ==

            try
            {
                // アプリ設定情報の読み込み
                if (System.IO.File.Exists(_appConfigFilePath))
                {
                    XDocument xdoc = XDocument.Load(_appConfigFilePath);

                    #region == ウィンドウ関連 ==

                    if (sender is Window)
                    {
                        // Main Window element
                        var mainWindow = xdoc.Root.Element("MainWindow");
                        if (mainWindow != null)
                        {
                            var hoge = mainWindow.Attribute("top");
                            if (hoge != null)
                            {
                                (sender as Window).Top = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("left");
                            if (hoge != null)
                            {
                                (sender as Window).Left = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("height");
                            if (hoge != null)
                            {
                                (sender as Window).Height = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("width");
                            if (hoge != null)
                            {
                                (sender as Window).Width = double.Parse(hoge.Value);
                            }

                            hoge = mainWindow.Attribute("state");
                            if (hoge != null)
                            {
                                if (hoge.Value == "Maximized")
                                {
                                    (sender as Window).WindowState = WindowState.Maximized;
                                }
                                else if (hoge.Value == "Normal")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                                else if (hoge.Value == "Minimized")
                                {
                                    (sender as Window).WindowState = WindowState.Normal;
                                }
                            }

                        }

                    }

                    #endregion

                }
            }
            catch (System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルのロード中 - FileNotFoundException : " + _appConfigFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルのロード中: " + ex + " while opening : " + _appConfigFilePath);
            }

            #endregion
        }

        // 終了時の処理
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
 
            #region == アプリ設定の保存 ==

            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == ウィンドウ関連 ==

            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);


                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }

            #endregion

            try
            {
                // 設定ファイルの保存
                doc.Save(_appConfigFilePath);
            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + _appConfigFilePath);
            }

            #endregion

        }


        #endregion

        public void OnFileDrop(string[] filepaths)
        {
            StatusBarErrorMessage = "";
            StatusBarMessage = "";

            if (filepaths.Length > 0)
            {
                for (int i = 0; i < filepaths.Length; i++)
                {
                    string filePath = filepaths[i];
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string fileExt = System.IO.Path.GetExtension(fileName);

                    if ((fileExt.ToLower() == ".mp4") || (fileExt.ToLower() == ".mkv") || (fileExt.ToLower() == ".avi"))
                    {
                        VideoFilePath = filePath;
                        VideoFileName = fileName;
                        AlbumArt = null;

                        TagLib.File file = TagLib.File.Create(VideoFilePath);

                        if (file.Tag.Pictures.Length > 0)
                        {
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
                        }

                    }
                    else if ((fileExt.ToLower() == ".jpg") || (fileExt.ToLower() == ".png"))
                    {
                        PictureFilePath = filePath;

                        ImageLoader imgLoader = new ImageLoader();

                        imgLoader.BmpImg = imgLoader.GetBitmapImage(PictureFilePath);

                        NewAlbumArt = imgLoader.BmpImg;
                    }
                    else
                    {
                        StatusBarErrorMessage = string.Format("File type {0} not recognized.", fileExt);
                    }

                    if (i == 1)
                        break;
                }

                if (!string.IsNullOrEmpty(VideoFilePath) && !string.IsNullOrEmpty(PictureFilePath))
                {
                    StatusBarMessage = "Ready";
                }
                else if (string.IsNullOrEmpty(PictureFilePath))
                {
                    StatusBarMessage = "Drop a pic";
                }
                else if (string.IsNullOrEmpty(VideoFilePath))
                {
                    StatusBarMessage = "Drop a vid";
                }

                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());

            }
        }

        #region == メソッド ==

        #endregion

        #region == コマンド ==
        public ICommand PlayCommand { get; }
        public bool PlayCommand_CanExecute()
        {
            if (string.IsNullOrEmpty(VideoFilePath) || string.IsNullOrEmpty(PictureFilePath))
                return false;

            return true;
        }
        public void PlayCommand_Execute()
        {
            // https://github.com/mono/taglib-sharp/blob/master/examples/SetPictures/SetPictures.cs

            StatusBarMessage = "";

            if (!string.IsNullOrEmpty(VideoFilePath) && !string.IsNullOrEmpty(PictureFilePath))
            {
                
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
                    file.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(@"C:\Users\torum\Desktop\CBRL-002 Anita.jpg") };

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
                        file.Save();

                        StatusBarMessage = "Done";

                    }
                    catch (Exception e)
                    {
                        StatusBarErrorMessage = string.Format("Error trying to save file:  {0}", e.Message);

                        StatusBarMessage = "Error";

                        return;
                    }
                }
                catch (Exception e)
                {
                    StatusBarErrorMessage = string.Format("Error trying to open file:  {0}", e.Message);

                    StatusBarMessage = "Error";

                    return;
                }


            }

        }

        #endregion

        class ImageLoader
        {
            public BitmapImage BmpImg { get; set; }
            public BitmapImage GetBitmapImage(string file)
            {
                var bmpImg = new BitmapImage();
                bmpImg.BeginInit();
                bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                //bmpImg.DecodePixelWidth = 50;
                bmpImg.CreateOptions = BitmapCreateOptions.None;
                bmpImg.UriSource = new Uri(file);
                bmpImg.EndInit();

                bmpImg.Freeze();

                return bmpImg;
            }
        }

    }
}
