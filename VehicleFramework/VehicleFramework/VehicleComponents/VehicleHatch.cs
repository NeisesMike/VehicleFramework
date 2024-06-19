using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
	public class VehicleHatch : HandTarget, IHandTarget
	{
		public bool isLive = true;
		public ModVehicle mv;
		public Transform EntryLocation;
		public Transform ExitLocation;
		public Transform SurfaceExitLocation;

		public void OnHandHover(GUIHand hand)
		{
			if (!isLive || Drone.mountedDrone != null)
			{
				return;
			}
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			if ((mv as Submarine != null))
			{
				if ((mv as Submarine).IsPlayerInside())
				{
					HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.ExitVehicle));
				}
				else
				{
					HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.EnterVehicle));
				}
			}
			else if ((mv as Submersible != null) || (mv as Walker != null) || (mv as Skimmer != null))
			{
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, LocalizationManager.GetString(EnglishString.EnterVehicle));
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			if (!isLive || Drone.mountedDrone != null)
			{
				return;
			}
			Player.main.rigidBody.velocity = Vector3.zero;
			Player.main.rigidBody.angularVelocity = Vector3.zero;
			if ((mv as Submarine != null))
			{
				if ((mv as Submarine).IsPlayerInside())
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
					(mv as Submarine).PlayerEntry();
				}
			}
			else if (mv as Submersible != null)
			{
				Player.main.transform.position = (mv as Submersible).PilotSeat.SitLocation.transform.position;
				Player.main.transform.rotation = (mv as Submersible).PilotSeat.SitLocation.transform.rotation;
				(mv as Submersible).PlayerEntry();
			}
			/*
			if (mv as Walker != null)
			{
				Player.main.transform.position = (mv as Walker).PilotSeat.SitLocation.transform.position;
				Player.main.transform.rotation = (mv as Walker).PilotSeat.SitLocation.transform.rotation;
				mv.PlayerEntry();
			}
			if (mv as Skimmer != null)
			{
				Player.main.transform.position = (mv as Skimmer).PilotSeats.First().SitLocation.transform.position;
				Player.main.transform.rotation = (mv as Skimmer).PilotSeats.First().SitLocation.transform.rotation;
				mv.PlayerEntry();
			}
			*/
		}

		public IEnumerator ExitToSurface()
		{
			int tryCount = 0;
			float playerHeightBefore = Player.main.transform.position.y;
			while (Player.main.transform.position.y < 2 + playerHeightBefore)
			{
				if (100 < tryCount)
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
