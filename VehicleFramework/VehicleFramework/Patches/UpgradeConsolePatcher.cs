using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VehicleFramework.Patches
{
	public class VFUpgradesListener : MonoBehaviour
	{
		private UpgradeConsole UpgradeConsole => GetComponent<UpgradeConsole>();
		private SubRoot Subroot => GetComponentInParent<SubRoot>();
		private int GetSlotNumber(string slot)
		{
			for (int i = 0; i < UpgradeConsole.modules.equipment.Count; i++)
			{
				try
				{
					if (slot == SubRoot.slotNames[i])
					{
						return i;
					}
				}
				catch
				{
					Logger.Warn("Cyclops Upgrades Error: Didn't know about Cyclops Upgrade Slot Name for Slot #" + i.ToString());
				}
			}
			return -1;
		}
		public void OnSlotEquipped(string slot, InventoryItem item)
		{
			IEnumerator BroadcastMessageSoon()
			{
				yield return new WaitUntil(() => Subroot != null);
				Subroot.BroadcastMessage("UpdateAbilities", null, SendMessageOptions.DontRequireReceiver);
			}
			if (item.techType != TechType.None)
			{
				UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
				{
					cyclops = Subroot,
					slotID = GetSlotNumber(slot),
					techType = item.techType,
					isAdded = true
				};
				VehicleFramework.Admin.UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
				UWE.CoroutineHost.StartCoroutine(BroadcastMessageSoon());
			}
		}
		public void OnSlotUnequipped(string slot, InventoryItem item)
		{
			IEnumerator BroadcastMessageSoon()
			{
				yield return new WaitUntil(() => Subroot != null);
				Subroot.BroadcastMessage("UpdateAbilities", null, SendMessageOptions.DontRequireReceiver);
			}
			if (item.techType != TechType.None)
			{
				UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
				{
					cyclops = Subroot,
					slotID = GetSlotNumber(slot),
					techType = item.techType,
					isAdded = false
				};
				VehicleFramework.Admin.UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
				UWE.CoroutineHost.StartCoroutine(BroadcastMessageSoon());
			}
		}
		internal void BumpUpgrade(KeyValuePair<string, InventoryItem> upgrade)
		{
			OnSlotUnequipped(upgrade.Key, upgrade.Value);
			OnSlotEquipped(upgrade.Key, upgrade.Value);
		}
	}
	[HarmonyPatch(typeof(UpgradeConsole))]
	public class UpgradeConsolePatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UpgradeConsole.Awake))]
		public static void UpgradeConsoleAwakeHarmonyPostfix(UpgradeConsole __instance)
		{
			UpdateSignals(__instance);
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UpgradeConsole.InitializeModules))]
		public static void UpgradeConsoleInitializeModulesHarmonyPostfix(UpgradeConsole __instance)
		{
			UpdateSignals(__instance);
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(UpgradeConsole.OnProtoDeserialize))]
		public static void UpgradeConsoleOnProtoDeserializePrefix(UpgradeConsole __instance)
		{
			UpdateSignals(__instance);
		}
		[HarmonyPrefix]
		[HarmonyPatch(nameof(UpgradeConsole.OnProtoDeserializeObjectTree))]
		public static void UpgradeConsoleOnProtoDeserializeObjectTreePrefix(UpgradeConsole __instance)
		{
			UpdateSignals(__instance);
		}
		private static void UpdateSignals(UpgradeConsole console)
		{
			SubRoot thisSubRoot = console.GetComponentInParent<SubRoot>();
			if (thisSubRoot == null) return;

			if (thisSubRoot.isCyclops && console.modules != null && console.modules.equipment != null)
			{
				var listener = console.gameObject.EnsureComponent<VFUpgradesListener>();
				console.modules.onEquip -= listener.OnSlotEquipped;
				console.modules.onUnequip -= listener.OnSlotUnequipped;
				console.modules.onEquip += listener.OnSlotEquipped;
				console.modules.onUnequip += listener.OnSlotUnequipped;
				console.modules.equipment.ForEach(listener.BumpUpgrade);
			}
		}
	}
}
