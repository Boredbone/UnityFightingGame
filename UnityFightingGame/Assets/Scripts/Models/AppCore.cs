using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boredbone.UnityFightingGame.Scripts.Models
{
    public class AppCore
    {
        private static AppCore _instance = new AppCore();

        private AppCore()
        {

        }

        public static AppCore GetEnvironment(AppEnvironmentArgs args)
        {
            return _instance;
        }


    }

    public class AppEnvironmentArgs
    {

    }
}
