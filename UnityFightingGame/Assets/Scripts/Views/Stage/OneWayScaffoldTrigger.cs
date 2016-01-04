using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boredbone.UnityFightingGame.Scripts.Views.Stage
{
    public class OneWayScaffoldTrigger : MonoBehaviour
    {
        public GameObject target;

        private Collider targetCollider;

        public void Start()
        {
            this.targetCollider = target.GetComponent<Collider>();
            //Physics.IgnoreCollision(target.GetComponent<Collider>());
        }

        public void OnTriggerEnter(Collider other)
        {
            if (this.target == null)
            {
                return;
            }
            Physics.IgnoreCollision(this.targetCollider, other);
        }

        void OnTriggerExit(Collider other)
        {
            if (this.target == null)
            {
                return;
            }
            Physics.IgnoreCollision(this.targetCollider, other, false);
        }
    }
}
