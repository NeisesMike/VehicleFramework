using HarmonyLib;
using UnityEngine;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(SubRoot))]
	public class CyclopsUpgradesPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateSubModules))]
		public static void SubRootUpdateSubModulesPostfix(SubRoot __instance)
		{
			if (__instance.subModulesDirty)
			{
				DoModUpgradeAddActions(__instance);
			}
		}
		public static void DoModUpgradeAddActions(SubRoot subroot)
		{
			if (subroot.upgradeConsole != null)
			{
				Equipment modules = subroot.upgradeConsole.modules;
				for (int i = 0; i < subroot.upgradeConsole.modules.equipment.Count; i++)
				{
					string slot = SubRoot.slotNames[i];
					TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
					UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
					{
						cyclops = subroot,
						slotID = i,
						techType = techTypeInSlot,
						isAdded = true
					};
					Admin.UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
				}
			}
		}
	}
}
