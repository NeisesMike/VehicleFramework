using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtramaVehicle.Builder
{
    public static partial class AtramaBuilder
    {
        public static void addStorageSubsystem(Atrama atrama)
        {
            Logger.Log("Add Storage Subsystem");

            // Add the storage modules to the construction pods
            atrama.leftStorage = atrama.transform.Find("LeftStorage").gameObject;
            atrama.rightStorage = atrama.transform.Find("RightStorage").gameObject;

            atrama.leftStorage.SetActive(false);
            atrama.rightStorage.SetActive(false);

            var lStor = atrama.leftStorage.EnsureComponent<AtramaStorageContainer>();
            lStor.storageRoot = atrama.storageRoot;
            lStor.storageLabel = "Left Pod Storage";
            lStor.height = 6;
            lStor.width = 8;
            var rStor = atrama.rightStorage.EnsureComponent<AtramaStorageContainer>();
            rStor.storageRoot = atrama.storageRoot;
            rStor.storageLabel = "Right Pod Storage";
            rStor.height = 6;
            rStor.width = 8;

            atrama.leftStorage.EnsureComponent<AtramaStorageInput>();
            atrama.rightStorage.EnsureComponent<AtramaStorageInput>();

            atrama.leftStorage.SetActive(true);
            atrama.rightStorage.SetActive(true);
        }
    }
}
