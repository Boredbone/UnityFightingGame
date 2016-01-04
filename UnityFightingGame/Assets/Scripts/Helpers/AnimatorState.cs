using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;

namespace Boredbone.GameScripts.Helpers
{

    public class AnimatorState
    {

        public int Hash { get; private set; }
        public string Trigger { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsAutoTriggerClear { get; set; }

        public HashSet<string> Tags { get; private set; }


        public AnimatorState(string name, string trigger, AnimatorStateManager manager)
        {
            this.Hash = Animator.StringToHash(name);
            this.Trigger = trigger;
            this.IsAutoTriggerClear = trigger != null;

            this.Tags = new HashSet<string>();

            manager.Register(this);
        }

        public bool Check(int current)
        {
            this.IsActive = current == this.Hash;
            return this.IsActive;
        }

        public void Request(Animator animator, bool value)
        {
            if (this.Trigger == null)
            {
                return;
            }
            animator.SetBool(this.Trigger, value);
        }

        public void Request(Animator animator)
        {
            this.Request(animator, true);
        }
    }

}
