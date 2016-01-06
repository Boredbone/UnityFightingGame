using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;

namespace Boredbone.GameScripts.Helpers
{
    public class AnimatorStateManager
    {
        private List<AnimatorState> States { get; set; }

        public bool IsInTransition { get; set; }

        public bool IsNotInTransition { get { return !this.IsInTransition; } }


        private AnimatorState[] ActiveStates { get; set; }
        // { get { return this.States.Where(y => y.IsActive).ToArray(); } }

        public AnimatorStateManager()
        {
            this.States = new List<AnimatorState>();

        }

        public void Register(AnimatorState state)
        {
            this.States.Add(state);
        }

        public void CheckAll(int current)
        {
            foreach (var item in this.States)
            {
                item.Check(current);
            }
            this.ActiveStates = this.States.Where(y => y.IsActive).ToArray();
        }

        public bool HasTag(string tag)
        {
            return this.ActiveStates.Any(y => y.Tags.Contains(tag));
        }

        //public void ClearTriggers(Animator animator)
        //{
        //    if (this.IsInTransition)
        //    {
        //        return;
        //    }
        //
        //    foreach (var item in this.States)
        //    {
        //        if (item.IsAutoTriggerClear && item.IsActive && item.Trigger != null)
        //        {
        //            animator.SetBool(item.Trigger, false);
        //        }
        //    }
        //
        //
        //}
    }
}
