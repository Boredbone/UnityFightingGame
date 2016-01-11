using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;
using System;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;
using Boredbone.GameScripts.Helpers;

namespace Boredbone.UnityFightingGame.Scripts.Presenters
{
    public class GameController : BehaviorBase
    {

        //private MainSceneModel Presenter { get; set; }

        //public UnityEngine.UI.Text scoreLabel;
        //public GameObject winnerLabelObject;

        //private bool initialized = false;

        private AppCore Core { get; set; }

        private InputReceiver InputReceiver { get; set; }


        public Text life0;
        public Text life1;



        protected override void OnAwake()
        {
            base.OnAwake();

            this.Core = AppCore.GetEnvironment(null);
            this.Core.Initialize();


            this.InputReceiver = new InputReceiver(this.Core);

        }

        // Use this for initialization
        protected override void OnStart()
        {
            base.OnStart();

            this.Core.Characters[0].Life.Subscribe(y => this.life0.text = y.ToString()).AddTo(this.Disposables);
            this.Core.Characters[1].Life.Subscribe(y => this.life1.text = y.ToString()).AddTo(this.Disposables);

            //this.Presenter = new MainSceneModel();


            //this.scoreLabel.text = "none";
            //
            //this.Presenter.ValueChanged
            //    .ObserveOnMainThread()
            //    .Subscribe(y => this.scoreLabel.text = y.ToString());

            //Input.GetKeyDown(KeyCode.UpArrow).AsObserbable

        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            base.OnUpdate();

            this.InputReceiver.UpdateInputs();

            this.Core.Update(Time.deltaTime);


            //if (Input.GetKeyDown(KeyCode.UpArrow))
            //{
            //    this.Presenter.StartProcess();
            //}
            //
            //
            //if (Input.GetKeyDown(KeyCode.DownArrow))
            //{
            //    this.Presenter.Update();
            //}
            //
            //var count = GameObject.FindGameObjectsWithTag("Item").Length;
            //
            //
            //if (count == 0)
            //{
            //
            //    // オブジェクトをアクティブにする
            //    //winnerLabelObject.SetActive(true);
            //}
        }
    }
}
