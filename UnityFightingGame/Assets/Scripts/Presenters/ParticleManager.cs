using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.GameScripts.Helpers;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using UniRx;
using UnityEngine;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;

namespace Boredbone.UnityFightingGame.Presenters
{
    public class ParticleManager : BehaviorBase
    {

        public Transform burst;
        public Transform burstSmall;
        public Transform shock;

        private HashSet<EffectObject> Pool { get; set; }

        protected override void OnAwake()
        {
            base.OnAwake();

            this.Pool = new HashSet<EffectObject>();
            

            this.Pool.Add(new EffectObject(this.burst, EffectType.Burst));
            this.Pool.Add(new EffectObject(this.burstSmall, EffectType.BurstSmall));
            this.Pool.Add(new EffectObject(this.shock, EffectType.Shock));

        }


        public void Execute(EffectRequest request)
        {

            var obj = this.Pool.Where(y => y.Type == request.Type).FirstOrDefault(y => !y.Particle.IsAlive());
            //||y.Particle.time>y.Particle.startLifetime);

            //Debug.Log(((obj == null) ? "no idle effect" : "use pool") + ", " + this.Pool.Count);

            if (obj == null)
            {
                Transform tr = null;
                switch (request.Type)
                {
                    case EffectType.Burst:
                        tr = Instantiate(this.burst);
                        break;
                    case EffectType.BurstSmall:
                        tr = Instantiate(this.burstSmall);
                        break;
                    case EffectType.Flash:
                        tr = Instantiate(this.burst);
                        break;
                    case EffectType.Shock:
                        tr = Instantiate(this.shock);
                        break;
                }

                obj = new EffectObject(tr, request.Type);
                

                this.Pool.Add(obj);
            }
            
            obj.Particle.Stop();
            obj.Particle.Clear();//.Stop();

            //obj.Particle.loop = true;
            obj.Container.position = new Vector3(request.X, request.Y, request.Z);
            obj.Particle.Play();


        }

    }

    class EffectObject
    {
        public Transform Container { get; private set; }
        public ParticleSystem Particle { get; private set; }
        public EffectType Type { get; private set; }

        public EffectObject(Transform container, EffectType type)
        {
            this.Container = container;
            this.Particle = container.GetComponent<ParticleSystem>();
            this.Type = type;
        }
    }
}
