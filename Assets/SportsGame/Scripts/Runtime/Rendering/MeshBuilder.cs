using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SportsGame
{
    /// <summary>
    /// XZ平面(上向き)のライン・円弧・面を1つのメッシュへまとめるビルダー。コートのライン描画に使う。
    /// 座標は (x, z) を Vector2 で受け取る。
    /// </summary>
    public class MeshBuilder
    {
        readonly List<Vector3> verts = new List<Vector3>();
        readonly List<Vector2> uvs = new List<Vector2>();
        readonly List<int> tris = new List<int>();
        readonly float y;

        public MeshBuilder(float height = 0f) { y = height; }

        int AddVert(Vector2 p)
        {
            verts.Add(new Vector3(p.x, y, p.y));
            uvs.Add(p);
            return verts.Count - 1;
        }

        /// <summary>上(+Y)から見て表になるよう巻き順を自動調整して三角形を追加。</summary>
        void Tri(int a, int b, int c)
        {
            if (Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]).y < 0f) (b, c) = (c, b);
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }

        public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int i0 = AddVert(a), i1 = AddVert(b), i2 = AddVert(c), i3 = AddVert(d);
            Tri(i0, i1, i2);
            Tri(i0, i2, i3);
        }

        public void Line(float x0, float z0, float x1, float z1, float width)
        {
            Vector2 a = new Vector2(x0, z0), b = new Vector2(x1, z1);
            Vector2 dir = b - a;
            if (dir.sqrMagnitude < 1e-8f) return;
            dir.Normalize();
            Vector2 n = new Vector2(-dir.y, dir.x) * (width * 0.5f);
            Quad(a - n, a + n, b + n, b - n);
        }

        public void FilledRect(float x0, float z0, float x1, float z1)
            => Quad(new Vector2(x0, z0), new Vector2(x1, z0), new Vector2(x1, z1), new Vector2(x0, z1));

        /// <summary>中心線上にラインを描く長方形の枠。</summary>
        public void RectOutline(float x0, float z0, float x1, float z1, float width)
        {
            float h = width * 0.5f;
            Line(x0 - h, z0, x1 + h, z0, width);
            Line(x0 - h, z1, x1 + h, z1, width);
            Line(x0, z0, x0, z1, width);
            Line(x1, z0, x1, z1, width);
        }

        /// <summary>円弧。角度は度で、点 = c + r * (cos a, sin a)。</summary>
        public void Arc(Vector2 c, float r, float deg0, float deg1, float width, int segments = 32)
        {
            float rin = r - width * 0.5f, rout = r + width * 0.5f;
            for (int i = 0; i < segments; i++)
            {
                Vector2 d0 = Dir(Mathf.Lerp(deg0, deg1, i / (float)segments));
                Vector2 d1 = Dir(Mathf.Lerp(deg0, deg1, (i + 1) / (float)segments));
                Quad(c + d0 * rin, c + d0 * rout, c + d1 * rout, c + d1 * rin);
            }
        }

        public void Disc(Vector2 c, float r, int segments = 32, float deg0 = 0f, float deg1 = 360f)
        {
            int ci = AddVert(c);
            int prev = -1;
            for (int i = 0; i <= segments; i++)
            {
                int vi = AddVert(c + Dir(Mathf.Lerp(deg0, deg1, i / (float)segments)) * r);
                if (prev >= 0) Tri(ci, prev, vi);
                prev = vi;
            }
        }

        static Vector2 Dir(float deg)
        {
            float a = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }

        public Mesh ToMesh(string name)
        {
            var m = new Mesh { name = name };
            if (verts.Count > 65000) m.indexFormat = IndexFormat.UInt32;
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0);
            var normals = new Vector3[verts.Count];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
            m.normals = normals;
            m.RecalculateBounds();
            return m;
        }

        public GameObject Build(string name, Transform parent, Material mat)
            => Geo.MeshObject(name, parent, ToMesh(name), mat, false, true);
    }
}
