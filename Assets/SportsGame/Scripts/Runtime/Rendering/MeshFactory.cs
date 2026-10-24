using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>手続き的に作る立体メッシュ(球・カプセル・円盤・リング・トーラス・円錐台・帯)。</summary>
    public static class MeshFactory
    {
        /// <summary>
        /// 経緯度 UV (u=経度, v=緯度) に対応する球面上の方向。球メッシュとボールのテクスチャ生成で同じ対応を使う。
        /// </summary>
        public static Vector3 SphereDirection(float u, float v)
        {
            float lon = u * Mathf.PI * 2f;
            float lat = (v - 0.5f) * Mathf.PI;
            float cl = Mathf.Cos(lat);
            return new Vector3(-Mathf.Cos(lon) * cl, Mathf.Sin(lat), -Mathf.Sin(lon) * cl);
        }

        /// <summary>直径 1 の UV 球(Unity の Sphere プリミティブと同じ大きさ)。</summary>
        public static Mesh Sphere(int segments, int rings)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            for (int j = 0; j <= rings; j++)
            {
                for (int i = 0; i <= segments; i++)
                {
                    var uv = new Vector2(i / (float)segments, j / (float)rings);
                    Vector3 n = SphereDirection(uv.x, uv.y);
                    verts.Add(n * 0.5f);
                    normals.Add(n);
                    uvs.Add(uv);
                }
            }
            return Finish("Sphere", verts, normals, uvs, GridTriangles(verts, normals, segments, rings));
        }

        /// <summary>
        /// 半径 0.5・全長 2 の Y 軸カプセル(Unity の Capsule プリミティブと同じ大きさ)。capRings は半球の分割数。
        /// </summary>
        public static Mesh Capsule(int segments, int capRings)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            int rows = capRings * 2 + 1;
            for (int j = 0; j <= rows; j++)
            {
                // 下の半球(緯度 -90°→0°) → 円柱部 → 上の半球(0°→90°)
                bool upper = j > capRings;
                float t = upper ? (j - capRings - 1) / (float)capRings : j / (float)capRings;
                float lat = upper ? t * Mathf.PI * 0.5f : (t - 1f) * Mathf.PI * 0.5f;
                float yOffset = upper ? 0.5f : -0.5f;
                for (int i = 0; i <= segments; i++)
                {
                    float lon = i / (float)segments * Mathf.PI * 2f;
                    Vector3 n = new Vector3(Mathf.Cos(lon) * Mathf.Cos(lat), Mathf.Sin(lat), Mathf.Sin(lon) * Mathf.Cos(lat));
                    verts.Add(n * 0.5f + Vector3.up * yOffset);
                    normals.Add(n);
                }
            }
            return Finish("Capsule", verts, normals, null, GridTriangles(verts, normals, segments, rows));
        }

        /// <summary>(segments+1)×(rows+1) の格子状頂点を三角形でつなぐ。</summary>
        static List<int> GridTriangles(List<Vector3> verts, List<Vector3> normals, int segments, int rows)
        {
            var tris = new List<int>();
            int stride = segments + 1;
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < segments; i++)
                {
                    int a = j * stride + i, b = a + 1, c = a + stride, d = c + 1;
                    AddOriented(tris, verts, normals, a, c, b);
                    AddOriented(tris, verts, normals, b, c, d);
                }
            }
            return tris;
        }

        public static Mesh Disc(float radius, int segments = 32)
        {
            var mb = new MeshBuilder();
            mb.Disc(Vector2.zero, radius, segments);
            return mb.ToMesh("Disc");
        }

        public static Mesh Ring(float inner, float outer, int segments = 48)
        {
            var mb = new MeshBuilder();
            mb.Arc(Vector2.zero, (inner + outer) * 0.5f, 0f, 360f, outer - inner, segments);
            return mb.ToMesh("Ring");
        }

        /// <summary>XZ平面上のトーラス(バスケットのリング・ラケットの枠)。</summary>
        public static Mesh Torus(float radius, float tube, int seg = 32, int tubeSeg = 8)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                for (int j = 0; j <= tubeSeg; j++)
                {
                    float b = j / (float)tubeSeg * Mathf.PI * 2f;
                    Vector3 n = dir * Mathf.Cos(b) + Vector3.up * Mathf.Sin(b);
                    verts.Add(dir * radius + n * tube);
                    normals.Add(n);
                }
            }
            int row = tubeSeg + 1;
            for (int i = 0; i < seg; i++)
            {
                for (int j = 0; j < tubeSeg; j++)
                {
                    int a0 = i * row + j, a1 = a0 + 1, b0 = (i + 1) * row + j, b1 = b0 + 1;
                    AddOriented(tris, verts, normals, a0, a1, b0);
                    AddOriented(tris, verts, normals, a1, b1, b0);
                }
            }
            return Finish("Torus", verts, normals, null, tris);
        }

        /// <summary>円錐台(バスケットのネット)。上端が原点。両面描画のマテリアルで使う。</summary>
        public static Mesh Frustum(float topRadius, float bottomRadius, float height, int seg = 24)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i <= seg; i++)
            {
                float u = i / (float)seg;
                float a = u * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(dir * topRadius);
                verts.Add(dir * bottomRadius + Vector3.down * height);
                uvs.Add(new Vector2(u, 1f));
                uvs.Add(new Vector2(u, 0f));
                normals.Add(dir);
                normals.Add(dir);
            }
            for (int i = 0; i < seg; i++)
            {
                int t0 = i * 2, b0 = t0 + 1, t1 = t0 + 2, b1 = t0 + 3;
                tris.AddRange(new[] { t0, t1, b0, t1, b1, b0 });
            }
            return Finish("Frustum", verts, normals, uvs, tris);
        }

        /// <summary>
        /// x 方向に伸びる垂直な帯(z=0 平面)。上端の高さを関数で与える(ネットのたるみ)。
        /// uvScale を指定すると UV = 位置 / uvScale になる(格子テクスチャのタイル用)。
        /// </summary>
        public static Mesh VerticalStrip(float x0, float x1, int segments, System.Func<float, float> bottom, System.Func<float, float> top, float uvScale)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float x = Mathf.Lerp(x0, x1, i / (float)segments);
                float yb = bottom(x), yt = top(x);
                verts.Add(new Vector3(x, yb, 0f));
                verts.Add(new Vector3(x, yt, 0f));
                uvs.Add(new Vector2(x / uvScale, yb / uvScale));
                uvs.Add(new Vector2(x / uvScale, yt / uvScale));
                if (i < segments)
                {
                    int a = i * 2;
                    tris.AddRange(new[] { a, a + 1, a + 2, a + 1, a + 3, a + 2 });
                }
            }
            var m = Finish("Strip", verts, null, uvs, tris);
            m.RecalculateNormals();
            return m;
        }

        static void AddOriented(List<int> tris, List<Vector3> v, List<Vector3> n, int a, int b, int c)
        {
            Vector3 avg = n[a] + n[b] + n[c];
            if (Vector3.Dot(Vector3.Cross(v[b] - v[a], v[c] - v[a]), avg) < 0f) (b, c) = (c, b);
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }

        static Mesh Finish(string name, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> tris)
        {
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            if (normals != null) m.SetNormals(normals);
            if (uvs != null) m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            m.RecalculateBounds();
            return m;
        }
    }
}
