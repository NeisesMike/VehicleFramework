using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.Localization;
using VehicleFramework.Assets;

namespace VehicleFramework.Admin
{
    internal static class CraftTreeHandler
    {
        internal static readonly string[] rootVehicleFrameworkTab = { "VFUM" };
        internal static List<string> AddedTabs = new List<string>();
        internal static string[] UpgradeTypeToPath(VehicleType path)
        {
            switch (path)
            {
                case VehicleType.ModVehicle:
                    return rootVehicleFrameworkTab.Append("MVCM").ToArray();
                case VehicleType.Seamoth:
                    return rootVehicleFrameworkTab.Append("VVSM").ToArray();
                case VehicleType.Prawn:
                    return rootVehicleFrameworkTab.Append("VVEM").ToArray();
                case VehicleType.Cyclops:
                    return rootVehicleFrameworkTab.Append("VVCM").ToArray();
                default:
                    return new string[] { "error" };
            }
        }
        internal static void AddCraftTreeNodesVF(string tabName, string displayName, Atlas.Sprite icon)
        {
            if (AddedTabs.Contains(tabName))
            {
                return;
            }
            List<VehicleType> upgradeTypes = new List<VehicleType> 
            { 
                VehicleType.ModVehicle,
                VehicleType.Seamoth,
                VehicleType.Prawn,
                VehicleType.Cyclops
            };
            Dictionary<VehicleType, string[]> result = new Dictionary<VehicleType, string[]>();
            try
            {
                foreach (VehicleType path in upgradeTypes)
                {
                    string[] thisPath = UpgradeTypeToPath(path).Append(tabName).ToArray();
                    Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, tabName, displayName, icon, UpgradeTypeToPath(path));
                    result.Add(path, thisPath);
                }
            }
            catch(Exception e)
            {
                Logger.Warn("Could not add new crafting tree tab: " + tabName + " : " + displayName + ". Error follows:");
                Logger.Warn(e.Message);
            }
            AddedTabs.Add(tabName);
            return;
        }
        internal static void AddFabricatorMenus()
        {
            var vfIcon = SpriteHelper.GetSpriteInternal("VFUpgradesIcon.png") ?? StaticAssets.ModVehicleIcon;
            var mvIcon = StaticAssets.UpgradeIcon;
            var seamothIcon = SpriteManager.Get(TechType.Seamoth) ?? StaticAssets.ModVehicleIcon;
            var prawnIcon = SpriteManager.Get(TechType.Exosuit) ?? StaticAssets.ModVehicleIcon;
            var cyclopsIcon = SpriteManager.Get(TechType.Cyclops) ?? StaticAssets.ModVehicleIcon;

            //Add root VF tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, rootVehicleFrameworkTab[0], "VF Upgrades", vfIcon);
            // add MV tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(VehicleType.ModVehicle).Last(), LocalizationManager.GetString(EnglishString.MVModules), mvIcon, rootVehicleFrameworkTab);
            // add seamoth tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(VehicleType.Seamoth).Last(), "Seamoth Upgrades", seamothIcon, rootVehicleFrameworkTab);
            // add prawn tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(VehicleType.Prawn).Last(), "Prawn Upgrades", prawnIcon, rootVehicleFrameworkTab);
            // add cyclops tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(VehicleType.Cyclops).Last(), "Cyclops Upgrades", cyclopsIcon, rootVehicleFrameworkTab);
        }
    }
}
