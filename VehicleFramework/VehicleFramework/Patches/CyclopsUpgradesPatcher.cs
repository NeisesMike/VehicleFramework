using HarmonyLib;
using System.Collections.Generic;
using VehicleFramework.Admin;
using System.Linq;

namespace VehicleFramework.Patches
{
	[HarmonyPatch(typeof(SubRoot))]
	public class CyclopsUpgradesPatcher
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SubRoot.UpdateSubModules))]
		public static void SubRootUpdateSubModulesPrefix(SubRoot __instance, out bool __state)
		{
			__state = __instance.subModulesDirty;
		}
		[HarmonyPostfix]
		[HarmonyPatch(nameof(SubRoot.UpdateSubModules))]
		public static void SubRootUpdateSubModulesPostfix(SubRoot __instance, bool __state)
		{
			if (__state)
			{
				DoModUpgradeAddActions(__instance);
			}
		}
		public static void DoModUpgradeAddActions(SubRoot subroot)
		{
			if (subroot.upgradeConsole != null)
			{
				List<TechType> ObservedUpgradeTechTypes = new List<TechType>();
				Equipment modules = subroot.upgradeConsole.modules;
				for (int i = 0; i < subroot.upgradeConsole.modules.equipment.Count; i++)
				{
					string slot = "";
					try
					{
						slot = SubRoot.slotNames[i];
					}
                    catch
                    {
						Logger.Warn("Cyclops Upgrades Error: Didn't know about Cyclops Upgrade Slot Name for Slot #" + i.ToString());
						return;
                    }
					TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
					if (techTypeInSlot != TechType.None)
					{
						ObservedUpgradeTechTypes.Add(techTypeInSlot);
						UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
						{
							cyclops = subroot,
							slotID = i,
							techType = techTypeInSlot,
							isAdded = true
						};
						UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
					}
				}

				// bump even those modules that are not currently loaded into the cyclops.
				foreach(var but in VanillaUpgradeMaker.CyclopsUpgradeTechTypes.Where(x => !ObservedUpgradeTechTypes.Contains(x)))
				{
					UpgradeTypes.AddActionParams addedParams = new UpgradeTypes.AddActionParams
					{
						cyclops = subroot,
						slotID = -1,
						techType = but,
						isAdded = false
					};
					UpgradeRegistrar.OnAddActions.ForEach(x => x(addedParams));
				}
			}
		}
	}
}
