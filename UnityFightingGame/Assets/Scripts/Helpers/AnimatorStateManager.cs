using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;
using Boredbone.UnityFightingGame.CoreLibrary.Helpers;
using UniRx;

namespace Boredbone.GameScripts.Helpers
{
    public class AnimatorStateManager : DisposableBase
    {
        private List<AnimatorState> States { get; set; }

        public bool IsInTransition { get; private set; }

        public bool IsNotInTransition { get { return !this.IsInTransition; } }

        public int CurrentState { get; private set; }
        public int PreviousState { get; private set; }
        private Subject<int> StateChangedSubject { get; set; }
        public IObservable<int> StateChanged { get { return this.StateChangedSubject.AsObservable(); } }

        private AnimatorState[] ActiveStates { get; set; }
        // { get { return this.States.Where(y => y.IsActive).ToArray(); } }

        public AnimatorStateManager()
        {
            this.States = new List<AnimatorState>();
            this.StateChangedSubject = new Subject<int>().AddTo(this.Disposables);
        }

        public void Register(AnimatorState state)
        {
            this.States.Add(state);
        }

        public void CheckAll(bool isInTransition, int current)
        {
            this.IsInTransition = IsInTransition;

            foreach (var item in this.States)
            {
                item.Check(current);
            }
            this.ActiveStates = this.States.Where(y => y.IsActive).ToArray();

            if (!IsInTransition && this.PreviousState != current)
            {
                this.PreviousState = this.CurrentState;
                this.CurrentState = current;
                this.StateChangedSubject.OnNext(current);
            }
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
