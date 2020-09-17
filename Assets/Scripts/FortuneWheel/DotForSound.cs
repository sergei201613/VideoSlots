    using UnityEngine;
    using UnityEngine.UI;
    public class DotForSound : MonoBehaviour
    {
        public Image pointSprite;
        void Start()
        {
            if (pointSprite == null)
                pointSprite = GetComponent<Image>();
        }
        void OnCollisionEnter2D(Collision2D coll)
        {
            FortuneWheel.Instance.HitStart(pointSprite);
        }
    }