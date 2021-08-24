using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public class VehicleHatch : HandTarget, IHandTarget
	{
		public ModVehicle mv;
		public Transform EntryLocation;
		public Transform ExitLocation;

		public void OnHandHover(GUIHand hand)
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			if (mv.IsPlayerInside())
			{
				HandReticle.main.SetInteractText("Exit Vehicle");
			}
			else
			{
				HandReticle.main.SetInteractText("Enter Vehicle");
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			if (mv.IsPlayerInside())
			{
				Player.main.transform.position = ExitLocation.position;
				mv.PlayerExit();
			}
			else
			{
				Player.main.transform.position = EntryLocation.position;
				mv.PlayerEntry();
			}
		}
	}
}
