using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;
using Boredbone.GameScripts.Helpers;

namespace Boredbone.UnityFightingGame.Presenters.Characters.Humanoid
{

    public class OffsetCorrection : MonoBehaviour
    {
        public Component TargetObject { get; set; }

        private Animator Animator { get; set; }
        private CharacterController Controller { get; set; }

        private Vector3 originalTargetCenter;
        

        /// <summary>
        /// Use this for initialization
        /// </summary>
        public void Start()
        {
            this.Controller = gameObject.GetComponent<CharacterController>();
            //this.Animator = this.TargetObject.GetComponent<Animator>();
            //this.originalTargetCenter = TargetObject.transform.position - this.transform.position;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public void Update()
        {

            if(this.Animator==null && this.TargetObject != null)
            {
                this.Animator = this.TargetObject.GetComponent<Animator>();
                this.originalTargetCenter = TargetObject.transform.position - this.transform.position;
            }


            //if (!this.Controller.isGrounded)
            //{
            var jumpHeight = this.Animator.GetFloat("JumpHeight");
            this.TargetObject.transform.position = this.transform.position + this.originalTargetCenter
                - new Vector3(0, jumpHeight, 0);
            //}
            //else
            //{
            //    this.targetObject.transform.position = this.transform.position + this.originalTargetCenter;
            //}
        }
    }
}
