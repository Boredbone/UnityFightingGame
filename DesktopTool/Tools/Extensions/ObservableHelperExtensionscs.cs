using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using UniRx;

namespace DesktopTool.Tools.Extensions
{
    public static class ObservableHelperExtensionscs
    {
        public static System.IObservable<T> AsObservable<T>(this UniRx.IObservable<T> target)
            => System.Reactive.Linq.Observable.Create<T>(o => target.Subscribe(o.OnNext, o.OnError, o.OnCompleted));

    }
}
