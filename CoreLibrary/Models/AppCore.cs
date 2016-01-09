using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boredbone.UnityFightingGame.CoreLibrary.Models
{
    public class AppCore
    {
        private static AppCore _instance = new AppCore();

        public CharacterManager Characters { get; private set; }

        private UpdateArgs UpdateArgs { get; set; }

        public KeyInputStatus[] KeyboardInput { get; private set; }


        private AppCore()
        {
            this.Characters = new CharacterManager(this);

            this.UpdateArgs = new UpdateArgs();

            this.KeyboardInput = Enumerable.Range(0, 2).Select(_ => new KeyInputStatus()).ToArray();
        }

        public void Initialize()
        {
            this.Characters.Initialize();
        }

        public static AppCore GetEnvironment(AppEnvironmentArgs args)
        {
            return _instance;
        }

        public void Update(float elapsedTime)
        {
            this.UpdateArgs.ElapsedTime = elapsedTime;

            this.Characters.Update(this.UpdateArgs);
        }
    }

    public class AppEnvironmentArgs
    {

    }


    public class UpdateArgs
    {
        public float ElapsedTime { get; internal set; }
    }

    public class KeyInputStatus
    {
        public KeyStatus[] Buttons { get; private set; }
        public KeyStatus Up { get; private set; }
        public KeyStatus Down { get; private set; }
        public KeyStatus Left { get; private set; }
        public KeyStatus Right { get; private set; }

        internal KeyInputStatus()
        {
            this.Buttons = Enumerable.Range(0, 4).Select(_ => new KeyStatus()).ToArray();
            this.Up = new KeyStatus();
            this.Down = new KeyStatus();
            this.Left = new KeyStatus();
            this.Right = new KeyStatus();

        }

    }

    public class KeyStatus
    {
        public bool IsPressed { get; set; }
        public bool IsDown { get; set; }
        public bool IsReleased { get; set; }

        internal KeyStatus()
        {

        }
    }
}
