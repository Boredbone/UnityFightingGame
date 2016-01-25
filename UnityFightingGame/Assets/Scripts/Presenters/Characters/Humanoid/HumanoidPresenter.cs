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

namespace Boredbone.UnityFightingGame.Presenters.Characters.Humanoid
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

        private AnimationTrigger WeakDamageTrigger { get; set; }
        private AnimationTrigger StrongDamageTrigger { get; set; }
        private AnimationTrigger FlyDamageTrigger { get; set; }
        private AnimationTrigger StunDamageTrigger { get; set; }

        private AnimatorState IdleState { get; set; }
        private AnimatorState LocoState { get; set; }
        private AnimatorState JumpState { get; set; }
        private AnimatorState Jmp2State { get; set; }
        private AnimatorState LandState { get; set; }
        private AnimatorState Lan2State { get; set; }
        private AnimatorState RestState { get; set; }

        private AnimatorState StrongDamageState { get; set; }
        private AnimatorState FlyDamageState { get; set; }

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

        private AnimatorValueFloat AnimatorSpeed { get; set; }
        private ReadOnlyAnimatorValueFloat AttackEnabled { get; set; }
        private ReadOnlyAnimatorValueFloat DamageMove { get; set; }
        private ReadOnlyAnimatorValueFloat JumpStability { get; set; }

        private AnimatorValueBool OnGrond { get; set; }
        private AnimatorValueBool Attack1 { get; set; }
        private AnimatorValueBool Attack2_0 { get; set; }
        private AnimatorValueBool Attack2 { get; set; }


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
                //Debug.Log("jump request" + value.ToString());
            }
        }

        private bool JumpStartTrigger { get; set; }


        private bool isSecondJumpDone;
        private bool isInAttack;


        private const string inAirTag = "InAir";
        private const string inAttackTag = "InAttack";
        private const string inRushAttackTag = "InRushAttack";
        private const string attackableTag = "Attackable";
        //private const string clearSecondJumpFlagTag = "ClearSecondJumpFlag";
        private const string damagedTag = "Damaged";

        private const string punchAttackTag = "PunchAttack";
        private const string punch2AttackTag = "Punch2Attack";
        private const string kickAttackTag = "KickAttack";


        private int previousAttackState;

        private float moveDirection = 1f;

        //private HumanoidModel Model { get; set; }

        private AttackColliderController AttackCollider { get; set; }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            var model = AppCore.GetEnvironment(null).Characters.GetModel<HumanoidModel>();

            var children = this.transform.AsEnumerable().Where(y => y.tag.Equals("HumanoidModel")).ToArray();
            //var children = GameObject.FindGameObjectsWithTag("HumanoidModel");

            var index = model.ColorIndex;

            children.ForEach((c, i) => c.gameObject.SetActive(i == index));
            

            this.targetObject = children[index];


            var offsetCorrection = this.GetComponent<OffsetCorrection>();
            if (offsetCorrection != null)
            {
                offsetCorrection.TargetObject = this.targetObject;
            }

            this.Controller = gameObject.GetComponent<CharacterController>();
            this.Animator = this.targetObject.GetComponent<Animator>();

            this.InitializeAnimator();


            this.AttackCollider = this.transform.AsEnumerable()
                .First(x => x.name.Equals("AttackColliders")).GetComponent<AttackColliderController>();
            this.AttackCollider.Id = model.Id;

            this.AttackCollider.Guarding.Subscribe(y =>
            {
                model.OnGuarding(y);
                //Debug.Log(y.Id.ToString());
            }).AddTo(this.Disposables);



            var vital = this.transform.AsEnumerable()
                .First(x => x.name.Equals("VitalBody")).GetComponent<VitalController>();
            vital.Id = model.Id;
            vital.Damaged.Subscribe(y =>
            {
                model.OnDamaged(y);
                //Debug.Log(y.Id.ToString());
            }).AddTo(this.Disposables);

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
            pos.x = model.InitialPosition * -3f;
            this.transform.position = pos;


            this.moveDirection = 1f;// this.Model.Mirror ? -1f : 1f;
            if (model.Mirror)
            {
                this.ToggleMoveDirection();
            }

            var parameters = new TemporaryParameters(model.DesiredParameters);

            model.Updated.Subscribe(_ =>
            {
                // Check state
                this.AnimatorStates.CheckAll(this.Animator.IsInTransition(0),
                this.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash);

                // store parameters
                //parameters.Input = model.DesiredParameters;
                parameters.IsGrounded = this.Controller.isGrounded;
                parameters.HorizontalVelocity = this.Velocity.z;
                parameters.VerticalVelocity = this.Velocity.y;

                this.AttackCollider.Information.SourcePositionHorizontal = this.transform.position.x;
                this.AttackCollider.Information.SourcePositionVertical = this.transform.position.y;

                // Action
                this.HorizontalMove(parameters);
                this.Jump(parameters);
                this.AttackOrDamage(parameters);
                this.Gravity(parameters);

                // Commit
                this.Velocity = new Vector3(0, parameters.VerticalVelocity, parameters.HorizontalVelocity);

                // Move character
                this.Controller.Move(transform.TransformDirection(this.Velocity) * Time.deltaTime);

                // update model
                model.ViewParameters.HorizontalPosition = this.transform.position.x;
                model.ViewParameters.VerticalPosition = this.transform.position.y;
                model.ViewParameters.Direction = this.moveDirection;
                //Debug.Log(this.transform.position.x.ToString() + ", " + this.transform.position.y.ToString() + ", " + this.transform.position.z.ToString());
            })
            .AddTo(this.Disposables);

            //this.Model = model;
        }


        /// <summary>
        /// Define Animator states
        /// </summary>
        private void InitializeAnimator()
        {


            this.JumpTrigger = new AnimationTrigger("Jump", this.Animator);
            this.SecondJumpTrigger = new AnimationTrigger("SecondJump", this.Animator);
            this.LandingTrigger = new AnimationTrigger("Landing", this.Animator);
            this.RestTrigger = new AnimationTrigger("Rest", this.Animator);

            this.WeakDamageTrigger = new AnimationTrigger("WeakDamage", this.Animator);
            this.StrongDamageTrigger = new AnimationTrigger("StrongDamage", this.Animator);
            this.FlyDamageTrigger = new AnimationTrigger("FlyDamage", this.Animator);
            this.StunDamageTrigger = new AnimationTrigger("StunDamage", this.Animator);


            this.AnimatorSpeed = new AnimatorValueFloat("Speed", this.Animator);
            this.AttackEnabled = new ReadOnlyAnimatorValueFloat("AttackEnabled", this.Animator);
            this.DamageMove = new ReadOnlyAnimatorValueFloat("DamageMove", this.Animator);
            this.JumpStability = new ReadOnlyAnimatorValueFloat("JumpStability", this.Animator);

            this.OnGrond = new AnimatorValueBool("OnGround", this.Animator);
            this.Attack1 = new AnimatorValueBool("Attack1", this.Animator);
            this.Attack2_0 = new AnimatorValueBool("Attack2_0", this.Animator);
            this.Attack2 = new AnimatorValueBool("Attack2", this.Animator);


            this.AnimatorStates = new AnimatorStateManager().AddTo(this.Disposables);


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


            var weakDamageState = new AnimatorState("Base Layer.WeakDamage", this.AnimatorStates);
            this.StrongDamageState = new AnimatorState("Base Layer.StrongDamage", this.AnimatorStates);
            this.FlyDamageState = new AnimatorState("Base Layer.FlyDamage", this.AnimatorStates);
            var stunDamageState = new AnimatorState("Base Layer.StunDamage", this.AnimatorStates);

            //var damagedGround1State = new AnimatorState("Base Layer.DamagedGround1", this.AnimatorStates);
            //var damagedGround2State = new AnimatorState("Base Layer.DamagedGround2", this.AnimatorStates);
            //var damagedGround3State = new AnimatorState("Base Layer.DamagedGround3", this.AnimatorStates);
            //var damagedAir1State = new AnimatorState("Base Layer.DamagedAir1", this.AnimatorStates);
            //var damagedAir2State = new AnimatorState("Base Layer.DamagedAir2", this.AnimatorStates);
            //var damagedAir3State = new AnimatorState("Base Layer.DamagedAir3", this.AnimatorStates);


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
                weakDamageState,
                this.StrongDamageState,
                this.FlyDamageState,
                stunDamageState,
            }
            .ForEach(y => y.Tags.Add(damagedTag));

            //new[]
            //{
            //    damagedAir1State,
            //    damagedAir2State,
            //    damagedAir3State,
            //}
            //.ForEach(y => y.Tags.Add(clearSecondJumpFlagTag));


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
                //airAttack2_2State,
                landAttack2_0State,
                landAttack2_1State,
                //landAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(punchAttackTag));

            new[]
            {
                airAttack2_2State,
                landAttack2_2State,
            }
            .ForEach(y => y.Tags.Add(punch2AttackTag));

            new[]
            {
                airAttack1State,
                landAttack1State,
            }
            .ForEach(y => y.Tags.Add(kickAttackTag));

        }

        /// <summary>
        /// Mirror walking direction
        /// </summary>
        private void ToggleMoveDirection()
        {
            this.transform.Rotate(new Vector3(0, 1, 0), 180);
            this.moveDirection *= -1f;
            this.currentSpeed *= -1f;
        }



        /// <summary>
        /// Walk or run
        /// </summary>
        /// <param name="parameters"></param>
        private void HorizontalMove(TemporaryParameters parameters)
        {

            var velocity = parameters.Desired.HorizontalVelocity
                * (parameters.Desired.IsRunning ? 1f : 0.4f);

            // Change Direction
            if (velocity * this.moveDirection < 0 && parameters.IsGrounded)
            {
                this.ToggleMoveDirection();
                parameters.HorizontalVelocity *= -1f;
            }

            this.targetObject.transform.rotation = Quaternion.Euler(0, this.moveDirection * -70 + 10, 0);

            velocity *= this.moveDirection;


            // Horizontal Move
            if (parameters.IsGrounded)
            {
                if (this.Lan2State.IsActive)
                {
                    velocity = 0;
                }

                this.AnimatorSpeed.Value = velocity * 5f;
                //this.Animator.SetFloat("Speed", velocity * 5f);


                var desiredSpeed = velocity > 0 ? velocity * 0.6f : 0;

                this.currentSpeed = desiredSpeed
                    - (desiredSpeed - this.currentSpeed) * this.accelTime;// / Time.deltaTime;

                if (this.AnimatorStates.HasTag(inAttackTag))
                {
                    parameters.HorizontalVelocity *= 0.9f;
                }
                else
                {
                    parameters.HorizontalVelocity = this.currentSpeed * velocityCoefficient;
                }
            }
            else
            {
                this.currentSpeed = 0f;
            }

        }

        /// <summary>
        /// Jump or second jump
        /// </summary>
        /// <param name="parameters"></param>
        private void Jump(TemporaryParameters parameters)
        {

            // Clear jump flags
            if (parameters.IsGrounded)
            {
                this.JumpTrigger.Clear();
                this.LandingTrigger.Clear();
                this.SecondJumpTrigger.Clear();
                parameters.VerticalVelocity = 0f;
            }


            //if (this.AnimatorStates.IsNotInTransition
            //    && this.AnimatorStates.HasTag(clearSecondJumpFlagTag))
            //{
            //    this.isSecondJumpDone = false;
            //}

            //if (this.AnimatorStates.IsNotInTransition
            //    && !this.JumpState.IsActive)
            //{
            //    this.JumpRequest = false;
            //}

            // Jump
            if (parameters.Desired.Jump)// && !this.JumpRequest)
            {
                if (parameters.IsGrounded)
                {
                    if (this.IdleState.IsActive || this.LocoState.IsActive)
                    {
                        this.JumpRequest = true;

                        this.JumpTrigger.Set();
                        this.LandingTrigger.Clear();
                        this.SecondJumpTrigger.Clear();

                        this.isSecondJumpDone = false;

                        //Debug.Log("jump");
                    }
                }
                else
                {
                    if ((this.JumpState.IsActive || this.LandState.IsActive)
                        && !this.isSecondJumpDone)
                    {

                        var vd = parameters.Desired.HorizontalVelocity * velocityCoefficient * this.moveDirection * 0.3f;

                        if (parameters.HorizontalVelocity * vd < -0.0001)
                        {
                            parameters.HorizontalVelocity = vd;

                        }
                        else if (parameters.HorizontalVelocity * vd > 0.0001)
                        {
                            parameters.HorizontalVelocity += vd * 0.3f;
                        }
                        else
                        {
                            parameters.HorizontalVelocity += vd;
                        }

                        parameters.VerticalVelocity = jumpSpeed * 0.8f;

                        this.SecondJumpTrigger.Set();
                        this.LandingTrigger.Clear();
                        this.JumpTrigger.Clear();

                        this.isSecondJumpDone = true;

                    }
                }
            }

            // Jump
            if (this.JumpState.IsActive && this.JumpRequest && this.JumpStartTrigger)
            {
                parameters.VerticalVelocity = jumpSpeed;
                this.JumpRequest = false;
            }

        }


        /// <summary>
        /// Avtivate attack collider
        /// </summary>
        /// <param name="key"></param>
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
        /// attack, damage
        /// </summary>
        /// <param name="parameters"></param>
        private void AttackOrDamage(TemporaryParameters parameters)
        {

            // not in attack
            if (this.AnimatorStates.IsNotInTransition
                //&& this.AnimatorStates.HasTag(damagedTag))
                && !this.AnimatorStates.HasTag(inAttackTag))
            {
                this.isInAttack = false;
                this.AttackCollider.ClearCollider();

                if (this.AnimatorStates.HasTag(damagedTag))
                {
                    this.Attack1.Value = false;
                    this.Attack2.Value = false;
                    this.Attack2_0.Value = false;
                    //this.Animator.SetBool("Attack1", false);
                    //this.Animator.SetBool("Attack2", false);
                    //this.Animator.SetBool("Attack2_0", false);
                }

            }

            var attackEnabled = this.AttackEnabled.Value > 0.5f;// this.Animator.GetFloat("AttackEnabled") > 0.5f;


            if (attackEnabled && this.AnimatorStates.HasTag(punchAttackTag))
            {
                this.StartAttack("Punch");
                //Debug.Log("p1");
            }
            else if (attackEnabled && this.AnimatorStates.HasTag(punch2AttackTag))
            {
                this.StartAttack("Punch2");
                //Debug.Log("p2");
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

            if (this.AnimatorStates.IsNotInTransition)
            {
                if (this.StrongDamageState.IsActive)
                {
                    parameters.HorizontalVelocity = (parameters.Desired.BackDamage ? 1f : -1f)
                        * this.DamageMove.Value * (parameters.IsGrounded ? 1f : 0.2f);
                    //Debug.Log(parameters.HorizontalVelocity.ToString());
                }
                else if (this.FlyDamageState.IsActive)
                {
                    parameters.HorizontalVelocity = (parameters.Desired.BackDamage ? 1f : -1f)
                        * this.DamageMove.Value;
                }
                else if (this.AnimatorStates.HasTag(damagedTag))
                {
                    parameters.HorizontalVelocity = 0f;
                }
            }

            var attack2Flag = false;

            if (parameters.Desired.IsDamaged)
            {
                // Damage
                
                switch (parameters.Desired.DamageType)
                {
                    case AttackType.Weak:
                        this.WeakDamageTrigger.Set();
                        break;
                    case AttackType.Strong:
                        this.StrongDamageTrigger.Set();
                        break;
                    //case AttackType.Stun:
                    //    this.StunDamageTrigger.Set();
                    //    break;
                    //case AttackType.Fly:
                    //    this.FlyDamageTrigger.Set();
                    //    break;
                    default:
                        this.WeakDamageTrigger.Set();
                        break;
                }
                this.isSecondJumpDone = false;
                this.AttackCollider.ClearCollider();
            }
            else
            {
                
                // Attack

                if (this.AnimatorStates.HasTag(attackableTag)
                    && !this.AnimatorStates.IsInTransition
                    && !this.isInAttack)
                {
                    if (parameters.Desired.Kick)
                    {
                        this.isInAttack = true;
                        this.Attack1.Value = true;
                        //this.Animator.SetBool("Attack1", true);
                        parameters.VerticalVelocity += jumpSpeed * 0.2f;
                        this.AttackCollider.Information.Power = 3;
                        this.AttackCollider.Information.Type = AttackType.Strong;
                    }
                    else if (parameters.Desired.Punch)
                    {
                        this.isInAttack = true;
                        this.Attack2_0.Value = true;
                        //this.Animator.SetBool("Attack2_0", true);
                        this.AttackCollider.Information.Power = 2;
                        this.AttackCollider.Information.Type = AttackType.Weak;
                    }
                    else if (parameters.Desired.RushPunch)
                    {
                        this.isInAttack = true;
                        attack2Flag = true;
                        this.AttackCollider.Information.Power = 1;
                        this.AttackCollider.Information.Type = AttackType.Weak;
                    }
                    else if ((this.IdleState.IsActive || this.RestState.IsActive) && parameters.Desired.Rest)
                    {
                        this.RestTrigger.Set();
                    }
                }
                else if (this.isInAttack
                    && this.AnimatorStates.HasTag(inRushAttackTag))
                {
                    if (parameters.Desired.RushPunch)
                    {
                        attack2Flag = true;
                        this.Attack2_0.Value = true;
                        //this.Animator.SetBool("Attack2_0", false);
                        this.AttackCollider.Information.Power = 1;
                        this.AttackCollider.Information.Type = AttackType.Weak;
                    }
                }
            }


            this.Attack2.Value = attack2Flag;
            //this.Animator.SetBool("Attack2", attack2Flag);

        }

        /// <summary>
        /// gravity effect
        /// </summary>
        /// <param name="parameters"></param>
        private void Gravity(TemporaryParameters parameters)
        {

            // Gravity
            var gravity
                = (!isInAttack) ? 20f
                : (parameters.VerticalVelocity > 0) ? 10f
                : 5f;
            parameters.VerticalVelocity -= gravity * Time.deltaTime;


            // Landing
            var jumping = this.JumpState.IsActive || this.Jmp2State.IsActive;
            var stability = jumping ? this.JumpStability.Value : 0f;// this.Animator.GetFloat("JumpStability") : 0f;

            if ((parameters.VerticalVelocity < -jumpSpeed * 0.6f && jumping)
                || (jumping && parameters.IsGrounded && (stability > 0.8)))
            // || (Math.Abs(desiredVelocity.y) > 0.1 && desiredVelocity.y < this.jumpspeed * 0.5))))
            //!this.JumpRequest))// // || (!jumping && !isIdle && cc.isGrounded))
            {
                if (!this.LandState.IsActive)
                {
                    this.LandingTrigger.Set();
                    this.isSecondJumpDone = true;
                }
            }

            this.OnGrond.Value = parameters.IsGrounded;
            //this.Animator.SetBool("OnGround", parameters.IsGrounded);

            if (parameters.IsGrounded && !this.AnimatorStates.IsInTransition)
            {
                this.isSecondJumpDone = false;
            }

        }

        ///// <summary>
        ///// Update is called once per frame
        ///// </summary>
        //private void Move(TemporaryParameters parameters)
        //{
        //    
        //}

        /// <summary>
        /// Attack animation end event
        /// </summary>
        /// <param name="arg"></param>
        public void OnMotionEnded(string arg)
        {
            if (arg.Equals("KickEnded"))
            {
                this.Attack1.Value = false;
                //this.Animator.SetBool("Attack1", false);
                this.isInAttack = false;
            }
            else if (arg.Equals("PunchEnded"))
            {
                this.Attack2_0.Value = false;
                //this.Animator.SetBool("Attack2_0", false);
                this.isInAttack = false;
            }
        }

        /// <summary>
        /// jump animation event
        /// </summary>
        /// <param name="arg"></param>
        public void OnJumpStateChanged(string arg)
        {
            if (arg.Equals("JumpTakeoff"))
            {
                this.JumpStartTrigger = true;
            }
        }

        /// <summary>
        /// Collider hit event
        /// </summary>
        /// <param name="hit"></param>
        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var target = hit.gameObject;

            // avoid that ride on another character
            if (target.CompareTag("Player")
                && this.transform.position.y > target.transform.position.y + this.Controller.height * 0.5f
                && this.Velocity.y < jumpSpeed * 0.1f)
            {
                var distance = this.transform.position.z - target.transform.position.z;
                var direction = 1;// distance > 0 ? 1 : -1;


                this.Controller.Move(transform.TransformDirection(new Vector3(0, 0, direction * 4f * Time.deltaTime)));


            }
            
        }


        /// <summary>
        /// Parameters for move
        /// </summary>
        private class TemporaryParameters
        {
            public DesiredParameters Desired { get; private set; }
            public bool IsGrounded { get; set; }
            public float HorizontalVelocity { get; set; }
            public float VerticalVelocity { get; set; }

            public TemporaryParameters(DesiredParameters input)
            {
                this.Desired = input;
            }
        }
    }
}
