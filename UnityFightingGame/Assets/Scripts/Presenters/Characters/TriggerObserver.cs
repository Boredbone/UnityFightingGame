using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.GameScripts.Helpers;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using UniRx;
using UnityEngine;

namespace Boredbone.UnityFightingGame.Presenters.Characters
{
    class TriggerObserver : BehaviorBase
    {

        private Subject<Collider> TriggeredSubject { get; set; }
        public IObservable<Collider> Triggered { get { return this.TriggeredSubject.AsObservable(); } }


        private Subject<AttackInformation> GuardingSubject { get; set; }
        public IObservable<AttackInformation> Guarding { get { return this.GuardingSubject.AsObservable(); } }


        protected override void OnAwake()
        {
            base.OnAwake();

            this.TriggeredSubject = new Subject<Collider>().AddTo(this.Disposables);
            this.GuardingSubject = new Subject<AttackInformation>().AddTo(this.Disposables);
        }


        public void OnTriggerEnter(Collider other)
        {
            this.TriggeredSubject.OnNext(other);
        }
        //void OnCollisionEnter(Collision other)
        //{
        //    this.TriggeredSubject.OnNext(null);
        //}

        public void OnGuarding(AttackInformation information)
        {
            this.GuardingSubject.OnNext(information);
            //Debug.Log(information.Id.ToString());
        }
    }
}
