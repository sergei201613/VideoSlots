    using UnityEngine;
    using UnityEngine.UI;

    public class WheelPart : MonoBehaviour
    {
        public Image spRend;
        public Text valueText;
        public DotForSound[] pointCollider;
        int index;
        public void UpdateData(int index)
        {
            this.index = index;
            valueText.text = FortuneWheelConfig.Instance.prizes[index].ToString();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            FortuneWheel.Instance.SelectedReward = this.index;
        }
    }