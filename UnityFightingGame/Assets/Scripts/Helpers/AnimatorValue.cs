using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boredbone.GameScripts.Helpers
{

    public abstract class AnimatorValue<T>
    {
        protected readonly int hash;
        protected readonly Animator animator;

        public AnimatorValue(string name, Animator animator)
        {
            this.animator = animator;
            this.hash = Animator.StringToHash(name);
        }
    }



    public class AnimatorValueFloat : AnimatorValue<float>
    {
        public float Value
        {
            get { return this.animator.GetFloat(this.hash); }
            set { this.animator.SetFloat(this.hash, value); }
        }

        public AnimatorValueFloat(string name, Animator animator) : base(name, animator)
        {
        }
    }
    public class ReadOnlyAnimatorValueFloat : AnimatorValue<float>
    {
        public float Value
        {
            get { return this.animator.GetFloat(this.hash); }
        }

        public ReadOnlyAnimatorValueFloat(string name, Animator animator) : base(name, animator)
        {
        }
    }
    public class AnimatorValueBool : AnimatorValue<bool>
    {
        public bool Value
        {
            get { return this.animator.GetBool(this.hash); }
            set { this.animator.SetBool(this.hash, value); }
        }

        public AnimatorValueBool(string name, Animator animator) : base(name, animator)
        {
        }
    }
    public class ReadOnlyAnimatorValueBool : AnimatorValue<bool>
    {
        public bool Value
        {
            get { return this.animator.GetBool(this.hash); }
        }

        public ReadOnlyAnimatorValueBool(string name, Animator animator) : base(name, animator)
        {
        }
    }
    
}
