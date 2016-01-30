using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boredbone.UnityFightingGame.CoreLibrary.Models
{
    public class AttackInformation
    {
        public float Power { get; set; }

        public float StayingTime { get; set; }

        public int Id { get; private set; }
        //private int _fieldId;
        //public int Id
        //{
        //    get { return _fieldId; }
        //    set
        //    {
        //        if (_fieldId != value)
        //        {
        //            _fieldId = value;
        //            this.OnIdChanged();
        //        }
        //    }
        //}
        

        public float SourcePositionHorizontal { get; set; }
        public float SourcePositionVertical { get; set; }

        public AttackType Type { get; set; }

        public EffectType Effect { get; set; }

        private static int number = 0;

        public void GenerateNewId()
        {
            number++;
            if (number > 10000)
            {
                number = 1000;
            }
            this.Id = number;
            this.OnIdChanged();

        }

        private void OnIdChanged()
        {
            this.Effect = EffectType.Burst;
            this.StayingTime = 0f;

        }

        public void CopyFrom(AttackInformation other)
        {
            this.Id = other.Id;
            this.Power = other.Power;
            this.StayingTime = other.StayingTime;
            this.Type = other.Type;

            this.Effect = other.Effect;

            this.SourcePositionHorizontal = other.SourcePositionHorizontal;
            this.SourcePositionVertical = other.SourcePositionVertical;
        }
    }

    public enum AttackType
    {
        Weak,
        Strong,
        Stun,
        Fly,
    }
}
