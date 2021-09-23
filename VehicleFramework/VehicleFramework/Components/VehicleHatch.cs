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
		public Transform SurfaceExitLocation;

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
			Logger.Log("click");
			if (mv.IsPlayerInside())
			{
				Logger.Log("gotta exit");
				mv.PlayerExit();
				if (mv.transform.position.y < -3f)
				{
					Player.main.transform.position = ExitLocation.position;
				}
				else
				{
					Logger.Log("go to surface");
					Player.main.transform.position = Vector3.zero;// SurfaceExitLocation.position + SurfaceExitLocation.right * 10f;
				}
			}
			else
			{
				Player.main.transform.position = EntryLocation.position;
				mv.PlayerEntry();
			}
		}
	}
}
