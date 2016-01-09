using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;
using Boredbone.GameScripts.Helpers;
using System;
using Boredbone.GameScripts.Extensions;
using Boredbone.UnityFightingGame.CoreLibrary.Models.Characters.Humanoid;
using Boredbone.UnityFightingGame.CoreLibrary.Models;
using UniRx;

namespace Boredbone.UnityFightingGame.Scripts.Presenters.Characters.Humanoid
{
   

    public class HumanoidPresenter : MonoBehaviour
    {
        private Component targetObject;
        
        private float speed = 6f;
        private float jumpspeed = 10f;
        private float accelTime = 0.5f;
        
        private CharacterController Controller { get; set; }
        private Animator Animator { get; set; }

        private Vector3 Velocity { get; set; }
        private float currentSpeed;

        private AnimatorStateManager AnimatorStates { get; set; }

        private AnimationTrigger JumpTrigger { get; set; }
        private AnimationTrigger SecondJumpTrigger { get; set; }
        private AnimationTrigger LandingTrigger { get; set; }
        private AnimationTrigger RestTrigger { get; set; }

        private AnimatorState IdleState { get; set; }
        private AnimatorState LocoState { get; set; }
        private AnimatorState JumpState { get; set; }
        private AnimatorState Jmp2State { get; set; }
        private AnimatorState LandState { get; set; }
        private AnimatorState Lan2State { get; set; }
        private AnimatorState RestState { get; set; }

        //private AnimatorState AirAttack1State { get; set; }
        //private AnimatorState AirAttack2_0State { get; set; }
        //private AnimatorState AirAttack2_1State { get; set; }
        //private AnimatorState AirAttack2_2State { get; set; }
        //
        //private AnimatorState LandAttack1State { get; set; }
        //private AnimatorState LandAttack2_0State { get; set; }
        //private AnimatorState LandAttack2_1State { get; set; }
        //private AnimatorState LandAttack2_2State { get; set; }
        //
        //private AnimatorState DamagedGround1State { get; set; }
        //private AnimatorState DamagedGround2State { get; set; }
        //private AnimatorState DamagedGround3State { get; set; }
        //private AnimatorState DamagedAir1State { get; set; }
        //private AnimatorState DamagedAir2State { get; set; }
        //private AnimatorState DamagedAir3State { get; set; }

        

        private bool _fieldJumpRequest = false;
        public bool JumpRequest
        {
            get { return _fieldJumpRequest; }
            set
            {
                if (_fieldJumpRequest != value)
                {
                    _fieldJumpRequest = value;
                    this.JumpStartTrigger = false;
                }
            }
        }
        
        private bool JumpStartTrigger { get; set; }


        private bool isSecondJumpDone;
        private bool isInAttack;
        

        private const string inAirTag = "InAir";
        private const string inAttackTag = "InAttack";
        private const string inRushAttackTag = "InRushAttack";
        private const string attackableTag = "Attackable";
        private const string clearSecondJumpFlagTag = "ClearSecondJumpFlag";
        private const string damagedTag = "Damaged";



        private float moveDirection = 1f;

        private HumanoidModel Model { get; set; }



        /// <summary>
        /// Use this for initialization
        /// </summary>
        public void Start()
        {
            this.Model = AppCore.GetEnvironment(null).Characters.GetModel<HumanoidModel>();

            var children = this.transform.AsEnumerable().Where(y => y.tag.Equals("HumanoidModel")).ToArray();

            var index = this.Model.ColorIndex;

            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.SetActive(i == index);
            }

            this.targetObject = children[index];


            var offsetCorrection = this.GetComponent<OffsetCorrection>();
            if (offsetCorrection != null)
            {
                offsetCorrection.TargetObject = this.targetObject;
            }

            this.Controller = gameObject.GetComponent<CharacterController>();
            this.Animator = this.targetObject.GetComponent<Animator>();

            this.InitializeAnimatorStates();

            this.Velocity = Vector3.zero;
            this.currentSpeed = 0f;

            this.JumpRequest = false;
            this.isSecondJumpDone = false;
            this.isInAttack = false;

            this.moveDirection = 1f;// this.Model.Mirror ? -1f : 1f;
            if (this.Model.Mirror)
            {
                this.ToggleMoveDirection();
            }

            this.Model.Updated.Subscribe(_ => this.OnUpdate()).AddTo(this.Disposables);
        }


        private void InitializeAnimatorStates()
        {

            this.AnimatorStates = new AnimatorStateManager();

            this.JumpTrigger = new AnimationTrigger("Jump", this.Animator);
            this.SecondJumpTrigger = new AnimationTrigger("SecondJump", this.Animator);
            this.LandingTrigger = new AnimationTrigger("Landing", this.Animator);
            this.RestTrigger = new AnimationTrigger("Rest", this.Animator);


            this.IdleState = new AnimatorState("Base Layer.Idle", this.AnimatorStates);
            this.LocoState = new AnimatorState("Base Layer.Locomotion", this.AnimatorStates);
            this.JumpState = new AnimatorState("Base Layer.Jump", this.AnimatorStates);
            this.Jmp2State = new AnimatorState("Base Layer.SecondJump", this.AnimatorStates);
            this.LandState = new AnimatorState("Base Layer.Landing", this.AnimatorStates);
            this.Lan2State = new AnimatorState("Base Layer.Landed", this.AnimatorStates);
            this.RestState = new AnimatorState("Base Layer.Rest", this.AnimatorStates);


            var airAttack1State = new AnimatorState("Base Layer.AirAtk1", this.AnimatorStates);
            var airAttack2_0State = new AnimatorState("Base Layer.AirAtk2_0", this.AnimatorStates);
            var airAttack2_1State = new AnimatorState("Base Layer.AirAtk2_1", this.AnimatorStates);
            var airAttack2_2State = new AnimatorState("Base Layer.AirAtk2_2", this.AnimatorStates);
            var landAttack1State = new AnimatorState("Base Layer.LandAtk1", this.AnimatorStates);
            var landAttack2_0State = new AnimatorState("Base Layer.LandAtk2_0", this.AnimatorStates);
            var landAttack2_1State = new AnimatorState("Base Layer.LandAtk2_1", this.AnimatorStates);
            var landAttack2_2State = new AnimatorState("Base Layer.LandAtk2_2", this.AnimatorStates);
            
            
            var damagedGround1State = new AnimatorState("Base Layer.DamagedGround1", this.AnimatorStates);
            var damagedGround2State = new AnimatorState("Base Layer.DamagedGround2", this.AnimatorStates);
            var damagedGround3State = new AnimatorState("Base Layer.DamagedGround3", this.AnimatorStates);
            var damagedAir1State = new AnimatorState("Base Layer.DamagedAir1", this.AnimatorStates);
            var damagedAir2State = new AnimatorState("Base Layer.DamagedAir2", this.AnimatorStates);
            var damagedAir3State = new AnimatorState("Base Layer.DamagedAir3", this.AnimatorStates);



            new[]
            {
                this.JumpState,
                this.Jmp2State,
                this.LandState,
                airAttack1State,
                airAttack2_0State,
                airAttack2_1State,
                airAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(inAirTag));


            new[]
            {
                airAttack1State,
                airAttack2_0State,
                airAttack2_1State,
                airAttack2_2State,
                landAttack1State,
                landAttack2_0State,
                landAttack2_1State,
                landAttack2_2State,
            }
            .ForEach(y =>
            {
                y.Tags.Add(inAttackTag);
                //y.Tags.Add(clearSecondJumpFlagTag);
            });


            new[]
            {
                airAttack2_1State,
                airAttack2_2State,
                landAttack2_1State,
                landAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(inRushAttackTag));

            new[]
            {
                damagedGround1State,
                damagedGround2State,
                damagedGround3State,
                damagedAir1State,
                damagedAir2State,
                damagedAir3State,
            }
            .ForEach(y => y.Tags.Add(damagedTag));

            new[]
            {
                damagedAir1State,
                damagedAir2State,
                damagedAir3State,
            }
            .ForEach(y => y.Tags.Add(clearSecondJumpFlagTag));


            new[]
            {
                this.JumpState,
                this.Jmp2State,
                this.IdleState,
                this.LocoState,
                this.LandState,
                this.RestState,
            }
            .ForEach(y => y.Tags.Add(attackableTag));


        }

        private void ToggleMoveDirection()
        {
            this.transform.Rotate(new Vector3(0, 1, 0), 180);
            this.moveDirection *= -1f;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void OnUpdate()
        {

            


            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            var vlc = v * 0.3f + h;






            // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
            var currentBaseState = this.Animator.GetCurrentAnimatorStateInfo(0);

            this.AnimatorStates.CheckAll(currentBaseState.fullPathHash);
            this.AnimatorStates.IsInTransition = this.Animator.IsInTransition(0);


            if (this.AnimatorStates.IsNotInTransition
                && !this.AnimatorStates.HasTag(inAttackTag))
            {
                this.isInAttack = false;
                this.Animator.SetBool("Attack1", false);
                this.Animator.SetBool("Attack2", false);
                this.Animator.SetBool("Attack2_0", false);
            }
            
            if(this.AnimatorStates.IsNotInTransition
                && this.AnimatorStates.HasTag(clearSecondJumpFlagTag))
            {
                this.isSecondJumpDone = false;
            }



            var desiredHorizontalVelocity = this.Velocity.z;
            var desiredVerticalVelocity = this.Velocity.y;

            //var desiredVelocity = this.Velocity;
            var isGrounded = this.Controller.isGrounded;

            // Change Direction
            if (vlc * this.moveDirection < 0)//isGrounded && 
            {
                //transform.Rotate(new Vector3(0, 1, 0), 180);
                //this.moveDirection *= -1f;
                this.ToggleMoveDirection();
                desiredHorizontalVelocity *= -1f;
            }

            this.targetObject.transform.rotation = Quaternion.Euler(0, this.moveDirection * -60 + 10, 0);

            vlc *= this.moveDirection;


            if (isGrounded)
            {
                //var vlc = h * 0.3f + v;

                if (this.Lan2State.IsActive)
                {
                    vlc = 0;
                }

                // Animator側で設定している"Speed"パラメタにvを渡す
                this.Animator.SetFloat("Speed", vlc * 5f);




                var realVlc = vlc > 0 ? vlc : 0;

                var desiredSpeed = realVlc * 0.6f;

                var setVlc = desiredSpeed - (desiredSpeed - this.currentSpeed) * this.accelTime;// / Time.deltaTime;
                this.currentSpeed = setVlc;

                if (this.AnimatorStates.HasTag(inAttackTag))
                {
                    desiredHorizontalVelocity *= 0.9f;
                }
                else
                {
                    //desiredVelocity = new Vector3(0, 0, setVlc) * speed; //new Vector3(h, 0, v);//
                    desiredHorizontalVelocity = setVlc * speed;
                }

                desiredVerticalVelocity = 0f;

                //if (Input.GetKeyDown(KeyCode.DownArrow))
                //{
                //    transform.Rotate(new Vector3(0, 1, 0), 45);
                //}

                if (Input.GetKeyDown(KeyCode.Space))// && !this.jumpRequest && (this.IdleState.IsActive || this.LocoState.IsActive))
                {
                    if (!this.JumpRequest && (this.IdleState.IsActive || this.LocoState.IsActive))
                    {
                        this.JumpRequest = true;

                        this.JumpTrigger.Set();// .Request(this.Animator);
                        this.LandingTrigger.Clear();// .Request(this.Animator, false);
                        this.SecondJumpTrigger.Clear();//.Request(this.Animator, false);
                        
                        this.isSecondJumpDone = false;

                        //Debug.Log("jump");
                    }
                }
            }
            else
            {
                this.currentSpeed = 0f;


                if ((this.JumpState.IsActive || this.LandState.IsActive)
                    && !this.JumpRequest && !this.isSecondJumpDone)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {

                        //var vlc = h * 0.3f + v;

                        var vd = vlc * speed;

                        if (desiredHorizontalVelocity * vd < 0.0001)
                        {
                            desiredHorizontalVelocity = vd;

                        }
                        else if (desiredHorizontalVelocity * vd > 0.0001)
                        {
                            desiredHorizontalVelocity += vd * 0.3f;
                        }
                        else
                        {
                            desiredHorizontalVelocity += vd;
                        }

                        desiredVerticalVelocity = jumpspeed;// * 0.8f;

                        this.SecondJumpTrigger.Set();// .Request(this.Animator);
                        this.LandingTrigger.Clear();//.Request(this.Animator, false);
                        this.JumpTrigger.Clear();//.Request(this.Animator, false);
                        
                        this.isSecondJumpDone = true;
                        
                    }
                }
            }
            
            

            // Attack

            var attack2Flag = false;

            if (this.AnimatorStates.HasTag(attackableTag)
                && !this.AnimatorStates.IsInTransition
                && !this.isInAttack)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    this.isInAttack = true;
                    this.Animator.SetBool("Attack1", true);
                    desiredVerticalVelocity += jumpspeed * 0.2f;
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    this.isInAttack = true;
                    this.Animator.SetBool("Attack2_0", true);
                }
                else if (Input.GetKey(KeyCode.Z))
                {
                    this.isInAttack = true;
                    attack2Flag = true;
                    //this.Animator.SetBool("Attack2", attack2Flag);
                }
                else if ((this.IdleState.IsActive|| this.RestState.IsActive) && Input.GetKeyDown(KeyCode.V))
                {
                    this.RestTrigger.Set();//.Request(this.Animator);
                }
            }
            else if (this.isInAttack
                && this.AnimatorStates.HasTag(inRushAttackTag))
            {
                if (Input.GetKey(KeyCode.Z))
                {
                    attack2Flag = true;
                    //this.Animator.SetBool("Attack2", attack2Flag);
                }
            }
            
            this.Animator.SetBool("Attack2", attack2Flag);



            // Jump
            if (this.JumpState.IsActive && this.JumpRequest && this.JumpStartTrigger)
            {
                desiredVerticalVelocity = jumpspeed;
                this.JumpRequest = false;
            }

            // Gravity
            var gravity
                = (!isInAttack) ? 20f
                : (desiredVerticalVelocity > 0) ? 10f
                : 5f;
            desiredVerticalVelocity -= gravity * Time.deltaTime;

            //if (!isInAttack)
            //{
            //    desiredVerticalVelocity -= 20f * Time.deltaTime;
            //}
            //else
            //{
            //    if (desiredVerticalVelocity > 0)
            //    {
            //        desiredVerticalVelocity -= 10f * Time.deltaTime;
            //    }
            //    else
            //    {
            //        desiredVerticalVelocity -= 5f * Time.deltaTime;
            //    }
            //}

            // Landing
            var jumping = this.JumpState.IsActive || this.Jmp2State.IsActive;
            var stability = jumping ? this.Animator.GetFloat("JumpStability") : 0f;

            if ((desiredVerticalVelocity < -jumpspeed * 0.6f && jumping)
                || (jumping && isGrounded && (stability > 0.8)))
               // || (Math.Abs(desiredVelocity.y) > 0.1 && desiredVelocity.y < this.jumpspeed * 0.5))))
                //!this.JumpRequest))// // || (!jumping && !isIdle && cc.isGrounded))
            {
                if (!this.LandState.IsActive)
                {
                    this.LandingTrigger.Set();//.Request(this.Animator);
                    this.isSecondJumpDone = true;
                }
            }            


            this.Animator.SetBool("OnGround", isGrounded);

            if (isGrounded && !this.AnimatorStates.IsInTransition)
            {
                this.isSecondJumpDone = false;
            }




            // Commit

            //desiredVelocity.z = 0f;

            //desiredVelocity.y = desiredVerticalVelocity;
            //desiredVelocity.z = desiredHorizontalVelocity;

            this.Velocity = new Vector3(0, desiredVerticalVelocity, desiredHorizontalVelocity);// desiredVelocity;

            // Move character
            this.Controller.Move(transform.TransformDirection(this.Velocity) * Time.deltaTime);


            // update model
            this.Model.ViewParameters.HorizontalPosition = this.transform.position.x;
            this.Model.ViewParameters.VerticalPosition = this.transform.position.z;

            // Clear animator triggers
            //this.AnimatorStates.ClearTriggers(this.Animator);

        }


        public void OnMotionEnded(string arg)
        {
            if (arg.Equals("KickEnded"))
            {
                this.Animator.SetBool("Attack1", false);
                this.isInAttack = false;
            }
            else if (arg.Equals("PunchEnded"))
            {
                this.Animator.SetBool("Attack2_0", false);
                this.isInAttack = false;
            }
        }

        public void OnJumpStateChanged(string arg)
        {
            if (arg.Equals("JumpTakeoff"))
            {
                this.JumpStartTrigger = true;
            }
        }




        private CompositeDisposable Disposables { get; set; }

        public void Awake()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
            this.Disposables = new CompositeDisposable();
        }
        public void OnDestroy()
        {
            if (this.Disposables != null)
            {
                this.Disposables.Clear();
            }
        }
    }
}
