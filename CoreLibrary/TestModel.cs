using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Boredbone.UnityFightingGame.CoreLibrary
{
    public class TestModel : IDisposable
    {

        private CompositeDisposable Disposables { get; }

        private BehaviorSubject<int> ValueUpdatedSubject { get; }
        public IObservable<int> ValueChanged => this.ValueUpdatedSubject.AsObservable();

        public TestModel()
        {
            this.Disposables = new CompositeDisposable();

            this.ValueUpdatedSubject = new BehaviorSubject<int>(0).AddTo(this.Disposables);
        }

        public void StartHeavyProcess()
        {
            var _ = this.ProcessAsync();
        }

        private async Task ProcessAsync()
        {
            await Task.Delay(1000);
            this.ValueUpdatedSubject.OnNext(this.ValueUpdatedSubject.Value + 1);
        }

        public void Update()
        {
            this.ValueUpdatedSubject.OnNext(this.ValueUpdatedSubject.Value + 1);
        }

        public void Dispose()
        {
            this.Disposables.Dispose();
        }
    }
}
