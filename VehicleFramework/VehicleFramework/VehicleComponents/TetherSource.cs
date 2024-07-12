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
        public Bounds bounds
        {
            get
            {
                if (mv == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                BoxCollider collider = mv.BoundingBox.GetComponentInChildren<BoxCollider>(true);
                if(collider == null)
                {
                    return new Bounds(Vector3.zero, Vector3.zero);
                }
                collider.enabled = true;
                Bounds result = collider.bounds;
                collider.enabled = false;
                return result;
            }
        }

        public void Start()
        {
            mv.BoundingBox.SetActive(true);
            mv.BoundingBox.GetComponentInChildren<BoxCollider>(true).enabled = false;
            Player.main.StartCoroutine(ManageTether());
        }

        public void TryToDropLeash()
        {
            if (!bounds.Contains(Player.main.transform.position))
            {
                mv.PlayerExit();
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
            if (mv.TetherSources.Where(x => PlayerWithinLeash(x)).Count() > 0)
            {
                mv.PlayerEntry();
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
