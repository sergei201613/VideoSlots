using UnityEngine;
using UnityEngine.UI;

public class ItemRule : MonoBehaviour {
    public Image sprite;
    public Text description;
    
    public void UpdateData(int index)
    {
        sprite.sprite = PayTable.Instance.settingRules[index].sprite;
        description.text = PayTable.Instance.settingRules[index].description;
    }
}
