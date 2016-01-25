using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UniRx;
using System;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;
using Boredbone.GameScripts.Helpers;

namespace Boredbone.UnityFightingGame.Presenters
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

        public ParticleManager particleManager;

        //public GameObject effectContainer;
        //private ParticleSystem effect;



        protected override void OnAwake()
        {
            base.OnAwake();

            this.Core = AppCore.GetEnvironment(null);
            this.Core.Initialize();


            this.InputReceiver = new InputReceiver(this.Core);

            //this.effect = this.effectContainer.GetComponent<ParticleSystem>();
        }

        // Use this for initialization
        protected override void OnStart()
        {
            base.OnStart();

            this.Core.Characters[0].Life.SubscribeToText(this.life0, y => y.ToString()).AddTo(this.Disposables);
            this.Core.Characters[1].Life.SubscribeToText(this.life1, y => y.ToString()).AddTo(this.Disposables);

            this.Core.DebugLog.Subscribe(y => Debug.Log(y)).AddTo(this.Disposables);

            this.Core.Effect.Requested.Subscribe(r =>
            {
                this.particleManager.Execute(r);
                //Debug.Log(r.X.ToString() + ", " + r.Y.ToString() + ", " + r.Z.ToString());
                //this.effectContainer.transform.position = new Vector3(r.X, r.Y+1, r.Z);
                //effect.Play();
            })
            .AddTo(this.Disposables);

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
