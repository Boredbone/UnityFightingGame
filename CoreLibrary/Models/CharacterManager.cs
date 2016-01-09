using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boredbone.UnityFightingGame.CoreLibrary.Models
{
    public class CharacterManager
    {

        private AppCore Core { get; set; }

        private List<ICharacter> Characters { get; set; }

        private PlayerSettings[] Players = new[]
        {
            new PlayerSettings()
            {
                CharacterType = CharacterType.Humanoid,
                InputType = InputType.Keyboard,
                InputIndex = 0,
                Team = 0,
                Mirror = false,
                ColorIndex = 0,
            },
            new PlayerSettings()
            {
                CharacterType = CharacterType.Humanoid,
                InputType = InputType.Keyboard,
                InputIndex = 1,
                Team = 1,
                Mirror = true,
                ColorIndex = 1,
            },
        };


        public CharacterManager(AppCore core)
        {
            this.Core = core;
            this.Characters = new List<ICharacter>();
        }

        public void Initialize()
        {
            this.Characters.ForEach(y => y.Dispose());
            this.Characters.Clear();
        }

        //public void Register(ICharacter charcter)
        //{
        //    this.Characters.Add(charcter);
        //}


        public T GetModel<T>() where T : ICharacter, new()
        {
            var model = new T();

            var settingList = this.Players.Where(y => y.CharacterType == model.CharacterType).ToArray();

            var existsBrothers = this.Characters.Where(y => y.CharacterType == model.CharacterType).ToArray();

            //if (settingList.Length <= existsBrothers.Length)
            //{
            //    throw new ArgumentException("player over");
            //}

            var player = settingList[existsBrothers.Length];

            //if(this.Characters.Count(y=>y.CharacterType==))

            model.Initialize(this.Core, player);

            this.Characters.Add(model);
            return model;
        }


        public void Update(UpdateArgs args)
        {
            this.Characters.ForEach(y => y.Update(args));
        }
    }

    public enum InputType
    {
        Keyboard,
        Gamepad,
        AI,
        LeapMotion,
    }


    public interface ICharacter : IDisposable
    {
        void Initialize(AppCore core, PlayerSettings player);
        void Update(UpdateArgs args);
        CharacterType CharacterType { get; }
    }

    public enum CharacterType
    {
        Humanoid,
        Hand,
    }

    public class PlayerSettings
    {
        public InputType InputType { get; set; }
        public int InputIndex { get; set; }

        //public int PlayerIndex { get; set; }
        public CharacterType CharacterType { get; set; }
        public int ColorIndex { get; set; }

        public int Team { get; set; }
        public bool Mirror { get; set; }
    }


}
