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

namespace Boredbone.UnityFightingGame.Scripts.Presenters.Characters.Humanoid
{
    public class VitalController : BehaviorBase
    {

        private Subject<AttackInformation> DamagedSubject { get; set; }
        public IObservable<AttackInformation> Damaged { get { return this.DamagedSubject.AsObservable(); } }

        public int Id { get; set; }

        protected override void OnStart()
        {
            base.OnStart();
            this.DamagedSubject = new Subject<AttackInformation>().AddTo(this.Disposables);
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

        public void OnDamaged(AttackInformation information)
        {
            this.DamagedSubject.OnNext(information);
        }
    }
}
