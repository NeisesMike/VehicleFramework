//using VehicleFramework.Localization;
using UnityEngine;

namespace VehicleFramework.StorageComponents
{
    public class VehicleBatteryInput : HandTarget, IHandTarget
	{
		public EnergyMixin? mixin;

		// need this SerializeField attribute or else assignment in
		// VehicleBuilder is not propogated to instances
		[SerializeField]
		internal string? tooltip;

		public void OnHandHover(GUIHand hand)
		{
			if (VehicleTypes.Drone.MountedDrone != null)
			{
				return;
			}
			HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get(tooltip));
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
		}

		public void OnHandClick(GUIHand hand)
		{
			if (VehicleTypes.Drone.MountedDrone != null || mixin == null)
			{
				return;
			}
			gameObject.GetComponentInParent<ModVehicle>().OnAIBatteryReload();
			mixin.InitiateReload(); // this brings up the battery-changing gui
		}
	}
}
