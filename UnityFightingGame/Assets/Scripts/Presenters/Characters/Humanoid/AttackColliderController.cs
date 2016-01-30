using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;
using Boredbone.GameScripts.Helpers;
using System;
using Boredbone.GameScripts.Extensions;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using UniRx;

namespace Boredbone.UnityFightingGame.Presenters.Characters.Humanoid
{
    public class AttackColliderController : BehaviorBase
    {
        private Dictionary<string, GameObject> Colliders { get; set; }


        private Subject<Collider> HitSubject { get; set; }
        public IObservable<Collider> Hit { get { return this.HitSubject.AsObservable(); } }


        private Subject<AttackInformation> GuardingSubject { get; set; }
        public IObservable<AttackInformation> Guarding { get { return this.GuardingSubject.AsObservable(); } }

        public AttackInformation Information { get; private set; }

        public int Id { get; set; }


        protected override void OnAwake()
        {
            base.OnAwake();

            this.HitSubject = new Subject<Collider>().AddTo(this.Disposables);
            this.GuardingSubject = new Subject<AttackInformation>().AddTo(this.Disposables);

            //this.HitSubject.Subscribe(_ => this.ClearCollider()).AddTo(this.Disposables);
        }

        protected override void OnStart()
        {
            base.OnStart();

            this.Information = new AttackInformation();

            this.Colliders = this.transform.AsEnumerable().ToDictionary(y => y.name, y => y.gameObject);

            foreach (var item in this.Colliders)
            {
                var observer = item.Value.GetComponent<TriggerObserver>();
                observer.Triggered.Subscribe(other =>
                {
                    if (other.tag.Equals("VitalBody"))
                    {
                        var target = other.GetComponent<VitalController>();
                        if (target == null || target.Id == this.Id)
                        {
                            return;
                        }
                        target.OnDamaged(this.Information);

                        this.HitSubject.OnNext(other);
                    }
                    else if (other.gameObject.layer == LayerMask.NameToLayer("Attack"))
                    {
                        var target = other.GetComponent<TriggerObserver>();
                        if (target == null)
                        {
                            return;
                        }
                        target.OnGuarding(this.Information);
                    }
                })
                .AddTo(this.Disposables);

                observer.Guarding.Subscribe(this.GuardingSubject).AddTo(this.Disposables);
            }

            this.ClearCollider();
        }





        public void ActivateCollider(string key)
        {
            if (!this.Colliders.ContainsKey(key))
            {
                Debug.Log("key not found");
            }
            this.Information.GenerateNewId();

            this.Colliders.ForEach(y => y.Value.SetActive(y.Key.Equals(key)));
        }

        public void ClearCollider()
        {
            this.Colliders.ForEach(y => y.Value.SetActive(false));
        }


        //public void OnTriggerEnter(Collider other)
        //{
        //
        //    if (other.tag.Equals("VitalBody"))
        //    {
        //        var target = other.GetComponent<VitalController>();
        //        if (target == null)
        //        {
        //            return;
        //        }
        //        target.OnDamaged(this.Information);
        //
        //        this.HitSubject.OnNext(other);
        //    }
        //}
        //
        //void OnTriggerExit(Collider other)
        //{
        //}

    }


}
