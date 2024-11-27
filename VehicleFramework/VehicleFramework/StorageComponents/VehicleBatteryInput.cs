using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.Localization;

namespace VehicleFramework
{
    public class VehicleBatteryInput : HandTarget, IHandTarget
	{
		public EnergyMixin mixin;
		public EnglishString tooltip;

		public void OnHandHover(GUIHand hand)
		{
			if (VehicleTypes.Drone.mountedDrone != null)
			{
				return;
			}
			HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(tooltip));
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
		}

		public void OnHandClick(GUIHand hand)
		{
			if (VehicleTypes.Drone.mountedDrone != null)
			{
				return;
			}
			gameObject.GetComponentInParent<ModVehicle>().OnAIBatteryReload();
			mixin.InitiateReload(); // this brings up the battery-changing gui
		}
	}
}
