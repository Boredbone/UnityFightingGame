using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;

namespace Boredbone.GameScripts.Helpers
{
    public class BehaviorBase : MonoBehaviour
    {
        protected CompositeDisposable Disposables { get; set; }


        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }


        public void Awake()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
            this.Disposables = new CompositeDisposable();

            this.OnAwake();
        }

        public void Start()
        {
            this.OnStart();
        }



        public void Update()
        {
            this.OnUpdate();
        }
        

        public void OnDestroy()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
        }
    }
}
