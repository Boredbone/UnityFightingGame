using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using UnityEngine;

namespace Boredbone.UnityFightingGame.Presenters
{
    public class InputReceiver
    {

        private AppCore Core { get; set; }

        public InputReceiver(AppCore core)
        {
            this.Core = core;
        }
        


        public void UpdateInputs()
        {
            this.CheckKeyboard(this.Core.KeyboardInput[0].Left, KeyCode.A);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Down, KeyCode.S);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Right, KeyCode.D);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Up, KeyCode.W);

            this.CheckKeyboard(this.Core.KeyboardInput[0].Buttons[0], KeyCode.H);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Buttons[1], KeyCode.J);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Buttons[2], KeyCode.K);
            this.CheckKeyboard(this.Core.KeyboardInput[0].Buttons[3], KeyCode.L);


            this.CheckKeyboard(this.Core.KeyboardInput[1].Left, KeyCode.LeftArrow);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Down, KeyCode.DownArrow);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Right, KeyCode.RightArrow);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Up, KeyCode.UpArrow);

            this.CheckKeyboard(this.Core.KeyboardInput[1].Buttons[0], KeyCode.Keypad1);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Buttons[1], KeyCode.Keypad2);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Buttons[2], KeyCode.Keypad3);
            this.CheckKeyboard(this.Core.KeyboardInput[1].Buttons[3], KeyCode.Keypad4);

        }

        private void CheckKeyboard(KeyStatus target,KeyCode key)
        {
            target.IsDown = Input.GetKey(key);
            target.IsPressed = Input.GetKeyDown(key);
            target.IsReleased = Input.GetKeyUp(key);
        }
        


    }
}
