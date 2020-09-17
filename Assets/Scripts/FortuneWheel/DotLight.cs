    using UnityEngine;
    using UnityEngine.UI;
public class DotLight : MonoBehaviour
{
    [HideInInspector]
    public Image spRend;
    private void Awake()
    {
        spRend = transform.GetComponent<Image>();
    }
}