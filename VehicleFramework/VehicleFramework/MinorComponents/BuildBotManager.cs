using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework
{
    public static class BuildBotManager
    {
        public static void SetupBuildBotBeamPoints(ModVehicle mv)
        {
            BuildBotBeamPoints bbbp = mv.gameObject.EnsureComponent<BuildBotBeamPoints>();
            List<Transform> bbbpList = new List<Transform>();
            foreach (Transform child in mv.transform)
            {
                bbbpList.Add(child);
            }
            bbbp.beamPoints = bbbpList.ToArray();
        }
        public static void SetupVFXConstructing(ModVehicle mv)
        {
            VFXConstructing seamothVFXC = CraftData.GetPrefabForTechType(TechType.Seamoth, true).GetComponent<VFXConstructing>();
            VFXConstructing rocketPlatformVfx = CraftData.GetPrefabForTechType(TechType.RocketBase).GetComponentInChildren<VFXConstructing>();

            VFXConstructing vfxc = mv.gameObject.EnsureComponent<VFXConstructing>();
            vfxc.timeToConstruct = 50f;
            vfxc.ghostMaterial = seamothVFXC.ghostMaterial;
            vfxc.alphaTexture = seamothVFXC.alphaTexture;
            vfxc.alphaDetailTexture = seamothVFXC.alphaDetailTexture;
            vfxc.wireColor = seamothVFXC.wireColor;
            vfxc.rBody = mv.useRigidbody;

            // we'll take the seamoth sound bc the other sound is goofy
            // we don't really like this splash, but at least the size is pretty good
            vfxc.surfaceSplashFX = rocketPlatformVfx.surfaceSplashFX;
            vfxc.surfaceSplashSound = seamothVFXC.surfaceSplashSound;
            vfxc.surfaceSplashVelocity = rocketPlatformVfx.surfaceSplashVelocity;

            vfxc.heightOffset = seamothVFXC.heightOffset;
            vfxc.constructSound = seamothVFXC.constructSound;
            vfxc.delay = 10f;
            vfxc.isDone = false;
            vfxc.informGameObject = null;
            vfxc.transparentShaders = null; // TODO maybe we'll want to use this?
            vfxc.Regenerate();
        }
        public static void SetupBuildBotPaths()
        {
            foreach (ModVehicle mv in VehicleBuilder.prefabs)
            {
                SetupBuildBotBeamPoints(mv);
                SetupVFXConstructing(mv);

                Bounds vbounds = mv.BoundingBox.GetComponent<MeshRenderer>().bounds;
                GameObject bbPointsRoot = new GameObject("BuildBotPoints");
                bbPointsRoot.transform.SetParent(mv.transform);

                // utility functions for visualizing locations around the bounding box
                Transform GetCorner(string name, bool inputx, bool inputy, bool inputz)
                {
                    Vector3 GetThisCorner(bool x, bool y, bool z)
                    {
                        Vector3 ret = vbounds.center;
                        ret += x ? vbounds.extents.x * Vector3.right : -1 * vbounds.extents.x * Vector3.right;
                        ret += y ? vbounds.extents.y * Vector3.up : -1 * vbounds.extents.y * Vector3.up;
                        ret += z ? vbounds.extents.z * Vector3.forward : -1 * vbounds.extents.z * Vector3.forward;
                        return ret;
                    }
                    GameObject pointGO = new GameObject(name);
                    Transform pointTR = pointGO.transform;
                    pointTR.SetParent(bbPointsRoot.transform);
                    pointTR.localPosition = GetThisCorner(inputx, inputy, inputz);
                    mv.boundingPoints.Add(name, pointGO);
                    return pointTR;
                }
                Transform GetCentroid(string name, Vector3 cardinalDir)
                {
                    Vector3 GetThisCentroid(Vector3 cDirection)
                    {
                        return vbounds.center +
                               cDirection.x * vbounds.extents.x * Vector3.right +
                               cDirection.y * vbounds.extents.y * Vector3.up +
                               cDirection.z * vbounds.extents.z * Vector3.forward;
                    }
                    GameObject pointGO = new GameObject(name);
                    Transform pointTR = pointGO.transform;
                    pointTR.SetParent(bbPointsRoot.transform);
                    pointTR.localPosition = GetThisCentroid(cardinalDir);
                    mv.boundingPoints.Add(name, pointGO);
                    return pointTR;
                }
                Transform GetMidpoint(Transform left, Transform right)
                {
                    GameObject pointGO = new GameObject(left.name + right.name);
                    Transform pointTR = pointGO.transform;
                    pointTR.SetParent(bbPointsRoot.transform);
                    pointTR.localPosition = (left.position + right.position) / 2;
                    mv.boundingPoints.Add(pointGO.name, pointGO);
                    return pointTR;
                }

                #region declare_constants
                Transform A = GetCorner("A", false, true, true);
                Transform B = GetCorner("B", true, true, true);
                Transform C = GetCorner("C", false, true, false);
                Transform D = GetCorner("D", true, true, false);
                Transform E = GetCorner("E", false, false, true);
                Transform F = GetCorner("F", true, false, true);
                Transform G = GetCorner("G", false, false, false);
                Transform H = GetCorner("H", true, false, false);
                Transform I = GetCentroid("I", Vector3.up);
                Transform J = GetCentroid("J", Vector3.down);
                Transform L = GetCentroid("L", Vector3.forward);
                Transform K = GetCentroid("K", Vector3.back);
                Transform M = GetCentroid("M", Vector3.left);
                Transform N = GetCentroid("N", Vector3.right);
                Transform AB = GetMidpoint(A, B);
                Transform CD = GetMidpoint(C, D);
                Transform EF = GetMidpoint(E, F);
                Transform GH = GetMidpoint(G, H);
                Transform BD = GetMidpoint(B, D);
                Transform FH = GetMidpoint(F, H);
                Transform EG = GetMidpoint(E, G);
                Transform AC = GetMidpoint(A, C);
                Transform BF = GetMidpoint(B, F);
                Transform AE = GetMidpoint(A, E);
                Transform CG = GetMidpoint(C, G);
                Transform DH = GetMidpoint(D, H);
                #endregion

                GameObject bbPathsRoot = new GameObject("BuildBotPaths");
                bbPathsRoot.transform.SetParent(mv.transform);

                GameObject bbPath1 = new GameObject("Path1");
                bbPath1.transform.SetParent(bbPathsRoot.transform);
                BuildBotPath path1 = bbPath1.AddComponent<BuildBotPath>();
                List<Transform> path1List = new List<Transform>
                {
                    K,
                    GH,
                    J,
                    EF,
                    L,
                    AB,
                    I,
                    CD
                };
                path1.points = path1List.ToArray();

                GameObject bbPath2 = new GameObject("Path2");
                bbPath2.transform.SetParent(bbPathsRoot.transform);
                BuildBotPath path2 = bbPath2.gameObject.AddComponent<BuildBotPath>();
                List<Transform> path2List = new List<Transform>
                {
                    I,
                    BD,
                    N,
                    FH,
                    J,
                    EG,
                    M,
                    AC
                };
                path2.points = path2List.ToArray();

                GameObject bbPath3 = new GameObject("Path3");
                bbPath3.transform.SetParent(bbPathsRoot.transform);
                BuildBotPath path3 = bbPath3.gameObject.AddComponent<BuildBotPath>();
                List<Transform> path3List = new List<Transform>
                {
                    N,
                    BF,
                    L,
                    AE,
                    M,
                    CG,
                    K,
                    DH
                };
                path3.points = path3List.ToArray();

                GameObject bbPath4 = new GameObject("Path4");
                bbPath4.transform.SetParent(bbPathsRoot.transform);
                BuildBotPath path4 = bbPath4.gameObject.AddComponent<BuildBotPath>();
                List<Transform> path4List = new List<Transform>
                {
                    J,
                    G,
                    C,
                    I,
                    B,
                    F
                };
                path4.points = path4List.ToArray();
            }
        }
    }
}
