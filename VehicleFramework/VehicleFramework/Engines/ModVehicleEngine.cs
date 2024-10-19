using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.Engines
{
    public abstract class ModVehicleEngine : MonoBehaviour
    {
        public ModVehicle mv;
        public Rigidbody rb;
        private EngineSounds _sounds = null;
        public EngineSounds sounds
        {
            get
            {
                return _sounds;
            }
            set
            {
                _sounds = value;
                EngineSource1.clip = value.hum;
                EngineSource2.clip = value.whistle;
            }
        }


        public float WhistleFactor = 0.4f;
        public float HumFactor = 1f;
        public bool blockVoiceChange => false;
        public virtual bool CanMoveAboveWater { get; set; } = false;
        public virtual bool CanRotateAboveWater { get; set; } = false;


        public float damageModifier { get; set; } = 1f;

        protected virtual float FORWARD_TOP_SPEED => 1000;
        protected virtual float REVERSE_TOP_SPEED => 1000;
        protected virtual float STRAFE_MAX_SPEED => 1000;
        protected virtual float VERT_MAX_SPEED => 1000;
        protected virtual float FORWARD_ACCEL => FORWARD_TOP_SPEED / 10f;
        protected virtual float REVERSE_ACCEL => REVERSE_TOP_SPEED / 10f;
        protected virtual float STRAFE_ACCEL => STRAFE_MAX_SPEED / 10f;
        protected virtual float VERT_ACCEL => VERT_MAX_SPEED / 10f;

        protected virtual float waterDragDecay => 4.5f;
        protected virtual float airDragDecay => 1.5f;
        protected virtual float DragDecay
        {
            get
            {
                if (mv.GetIsUnderwater())
                {
                    return waterDragDecay;
                }
                else
                {
                    return airDragDecay;
                }
            }
        }

        protected float _forwardMomentum = 0;
        protected virtual float ForwardMomentum
        {
            get
            {
                return _forwardMomentum;
            }
            set
            {
                if (value < -REVERSE_TOP_SPEED)
                {
                    _forwardMomentum = -REVERSE_TOP_SPEED;
                }
                else if (FORWARD_TOP_SPEED < value)
                {
                    _forwardMomentum = FORWARD_TOP_SPEED;
                }
                else
                {
                    _forwardMomentum = value;
                }
            }
        }
        protected virtual void UpdateForwardMomentum(float inputMagnitude)
        {
            if (0 < inputMagnitude)
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * FORWARD_ACCEL * Time.fixedDeltaTime;
            }
            else
            {
                ForwardMomentum = ForwardMomentum + inputMagnitude * REVERSE_ACCEL * Time.fixedDeltaTime;
            }
        }

        protected float _rightMomentum = 0;
        protected virtual float RightMomentum
        {
            get
            {
                return _rightMomentum;
            }
            set
            {
                if (value < -STRAFE_MAX_SPEED)
                {
                    _rightMomentum = -STRAFE_MAX_SPEED;
                }
                else if (STRAFE_MAX_SPEED < value)
                {
                    _rightMomentum = STRAFE_MAX_SPEED;
                }
                else
                {
                    _rightMomentum = value;
                }
            }
        }
        protected virtual void UpdateRightMomentum(float inputMagnitude)
        {
            if (inputMagnitude != 0)
            {
                RightMomentum += inputMagnitude * STRAFE_ACCEL * Time.fixedDeltaTime;
            }
        }

        protected float _upMomentum = 0;
        protected virtual float UpMomentum
        {
            get
            {
                return _upMomentum;
            }
            set
            {
                if (value < -VERT_MAX_SPEED)
                {
                    _upMomentum = -VERT_MAX_SPEED;
                }
                else if (VERT_MAX_SPEED < value)
                {
                    _upMomentum = VERT_MAX_SPEED;
                }
                else
                {
                    _upMomentum = value;
                }
            }
        }
        protected virtual void UpdateUpMomentum(float inputMagnitude)
        {
            UpMomentum += inputMagnitude * VERT_ACCEL * Time.fixedDeltaTime;
        }

        protected float _engineHum = 0;
        protected virtual float EngineHum
        {
            get
            {
                return _engineHum;
            }
            set
            {
                if (value < 0)
                {
                    _engineHum = 0;
                }
                else if (10 < value)
                {
                    _engineHum = 10;
                }
                else
                {
                    _engineHum = value;
                }
            }
        }
        protected virtual void UpdateEngineHum(float inputMagnitude)
        {
            if (inputMagnitude == 0)
            {
                inputMagnitude = -1;
            }
            EngineHum += inputMagnitude * Time.deltaTime;
        }
        protected bool isReadyToWhistle = true;
        private AudioSource EngineSource1;
        private AudioSource EngineSource2;

        public virtual void Awake()
        {
            // register self with mainpatcher, for on-the-fly voice selection updating
            EngineSoundsManager.engines.Add(this);
        }
        // Start is called before the first frame update
        public virtual void Start()
        {
            rb.centerOfMass = Vector3.zero;
            rb.angularDrag = 5f;

            EngineSource1 = mv.gameObject.AddComponent<AudioSource>();
            EngineSource1.loop = true;
            EngineSource1.playOnAwake = false;
            EngineSource1.priority = 0;

            EngineSource2 = mv.gameObject.AddComponent<AudioSource>();
            EngineSource2.loop = false;
            EngineSource2.playOnAwake = false;
            EngineSource2.priority = 0;
            sounds = EngineSoundsManager.GetDefaultVoice(mv);
        }
        public void OnDisable()
        {
            EngineSource1?.Stop();
            EngineSource2?.Stop();
        }
        public virtual void FixedUpdate()
        {
            var fcc = MainCameraControl.main.GetComponent<FreecamController>();
            bool isFreecam = false;
            if (fcc.mode || fcc.ghostMode)
            {
                isFreecam = true;
            }
            Vector3 DoMoveAction()
            {
                // Get Input Vector
                Vector3 innerMoveDirection = GameInput.GetMoveDirection();
                // Apply controls to the vehicle state
                ApplyPlayerControls(innerMoveDirection);
                // Drain power based on Input Vector (and modifiers)
                // TODO: DrainPower with ApplyPlayerControls...
                // or would it be better with ExecutePhysicsMove...?
                DrainPower(innerMoveDirection);
                return innerMoveDirection;
            }
            Vector3 moveDirection = Vector3.zero;
            if (mv.GetIsUnderwater() || CanMoveAboveWater)
            {
                if (mv.CanPilot() && mv.IsUnderCommand && !isFreecam)
                {
                    if (mv as Submarine != null)
                    {
                        if(mv.IsPlayerControlling())
                        {
                            moveDirection = DoMoveAction();
                        }
                    }
                    else
                    {
                        moveDirection = DoMoveAction();
                    }
                }
                // Execute a state-based physics move
                ExecutePhysicsMove();
                if (moveDirection == Vector3.zero)
                {
                    UpdateEngineHum(-3);
                }
                else
                {
                    UpdateEngineHum(moveDirection.magnitude);
                }
                PlayEngineHum();
                PlayEngineWhistle(moveDirection);
            }
            else
            {
                UpdateEngineHum(-3);
            }
            ApplyDrag(moveDirection);
        }
        protected virtual float DragThresholdSpeed
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }
        protected virtual void ApplyDrag(Vector3 move)
        {
            // Only apply drag if we aren't applying movement in that direction (or falling).
            // That is, if we aren't holding forward, our forward momentum should decay.
            // Kill anything under 1%
            bool isForward = move.z != 0;
            bool isRight = move.x != 0;
            bool isUp = move.y != 0;
            bool activated = isForward || isRight || isUp || mv.worldForces.IsAboveWater();

            if (!isForward)
            {
                if (0 < Mathf.Abs(ForwardMomentum))
                {
                    ForwardMomentum -= DragDecay * ForwardMomentum * Time.deltaTime;
                }
            }
            if (!isRight)
            {
                if (0 < Mathf.Abs(RightMomentum))
                {
                    RightMomentum -= DragDecay * RightMomentum * Time.deltaTime;
                }
            }
            if (!isUp)
            {
                if (0 < Mathf.Abs(UpMomentum))
                {
                    UpMomentum -= DragDecay * UpMomentum * Time.deltaTime;
                }
            }
            if(!activated && rb.velocity.magnitude < DragThresholdSpeed)
            {
                ForwardMomentum = 0;
                RightMomentum = 0;
                UpMomentum = 0;
                rb.velocity = Vector3.zero;
            }
        }
        public virtual void ExecutePhysicsMove()
        {
            rb.AddForce(damageModifier * mv.transform.forward * (ForwardMomentum / 100f) * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddForce(damageModifier * mv.transform.right   * (RightMomentum   / 100f) * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddForce(damageModifier * mv.transform.up      * (UpMomentum      / 100f) * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        public enum ForceDirection
        {
            forward,
            backward,
            strafe,
            updown
        }
        public virtual void ApplyPlayerControls(Vector3 moveDirection)
        {
            if(Player.main.GetPDA().isOpen)
            {
                return;
            }

            // Thank you to MrPurple6411 for this snip regarding VehicleAccelerationModifier
            var modifiers = base.gameObject.GetComponentsInChildren<VehicleAccelerationModifier>();
            foreach (var modifier in modifiers)
            {
                modifier.ModifyAcceleration(ref moveDirection);
            }

            // Control velocity
            UpdateRightMomentum(moveDirection.x);
            UpdateUpMomentum(moveDirection.y);
            UpdateForwardMomentum(moveDirection.z);
            return;
        }
        public virtual void ControlRotation()
        {
            if (mv.GetIsUnderwater() || CanRotateAboveWater)
            {
                // Control rotation
                float pitchFactor = 1.4f;
                float yawFactor = 1.4f;
                Vector2 mouseDir = GameInput.GetLookDelta();
                float xRot = mouseDir.x;
                float yRot = mouseDir.y;
                rb.AddTorque(mv.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
                rb.AddTorque(mv.transform.right * yRot * -pitchFactor * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
        public virtual void DrainPower(Vector3 moveDirection)
        {
            /* Rationale for these values
             * Seamoth spends this on Update
             * base.ConsumeEngineEnergy(Time.deltaTime * this.enginePowerConsumption * vector.magnitude);
             * where vector.magnitude in [0,3];
             * instead of enginePowerConsumption, we have upgradeModifier, but they are similar if not identical
             * so the power consumption is similar to that of a seamoth.
             */
            float scalarFactor = 1.0f;
            float basePowerConsumptionPerSecond = moveDirection.x + moveDirection.y + moveDirection.z;
            float upgradeModifier = Mathf.Pow(0.85f, mv.numEfficiencyModules);
            mv.GetComponent<PowerManager>().TrySpendEnergy(scalarFactor * basePowerConsumptionPerSecond * upgradeModifier * Time.deltaTime);
        }
        public virtual void PlayEngineHum()
        {
            EngineSource1.volume = EngineHum / 10f * (MainPatcher.VFConfig.engineVolume / 100) * HumFactor;
            if (mv.IsPowered())
            {
                if (!EngineSource1.isPlaying && rb.velocity.magnitude > 0.2f) // why 0.2f ?
                {
                    EngineSource1.Play();
                }
            }
            else
            {
                EngineSource1.Stop();
            }
        }
        public virtual void PlayEngineWhistle(Vector3 moveDirection)
        {
            if (gameObject.GetComponent<Rigidbody>().velocity.magnitude < 1)
            {
                isReadyToWhistle = true;
            }
            else
            {
                isReadyToWhistle = false;
            }
            if (EngineSource2.isPlaying)
            {
                if (moveDirection.magnitude == 0)
                {
                    EngineSource2.Stop();
                }
            }
            else
            {
                if (isReadyToWhistle && moveDirection.magnitude > 0)
                {
                    EngineSource2.volume = (MainPatcher.VFConfig.engineVolume / 100f) * 0.4f * WhistleFactor;
                    EngineSource2.Play();
                }
            }
        }

        public float GetTimeToStop()
        {
            float timeToXStop = Mathf.Log(0.05f * STRAFE_MAX_SPEED / RightMomentum) / (Mathf.Log(.25f));
            float timeToYStop = Mathf.Log(0.05f * VERT_MAX_SPEED / UpMomentum) / (Mathf.Log(.25f));
            float timeToZStop = Mathf.Log(0.05f * FORWARD_TOP_SPEED / ForwardMomentum) / (Mathf.Log(.25f));
            return Mathf.Max(timeToXStop,timeToYStop,timeToZStop);
        }
        public virtual void KillMomentum()
        {
            ForwardMomentum = 0f;
            RightMomentum = 0f;
            UpMomentum = 0f;
        }

        public void SetVoice(EngineSounds inputVoice)
        {
            if (!blockVoiceChange)
            {
                sounds = inputVoice;
            }
        }
        public void SetVoice(KnownEngineSounds voiceName)
        {
            if (!blockVoiceChange)
            {
                sounds = EngineSoundsManager.GetVoice(EngineSoundsManager.GetKnownVoice(voiceName));
            }
        }
    }
}
        

