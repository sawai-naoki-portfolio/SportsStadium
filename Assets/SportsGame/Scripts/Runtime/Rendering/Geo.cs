using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SportsGame
{
    /// <summary>プリミティブ配置のヘルパー。物理は自前なのでコライダーは付けない。</summary>
    public static class Geo
    {
        public static GameObject Empty(string name, Transform parent, Vector3 localPos = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go;
        }

        // 体のパーツなど大量に使う球・カプセルは、Unity 標準(約800ポリゴン)より軽いメッシュを共有する
        static Mesh lowSphere, lowCapsule, detailedSphere;

        static Mesh SharedMesh(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Sphere: return lowSphere != null ? lowSphere : (lowSphere = MeshFactory.Sphere(14, 10));
                case PrimitiveType.Capsule: return lowCapsule != null ? lowCapsule : (lowCapsule = MeshFactory.Capsule(14, 4));
                default: return null;
            }
        }

        public static GameObject Prim(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, string name = null, bool shadows = true)
        {
            var shared = SharedMesh(type);
            GameObject go;
            if (shared != null) go = MeshObject(name ?? type.ToString(), parent, shared, mat, shadows);
            else
            {
                go = GameObject.CreatePrimitive(type);
                go.name = name ?? type.ToString();
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
                go.transform.SetParent(parent, false);
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            return go;
        }

        /// <summary>近くで大きく映るボール用のなめらかな球。</summary>
        public static GameObject DetailedSphere(Transform parent, Vector3 pos, float diameter, Material mat, string name)
        {
            if (detailedSphere == null) detailedSphere = MeshFactory.Sphere(32, 20);
            var go = MeshObject(name, parent, detailedSphere, mat, true);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * diameter;
            return go;
        }

        public static GameObject Box(Transform parent, Vector3 pos, Vector3 size, Material mat, string name = null, bool shadows = true)
            => Prim(PrimitiveType.Cube, parent, pos, size, mat, name, shadows);

        /// <summary>Y軸方向の円柱。height は全長。</summary>
        public static GameObject Cylinder(Transform parent, Vector3 pos, float diameter, float height, Material mat, string name = null, bool shadows = true)
            => Prim(PrimitiveType.Cylinder, parent, pos, new Vector3(diameter, height * 0.5f, diameter), mat, name, shadows);

        public static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material mat, bool shadows = false, bool receive = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            mr.receiveShadows = receive;
            return go;
        }

        /// <summary>動かない背景をまとめてドローコールを減らす。</summary>
        public static void MakeStatic(GameObject root) => StaticBatchingUtility.Combine(root);

        /// <summary>
        /// 子のメッシュをマテリアルごとに1つへ結合する(観客など大量の小物用)。
        /// 結合後も root を動かせば全体が動く。
        /// </summary>
        public static void CombineByMaterial(Transform root, bool shadows = false)
        {
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var vertexCounts = new Dictionary<Material, int>();
            var filters = root.GetComponentsInChildren<MeshFilter>();
            Matrix4x4 toLocal = root.worldToLocalMatrix;
            foreach (var mf in filters)
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mf.sharedMesh == null) continue;
                var mat = mr.sharedMaterial;
                if (!groups.TryGetValue(mat, out var list))
                {
                    list = new List<CombineInstance>();
                    groups[mat] = list;
                    vertexCounts[mat] = 0;
                }
                list.Add(new CombineInstance { mesh = mf.sharedMesh, transform = toLocal * mf.transform.localToWorldMatrix });
                vertexCounts[mat] += mf.sharedMesh.vertexCount;
            }
            foreach (var mf in filters) Object.Destroy(mf.gameObject);
            foreach (var kv in groups)
            {
                var mesh = new Mesh { name = "Combined_" + kv.Key.name };
                if (vertexCounts[kv.Key] > 65000) mesh.indexFormat = IndexFormat.UInt32;
                mesh.CombineMeshes(kv.Value.ToArray(), true, true);
                MeshObject("Combined", root, mesh, kv.Key, shadows, false);
            }
        }
    }
}
