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
    public class AttackColliderControllr : MonoBehaviour
    {
        private Dictionary<string,GameObject> Colliders { get; set; }


        private Subject<Collider> HitSubject { get; set; }
        private CompositeDisposable Disposables { get; set; }

        public AttackInformation Information { get; private set; }



        public void Awake()
        {
        }



        public void Start()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
            this.Disposables = new CompositeDisposable();
            this.HitSubject = new Subject<Collider>().AddTo(this.Disposables);

            this.Information = new AttackInformation();

            this.Colliders = this.transform.AsEnumerable().ToDictionary(y => y.name, y => y.gameObject);
            this.ClearCollider();
        }



        public void Update()
        {

        }


        public void ActivateCollider(string key)
        {
            if (!this.Colliders.ContainsKey(key))
            {
                Debug.Log("key not found");
            }
            this.Colliders.ForEach(y => y.Value.SetActive(y.Key.Equals(key)));
        }

        public void ClearCollider()
        {
            this.Colliders.ForEach(y => y.Value.SetActive(false));
        }


        public void OnTriggerEnter(Collider other)
        {

            if (other.tag.Equals("VitalBody"))
            {
                //var target = other.GetComponent<Vital>();
                //target.OnHIt(this);

                this.HitSubject.OnNext(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
        }


        public void OnDestroy()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
        }
    }

    public class AttackInformation
    {
        public float Power { get; set; }

    }
}
