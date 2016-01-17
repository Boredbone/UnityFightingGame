using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.UnityFightingGame.CoreLibrary.Helpers;
using UniRx;

namespace Boredbone.UnityFightingGame.CoreLibrary.Models
{
    /// <summary>
    /// this is not thread-safe
    /// </summary>
    public class EffectServer : DisposableBase
    {

        private Subject<UpdateArgs> UpdateSubject { get; }

        private HashSet<EffectRequest> Pool { get; }

        //private Subject<EffectRequest> RequestedSubject { get; }
        public IObservable<EffectRequest> Requested
            => this.UpdateSubject.SelectMany(_ => this.Pool.Where(y => y.IsActive)).Do(y => y.IsActive = false);



        public EffectServer(int initialSize)
        {
            this.UpdateSubject = new Subject<UpdateArgs>().AddTo(this.Disposables);

            this.Pool = new HashSet<EffectRequest>(Enumerable.Range(0, initialSize).Select(y => new EffectRequest(this)));


        }


        public EffectRequest GenerateRequest()
        {
            var item = this.Pool.FirstOrDefault(y => !y.IsActive);
            if (item == null)
            {
                item = new EffectRequest(this);
                this.Pool.Add(item);
            }
            //item.IsActive = true;
            return item;
        }




        public void Update(UpdateArgs args) => this.UpdateSubject.OnNext(args);




    }

    public class EffectRequest
    {
        internal bool IsActive { get; set; }
        private EffectServer Parent { get; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public EffectType Type { get; set; }


        internal EffectRequest(EffectServer parent)
        {
            this.Parent = parent;
        }

        public void Commit() => this.IsActive = true;

    }

    public enum EffectType
    {
        Shock,
        Burst,
        Flash,
    }


}
