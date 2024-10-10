using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleFramework.UpgradeTypes;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;

namespace VehicleFramework.Admin
{
    internal static class VanillaUpgradeMaker
    {
        #region CreationMethods
        internal static void CreatePassiveModule(ModVehicleUpgrade upgrade, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
        }
        internal static void CreateSelectModule(SelectableUpgrade select, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
        }
        internal static void CreateChargeModule(SelectableChargeableUpgrade selectcharge, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
        }
        internal static void CreateToggleModule(ToggleableUpgrade toggle, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
        }
        internal static void CreateExosuitArm(ModVehicleArm arm, UpgradeCompat compat, ref UpgradeTechTypes utt, bool isPdaSetup)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
