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

        private Subject<Unit> UpdatedSubject { get; }
        public IObservable<Unit> Updated => this.UpdatedSubject.AsObservable(); 

        public CharacterType CharacterType => CharacterType.Humanoid;

        private PlayerSettings Player { get; set; }

        public int ColorIndex => this.Player.ColorIndex;
        public bool Mirror => this.Player.Mirror;

        public int InitialPosition => this.Player.Team == 0 ? -1 : 1;
        
        public ReactiveProperty<int> Life { get; }
        //private float Life;
        private int LifeMax = 0;

        private AppCore Core { get; set; }

        private Subject<Unit> MoveStartSubject { get; set; }
        private Subject<Unit> MoveStopSubject { get; set; }
        private Subject<Unit> PunchSubject { get; set; }

        public int Id { get; set; }

        private bool isLastDamaged = false;
        //private float lastDamage = 0f;
        //private AttackType lastAttackType;

        private AttackInformation LastDamage { get; }
        private int guardingAttack = -1;


        public HumanoidModel()
        {
            this.ViewParameters = new ViewParameters();
            this.DesiredParameters = new DesiredParameters();

            this.LastDamage = new AttackInformation();

            this.UpdatedSubject = new Subject<Unit>().AddTo(this.Disposables);

            this.MoveStartSubject = new Subject<Unit>().AddTo(this.Disposables);
            this.MoveStopSubject = new Subject<Unit>().AddTo(this.Disposables);

            this.MoveStartSubject
                .TimeInterval()
                .Where(y => y.Interval < TimeSpan.FromMilliseconds(400))
                .Select(_ => true)
                .Merge(this.MoveStopSubject.Select(_ => false))
                .Subscribe(y => this.DesiredParameters.IsRunning = y)
                .AddTo(this.Disposables);


            this.PunchSubject = new Subject<Unit>().AddTo(this.Disposables);

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
            // User input
            this.DecodeInput();

            // Damage
            if (this.isLastDamaged)
            {
                if (this.LastDamage.Id == this.guardingAttack)
                {
                    this.DesiredParameters.IsGuarding = true;
                    this.DesiredParameters.IsDamaged = false;
                }
                else
                {
                    this.Life.Value += (int)this.LastDamage.Power;

                    this.DesiredParameters.IsGuarding = false;
                    this.DesiredParameters.IsDamaged = true;
                    this.DesiredParameters.DamageType = this.LastDamage.Type;


                    // effect
                    var effect = this.Core.Effect.GenerateRequest();

                    effect.Z = 0;
                    effect.Y = this.ViewParameters.VerticalPosition;
                    effect.X = this.ViewParameters.HorizontalPosition;

                    //this.Core.Log($"{effect.X}, {effect.Y}, {effect.Z}");

                    effect.Type = EffectType.Burst;
                    effect.Commit();


                }
                this.isLastDamaged = false;
                this.LastDamage.Power = 0f;
            }
            else
            {

                this.DesiredParameters.IsGuarding = false;
                this.DesiredParameters.IsDamaged = false;
            }

            // Update View
            this.UpdatedSubject.OnNext(Unit.Default);
        }

        private void DecodeInput()
        {

            if (this.Player.InputType == InputType.Keyboard)
            {
                var key = this.Core.KeyboardInput[this.Player.InputIndex];

                this.DesiredParameters.HorizontalVelocity = (key.Right.IsDown ? 1 : 0) - (key.Left.IsDown ? 1 : 0);

                if (key.Right.IsPressed || key.Left.IsPressed)
                {
                    this.MoveStartSubject.OnNext(Unit.Default);
                }
                if (key.Right.IsReleased || key.Left.IsReleased)
                {
                    this.MoveStopSubject.OnNext(Unit.Default);
                }

                this.DesiredParameters.Jump = key.Up.IsPressed;
                this.DesiredParameters.Crouch = key.Down.IsDown;

                this.DesiredParameters.Punch = key.Buttons[0].IsPressed;
                this.DesiredParameters.Kick = key.Buttons[1].IsPressed;

                if (this.DesiredParameters.Punch)
                {
                    this.PunchSubject.OnNext(Unit.Default);
                }

                //this.DesiredParameters.RushPunch = key.Buttons[2].IsDown;
                this.DesiredParameters.Rest = key.Buttons[3].IsPressed;

            }
        }

        public void OnDamaged(AttackInformation information)
        {
            this.isLastDamaged = true;
            this.LastDamage.CopyFrom(information);
            //this.lastDamage = information.Power;
            //this.lastAttackType = information.Type;
            //this.Life.Value += (int)information.Power;
        }


        public void OnGuarding(AttackInformation information)
        {
            this.guardingAttack = information.Id;
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

        public bool IsDamaged { get; internal set; }
        public AttackType DamageType { get; internal set; }

        public bool IsGuarding { get; internal set; }

        internal DesiredParameters()
        {

        }

    }
}
