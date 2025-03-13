using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Engines
{
	public class GroundVehicleEngine : VFEngine
	{
		private ModVehicle mv => GetComponent<ModVehicle>();
		private bool onGround = false;
		private Vector3 surfaceNormal = Vector3.up;
		private float _timeLastJumped = 0;
		protected float TimeLastJumped
		{
			get
			{
				return _timeLastJumped;
			}
			private set
			{
				_timeLastJumped = value;
			}
		}
		private float _timeOnGround = 0;
		protected float TimeOnGround
		{
			get
			{
				return _timeOnGround;
			}
			private set
			{
				_timeOnGround = value;
			}
		}
		public float UnderwaterGravity
        {
            get
            {
				return mv.worldForces.underwaterGravity;
			}
            set
			{
				mv.worldForces.underwaterGravity = value;
			}
		}
		public float AbovewaterGravity
		{
			get
			{
				return mv.worldForces.aboveWaterGravity;
			}
			set
			{
				mv.worldForces.aboveWaterGravity = value;
			}
		}
		protected float OnGroundForceMultiplier { get; set; } = 1.4f;

		public virtual void Awake()
        {
			UnderwaterGravity = 5f;
			AbovewaterGravity = 9.8f;
		}

		#region collision_methods
		private void OnCollisionEnter(Collision collision)
		{
			HandleOnGround(collision);
		}
		private void OnCollisionStay(Collision collision)
		{
			HandleOnGround(collision);
		}
		private void OnCollisionExit(Collision collisionInfo)
		{
			HandleOnGroundExit();
		}
		private void HandleOnGroundExit()
		{
			onGround = false;
			surfaceNormal = Vector3.up;
			GetComponent<Vehicle>().worldForces.handleGravity = true;
		}
		private void HandleOnGround(Collision collision)
		{
			if (!CanLand())
			{
				return;
			}
			surfaceNormal = new Vector3(0f, -1f, 0f);
			int num = 0;
			for (int i = 0; i < collision.contacts.Length; i++)
			{
				ContactPoint contactPoint = collision.contacts[i];
				if (contactPoint.normal.y > surfaceNormal.y)
				{
					surfaceNormal.y = contactPoint.normal.y;
				}
				num++;
			}
			if (num > 0)
			{
				if (surfaceNormal.y > 0.5f)
				{
					if (!onGround)// && prevVelocity.y < -6f) // this makes no sense?
					{
						OnLand();
					}
					onGround = true;
					TimeOnGround = Time.time;
				}
				else
				{
					onGround = false;
				}
				if (onGround)
				{
					GetComponent<Vehicle>().worldForces.handleGravity = false;
					return;
				}
			}
			else
			{
				surfaceNormal = new Vector3(0f, 1f, 0f);
				GetComponent<Vehicle>().worldForces.handleGravity = true;
			}
		}
		protected virtual bool CanLand()
		{
			return Time.time - TimeLastJumped > 0.5f;
		}
		protected virtual void OnLand()
		{
			//Utils.PlayFMODAsset(this.landSound, this.bottomTransform, 20f);
			//this.fxcontrol.Play(2);
		}
		#endregion

		#region jump_methods
		#endregion

		#region movement_methods
		protected override void MoveWithInput(Vector3 moveInput)
		{
			Vector3 walkingForce = new Vector3(moveInput.x, 0, moveInput.z);
			float multiplier = 0.05f;
			if (onGround)
			{
				walkingForce = Vector3.ProjectOnPlane(walkingForce, surfaceNormal);
				walkingForce.y = Mathf.Clamp(walkingForce.y, -0.5f, 0.5f);
				multiplier *= OnGroundForceMultiplier;
			}
			MV.useRigidbody.AddRelativeForce(walkingForce * multiplier, ForceMode.VelocityChange);
		}
		public override void ControlRotation()
		{
			if (CanRotate())
			{
				float yawFactor = 1.4f;
				Vector2 mouseDir = GameInput.GetLookDelta();
				float xRot = mouseDir.x;
				RB.AddTorque(MV.transform.up * xRot * yawFactor * Time.deltaTime, ForceMode.VelocityChange);
			}
		}
		public override void KillMomentum()
		{
			MV.useRigidbody.velocity = Vector3.zero;
			MV.useRigidbody.angularVelocity = Vector3.zero;
		}
        protected override void DoFixedUpdate()
        {
            base.DoFixedUpdate();
			Quaternion quaternion = Quaternion.Euler(0f, transform.localEulerAngles.y, 0f);
			if (Mathf.Abs(transform.localEulerAngles.x) < 0.001f && Mathf.Abs(transform.localEulerAngles.z) < 0.001f)
			{
				transform.localRotation = quaternion;
				return;
			}
			transform.localRotation = Quaternion.Lerp(transform.localRotation, quaternion, Time.deltaTime * 3f);
		}
        #endregion
    }
}
