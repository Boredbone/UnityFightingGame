using UnityEngine;
using System.Collections;

namespace Boredbone.UnityFightingGame.Scripts.Views.Characters.Humanoid
{
    public class TriggerReceiver : MonoBehaviour
    {

        GameObject parent;

        // Use this for initialization
        void Start()
        {
            parent = gameObject.transform.parent.gameObject;
        }

        void OnTriggerEnter(Collider collider)
        {
            parent.SendMessage("RedirectedOnTriggerEnter", collider);
        }

        void OnTriggerStay(Collider collider)
        {
            parent.SendMessage("RedirectedOnTriggerStay", collider);
        }



        public void OnMotionEnded(string arg)
        {
            parent.SendMessage("OnMotionEnded", arg);
            //Debug.Log("c,"+arg);
        }

        public void OnJumpStateChanged(string arg)
        {
            parent.SendMessage("OnJumpStateChanged", arg);
        }
    }
}