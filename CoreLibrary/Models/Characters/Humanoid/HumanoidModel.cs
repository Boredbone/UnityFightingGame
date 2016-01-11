using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.UnityFightingGame.CoreLibrary.Helpers;
using UniRx;

namespace Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid
{
    public class HumanoidModel : DisposableBase, ICharacter
    {

        public ViewParameters ViewParameters { get; }
        public DesiredParameters DesiredParameters { get; }

        private Subject<bool> UpdatedSubject { get; }
        public IObservable<bool> Updated => this.UpdatedSubject.AsObservable(); 

        public CharacterType CharacterType => CharacterType.Humanoid;

        private PlayerSettings Player { get; set; }

        public int ColorIndex => this.Player.ColorIndex;
        public bool Mirror => this.Player.Mirror;

        public int InitialPosition => this.Player.Team == 0 ? -1 : 1;
        
        public ReactiveProperty<int> Life { get; }
        //private float Life;
        private int LifeMax = 0;

        private AppCore Core { get; set; }

        private Subject<bool> MoveStartSubject { get; set; }
        private Subject<bool> MoveStopSubject { get; set; }
        private Subject<bool> PunchSubject { get; set; }

        public int Id { get; set; }

        public HumanoidModel()
        {
            this.ViewParameters = new ViewParameters();
            this.DesiredParameters = new DesiredParameters();

            this.UpdatedSubject = new Subject<bool>().AddTo(this.Disposables);

            this.MoveStartSubject = new Subject<bool>().AddTo(this.Disposables);
            this.MoveStopSubject = new Subject<bool>().AddTo(this.Disposables);

            this.MoveStartSubject
                .TimeInterval()
                .Where(y => y.Interval < TimeSpan.FromMilliseconds(400))
                .Select(_ => true)
                .Merge(this.MoveStopSubject.Select(_ => false))
                .Subscribe(y => this.DesiredParameters.IsRunning = y)
                .AddTo(this.Disposables);


            this.PunchSubject = new Subject<bool>().AddTo(this.Disposables);

            this.PunchSubject
                .TimeInterval()
                .Where(y => y.Interval < TimeSpan.FromMilliseconds(200))
                .Select(_ => true)
                .Merge(this.PunchSubject.Throttle(TimeSpan.FromMilliseconds(200)).Select(_ => false))
                .Subscribe(y => this.DesiredParameters.RushPunch = y)
                .AddTo(this.Disposables);

            this.Life = new ReactiveProperty<int>(this.LifeMax).AddTo(this.Disposables);

        }

        public void Initialize(AppCore core, PlayerSettings player)
        {
            this.Life.Value = this.LifeMax;
            this.Player = player;
            this.Core = core;
        }


        public void Update(UpdateArgs args)
        {
            this.DecodeInput();

            this.UpdatedSubject.OnNext(true);
        }

        private void DecodeInput()
        {

            if (this.Player.InputType == InputType.Keyboard)
            {
                var key = this.Core.KeyboardInput[this.Player.InputIndex];

                this.DesiredParameters.HorizontalVelocity = (key.Right.IsDown ? 1 : 0) - (key.Left.IsDown ? 1 : 0);

                if (key.Right.IsPressed || key.Left.IsPressed)
                {
                    this.MoveStartSubject.OnNext(true);
                }
                if (key.Right.IsReleased || key.Left.IsReleased)
                {
                    this.MoveStopSubject.OnNext(true);
                }

                this.DesiredParameters.Jump = key.Up.IsPressed;
                this.DesiredParameters.Crouch = key.Down.IsDown;

                this.DesiredParameters.Punch = key.Buttons[0].IsPressed;
                this.DesiredParameters.Kick = key.Buttons[1].IsPressed;

                if (this.DesiredParameters.Punch)
                {
                    this.PunchSubject.OnNext(true);
                }

                //this.DesiredParameters.RushPunch = key.Buttons[2].IsDown;
                this.DesiredParameters.Rest = key.Buttons[3].IsPressed;

            }
        }

        public void OnDamaged(AttackInformation information)
        {
            this.Life.Value += information.Power;
        }
    }
    public class ViewParameters
    {

        public float HorizontalPosition { get; set; }
        public float VerticalPosition { get; set; }

        internal ViewParameters()
        {

        }
    }

    public class DesiredParameters
    {
        public float HorizontalVelocity { get; internal set; }
        //public float VerticalVelocity { get; internal set; }

        public bool IsRunning { get; internal set; }

        public bool Jump { get; internal set; }
        public bool Crouch { get; internal set; }

        public bool Punch { get; internal set; }
        public bool RushPunch { get; internal set; }
        public bool Kick { get; internal set; }
        public bool Rest { get; internal set; }

        internal DesiredParameters()
        {

        }

    }
}
