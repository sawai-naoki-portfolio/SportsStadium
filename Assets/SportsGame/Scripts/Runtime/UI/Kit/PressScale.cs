using UnityEngine;
using UnityEngine.EventSystems;

namespace SportsGame
{
    /// <summary>押下中に少し縮むボタン演出。</summary>
    public class PressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData e) => transform.localScale = Vector3.one * 0.95f;
        public void OnPointerUp(PointerEventData e) => transform.localScale = Vector3.one;
        void OnDisable() => transform.localScale = Vector3.one;
    }
}
