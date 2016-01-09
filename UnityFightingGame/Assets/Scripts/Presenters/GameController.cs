using UnityEngine;
using System.Collections;
using UniRx;
using System;
using Boredbone.UnityFightingGame.CoreLibrary.Models;

namespace Boredbone.UnityFightingGame.Scripts.Presenters
{
    public class GameController : MonoBehaviour
    {

        //private MainSceneModel Presenter { get; set; }

        //public UnityEngine.UI.Text scoreLabel;
        //public GameObject winnerLabelObject;

        //private bool initialized = false;

        private AppCore Core { get; set; }

        private InputReceiver InputReceiver { get; set; }


        public void Awake()
        {
            this.Core = AppCore.GetEnvironment(null);
            this.Core.Initialize();


            this.InputReceiver = new InputReceiver(this.Core);

        }

        // Use this for initialization
        public void Start()
        {
            //this.Presenter = new MainSceneModel();


            //this.scoreLabel.text = "none";
            //
            //this.Presenter.ValueChanged
            //    .ObserveOnMainThread()
            //    .Subscribe(y => this.scoreLabel.text = y.ToString());

            //Input.GetKeyDown(KeyCode.UpArrow).AsObserbable

        }

        // Update is called once per frame
        public void Update()
        {

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
