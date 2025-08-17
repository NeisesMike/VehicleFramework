using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace VehicleFramework
{
    //SetupBuildBotPaths is invoked by Player.Start
    public static class BuildBotManager
    {
        private static Color OriginalConstructingGhostColor = ((Material)Resources.Load("Materials/constructingGhost")).color;

        public static void SetupBuildBotBeamPoints(GameObject mv)
        {
            BuildBotBeamPoints bbbp = mv.EnsureComponent<BuildBotBeamPoints>();
            List<Transform> bbbpList = new List<Transform>();
            ModVehicle modVehicle = mv.GetComponent<ModVehicle>();
            if (modVehicle != null)
            {
                foreach (Transform child in modVehicle.transform.GetComponentsInChildren<Transform>())
                {
                    bbbpList.Add(child);
                }
            }
            else
            {
                foreach (Transform child in mv.GetComponentsInChildren<Transform>())
                {
                    bbbpList.Add(child);
                }
            }
            bbbp.beamPoints = bbbpList.ToArray();
        }
        public static IEnumerator SetupVFXConstructing(GameObject go)
        {
            yield return MainPatcher.Instance.StartCoroutine(SeamothHelper.EnsureSeamoth());
            GameObject seamoth = SeamothHelper.Seamoth;
            VFXConstructing seamothVFXC = seamoth.GetComponent<VFXConstructing>();
            VFXConstructing rocketPlatformVfx = seamoth.GetComponentInChildren<VFXConstructing>();
            VFXConstructing vfxc = go.EnsureComponent<VFXConstructing>();
            
            ModVehicle mv = go.GetComponent<ModVehicle>();
            vfxc.timeToConstruct = mv == null ? 10f : mv.TimeToConstruct;

            vfxc.alphaTexture = seamothVFXC.alphaTexture;
            vfxc.alphaDetailTexture = seamothVFXC.alphaDetailTexture;
            vfxc.wireColor = seamothVFXC.wireColor;
            vfxc.rBody = go.GetComponent<Rigidbody>();

            // we'll take the seamoth sound bc the other sound is goofy
            // we don't really like this splash, but at least the size is pretty good
            vfxc.surfaceSplashFX = rocketPlatformVfx.surfaceSplashFX;
            vfxc.surfaceSplashSound = seamothVFXC.surfaceSplashSound;
            vfxc.surfaceSplashVelocity = rocketPlatformVfx.surfaceSplashVelocity;

            vfxc.heightOffset = seamothVFXC.heightOffset;
            vfxc.constructSound = seamothVFXC.constructSound;
            vfxc.delay = 5f; // the time it takes for the buildbots to fly out and begin
            vfxc.isDone = false;
            vfxc.informGameObject = null;
            vfxc.transparentShaders = null; // TODO maybe we'll want to use this?
            vfxc.Regenerate();

            yield break;
        }
        public static void BuildPathsUsingCorners(GameObject root, GameObject pointsRoot, Transform A, Transform B, Transform C, Transform D, Transform E, Transform F, Transform G, Transform H)
        {
            GameObject bbPathsRoot = new GameObject("BuildBotPaths");
            bbPathsRoot.transform.SetParent(root.transform);
            #region declarations
            Transform I = GetCentroid(pointsRoot, A, B, C, D);
            Transform J = GetCentroid(pointsRoot, E, F, G, H);
            Transform L = GetCentroid(pointsRoot, A, B, E, F);
            Transform K = GetCentroid(pointsRoot, C, D, G, H);
            Transform M = GetCentroid(pointsRoot, A, C, E, G);
            Transform N = GetCentroid(pointsRoot, B, D, F, H);
            Transform AB = GetMidpoint(pointsRoot, A, B);
            Transform CD = GetMidpoint(pointsRoot, C, D);
            Transform EF = GetMidpoint(pointsRoot, E, F);
            Transform GH = GetMidpoint(pointsRoot, G, H);
            Transform BD = GetMidpoint(pointsRoot, B, D);
            Transform FH = GetMidpoint(pointsRoot, F, H);
            Transform EG = GetMidpoint(pointsRoot, E, G);
            Transform AC = GetMidpoint(pointsRoot, A, C);
            Transform BF = GetMidpoint(pointsRoot, B, F);
            Transform AE = GetMidpoint(pointsRoot, A, E);
            Transform CG = GetMidpoint(pointsRoot, C, G);
            Transform DH = GetMidpoint(pointsRoot, D, H);
            List<Transform> path1List = new List<Transform> { K, GH, J, EF, L, AB, I, CD };
            List<Transform> path2List = new List<Transform> { I, BD, N, FH, J, EG, M, AC };
            List<Transform> path3List = new List<Transform> { N, BF, L, AE, M, CG, K, DH };
            List<Transform> path4List = new List<Transform> { J, G, C, I, B, F };
            #endregion
            void BuildBuildBotPath(GameObject rootGO, int number, List<Transform> pathList)
            {
                GameObject bbPath = new GameObject("Path" + number.ToString());
                bbPath.transform.SetParent(rootGO.transform);
                BuildBotPath path = bbPath.AddComponent<BuildBotPath>();
                path.points = pathList.ToArray();
            }
            BuildBuildBotPath(bbPathsRoot, 1, path1List);
            BuildBuildBotPath(bbPathsRoot, 2, path2List);
            BuildBuildBotPath(bbPathsRoot, 3, path3List);
            BuildBuildBotPath(bbPathsRoot, 4, path4List);
        }
        public static Transform GetMidpoint(GameObject root, Transform left, Transform right)
        {
            GameObject pointGO = new GameObject(left.name + right.name);
            Transform pointTR = pointGO.transform;
            pointTR.SetParent(root.transform);
            pointTR.localPosition = (left.position + right.position) / 2;
            return pointTR;
        }
        public static Transform GetCentroid(GameObject root, Transform topleft, Transform topright, Transform botleft, Transform botright)
        {
            GameObject pointGO = new GameObject(topleft.name + topright.name + botleft.name + botright.name);
            Transform pointTR = pointGO.transform;
            pointTR.SetParent(root.transform);
            pointTR.localPosition = (topleft.position + topright.position + botleft.position + botright.position) / 4;
            return pointTR;
        }
        public enum CornerValue
        {
            lefttopfront,
            righttopfront,
            lefttopback,
            righttopback,
            leftbotfront,
            rightbotfront,
            leftbotback,
            rightbotback
        }
        public static (bool, bool, bool) CornerToBools(CornerValue corner)
        {
            switch (corner)
            {
                case CornerValue.lefttopfront:
                    return (false, true, true);
                case CornerValue.righttopfront:
                    return (true, true, true);
                case CornerValue.lefttopback:
                    return (false, true, false);
                case CornerValue.righttopback:
                    return (true, true, false);
                case CornerValue.leftbotfront:
                    return (false, false, true);
                case CornerValue.rightbotfront:
                    return (true, false, true);
                case CornerValue.leftbotback:
                    return (false, false, false);
                case CornerValue.rightbotback:
                    return (true, false, false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }
        public static Transform GetCorner(GameObject root, CornerValue corner, Vector3 center, float x, float y, float z)
        {
            Vector3 GetThisCorner((bool x, bool y, bool z) input)
            {
                Vector3 ret = center;
                ret += input.x ? x * Vector3.right   : -1 * x * Vector3.right;
                ret += input.y ? y * Vector3.up      : -1 * y * Vector3.up;
                ret += input.z ? z * Vector3.forward : -1 * z * Vector3.forward;
                return ret;
            }
            GameObject pointGO = new GameObject(corner.ToString());
            Transform pointTR = pointGO.transform;
            pointTR.SetParent(root.transform);
            pointTR.localPosition = GetThisCorner(CornerToBools(corner));
            return pointTR;
        }
        public static Transform GetCornerCube(GameObject root, Vector3 cubeDimensions, Vector3 center, CornerValue corner)
        {
            float x = cubeDimensions.x / 2f;
            float y = cubeDimensions.y / 2f;
            float z = cubeDimensions.z / 2f;
            return GetCorner(root, corner, center, x, y, z);
        }
        public static Transform GetCornerBoxCollider(GameObject root, BoxCollider box, CornerValue corner)
        {
            Vector3 worldScale = box.transform.lossyScale;
            Vector3 boxSizeScaled = Vector3.Scale(box.size, worldScale);
            boxSizeScaled *= 1.1f;
            float x = boxSizeScaled.x / 2f;
            float y = boxSizeScaled.y / 2f;
            float z = boxSizeScaled.z / 2f;
            Vector3 boxCenterScaled = Vector3.Scale(box.center, worldScale);
            return GetCorner(root, corner, boxCenterScaled, x, y, z);
        }
        public static void BuildPathsForGameObject(GameObject go, GameObject pointsRoot)
        {
            Vector3 localCenter = go.transform.localPosition;
            Vector3 localSize = new Vector3(6f, 8f, 12f);
            Transform A = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.lefttopfront);
            Transform B = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.righttopfront);
            Transform C = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.lefttopback);
            Transform D = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.righttopback);
            Transform E = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.leftbotfront);
            Transform F = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.rightbotfront);
            Transform G = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.leftbotback);
            Transform H = GetCornerCube(pointsRoot, localSize, localCenter, CornerValue.rightbotback);
            BuildPathsUsingCorners(go, pointsRoot, A, B, C, D, E, F, G, H);
        }
        public static void BuildPathsForModVehicle(ModVehicle mv, GameObject pointsRoot)
        {
            BoxCollider box = mv.BoundingBoxCollider;
            if (box != null)
            {
                Transform A = GetCornerBoxCollider(pointsRoot, box, CornerValue.lefttopfront);
                Transform B = GetCornerBoxCollider(pointsRoot, box, CornerValue.righttopfront);
                Transform C = GetCornerBoxCollider(pointsRoot, box, CornerValue.lefttopback);
                Transform D = GetCornerBoxCollider(pointsRoot, box, CornerValue.righttopback);
                Transform E = GetCornerBoxCollider(pointsRoot, box, CornerValue.leftbotfront);
                Transform F = GetCornerBoxCollider(pointsRoot, box, CornerValue.rightbotfront);
                Transform G = GetCornerBoxCollider(pointsRoot, box, CornerValue.leftbotback);
                Transform H = GetCornerBoxCollider(pointsRoot, box, CornerValue.rightbotback);
                BuildPathsUsingCorners(mv.gameObject, pointsRoot, A, B, C, D, E, F, G, H);
            }
            else
            {
                BuildPathsForGameObject(mv.gameObject, pointsRoot);
            }
        }
        public static void BuildBotPathsHelper(GameObject go)
        {
            ModVehicle mv = go.GetComponent<ModVehicle>();
            GameObject bbPointsRoot = new GameObject("BuildBotPoints");
            bbPointsRoot.transform.SetParent(go.transform);
            if (mv != null && mv.BoundingBoxCollider != null)
            {
                bbPointsRoot.transform.localPosition = go.transform.InverseTransformPoint(mv.BoundingBoxCollider.transform.position);
                BuildPathsForModVehicle(mv, bbPointsRoot);
            }
            else
            {
                bbPointsRoot.transform.localPosition = Vector3.zero;
                BuildPathsForGameObject(go, bbPointsRoot);
            }

        }
        public static IEnumerator SetupBuildBotPaths(GameObject go)
        {
            SetupBuildBotBeamPoints(go);
            yield return SetupVFXConstructing(go);
            BuildBotPathsHelper(go);
        }
        public static IEnumerator SetupBuildBotPathsForAllMVs()
        {
            foreach (ModVehicle mv in VehicleBuilder.prefabs)
            {
                if (mv.GetComponentInChildren<BuildBotPath>(true) == null)
                {
                    yield return SetupBuildBotPaths(mv.gameObject);
                }
            }
        }
        public static void ResetGhostMaterial()
        {
            Material ghostMat = (Material)Resources.Load("Materials/constructingGhost");
            ghostMat.color = OriginalConstructingGhostColor;
        }
    }

    /*
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
    return pointTR;
}
*/
}
