using HarmonyLib;
using UnityEngine;
using System.Collections;

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
	}
	[HarmonyPatch(typeof(UpgradeConsole))]
	public class UpgradeConsolePatcher
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UpgradeConsole.Awake))]
		public static void UpgradeConsoleAwakeHarmonyPostfix(UpgradeConsole __instance)
		{
			if (__instance.GetComponentInParent<SubRoot>().isCyclops)
			{
				var listener = __instance.gameObject.EnsureComponent<VFUpgradesListener>();
				__instance.modules.onEquip += listener.OnSlotEquipped;
				__instance.modules.onUnequip += listener.OnSlotUnequipped;
			}
		}
	}
}
