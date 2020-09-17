    using System;
    using UnityEngine;
    public class FortuneWheelConfig : MonoBehaviour
    {
        public int[] prizes = new int[8];
        public GameObject wheelPart;
        public AudioClip tingSound;
        [Range(1, 5)]
        public int speedMultiplier;
        [Range(2, 10)]
        public int duration;
        public bool timedTurn;
        public Sprite[] illuminatiDots;
        public AnimationCurve animationCurve;
        private static FortuneWheelConfig _instance;
        public static FortuneWheelConfig Instance
        { get { if (_instance == null) _instance = FindObjectOfType<FortuneWheelConfig>(); return _instance; } }

        public static string GetValueFormated(int val)
        {
            return String.Format("{0:n0}", val);
        }

        private void Awake()
        {
            for (int i = 1; i < prizes.Length; i++)
            {
                GameObject temp = Instantiate(wheelPart, wheelPart.transform.parent, false);
                temp.GetComponent<WheelPart>().UpdateData(i);
                temp.SetActive(true);
            }
            wheelPart.GetComponent<WheelPart>().UpdateData(0);
            wheelPart.SetActive(true);
        }
    }