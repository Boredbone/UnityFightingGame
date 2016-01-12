using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.GameScripts.Helpers;
using UniRx;
using UnityEngine;

namespace Boredbone.UnityFightingGame.Presenters.Characters
{
    class TriggerObserver : BehaviorBase
    {

        private Subject<Collider> TriggeredSubject { get; set; }
        public IObservable<Collider> Triggered { get { return this.TriggeredSubject.AsObservable(); } }

        protected override void OnAwake()
        {
            base.OnAwake();

            this.TriggeredSubject = new Subject<Collider>().AddTo(this.Disposables);
        }


        public void OnTriggerEnter(Collider other)
        {
            this.TriggeredSubject.OnNext(other);
        }
        //void OnCollisionEnter(Collision other)
        //{
        //    this.TriggeredSubject.OnNext(null);
        //}
    }
}
