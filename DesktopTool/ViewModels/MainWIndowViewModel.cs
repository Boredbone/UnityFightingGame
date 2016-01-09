using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using Boredbone.UnityFightingGame.CoreLibrary;
using DesktopTool.Tools.Extensions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace DesktopTool.ViewModels
{
    internal class MainWIndowViewModel : IDisposable
    {

        private CompositeDisposable Disposables { get; }
        public ReactiveProperty<int> Number { get; }
        public ReactiveCommand IncrementCommand { get; }

        private TestModel Model { get; }


        public MainWIndowViewModel()
        {
            this.Disposables = new CompositeDisposable();

            this.Model = new TestModel().AddTo(this.Disposables);



            this.Number = this.Model.ValueChanged.AsObservable().ToReactiveProperty().AddTo(this.Disposables);

            this.IncrementCommand = new ReactiveCommand().AddTo(this.Disposables);
            this.IncrementCommand.Subscribe(y =>
            {
                this.Model.StartHeavyProcess();
            })
            .AddTo(this.Disposables);


        }


        public void Dispose()
        {
            this.Disposables.Dispose();
        }
    }
}
