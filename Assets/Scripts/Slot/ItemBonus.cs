using UnityEngine;
using UnityEngine.UI;

public class ItemBonus : MonoBehaviour {
    public Image sprite;
    public Text description;

    public void UpdateData(int index)
    {
        foreach (var item in PayTable.Instance.settingPays)
            if (item.tyleStyle == PayTable.Instance.settingBonus[index].tyleStyle)
            {
                sprite.sprite = PayTable.Instance.GetAvatar(item.Name);
                break;
            }
        description.text = PayTable.Instance.settingBonus[index].description;
    }
}
