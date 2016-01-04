using UnityEngine;
using System.Collections;
using UniRx;
using System;
using Boredbone.UnityFightingGame.Scripts.Presenters;

public class GameController : MonoBehaviour
{

    private MainScenePresenter Presenter { get; set; }

    public UnityEngine.UI.Text scoreLabel;
    public GameObject winnerLabelObject;

    //private bool initialized = false;
    


    // Use this for initialization
    void Start()
    {
        this.Presenter = new MainScenePresenter();
        

        this.scoreLabel.text = "none";

        this.Presenter.ValueChanged
            .ObserveOnMainThread()
            .Subscribe(y => this.scoreLabel.text = y.ToString());
            
        //Input.GetKeyDown(KeyCode.UpArrow).AsObserbable
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.Presenter.StartProcess();
        }


        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.Presenter.Update();
        }

        var count = GameObject.FindGameObjectsWithTag("Item").Length;


        if (count == 0)
        {

            // オブジェクトをアクティブにする
            winnerLabelObject.SetActive(true);
        }
    }
}
