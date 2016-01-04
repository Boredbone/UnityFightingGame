using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLibrary;
using UniRx;

namespace Boredbone.UnityFightingGame.Scripts.Presenters
{
    public class MainScenePresenter
    {
        private TestModel Model { get; set; }

        public IObservable<int> ValueChanged { get { return this.Model.ValueChanged; } }

        //public ReadOnlyReactiveProperty<int> Number { get; private set; }

        public MainScenePresenter()
        {
            this.Model = new TestModel();

           // this.Number = this.Model.ValueChanged.ToReadOnlyReactiveProperty();
            
        }

        public void StartProcess()
        {
            this.Model.StartHeavyProcess();
        }

        public void Update()
        {
            this.Model.Update();
        }
    }
}
