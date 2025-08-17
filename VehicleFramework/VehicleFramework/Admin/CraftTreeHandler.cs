using System;
using System.Collections.Generic;
using System.Linq;
using VehicleFramework.Assets;
using VehicleFramework.UpgradeTypes;
//using VehicleFramework.Localization;
using UnityEngine;

namespace VehicleFramework.Admin
{
    internal static class CraftTreeHandler
    {
        internal const string GeneralTabName = "VFGeneral";
        private readonly static List<string[]> KnownPaths = new();
        internal readonly static List<string> TabNodeTabNodes = new();
        internal readonly static List<string> CraftNodeTabNodes = new();
        internal static string[] UpgradeTypeToPath(VehicleType path)
        {
            return path switch
            {
                VehicleType.ModVehicle => new string[] { "VFUniversal" },
                VehicleType.Seamoth => new string[] { "VFSeamoth" },
                VehicleType.Prawn => new string[] { "VFPrawn" },
                VehicleType.Cyclops => new string[] { "VFCyclops" },
                VehicleType.Custom => new string[] { "VFCustom" },
                _ => new string[] { "error" },
            };
        }
        internal static void AddFabricatorMenus()
        {
            var vfIcon = SpriteHelper.GetSpriteInternal("VFUpgradesIcon.png") ?? StaticAssets.ModVehicleIcon;
            var mvIcon = StaticAssets.UpgradeIcon;
            var seamothIcon = SpriteManager.Get(TechType.Seamoth) ?? StaticAssets.ModVehicleIcon;
            var prawnIcon = SpriteManager.Get(TechType.Exosuit) ?? StaticAssets.ModVehicleIcon;
            var cyclopsIcon = SpriteManager.Get(TechType.Cyclops) ?? StaticAssets.ModVehicleIcon;

            // add MV-universal tab
            AddCraftingTab(new string[]{ }, UpgradeTypeToPath(VehicleType.ModVehicle).Last(), Language.main.Get("VFMVModules"), vfIcon);
            AddCraftingTab(UpgradeTypeToPath(VehicleType.ModVehicle), $"{GeneralTabName}{VehicleType.ModVehicle}", Language.main.Get("VFGeneralTab"), mvIcon);
            // add MV-specific tab
            AddCraftingTab(new string[] { }, UpgradeTypeToPath(VehicleType.Custom).Last(), Language.main.Get("VFSpecificModules"), vfIcon);
            AddCraftingTab(UpgradeTypeToPath(VehicleType.Custom), $"{GeneralTabName}{VehicleType.Custom}", Language.main.Get("VFGeneralTab"), mvIcon);
            // add seamoth tab
            AddCraftingTab(new string[] { }, UpgradeTypeToPath(VehicleType.Seamoth).Last(), Language.main.Get("VFSeamothTab"), seamothIcon);
            AddCraftingTab(UpgradeTypeToPath(VehicleType.Seamoth), $"{GeneralTabName}{VehicleType.Seamoth}", Language.main.Get("VFGeneralTab"), mvIcon);
            // add prawn tab
            AddCraftingTab(new string[] { }, UpgradeTypeToPath(VehicleType.Prawn).Last(), Language.main.Get("VFPrawnTab"), prawnIcon);
            AddCraftingTab(UpgradeTypeToPath(VehicleType.Prawn), $"{GeneralTabName}{VehicleType.Prawn}", Language.main.Get("VFGeneralTab"), mvIcon);
            // add cyclops tab
            AddCraftingTab(new string[] { }, UpgradeTypeToPath(VehicleType.Cyclops).Last(), Language.main.Get("VFCyclopsTab"), cyclopsIcon);
            AddCraftingTab(UpgradeTypeToPath(VehicleType.Cyclops), $"{GeneralTabName}{VehicleType.Cyclops}", Language.main.Get("VFGeneralTab"), mvIcon);
        }
        internal static void EnsureCraftingTabsAvailable(ModVehicleUpgrade upgrade, UpgradeCompat compat)
        {
            if (upgrade.IsVehicleSpecific)
            {
                EnsureCraftingTabAvailable(upgrade, VehicleType.Custom);
                return;
            }
            if (!compat.skipCyclops)
            {
                EnsureCraftingTabAvailable(upgrade, VehicleType.Cyclops);
            }
            if (!compat.skipExosuit)
            {
                EnsureCraftingTabAvailable(upgrade, VehicleType.Prawn);
            }
            if (!compat.skipModVehicle)
            {
                EnsureCraftingTabAvailable(upgrade, VehicleType.ModVehicle);
            }
            if (!compat.skipSeamoth)
            {
                EnsureCraftingTabAvailable(upgrade, VehicleType.Seamoth);
            }
        }
        private static void EnsureCraftingTabAvailable(ModVehicleUpgrade upgrade, VehicleType vType)
        {
            if(upgrade.CraftingPath == null)
            {
                if (upgrade.TabName.Equals(string.Empty))
                {
                    // it goes in general, so we're good
                    return;
                }
                else
                {
                    AddCraftingTab(vType, upgrade.TabName, upgrade.TabDisplayName, upgrade.TabIcon);
                }
            }
            else
            {
                TraceCraftingPath(vType, upgrade.CraftingPath, (x,y) => AddCraftingTab(x, y.name, y.displayName, y.icon));
            }
        }
        internal static string[] TraceCraftingPath(VehicleType vType, List<CraftingNode> path, Action<string[], CraftingNode> perNodeAction)
        {
            string[] pathCurrently = UpgradeTypeToPath(vType);
            foreach (var node in path)
            {
                perNodeAction?.Invoke(pathCurrently, node);
                pathCurrently = pathCurrently.Append(node.name).ToArray();
            }
            return pathCurrently;
        }
        private static string[] AddCraftingTab(VehicleType vType, string tabName, string displayName, Sprite icon)
        {
            return AddCraftingTab(UpgradeTypeToPath(vType), tabName, displayName, icon);
        }
        private static string[] AddCraftingTab(string[] thisPath, string tabName, string displayName, Sprite icon)
        {
            string[] resultPath = thisPath.Append(tabName).ToArray();
            if (!IsKnownPath(resultPath))
            {
                if (thisPath.Length != 0 && !IsValidTabPath(thisPath))
                {
                    throw Admin.SessionManager.Fatal($"CraftTreeHandler: Invalid Tab Path: there were crafting nodes in that tab: {thisPath.Last()}. Cannot mix tab nodes and crafting nodes.");
                }
                Nautilus.Handlers.CraftTreeHandler.AddTabNode(VFFabricator.TreeType, tabName, displayName, icon ?? StaticAssets.UpgradeIcon, thisPath);
                if (thisPath.Any())
                {
                    TabNodeTabNodes.Add(thisPath.Last());
                }
                KnownPaths.Add(resultPath);
            }
            return resultPath;
        }
        private static bool IsKnownPath(string[] path)
        {
            List<string[]> innerKnownPaths = new();
            KnownPaths.ForEach(x => innerKnownPaths.Add(x));
            innerKnownPaths.RemoveAll(x => x.Length != path.Length);
            for(int i=0; i<path.Length; i++)
            {
                innerKnownPaths.RemoveAll(x => x[i] != path[i]);
            }
            return innerKnownPaths.Any();
        }
        internal static bool IsValidTabPath(string[] steps)
        {
            // return false only if this tab has crafting nodes
            return !CraftNodeTabNodes.Contains(steps.Last());
        }
        internal static bool IsValidCraftPath(string[] steps)
        {
            // return false only if this tab has tab nodes
            return !TabNodeTabNodes.Contains(steps.Last());
        }
    }
}
