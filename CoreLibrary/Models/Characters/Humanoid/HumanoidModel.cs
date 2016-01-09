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

        private float Life;
        private float LifeMax = 100;

        private AppCore Core { get; set; }


        public HumanoidModel()
        {
            this.ViewParameters = new ViewParameters();
            this.DesiredParameters = new DesiredParameters();

            this.UpdatedSubject = new Subject<bool>().AddTo(this.Disposables);


        }

        public void Initialize(AppCore core, PlayerSettings player)
        {
            this.Life = this.LifeMax;
            this.Player = player;
            this.Core = core;
        }


        public void Update(UpdateArgs args)
        {
            if (this.Player.InputType == InputType.Keyboard)
            {
                var key = this.Core.KeyboardInput[this.Player.InputIndex];

                this.DesiredParameters.HorizontalVelocity = ((key.Right.IsDown ? 1 : 0) - (key.Left.IsDown ? 1 : 0)) * 2f;

                this.DesiredParameters.Jump = key.Up.IsPressed;
                this.DesiredParameters.Crouch = key.Down.IsDown;

                this.DesiredParameters.Punch = key.Buttons[0].IsPressed;
                this.DesiredParameters.Kick = key.Buttons[1].IsPressed;
                this.DesiredParameters.RushPunch = key.Buttons[2].IsDown;
                this.DesiredParameters.Rest = key.Buttons[4].IsPressed;

            }


            this.UpdatedSubject.OnNext(true);
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
