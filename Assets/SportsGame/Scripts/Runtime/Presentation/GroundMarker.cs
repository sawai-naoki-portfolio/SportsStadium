using UnityEngine;

namespace SportsGame
{
    /// <summary>地面に表示する脈打つリング(落下予測地点・移動目標など)。</summary>
    public class GroundMarker : MonoBehaviour
    {
        float pulse;
        float baseScale = 1f;

        public static GroundMarker Create(Transform parent, string name, float radius, Color color, float thickness = 0.08f)
        {
            var go = Geo.MeshObject(name, parent, MeshFactory.Ring(radius - thickness, radius, 40), Mats.Transparent(color));
            var m = go.AddComponent<GroundMarker>();
            go.SetActive(false);
            return m;
        }

        public void Show(Vector3 pos, float scale = 1f)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            transform.position = new Vector3(pos.x, 0.015f, pos.z);
            baseScale = scale;
        }

        public void Hide()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        void Update()
        {
            pulse += Time.deltaTime * 5f;
            float s = baseScale * (1f + Mathf.Sin(pulse) * 0.08f);
            transform.localScale = new Vector3(s, 1f, s);
        }
    }
}
