using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boredbone.GameScripts.Helpers
{
    public class AnimationTrigger
    {

        private readonly Animator animator;
        private readonly string name;

        public AnimationTrigger(string name, Animator animator)
        {
            this.animator = animator;
            this.name = name;
        }

        public void Set()
        {
            this.animator.SetTrigger(this.name);
        }

        public void Clear()
        {
            this.animator.ResetTrigger(this.name);
        }
    }

}
