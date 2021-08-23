using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtramaVehicle
{
    public class AtramaBatteryInput : HandTarget, IHandTarget
	{
		public EnergyMixin mixin;

		public void OnHandHover(GUIHand hand)
		{
			HandReticle.main.SetInteractText("AtramaBatteryReplace");
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
		}

		public void OnHandClick(GUIHand hand)
		{
			mixin.InitiateReload();
		}

	}
}
