using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AtramaVehicle.Builder
{
    public static partial class AtramaBuilder
    {
        public static void addUpgradeSubsystem(Atrama atrama)
        {
            Logger.Log("Add Upgrade Subsystem");
            // Add the upgrade console code
            GameObject upgradePanel = atrama.transform.Find("Mechanical-Panel/Upgrades-Panel").gameObject;
            VehicleUpgradeConsoleInput vuci = upgradePanel.EnsureComponent<VehicleUpgradeConsoleInput>();
            vuci.flap = upgradePanel.transform.Find("flap");
            vuci.anglesOpened = new Vector3(80, 0, 0);
            atrama.vehicle.upgradesInput = vuci;
        }

        public static void addStorageModules(Atrama atrama)
        {
            Logger.Log("Add Storage Modules");
            atrama.modularStorage = atrama.transform.Find("ModularStorage").gameObject;
            atrama.modularStorage.transform.parent = atrama.transform;

            FMODAsset storageCloseSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().closeSound;
            FMODAsset storageOpenSound = seamoth.transform.Find("Storage/Storage1").GetComponent<SeamothStorageInput>().openSound;

            GameObject atramaStorageModule1 = atrama.modularStorage.transform.Find("StorageModule1").gameObject;
            var stor1 = atramaStorageModule1.EnsureComponent<AtramaStorageContainer>();
            stor1.storageRoot = atrama.storageRoot;
            stor1.storageLabel = "Storage Module 1";
            stor1.height = 4;
            stor1.width = 4;
            var input1 = atramaStorageModule1.EnsureComponent<AtramaStorageInput>();
            input1.atrama = atrama.vehicle;
            input1.model = atramaStorageModule1;
            input1.collider = atramaStorageModule1.EnsureComponent<BoxCollider>();
            input1.openSound = storageOpenSound;
            input1.closeSound = storageCloseSound;

            GameObject atramaStorageModule2 = atrama.modularStorage.transform.Find("StorageModule2").gameObject;
            var stor2 = atramaStorageModule2.EnsureComponent<AtramaStorageContainer>();
            stor2.storageRoot = atrama.storageRoot;
            stor2.storageLabel = "Storage Module 2";
            stor2.height = 4;
            stor2.width = 4;
            var input2 = atramaStorageModule2.EnsureComponent<AtramaStorageInput>();
            input2.atrama = atrama.vehicle;
            input2.model = atramaStorageModule2;
            input2.collider = atramaStorageModule2.EnsureComponent<BoxCollider>();
            input2.openSound = storageOpenSound;
            input2.closeSound = storageCloseSound;

            GameObject atramaStorageModule3 = atrama.modularStorage.transform.Find("StorageModule3").gameObject;
            var stor3 = atramaStorageModule3.EnsureComponent<AtramaStorageContainer>();
            stor3.storageRoot = atrama.storageRoot;
            stor3.storageLabel = "Storage Module 3";
            stor3.height = 4;
            stor3.width = 4;
            var input3 = atramaStorageModule3.EnsureComponent<AtramaStorageInput>();
            input3.atrama = atrama.vehicle;
            input3.model = atramaStorageModule3;
            input3.collider = atramaStorageModule3.EnsureComponent<BoxCollider>();
            input3.openSound = storageOpenSound;
            input3.closeSound = storageCloseSound;

            GameObject atramaStorageModule4 = atrama.modularStorage.transform.Find("StorageModule4").gameObject;
            var stor4 = atramaStorageModule4.EnsureComponent<AtramaStorageContainer>();
            stor4.storageRoot = atrama.storageRoot;
            stor4.storageLabel = "Storage Module 4";
            stor4.height = 4;
            stor4.width = 4;
            var input4 = atramaStorageModule4.EnsureComponent<AtramaStorageInput>();
            input4.atrama = atrama.vehicle;
            input4.model = atramaStorageModule4;
            input4.collider = atramaStorageModule4.EnsureComponent<BoxCollider>();
            input4.openSound = storageOpenSound;
            input4.closeSound = storageCloseSound;

            GameObject atramaStorageModule5 = atrama.modularStorage.transform.Find("StorageModule5").gameObject;
            var stor5 = atramaStorageModule5.EnsureComponent<AtramaStorageContainer>();
            stor5.storageRoot = atrama.storageRoot;
            stor5.storageLabel = "Storage Module 5";
            stor5.height = 4;
            stor5.width = 4;
            var input5 = atramaStorageModule5.EnsureComponent<AtramaStorageInput>();
            input5.atrama = atrama.vehicle;
            input5.model = atramaStorageModule5;
            input5.collider = atramaStorageModule5.EnsureComponent<BoxCollider>();
            input5.openSound = storageOpenSound;
            input5.closeSound = storageCloseSound;

            GameObject atramaStorageModule6 = atrama.modularStorage.transform.Find("StorageModule6").gameObject;
            var stor6 = atramaStorageModule6.EnsureComponent<AtramaStorageContainer>();
            stor6.storageRoot = atrama.storageRoot;
            stor6.storageLabel = "Storage Module 6";
            stor6.height = 4;
            stor6.width = 4;
            var input6 = atramaStorageModule6.EnsureComponent<AtramaStorageInput>();
            input6.atrama = atrama.vehicle;
            input6.model = atramaStorageModule6;
            input6.collider = atramaStorageModule6.EnsureComponent<BoxCollider>();
            input6.openSound = storageOpenSound;
            input6.closeSound = storageCloseSound;



            atrama.vehicle.storageInputs = new AtramaStorageInput[6] { input1, input2, input3, input4, input5, input6 };

            var lStor = atrama.leftStorage.GetComponent<AtramaStorageInput>();
            lStor.openSound = storageOpenSound;
            lStor.closeSound = storageCloseSound;
            var rStor = atrama.rightStorage.GetComponent<AtramaStorageInput>();
            rStor.openSound = storageOpenSound;
            rStor.closeSound = storageCloseSound;
        }
    }
}
