using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VehicleFramework.Admin
{
	internal class ConsoleCommands : MonoBehaviour
	{
		internal static bool isUndockConsoleCommand = false; // hacky
		public void Awake()
		{
			DevConsole.RegisterConsoleCommand(this, "givevfupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfseamothupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfprawnupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfcyclopsupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfvehicles", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfvoices", false, false);
			DevConsole.RegisterConsoleCommand(this, "vfspawncodes", false, false);
			//DevConsole.RegisterConsoleCommand(this, "incrementme", false, false);
			DevConsole.RegisterConsoleCommand(this, "undockclosest", false, false);
		}
		public void OnConsoleCommand_givevfupgrades(NotificationCenter.Notification _)
		{
			UpgradeRegistrar.UpgradeIcons
				.Select(x => x.Key)
				.Where(x => UWE.Utils.TryParseEnum<TechType>(x, out TechType techType))
				.ForEach(x => DevConsole.instance.Submit("item " + x));
		}
		public void OnConsoleCommand_givevfseamothupgrades(NotificationCenter.Notification _)
		{
			UpgradeRegistrar.UpgradeIcons
				.Select(x => x.Key + "Seamoth")
				.Where(x => UWE.Utils.TryParseEnum<TechType>(x, out TechType techType))
				.ForEach(x => DevConsole.instance.Submit("item " + x));
		}
		public void OnConsoleCommand_givevfprawnupgrades(NotificationCenter.Notification _)
		{
			UpgradeRegistrar.UpgradeIcons
				.Select(x => x.Key + "Exosuit")
				.Where(x => UWE.Utils.TryParseEnum<TechType>(x, out TechType techType))
				.ForEach(x => DevConsole.instance.Submit("item " + x));
		}
		public void OnConsoleCommand_givevfcyclopsupgrades(NotificationCenter.Notification _)
		{
			UpgradeRegistrar.UpgradeIcons
				.Select(x => x.Key + "Cyclops")
				.Where(x => UWE.Utils.TryParseEnum<TechType>(x, out TechType techType))
				.ForEach(x => DevConsole.instance.Submit("item " + x));
		}
		public void OnConsoleCommand_logvfupgrades(NotificationCenter.Notification _)
		{
			UpgradeRegistrar.UpgradeIcons.Select(x => x.Key).ForEach(x => Logger.Log(x));
		}
		public void OnConsoleCommand_logvfvehicles(NotificationCenter.Notification _)
		{
			VehicleManager.vehicleTypes.Select(x => x.techType).ForEach(x => Logger.Log(x.AsString()));
		}
		public void OnConsoleCommand_logvfvoices(NotificationCenter.Notification _)
		{
			VoiceManager.LogAllAvailableVoices();
		}
		public void OnConsoleCommand_vfspawncodes(NotificationCenter.Notification _)
		{
			UWE.CoroutineHost.StartCoroutine(ListSpawnCodes());
		}
		private static IEnumerator ListSpawnCodes()
		{
			List<string> allCodes = new List<string>();
			allCodes.AddRange(VehicleManager.vehicleTypes.Select(x => x.techType.AsString()));
			allCodes.AddRange(UpgradeRegistrar.UpgradeIcons.Select(x => x.Key));
			foreach (string code in allCodes)
			{
				Logger.PDANote(code, 4f);
				yield return new WaitForSeconds(0.3f);
			}

		}
		public void OnConsoleCommand_incrementme(NotificationCenter.Notification _)
		{
			Patches.SDFCutoutPatcher.cutouts.ForEach(x =>
				UWE.CoroutineHost.StartCoroutine(Patches.SDFCutoutPatcher.UpdateIt(x))
			); ;
		}
		public void OnConsoleCommand_undockclosest(NotificationCenter.Notification _)
		{
			void MaybeUndock(VehicleDockingBay dock)
			{
				if (dock.dockedVehicle != null)
				{
					Logger.PDANote("Undocking " + dock.dockedVehicle.subName.name + " from bay.");
					isUndockConsoleCommand = true;
					dock.dockedVehicle.Undock();
					isUndockConsoleCommand = false;
				}
				else
				{
					Logger.PDANote("There was no vehicle docked in that bay.");
				}
			}

			float distanceToPlayer = float.PositiveInfinity;
			VehicleDockingBay closestBay = null;
			foreach (var marty in Patches.VehicleDockingBayPatch.dockingBays.Where(x => x != null))
			{
				float thisDistance = Vector3.Distance(Player.main.transform.position, marty.transform.position);
				if (thisDistance < distanceToPlayer)
				{
					closestBay = marty;
					distanceToPlayer = thisDistance;
				}
			}
			if (closestBay != null)
			{
				MaybeUndock(closestBay);
			}
			else
			{
				Logger.PDANote("There are no docking bays.");
			}
		}
	}
}
