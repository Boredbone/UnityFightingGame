using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boredbone.Utility.Extensions;
using Boredbone.GameScripts.Helpers;
using System;

namespace Boredbone.UnityFightingGame.Scripts.Views.Characters.Humanoid
{

    public class HumanoidController : MonoBehaviour
    {
        public Component targetObject;
        
        private float speed = 6f;
        private float jumpspeed = 10f;
        private float accelTime = 0.5f;
        
        private CharacterController Controller { get; set; }
        private Animator Animator { get; set; }

        private Vector3 Velocity { get; set; }
        private float currentSpeed;

        private AnimatorStateManager AnimatorStates { get; set; }

        private AnimatorState IdleState { get; set; }
        private AnimatorState LocoState { get; set; }
        private AnimatorState JumpState { get; set; }
        private AnimatorState Jmp2State { get; set; }
        private AnimatorState LandState { get; set; }
        private AnimatorState Lan2State { get; set; }
        private AnimatorState RestState { get; set; }

        private AnimatorState AirAttack1State { get; set; }
        private AnimatorState AirAttack2_0State { get; set; }
        private AnimatorState AirAttack2_1State { get; set; }
        private AnimatorState AirAttack2_2State { get; set; }

        private AnimatorState LandAttack1State { get; set; }
        private AnimatorState LandAttack2_0State { get; set; }
        private AnimatorState LandAttack2_1State { get; set; }
        private AnimatorState LandAttack2_2State { get; set; }

        private AnimatorState DamagedGround1State { get; set; }
        private AnimatorState DamagedGround2State { get; set; }
        private AnimatorState DamagedGround3State { get; set; }
        private AnimatorState DamagedAir1State { get; set; }
        private AnimatorState DamagedAir2State { get; set; }
        private AnimatorState DamagedAir3State { get; set; }

        

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


        /// <summary>
        /// Use this for initialization
        /// </summary>
        public void Start()
        {
            this.Controller = gameObject.GetComponent<CharacterController>();
            this.Animator = this.targetObject.GetComponent<Animator>();

            this.InitializeAnimatorStates();

            this.Velocity = Vector3.zero;
            this.currentSpeed = 0f;

            this.JumpRequest = false;
            this.isSecondJumpDone = false;
            this.isInAttack = false;

            this.moveDirection = 1f;
        }


        private void InitializeAnimatorStates()
        {

            this.AnimatorStates = new AnimatorStateManager();


            this.IdleState = new AnimatorState("Base Layer.Idle", null, this.AnimatorStates);
            this.LocoState = new AnimatorState("Base Layer.Locomotion", null, this.AnimatorStates);
            this.JumpState = new AnimatorState("Base Layer.Jump", "Jump", this.AnimatorStates);
            this.Jmp2State = new AnimatorState("Base Layer.SecondJump", "SecondJump", this.AnimatorStates);
            this.LandState = new AnimatorState("Base Layer.Landing", "Landing", this.AnimatorStates);
            this.Lan2State = new AnimatorState("Base Layer.Landed", null, this.AnimatorStates);
            this.RestState = new AnimatorState("Base Layer.Rest", "Rest", this.AnimatorStates);


            this.AirAttack1State = new AnimatorState("Base Layer.AirAtk1", null, this.AnimatorStates);
            this.AirAttack2_0State = new AnimatorState("Base Layer.AirAtk2_0", null, this.AnimatorStates);
            this.AirAttack2_1State = new AnimatorState("Base Layer.AirAtk2_1", null, this.AnimatorStates);
            this.AirAttack2_2State = new AnimatorState("Base Layer.AirAtk2_2", null, this.AnimatorStates);
            this.LandAttack1State = new AnimatorState("Base Layer.LandAtk1", null, this.AnimatorStates);
            this.LandAttack2_0State = new AnimatorState("Base Layer.LandAtk2_0", null, this.AnimatorStates);
            this.LandAttack2_1State = new AnimatorState("Base Layer.LandAtk2_1", null, this.AnimatorStates);
            this.LandAttack2_2State = new AnimatorState("Base Layer.LandAtk2_2", null, this.AnimatorStates);


            this.DamagedGround1State = new AnimatorState("Base Layer.DamagedGround1", null, this.AnimatorStates);
            this.DamagedGround2State = new AnimatorState("Base Layer.DamagedGround2", null, this.AnimatorStates);
            this.DamagedGround3State = new AnimatorState("Base Layer.DamagedGround3", null, this.AnimatorStates);
            this.DamagedAir1State = new AnimatorState("Base Layer.DamagedAir1", null, this.AnimatorStates);
            this.DamagedAir2State = new AnimatorState("Base Layer.DamagedAir2", null, this.AnimatorStates);
            this.DamagedAir3State = new AnimatorState("Base Layer.DamagedAir3", null, this.AnimatorStates);



            new[]
            {
                this.JumpState,
                this.Jmp2State,
                this.LandState,
                this.AirAttack1State,
                this.AirAttack2_0State,
                this.AirAttack2_1State,
                this.AirAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(inAirTag));


            new[]
            {
                this.AirAttack1State,
                this.LandAttack1State,
                this.AirAttack2_0State,
                this.AirAttack2_1State,
                this.AirAttack2_2State,
                this.LandAttack2_0State,
                this.LandAttack2_1State,
                this.LandAttack2_2State,
            }
            .ForEach(y =>
            {
                y.Tags.Add(inAttackTag);
                //y.Tags.Add(clearSecondJumpFlagTag);
            });


            new[]
            {
                this.AirAttack2_1State,
                this.AirAttack2_2State,
                this.LandAttack2_1State,
                this.LandAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(inRushAttackTag));

            new[]
            {
                this.DamagedGround1State,
                this.DamagedGround2State,
                this.DamagedGround3State,
                this.DamagedAir1State,
                this.DamagedAir2State,
                this.DamagedAir3State,
            }
            .ForEach(y => y.Tags.Add(damagedTag));

            new[]
            {
                this.DamagedAir1State,
                this.DamagedAir2State,
                this.DamagedAir3State,
            }
            .ForEach(y => y.Tags.Add(clearSecondJumpFlagTag));


            new[]
            {
                this.JumpState,
                this.Jmp2State,
                this.IdleState,
                this.LocoState,
                this.LandState,
            }
            .ForEach(y => y.Tags.Add(attackableTag));


        }


        /// <summary>
        /// Update is called once per frame
        /// </summary>
        public void Update()
        {

            


            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            var vlc = v * 0.3f + h;






            // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
            var currentBaseState = this.Animator.GetCurrentAnimatorStateInfo(0);

            this.AnimatorStates.CheckAll(currentBaseState.fullPathHash);
            this.AnimatorStates.IsInTransition = this.Animator.IsInTransition(0);


            if (this.AnimatorStates.IsNotInTransition
                && this.AnimatorStates.ActiveStates.Any(y => !y.Tags.Contains(inAttackTag)))
            {
                this.isInAttack = false;
                this.Animator.SetBool("Attack1", false);
                this.Animator.SetBool("Attack2", false);
                this.Animator.SetBool("Attack2_0", false);
            }
            
            if(this.AnimatorStates.IsNotInTransition
                && this.AnimatorStates.ActiveStates.Any(y => y.Tags.Contains(clearSecondJumpFlagTag)))
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
                transform.Rotate(new Vector3(0, 1, 0), 180);
                this.moveDirection *= -1f;
                desiredHorizontalVelocity *= -1f;
            }

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

                if (this.AnimatorStates.ActiveStates.Any(y => y.Tags.Contains(inAttackTag)))
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

                        this.JumpState.Request(this.Animator);
                        this.LandState.Request(this.Animator, false);
                        this.Jmp2State.Request(this.Animator, false);
                        
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

                        this.Jmp2State.Request(this.Animator);
                        this.LandState.Request(this.Animator, false);
                        this.JumpState.Request(this.Animator, false);
                        
                        this.isSecondJumpDone = true;

                        //Debug.Log(currentBaseState.fullPathHash.ToString());
                    }
                }
            }
            
            

            // Attack

            var attack2Flag = false;

            if (this.AnimatorStates.ActiveStates.All(y => y.Tags.Contains(attackableTag))
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
                else if (this.IdleState.IsActive && Input.GetKeyDown(KeyCode.V))
                {
                    this.RestState.Request(this.Animator);
                }
            }
            else if (this.isInAttack
                && this.AnimatorStates.ActiveStates.Any(y => y.Tags.Contains(inRushAttackTag)))
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
            if (!isInAttack)
            {
                desiredVerticalVelocity -= 20f * Time.deltaTime;
            }
            else
            {
                if (desiredVerticalVelocity > 0)
                {
                    desiredVerticalVelocity -= 10f * Time.deltaTime;
                }
                else
                {
                    desiredVerticalVelocity -= 5f * Time.deltaTime;
                }
            }

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
                    this.LandState.Request(this.Animator);
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

            // Clear animator triggers
            this.AnimatorStates.ClearTriggers(this.Animator);

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
    }
}
