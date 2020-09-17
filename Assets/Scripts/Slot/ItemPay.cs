using UnityEngine;
using UnityEngine.UI;

public class ItemPay : MonoBehaviour
{
    public Image sprite;
    public Text label, value;

    public void UpdateData(int index)
    {
        sprite.sprite = PayTable.Instance.GetAvatar(index);
        string l = "", v = "";
        int i = 0;
        if (PayTable.Instance.settingPays[index].tylePay != null)
        {
            foreach (var item in PayTable.Instance.settingPays[index].tylePay)
            {
                i++;
                if (item > 0)
                {
                    l += i + ":\n";
                    v += item + "\n";
                }
            }
        }
        label.text = l;
        value.text = v;
    }

    private void OnEnable()
    {
        this.gameObject.SetActive(label.text != "" || value.text != "");
    }
}
