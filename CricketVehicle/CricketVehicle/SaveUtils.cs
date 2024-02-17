using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CricketVehicle
{
    public static class SaveUtils
    {
        public static List<CricketContainer> AttachedContainers => VehicleFramework.VehicleManager.VehiclesInPlay.Select(x => x as Cricket).Where(x => x != null)?.Where(x=>x.HasContainerAttached)?.Select(x => x.currentMountedContainer).ToList();
        public static bool IsAttached(CricketContainer cont)
        {
            return AttachedContainers.Contains(cont);
        }
        private static bool IsNearMatch(Vector3 one, Vector3 two)
        {
            return Vector3.Distance(one, two) < 0.01f; // why 0.01f ?
        }
        private static void ReattachAndReload((CricketContainer cont, Vector3 loc) x)
        {
            if(x.cont == null || x.loc == null)
            {
                Logger.Error("ReattachAndReload received a bad pair. List zipping must have gone awry.");
            }
            ReattachThisContainer(x.cont, x.loc);
        }
        private static bool NoContainersNearby(IEnumerable<CricketContainer> conts, Vector3 x)
        {
            foreach(var cont in conts)
            {
                if(IsNearMatch(cont.transform.position, x))
                {
                    return false;
                }
            }
            return true;
        }
        private static bool IsNotWhereItShouldBe(IEnumerable<Vector3> attachmentStatuses, CricketContainer cont)
        {
            foreach(var loc in attachmentStatuses)
            {
                if(IsNearMatch(loc, cont.transform.position))
                {
                    return false;
                }
            }
            return true;
        }
        private static void ReattachThisContainer(CricketContainer container, Vector3 location)
        {
            VehicleFramework.VehicleManager.VehiclesInPlay
                .Where(x => (x as Cricket) != null)?
                .Select(x => x as Cricket)
                .Where(x => IsNearMatch(x.ContainerMountPoint.position, location))?
                .ForEach(x => x.AttachContainer(container));
        }
        internal static IEnumerator ReattachContainers(SaveData savedata)
        {
            IEnumerator WaitUntilWorldSettled()
            {
                // Wait until the world around the player is settled
                Vector3 boundSize = new Vector3(5f, 5f, 5f);
                while (LargeWorldStreamer.main == null || Player.main == null || !LargeWorldStreamer.main.IsRangeActiveAndBuilt(new Bounds(Player.main.transform.position, boundSize)))
                {
                    yield return null;
                }
                // then wait five more frames
                for (int i = 0; i < 100; i++)
                {
                    //yield return null;
                }
            }
            // real containers have real storage
            // that storage exists in savedata.InnateStorages
            // InnateStorages can tell where that container should be
            if (savedata.AttachmentStatuses == null)
            {
                yield break;
            }

            yield return WaitUntilWorldSettled();

            var allRealContainers = VehicleFramework.Admin.GameObjectManager<CricketContainer>.AllSuchObjects.Where(x => x != null && x.transform != null);

            // Containers that were attached at save-time will not already have their contents reloaded.
            var allPossiblyNeedsAttachedContainers = allRealContainers.Where(x => x.storageContainer.container.count == 0);

            // Some empty containers were correctly loaded, however.
            var allNeedsAttachedContainers = allPossiblyNeedsAttachedContainers.Where(x => IsNotWhereItShouldBe(savedata.AttachmentStatuses.Select(u => u.Item1), x));

            // Some locations are empty where a container should be.
            var allPlacesAContainerShouldBeButIsNot = savedata.AttachmentStatuses.Select(x => x.Item1).Where(x => NoContainersNearby(allRealContainers, x));

            // At this point, we have X empty containers, all of which are equivalent.
            // We also have Y places were containers should be but are not.
            // We hope that X == Y
            // So we'll zip the lists together, forming pairs of containers and locations
            var actionSequence = allNeedsAttachedContainers.Zip(allPlacesAContainerShouldBeButIsNot, (cont, loc) => (cont, loc));

            // Then we'll take action on each pair
            actionSequence.ForEach(x => ReattachAndReload(x));
        }

    }
}
