using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleFramework.Admin
{
    internal static class CraftTreeHandler
    {
        internal static readonly string[] rootVehicleFrameworkTab = { "VFUM" };
        internal enum UpgradeType
        {
            ModVehicle,
            Seamoth,
            Exosuit,
            Cyclops
        }
        internal static string[] UpgradeTypeToPath(UpgradeType path)
        {
            switch (path)
            {
                case UpgradeType.ModVehicle:
                    return rootVehicleFrameworkTab.Append("MVCM").ToArray();
                case UpgradeType.Seamoth:
                    return rootVehicleFrameworkTab.Append("VVSM").ToArray();
                case UpgradeType.Exosuit:
                    return rootVehicleFrameworkTab.Append("VVEM").ToArray();
                case UpgradeType.Cyclops:
                    return rootVehicleFrameworkTab.Append("VVCM").ToArray();
                default:
                    return new string[] { "error" };
            }
        }
        internal static List<string> AddedTabs = new List<string>();
        internal static void AddCraftTreeNodesVF(string tabName, string displayName, Atlas.Sprite icon)
        {
            if (AddedTabs.Contains(tabName))
            {
                return;
            }
            List<UpgradeType> upgradeTypes = new List<CraftTreeHandler.UpgradeType> 
            { 
                CraftTreeHandler.UpgradeType.ModVehicle,
                CraftTreeHandler.UpgradeType.Seamoth,
                CraftTreeHandler.UpgradeType.Exosuit,
                CraftTreeHandler.UpgradeType.Cyclops
            };
            Dictionary<UpgradeType, string[]> result = new Dictionary<UpgradeType, string[]>();
            try
            {
                foreach (UpgradeType path in upgradeTypes)
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
            //Add root VF tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, rootVehicleFrameworkTab[0], "VF Upgrades", MainPatcher.ModVehicleIcon);
            // add MV tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(UpgradeType.ModVehicle).Last(), LocalizationManager.GetString(EnglishString.MVModules), MainPatcher.ModVehicleIcon, rootVehicleFrameworkTab);
            // add seamoth tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(UpgradeType.Seamoth).Last(), "Seamoth Upgrades", MainPatcher.ModVehicleIcon, rootVehicleFrameworkTab);
            // add prawn tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(UpgradeType.Exosuit).Last(), "Prawn Upgrades", MainPatcher.ModVehicleIcon, rootVehicleFrameworkTab);
            // add cyclops tab
            Nautilus.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, UpgradeTypeToPath(UpgradeType.Cyclops).Last(), "Cyclops Upgrades", MainPatcher.ModVehicleIcon, rootVehicleFrameworkTab);
        }
    }
}
