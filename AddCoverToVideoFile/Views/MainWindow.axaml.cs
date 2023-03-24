using AddCoverToVideoFile.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AddCoverToVideoFile.Views
{
    public partial class MainWindow : BaseWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            AddHandler(DragDrop.DropEvent, Drop);

            this.Closing += (s, e) =>
            {
                if (DataContext != null)
                {
                    if (((MainWindowViewModel)DataContext).IsBusy)
                        e.Cancel = true;
                }
            };
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                if (DataContext != null)
                {
                    var files = e.Data.GetFiles();
                    if (files is not null)
                    {
                        List<string> hoge = new();
                        foreach (var fuga in files)
                        {
                            hoge.Add(fuga.Path.AbsolutePath);
                        }

                        ((MainWindowViewModel)DataContext).OnFileDrop(filepaths: hoge);
                    }
                }
            }
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
                Patterns = new string[] { "*.sln", "*.csproj" }
            };
            openFileDialog.FileTypeFilter = new FilePickerFileType[] { type };

            //var result = await openFileDialog.ShowAsync(this);
            var result = await StorageProvider.OpenFilePickerAsync(openFileDialog);
            if (result != null)
            {
                if (DataContext != null)
                {
                    List<string> hoge = new();
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
            this.WhenActivated((CompositeDisposable disposable) =>
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
