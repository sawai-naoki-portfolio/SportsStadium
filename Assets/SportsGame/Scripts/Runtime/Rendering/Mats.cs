using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// マテリアルの生成とキャッシュ。ビルドにシェーダーを確実に含めるため Resources 内のベースマテリアルを複製する。
    /// 同じ引数なら同じインスタンスを返すので、シーンを跨いでもマテリアルが増え続けない。
    /// </summary>
    public static class Mats
    {
        const string Folder = "SportsGame/Materials/";

        enum Base { Lit, Unlit, Cutout, Transparent, Particle }

        static readonly Dictionary<Base, Material> bases = new Dictionary<Base, Material>();
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        static readonly int MetallicId = Shader.PropertyToID("_Metallic");

        public static Material Lit(Color c, float smoothness = 0.25f, float metallic = 0f)
            => Get(Base.Lit, c, null, Vector2.one, smoothness, metallic);

        public static Material LitTex(Texture tex, Color tint, float smoothness = 0.25f, Vector2? tiling = null)
            => Get(Base.Lit, tint, tex, tiling ?? Vector2.one, smoothness, 0f);

        public static Material Unlit(Color c) => Get(Base.Unlit, c, null, Vector2.one, 0f, 0f);

        /// <summary>アルファカットアウト・両面描画(ネットなど)。</summary>
        public static Material Cutout(Texture tex, Color tint, Vector2 tiling) => Get(Base.Cutout, tint, tex, tiling, 0.1f, 0f);

        /// <summary>半透明アンリット(影・マーカー)。</summary>
        public static Material Transparent(Color c, Texture tex = null) => Get(Base.Transparent, c, tex, Vector2.one, 0f, 0f);

        /// <summary>頂点カラー対応の半透明(パーティクル・軌跡)。</summary>
        public static Material Particle => BaseMaterial(Base.Particle);

        static Material Get(Base kind, Color c, Texture tex, Vector2 tiling, float smoothness, float metallic)
        {
            string key = $"{kind}|{ColorUtility.ToHtmlStringRGBA(c)}|{(tex != null ? tex.GetHashCode() : 0)}|{tiling.x:F2},{tiling.y:F2}|{smoothness:F2}|{metallic:F2}";
            if (cache.TryGetValue(key, out var m) && m != null) return m;
            m = new Material(BaseMaterial(kind)) { name = key };
            m.SetColor(BaseColorId, c);
            if (tex != null)
            {
                m.SetTexture(BaseMapId, tex);
                m.SetTextureScale(BaseMapId, tiling);
            }
            if (kind == Base.Lit || kind == Base.Cutout)
            {
                m.SetFloat(SmoothnessId, smoothness);
                m.SetFloat(MetallicId, metallic);
            }
            cache[key] = m;
            return m;
        }

        static Material BaseMaterial(Base kind)
        {
            if (bases.TryGetValue(kind, out var m) && m != null) return m;
            m = Resources.Load<Material>(Folder + "Mat_" + kind);
            if (m == null)
            {
                GameLog.Warn("Mats", $"Base material Mat_{kind} is missing. Run SportsGame > Setup Project.");
                var shader = Shader.Find(kind == Base.Particle ? "Universal Render Pipeline/Particles/Unlit" : "Universal Render Pipeline/Lit");
                m = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"));
            }
            bases[kind] = m;
            return m;
        }
    }
}
