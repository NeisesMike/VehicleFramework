using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.Interfaces;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework.VehicleChildComponents
{
	public class VehicleHatch : HandTarget, IHandTarget, IDockListener
	{
		private bool isLive = true;
		public required ModVehicle mv;
		public required Transform EntryLocation;
		public required Transform ExitLocation;
		public Transform? SurfaceExitLocation;
		public string EnterHint = Language.main.Get("VFEnterVehicle");
		public string ExitHint = Language.main.Get("VFExitVehicle");

		public void OnHandHover(GUIHand hand)
		{
			if (!isLive || Drone.MountedDrone != null)
			{
				return;
			}
			HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
			Submarine? Sub = mv as Submarine;
			if (Sub != null)
			{
				if (Sub.IsPlayerInside())
				{
					HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, ExitHint);
				}
				else
				{
					HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, EnterHint);
				}
			}
			else if (mv as Submersible != null)// || (mv as Walker != null)) || (mv as Skimmer != null))
			{
				HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, EnterHint);
			}
		}

		public void OnHandClick(GUIHand hand)
		{
			if (!isLive || Drone.MountedDrone != null)
			{
				return;
			}
			Player.main.rigidBody.velocity = Vector3.zero;
			Player.main.rigidBody.angularVelocity = Vector3.zero;
			Submarine? Sub = mv as Submarine;
			Submersible? Subbie = mv as Submersible;
			if (Sub != null)
			{
				if (Sub.IsPlayerInside())
				{
					mv.PlayerExit();
					if (mv.transform.position.y < -3f || SurfaceExitLocation == null)
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
					Sub.PlayerEntry();
				}
			}
			else if (Subbie != null && !mv.IsScuttled)
			{
				Player.main.transform.position = Subbie.PilotSeat.SitLocation.transform.position;
				Player.main.transform.rotation = Subbie.PilotSeat.SitLocation.transform.rotation;
				Subbie.PlayerEntry();
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
			if (SurfaceExitLocation == null)
			{
				yield break;
			}
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

		void IDockListener.OnDock()
		{
			isLive = false;
		}

		void IDockListener.OnUndock()
		{
			isLive = true;
		}

		internal static void Create(VehicleHatchStruct vhs, ModVehicle mv)
		{
			var hatch = vhs.Hatch.EnsureComponent<VehicleHatch>();
			hatch.mv = mv;
			hatch.EntryLocation = vhs.EntryLocation;
			hatch.ExitLocation = vhs.ExitLocation;
			hatch.SurfaceExitLocation = vhs.SurfaceExitLocation;
		}

		public static void SetHintStrings(Submarine sub, string enterHint, string exitHint)
		{
			sub.Hatches.ForEach(x => x.Hatch.GetComponent<VehicleFramework.VehicleChildComponents.VehicleHatch>().EnterHint = enterHint);
			sub.Hatches.ForEach(x => x.Hatch.GetComponent<VehicleFramework.VehicleChildComponents.VehicleHatch>().ExitHint = exitHint);
		}
	}
}
