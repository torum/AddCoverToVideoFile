using Avalonia.Input;
using ReactiveUI.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace AddCoverToVideoFile.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
    }

}
