using AddCoverToVideoFile.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace AddCoverToVideoFile.Views
{
    public partial class MainWindow : BaseWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateThemeBackground(ActualThemeVariant);
            this.ActualThemeVariantChanged += OnActualThemeVariantChanged;

            AddHandler(DragDrop.DropEvent, Drop);
            AddHandler(DragDrop.DragOverEvent, DragOver);

            this.Closing += (s, e) =>
            {
                if (DataContext != null)
                {
                    if (((MainWindowViewModel)DataContext).IsBusy)
                        e.Cancel = true;
                }
            };
        }

        private void UpdateThemeBackground(ThemeVariant theme)
        {
            //(App.Current as App)!.RequestedThemeVariant
            //ActualThemeVariant

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.BackgroundLayerBorder.IsVisible = true;
                this.BackgroundLayerBorder.Opacity = 0.9;

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 1803))
                {
                    // Get the window's platform handle
                    var handle = this.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
                    if (handle != IntPtr.Zero)
                    {
                        this.Background = Brushes.Transparent;
                        //this.TransparencyLevelHint = [WindowTransparencyLevel.None];

                        this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                        //EnableBlurBehind(handle);
                    }
                    else
                    {
                        this.Background = Brushes.Transparent;
                        this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                        //this.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
                    }
                }
                else
                {
                    this.Background = Brushes.Transparent;
                    this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                    //this.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (theme == ThemeVariant.Dark)
                {
                    //this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#121212"));
                }
                else if (theme == ThemeVariant.Light)
                {
                    //this.TransparencyLevelHint = [WindowTransparencyLevel.Blur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#EEEEEE"));
                }
                else if (theme == ThemeVariant.Default)
                {
                    //this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#121212"));
                }
            }
            else
            {
                this.BackgroundLayerBorder.IsVisible = true;
                this.BackgroundLayerBorder.Opacity = 0.9;

                if (theme == ThemeVariant.Dark)
                {
                    this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#222222"));
                }
                else if (theme == ThemeVariant.Light)
                {
                    this.TransparencyLevelHint = [WindowTransparencyLevel.Blur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                }
                else if (theme == ThemeVariant.Default)
                {
                    this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                    this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#121212"));
                }
            }
        }

        private void OnActualThemeVariantChanged(object? sender, EventArgs e)
        {
            UpdateThemeBackground(ActualThemeVariant);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (e.DataTransfer is null)
            {
                return;
            }

            // Only allow copy effect for file drops
            if (e.DataTransfer.Contains(DataFormat.File))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }

            // Deprecated.
            /*
            e.DragEffects = e.Data.Contains(DataFormats.Files)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            */
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.DataTransfer is null)
            {
                return;
            }

            if (e.DataTransfer.Contains(DataFormat.File))
            {
                var fileNames = e.DataTransfer.GetItems(DataFormat.File)?.ToList(); //e.Data.GetFiles()?.ToList(); Deprecated.
                if (fileNames is not null && fileNames.Count != 0)
                {
                    var droppedFiles = new List<string>();
                    foreach (var file in fileNames)
                    {
                        var filePath = file.TryGetFile()?.TryGetLocalPath(); //file.TryGetLocalPath(); Deprecated.
                        if (filePath != null)
                        {
                            droppedFiles.Add(filePath);
                        }
                    }

                    if (this.DataContext is MainWindowViewModel vm)
                    {
                        vm.OnFileDrop(filepaths: droppedFiles);
                    }
                }
            }

            /*
             * Deprecated
            if (e.Data.Contains(DataFormats.Files))
            {
                if (DataContext != null)
                {
                    var files = e.Data.GetFiles();
                    if (files is not null)
                    {
                        List<string> hoge = [];
                        foreach (var fuga in files)
                        {
                            hoge.Add(fuga.Path.AbsolutePath);
                        }

                        ((MainWindowViewModel)DataContext).OnFileDrop(filepaths: hoge);
                    }
                }
            }
            */
        }

        private async void OnFileOpenButtonClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FilePickerOpenOptions()//OpenFileDialog()
            {
                Title = "Select movie file and picture",
                AllowMultiple = true
            };

            //var Filters = new List<FileDialogFilter>();
            //var filter = new FileDialogFilter();

            //List<string> extension = new()
            //{
            //    "mp4",
            //    "mkv",
            //    //"avi",
            //    "jpg",
            //    "jpeg",
            //    "png"
            //};

            //filter.Extensions = extension;
            //Filters.Add(filter);
            //openFileDialog.Filters = Filters;
            var type = new FilePickerFileType("files (*.mp4; *.mkv; *.jpg; *.jpeg; *.png)")
            {
                Patterns = ["*.mp4", "*.mkv", "*.jpg", "*.jpeg", "*.png"]
            };
            openFileDialog.FileTypeFilter = [type];

            //var result = await openFileDialog.ShowAsync(this);
            var result = await StorageProvider.OpenFilePickerAsync(openFileDialog);
            if (result != null)
            {
                if (DataContext != null)
                {
                    List<string> hoge = [];
                    foreach (var filePath in result)
                    {
                        hoge.Add(filePath.Path.AbsolutePath);
                    }

                    ((MainWindowViewModel)DataContext).OnFileDrop(hoge);
                }
            }
        }
    }

    public abstract class BaseWindow<T> : ReactiveWindow<T> where T : ViewModelBase
    {
        protected BaseWindow()
        {
            this.WhenActivated(disposable =>
            {
                this.ViewModel.WhenAnyValue(x => x.IsBusy)
                    .Do(UpdateCursor)
                    .Subscribe()
                    .DisposeWith(disposable);
            });
        }

        private void UpdateCursor(bool show)
        {
            this.Cursor = show ? new Cursor(StandardCursorType.Wait) : Cursor.Default;
        }
    }
}
