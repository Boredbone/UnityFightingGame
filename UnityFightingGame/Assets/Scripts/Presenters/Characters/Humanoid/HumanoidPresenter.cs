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
    public class HumanoidPresenter : BehaviorBase
    {
        private Component targetObject;
        
        private float velocityCoefficient = 6f;
        private float jumpSpeed = 10f;
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

        private const string punchAttackTag = "PunchAttack";
        private const string kickAttackTag = "KickAttack";


        private int previousAttackState;

        private float moveDirection = 1f;

        private HumanoidModel Model { get; set; }

        private AttackColliderController AttackCollider { get; set; }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

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

            
            this.AttackCollider= this.transform.AsEnumerable()
                .First(x => x.name.Equals("AttackColliders")).GetComponent<AttackColliderController>();
            this.AttackCollider.Id = this.Model.Id;
            


            var vital = this.transform.AsEnumerable()
                .First(x => x.name.Equals("VitalBody")).GetComponent<VitalController>();
            vital.Id = this.Model.Id;
            vital.Damaged.Subscribe(y => this.Model.OnDamaged(y)).AddTo(this.Disposables);

            this.previousAttackState = 0;

            /*
            var prevIsRush = false;
            this.AnimatorStates.StateChanged
                .Subscribe(current =>
                {
                    var isRush = this.AnimatorStates.HasTag(inRushAttackTag);

                    if (isRush && prevIsRush)
                    {
                        //this.AttackCollider.Information.Power = 1;
                        //this.AttackCollider.ActivateCollider("Punch");
                    }

                    prevIsRush = isRush;
                })
                .AddTo(this.Disposables);
                */



            this.Velocity = Vector3.zero;
            this.currentSpeed = 0f;

            this.JumpRequest = false;
            this.isSecondJumpDone = false;
            this.isInAttack = false;
            

            var pos = this.transform.position;
            pos.x = this.Model.InitialPosition * -3f;
            this.transform.position = pos;


            this.moveDirection = 1f;// this.Model.Mirror ? -1f : 1f;
            if (this.Model.Mirror)
            {
                this.ToggleMoveDirection();
            }

            this.Model.Updated.Subscribe(_ => this.Move()).AddTo(this.Disposables);
        }


        private void InitializeAnimatorStates()
        {

            this.AnimatorStates = new AnimatorStateManager().AddTo(this.Disposables);

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
                airAttack2_0State,
                airAttack2_1State,
                airAttack2_2State,
                landAttack2_0State,
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


            new[]
            {
                airAttack2_0State,
                airAttack2_1State,
                airAttack2_2State,
                landAttack2_0State,
                landAttack2_1State,
                landAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(punchAttackTag));

            new[]
            {
                airAttack1State,
                landAttack1State,
            }
            .ForEach(y => y.Tags.Add(kickAttackTag));

        }

        private void ToggleMoveDirection()
        {
            this.transform.Rotate(new Vector3(0, 1, 0), 180);
            this.moveDirection *= -1f;
            this.currentSpeed *= -1f;
        }

        private void StartAttack(string key)
        {
            if (this.AnimatorStates.IsNotInTransition
                && this.previousAttackState != this.AnimatorStates.CurrentState)
            {
                this.AttackCollider.ActivateCollider(key);
                this.previousAttackState = this.AnimatorStates.CurrentState;
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Move()
        {
            

            var input = this.Model.DesiredParameters;
            var velocity = input.HorizontalVelocity * (this.Model.DesiredParameters.IsRunning ? 1f : 0.4f);// v * 0.3f + h;
            

            //this.AnimatorStates.IsInTransition = this.Animator.IsInTransition(0);
            this.AnimatorStates.CheckAll(this.Animator.IsInTransition(0),
                this.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash);


            // not in attack
            if (this.AnimatorStates.IsNotInTransition
                //&& this.AnimatorStates.HasTag(damagedTag))
                && !this.AnimatorStates.HasTag(inAttackTag))
            {
                this.isInAttack = false;
                this.AttackCollider.ClearCollider();

                if (this.AnimatorStates.HasTag(damagedTag))
                {
                    this.Animator.SetBool("Attack1", false);
                    this.Animator.SetBool("Attack2", false);
                    this.Animator.SetBool("Attack2_0", false);
                }

            }

            var attackEnabled = this.Animator.GetFloat("AttackEnabled") > 0.5f;


            if (attackEnabled && this.AnimatorStates.HasTag(punchAttackTag))
            {
                this.StartAttack("Punch");
            }
            else if (attackEnabled && this.AnimatorStates.HasTag(kickAttackTag))
            {
                this.StartAttack("Kick");
            }
            else if (!attackEnabled)
            {
                this.AttackCollider.ClearCollider();
                this.previousAttackState = 0;
            }


            if (this.AnimatorStates.IsNotInTransition
                && this.AnimatorStates.HasTag(clearSecondJumpFlagTag))
            {
                this.isSecondJumpDone = false;
            }


            
            var desiredHorizontalVelocity = this.Velocity.z;
            var desiredVerticalVelocity = this.Velocity.y;

            //var desiredVelocity = this.Velocity;
            var isGrounded = this.Controller.isGrounded;

            // Change Direction
            if (velocity * this.moveDirection < 0 && isGrounded)
            {
                //transform.Rotate(new Vector3(0, 1, 0), 180);
                //this.moveDirection *= -1f;
                this.ToggleMoveDirection();
                desiredHorizontalVelocity *= -1f;
            }

            this.targetObject.transform.rotation = Quaternion.Euler(0, this.moveDirection * -70 + 10, 0);

            velocity *= this.moveDirection;

            if (isGrounded)
            {
                this.JumpTrigger.Clear();
                this.LandingTrigger.Clear();
                this.SecondJumpTrigger.Clear();
            }


            if (isGrounded)
            {
                if (this.Lan2State.IsActive)
                {
                    velocity = 0;
                }
                
                this.Animator.SetFloat("Speed", velocity * 5f);




                var realVlc = velocity > 0 ? velocity : 0;

                var desiredSpeed = realVlc * 0.6f;

                var setVlc = desiredSpeed - (desiredSpeed - this.currentSpeed) * this.accelTime;// / Time.deltaTime;
                this.currentSpeed = setVlc;

                if (this.AnimatorStates.HasTag(inAttackTag))
                {
                    desiredHorizontalVelocity *= 0.9f;
                }
                else
                {
                    desiredHorizontalVelocity = setVlc * velocityCoefficient;
                }

                desiredVerticalVelocity = 0f;

                //if (Input.GetKeyDown(KeyCode.DownArrow))
                //{
                //    transform.Rotate(new Vector3(0, 1, 0), 45);
                //}

                if (input.Jump)
                {
                    if (!this.JumpRequest && (this.IdleState.IsActive || this.LocoState.IsActive))
                    {
                        this.JumpRequest = true;

                        this.JumpTrigger.Set();
                        this.LandingTrigger.Clear();
                        this.SecondJumpTrigger.Clear();
                        
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
                    if (input.Jump)
                    {
                        
                        var vd = input.HorizontalVelocity * velocityCoefficient * this.moveDirection * 0.3f;//velocity

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

                        desiredVerticalVelocity = jumpSpeed;// * 0.8f;

                        this.SecondJumpTrigger.Set();
                        this.LandingTrigger.Clear();
                        this.JumpTrigger.Clear();
                        
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
                if (input.Kick)
                {
                    this.isInAttack = true;
                    this.Animator.SetBool("Attack1", true);
                    desiredVerticalVelocity += jumpSpeed * 0.2f;
                    this.AttackCollider.Information.Power = 3;
                }
                else if (input.Punch)
                {
                    this.isInAttack = true;
                    this.Animator.SetBool("Attack2_0", true);
                    this.AttackCollider.Information.Power = 2;
                }
                else if (input.RushPunch)
                {
                    this.isInAttack = true;
                    attack2Flag = true;
                    this.AttackCollider.Information.Power = 1;
                }
                else if ((this.IdleState.IsActive|| this.RestState.IsActive) && input.Rest)
                {
                    this.RestTrigger.Set();
                }
            }
            else if (this.isInAttack
                && this.AnimatorStates.HasTag(inRushAttackTag))
            {
                if (input.RushPunch)
                {
                    attack2Flag = true;
                    this.Animator.SetBool("Attack2_0", false);
                    this.AttackCollider.Information.Power = 1;
                }
            }
            
            this.Animator.SetBool("Attack2", attack2Flag);



            // Jump
            if (this.JumpState.IsActive && this.JumpRequest && this.JumpStartTrigger)
            {
                desiredVerticalVelocity = jumpSpeed;
                this.JumpRequest = false;
            }

            // Gravity
            var gravity
                = (!isInAttack) ? 20f
                : (desiredVerticalVelocity > 0) ? 10f
                : 5f;
            desiredVerticalVelocity -= gravity * Time.deltaTime;
            

            // Landing
            var jumping = this.JumpState.IsActive || this.Jmp2State.IsActive;
            var stability = jumping ? this.Animator.GetFloat("JumpStability") : 0f;

            if ((desiredVerticalVelocity < -jumpSpeed * 0.6f && jumping)
                || (jumping && isGrounded && (stability > 0.8)))
               // || (Math.Abs(desiredVelocity.y) > 0.1 && desiredVelocity.y < this.jumpspeed * 0.5))))
                //!this.JumpRequest))// // || (!jumping && !isIdle && cc.isGrounded))
            {
                if (!this.LandState.IsActive)
                {
                    this.LandingTrigger.Set();
                    this.isSecondJumpDone = true;
                }
            }            


            this.Animator.SetBool("OnGround", isGrounded);

            if (isGrounded && !this.AnimatorStates.IsInTransition)
            {
                this.isSecondJumpDone = false;
            }




            // Commit
            this.Velocity = new Vector3(0, desiredVerticalVelocity, desiredHorizontalVelocity);

            // Move character
            this.Controller.Move(transform.TransformDirection(this.Velocity) * Time.deltaTime);


            // update model
            this.Model.ViewParameters.HorizontalPosition = this.transform.position.z;
            this.Model.ViewParameters.VerticalPosition = this.transform.position.y;
            
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
