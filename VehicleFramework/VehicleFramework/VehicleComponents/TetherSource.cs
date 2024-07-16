using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleFramework.VehicleTypes;

namespace VehicleFramework
{
    /*
     * Tether Sources are meant to be placed throughout the Submarine.
     * The Submarine has one TetherSource component to manage them.
     * They should be strictly inside the ship.
     * A player will "leash" to them when close enough,
     * which ensures the player's entry is recognized no matter what (warp in).
     */
    public class TetherSource : MonoBehaviour
    {
        public Submarine mv = null;
        public bool isLive = true;
        public bool isSimple;
        public Bounds bounds
        {
            get
            {
                if (mv == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                BoxCollider collider = mv.BoundingBoxCollider;
                if(collider == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                mv.BoundingBox.SetActive(true);
                collider.gameObject.SetActive(true);
                collider.enabled = true;
                Bounds result = collider.bounds;
                collider.enabled = false;
                if (result.size == Vector3.zero)
                {
                    Logger.Error("TetherSource Error: BoundingBox Bounds had zero volume (size was zero). You may see this message if one or more of the BoundingBox collider's parents are inactive.");
                    return new Bounds(Vector3.one, Vector3.zero);
                }
                return result;
            }
        }

        public void Start()
        {
            mv.BoundingBoxCollider = mv.BoundingBoxCollider ?? mv.BoundingBox?.GetComponentInChildren<BoxCollider>(true);
            if (mv.BoundingBoxCollider == null || mv.TetherSources.Count() == 0)
            {
                isSimple = true;
            }
            else
            {
                isSimple = false;
                mv.BoundingBoxCollider.gameObject.SetActive(true);
                mv.BoundingBoxCollider.enabled = false;
            }
            Player.main.StartCoroutine(ManageTether());
        }

        public void TryToDropLeash()
        {
            if (isSimple)
            {
                if (Vector3.Distance(Player.main.transform.position, transform.position) > 10)
                {
                    mv.PlayerExit();
                }
            }
            else
            {
                if (!bounds.Contains(Player.main.transform.position))
                {
                    mv.PlayerExit();
                }
            }
        }

        public void TryToEstablishLeash()
        {
            bool PlayerWithinLeash(GameObject tetherSrc)
            {
                float radius = 0.75f;
                if (tetherSrc.GetComponent<SphereCollider>() != null)
                {
                    radius = tetherSrc.GetComponent<SphereCollider>().radius;
                }
                return Vector3.Distance(Player.main.transform.position, tetherSrc.transform.position) < radius;
            }
            if (Player.main.GetVehicle() == null && Player.main.currentSub == null)
            {
                if (isSimple)
                {
                    if (Vector3.Distance(Player.main.transform.position, transform.position) < 1f)
                    {
                        mv.PlayerEntry();
                    }
                    else if (Vector3.Distance(Player.main.transform.position, mv.PilotSeats.First().Seat.transform.position) < 1f)
                    {
                        mv.PlayerEntry();
                    }
                }
                else
                {
                    if (mv.TetherSources.Where(x => PlayerWithinLeash(x)).Count() > 0)
                    {
                        mv.PlayerEntry();
                    }
                }
            }

        }

        public IEnumerator ManageTether()
        {
            yield return new WaitForSeconds(3f);
            while (true)
            {
                if(mv == null)
                {
                    yield break;
                }
                if(isLive)
                {
                    if (mv.IsPlayerInside())
                    {
                        TryToDropLeash();
                    }
                    else
                    {
                        TryToEstablishLeash();
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
