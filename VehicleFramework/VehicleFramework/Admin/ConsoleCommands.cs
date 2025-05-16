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
			DevConsole.RegisterConsoleCommand(this, "vfhelp", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfseamothupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfprawnupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "givevfcyclopsupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfupgrades", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfvehicles", false, false);
			DevConsole.RegisterConsoleCommand(this, "logvfvoices", false, false);
			DevConsole.RegisterConsoleCommand(this, "vfspawncodes", false, false);
			DevConsole.RegisterConsoleCommand(this, "undockclosest", false, false);
			DevConsole.RegisterConsoleCommand(this, "vfdestroy", false, false);
		}
		public void OnConsoleCommand_vfhelp(NotificationCenter.Notification _)
		{
			Logger.PDANote("givevfupgrades");
			Logger.PDANote("givevfseamothupgrades");
			Logger.PDANote("givevfprawnupgrades");
			Logger.PDANote("givevfcyclopsupgrades");
			Logger.PDANote("logvfupgrades");
			Logger.PDANote("logvfvehicles");
			Logger.PDANote("logvfvoices");
			Logger.PDANote("vfspawncodes");
			Logger.PDANote("undockclosest");
			Logger.PDANote("vfdestroy [vehicle type]");
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
		public void OnConsoleCommand_undockclosest(NotificationCenter.Notification _)
		{
			void MaybeUndock(VehicleDockingBay dock)
			{
				if (dock.dockedVehicle != null)
				{
					Logger.PDANote($"{Language.main.Get("VFUndockHint1")} {dock.dockedVehicle.GetName()}");
					isUndockConsoleCommand = true;
					dock.dockedVehicle.Undock();
					isUndockConsoleCommand = false;
				}
				else
				{
					Logger.PDANote(Language.main.Get("VFUndockHint2"));
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
				Logger.PDANote(Language.main.Get("VFUndockHint3"));
			}
		}
		public void OnConsoleCommand_vfdestroy(NotificationCenter.Notification notif)
		{
			if (notif.data == null || notif.data.Count < 1)
			{
				ErrorMessage.AddError("vfdestroy error: no vehicle type specified. Ex: \"vfdestroy exosuit\"");
			}
			string vehicleType = (string)notif.data[0];
			ErrorMessage.AddWarning($"vfdestroy doing destroy on {vehicleType}");
			Vehicle found = GameObjectManager<Vehicle>.FindNearestSuch(Player.main.transform.position, x => x.name.Equals($"{vehicleType}(Clone)", System.StringComparison.OrdinalIgnoreCase));
			if (found == null)
			{
				ErrorMessage.AddWarning($"vfdestroy found no vehicle matching \"{vehicleType}\"");
				Vehicle nearest = GameObjectManager<Vehicle>.FindNearestSuch(Player.main.transform.position);
				if (nearest != null)
				{
					ErrorMessage.AddWarning($"Did you mean \"vfdestroy {nearest.name.Substring(0, nearest.name.Length - "(Clone)".Length)}\"?".ToLower());
				}
				return;
			}
			ErrorMessage.AddWarning($"Destroying {found.name}");
			GameObject.DestroyImmediate(found.gameObject);
		}
	}
}
