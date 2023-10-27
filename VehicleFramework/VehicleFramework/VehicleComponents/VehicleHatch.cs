using System;
using System.Collections.Generic;
using System.Collections;
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
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.ExitVehicle));
			}
			else
			{
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.EnterVehicle));
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			Player.main.rigidBody.velocity = Vector3.zero;
			Player.main.rigidBody.angularVelocity = Vector3.zero;
			if (mv.IsPlayerInside())
			{
				mv.PlayerExit();
				if (mv.transform.position.y < -3f)
				{
					Player.main.transform.position = ExitLocation.position;
				}
				else
				{
					StartCoroutine(ExitToSurface());
				}
			}
			else
			{
				Player.main.transform.position = EntryLocation.position;
				mv.PlayerEntry();
			}
		}

		public IEnumerator ExitToSurface()
        {
			int tryCount = 0;
			float playerHeightBefore = Player.main.transform.position.y;
			while (Player.main.transform.position.y < 2 + playerHeightBefore)
			{
				if(100 < tryCount)
                {
					Logger.Error("Error: Failed to exit vehicle too many times. Stopping.");
					yield break;
                }
				Player.main.transform.position = SurfaceExitLocation.position;
				tryCount++;
				yield return null;
			}
		}
	}
}
