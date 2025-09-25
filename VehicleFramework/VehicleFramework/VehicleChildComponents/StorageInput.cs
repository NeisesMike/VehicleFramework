using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using VehicleFramework.Localization;

namespace VehicleFramework.StorageComponents
{
	public abstract class StorageInput : HandTarget, IHandTarget
	{
		public ModVehicle? mv;
		public int slotID = -1;
		public GameObject? model;
		public Collider? collider;
		public float timeOpen = 0.5f;
		public float timeClose = 0.25f;
		public FMODAsset? openSound;
		public FMODAsset? closeSound;
		protected Vehicle.DockType dockType;
		protected bool state;

		public string DisplayName { get; set; } = string.Empty;

		public abstract void OpenFromExternal();
		protected abstract void OpenPDA();



		public override void Awake()
		{
			base.Awake();
			this.UpdateColliderState();

			// go up in the transform heirarchy until we find the ModVehicle
			Transform modVe = transform;
			while (modVe.gameObject.GetComponent<ModVehicle>() == null)
			{
				modVe = modVe.parent;
			}
			mv = modVe.gameObject.GetComponent<ModVehicle>();
			SetEnabled(true);
		}
		protected static void OnDisable()
		{

		}
		protected void ChangeFlapState()
		{
			//Utils.PlayFMODAsset(open ? this.openSound : this.closeSound, base.transform, 1f);
			Utils.PlayFMODAsset(this.openSound, base.transform, 1f);
			OpenPDA();
		}
		protected void OnClosePDA(PDA? pda)
		{
			seq.Set(0, false, null);
			gameObject.GetComponentInParent<ModVehicle>().OnStorageOpen(transform.name, false);
			Utils.PlayFMODAsset(this.closeSound, base.transform, 1f);
			Logger.DebugLog($"pda closed: {pda?.name ?? string.Empty}");
		}
		protected void UpdateColliderState()
		{
			if (this.collider != null)
			{
				this.collider.enabled = (this.state && this.dockType != Vehicle.DockType.Cyclops);
			}
		}
		public void SetEnabled(bool state)
		{
			if (this.state == state)
			{
				return;
			}
			this.state = state;
			this.UpdateColliderState();
			this.model?.SetActive(state);
		}
		public void SetDocked(Vehicle.DockType dockType)
		{
			this.dockType = dockType;
			this.UpdateColliderState();
		}
		public void OnHandHover(GUIHand hand)
		{
			if (VehicleTypes.Drone.MountedDrone != null)
			{
				return;
			}
			string nameDisplayed;
			if (DisplayName.Equals(string.Empty))
			{
				nameDisplayed = Language.main.Get("VFOpenStorage");
			}
			else
			{
				nameDisplayed = DisplayName;
			}
			HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, nameDisplayed);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
		}

		public Sequence seq = new();
		public void Update()
		{
			seq.Update();
		}
		public void OnHandClick(GUIHand hand)
		{
			if (VehicleTypes.Drone.MountedDrone != null)
			{
				return;
			}
			float timeToWait = gameObject.GetComponentInParent<ModVehicle>().OnStorageOpen(transform.name, true);
			seq.Set(timeToWait, true, new SequenceCallback(ChangeFlapState));
		}
	}
}
