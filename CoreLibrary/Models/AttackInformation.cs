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

        public AttackType Type { get; set; }

        private static int number = 0;

        public void GenerateNewId()
        {
            number++;
            if (number > 10000)
            {
                number = 1000;
            }
            this.Id = number;
        }

        public void CopyFrom(AttackInformation other)
        {
            this.Power = other.Power;
            this.StayingTime = other.StayingTime;
            this.Id = other.Id;
            this.Type = other.Type;

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
