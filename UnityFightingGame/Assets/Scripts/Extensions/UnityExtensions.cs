using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boredbone.GameScripts.Extensions
{
    public static class UnityExtensions
    {
        public static IEnumerable<Transform> AsEnumerable(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                yield return child;
            }
        }
    }
}
