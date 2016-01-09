using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;

namespace Boredbone.UnityFightingGame.CoreLibrary.Helpers
{
    public class DisposableBase : IDisposable
    {
        public bool IsDisposed { get { return this.disposables.IsDisposed; } }
        private CompositeDisposable disposables = new CompositeDisposable();
        protected CompositeDisposable Disposables { get { return this.disposables; } }
        public virtual void Dispose() { this.disposables.Dispose(); }
    }
}
