using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using System;
using AddCoverToVideoFile.ViewModels;
using System.Collections.Immutable;
using Avalonia.ReactiveUI;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using System.Collections.Generic;

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
                if ((DataContext as MainWindowViewModel).IsBusy)
                    e.Cancel = true;
            };
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                (DataContext as MainWindowViewModel).OnFileDrop(e.Data.GetFileNames());
            }
        }

        private async void OnFileOpenButtonClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Select movie file and picture",
                AllowMultiple = true
            };

            var Filters = new List<FileDialogFilter>();
            var filter = new FileDialogFilter();

            List<string> extension = new()
            {
                "mp4",
                "mkv",
                //"avi",
                "jpg",
                "jpeg",
                "png"
            };

            filter.Extensions = extension;
            Filters.Add(filter);
            openFileDialog.Filters = Filters;
            var result = await openFileDialog.ShowAsync(this);

            if (result != null)
            {
                (DataContext as MainWindowViewModel).OnFileDrop(result);
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
