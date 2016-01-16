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
    public class VitalController : BehaviorBase
    {

        private Subject<AttackInformation> DamagedSubject { get; set; }
        public IObservable<AttackInformation> Damaged { get { return this.DamagedSubject.AsObservable(); } }

        public bool IsEnabled { get; private set; }

        private float invincibleTime;
        private int lastAttackId;

        public int Id { get; set; }

        protected override void OnStart()
        {
            base.OnStart();
            this.DamagedSubject = new Subject<AttackInformation>().AddTo(this.Disposables);
            this.invincibleTime = 0f;
            this.lastAttackId = -1;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public void OnTriggerEnter(Collider other)
        {
        }

        public void OnTriggerExit(Collider other)
        {
        }

        public bool OnDamaged(AttackInformation information)
        {
            //Debug.Log(information.Id.ToString() + ", " + this.lastAttackId.ToString() + ", " 
            //    + this.invincibleTime.ToString() + ", " + Time.time.ToString());
            if (information.Id != this.lastAttackId && this.invincibleTime < Time.time)
            {
                this.lastAttackId = information.Id;
                this.invincibleTime = information.StayingTime + Time.time;
                this.DamagedSubject.OnNext(information);
                return true;
            }
            return false;
        }
    }
}
