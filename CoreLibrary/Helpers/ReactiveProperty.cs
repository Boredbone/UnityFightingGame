using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;

namespace Boredbone.UnityFightingGame.CoreLibrary.Helpers
{
    public class ReactiveProperty<T> : IObservable<T>, IDisposable
    {

        private BehaviorSubject<T> subject;

        public T Value
        {
            get { return this.subject.Value; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.subject.Value, value))
                {
                    this.subject.OnNext(value);
                }
            }
        }

        public ReactiveProperty(T initialValue)
        {
            this.subject = new BehaviorSubject<T>(initialValue);
        }
        public ReactiveProperty() : this(default(T))
        {

        }


        public void Dispose()
        {
            this.subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subject.Subscribe(observer);
        }
    }
}
