using System.IO;
using UnityEditor;
using UnityEngine;

namespace SportsGame.EditorTools
{
    /// <summary>
    /// 実行時に複製して使うベースマテリアル(Resources/SportsGame/Materials)を作る。
    /// ここで参照したシェーダーとキーワードがビルドに含まれる。
    /// </summary>
    public static class MaterialSetup
    {
        const string Folder = "Assets/SportsGame/Resources/SportsGame/Materials";

        public static void CreateBaseMaterials()
        {
            Directory.CreateDirectory(Folder);
            Make("Mat_Lit", "Universal Render Pipeline/Lit", m => m.SetFloat("_Smoothness", 0.3f));
            Make("Mat_Unlit", "Universal Render Pipeline/Unlit", m => m.SetFloat("_Cull", 0f));
            Make("Mat_Cutout", "Universal Render Pipeline/Lit", m =>
            {
                m.SetFloat("_AlphaClip", 1f);
                m.SetFloat("_Cutoff", 0.5f);
                m.SetFloat("_Cull", 0f);
                m.EnableKeyword("_ALPHATEST_ON");
                m.SetFloat("_Smoothness", 0.1f);
            });
            Make("Mat_Transparent", "Universal Render Pipeline/Unlit", SetTransparent);
            Make("Mat_Particle", "Universal Render Pipeline/Particles/Unlit", SetTransparent);
            AssetDatabase.SaveAssets();
        }

        static void SetTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_ZWrite", 0f);
        }

        static void Make(string name, string shaderName, System.Action<Material> configure)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError("[SportsGame] Shader not found: " + shaderName);
                return;
            }
            string path = Folder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = mat == null;
            if (isNew) mat = new Material(shader);
            else mat.shader = shader;
            configure(mat);
            BaseShaderGUI.SetupMaterialBlendMode(mat);
            if (isNew) AssetDatabase.CreateAsset(mat, path);
            EditorUtility.SetDirty(mat);
        }
    }
}
