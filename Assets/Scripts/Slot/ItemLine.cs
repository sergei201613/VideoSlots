using UnityEngine;
using UnityEngine.UI;

public class ItemLine : MonoBehaviour {
    public GameObject pointLine;
    public Color colorEnable, colorDisable;

    public void UpdateData(int line)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 5; j++)
            {
                GameObject temp = Instantiate(pointLine, pointLine.transform.parent, false);
                if (PayData.line[line, j] == i)
                    temp.GetComponent<Image>().color = colorEnable;
                else
                    temp.GetComponent<Image>().color = colorDisable;
            }
        pointLine.SetActive(false);
    }
}
