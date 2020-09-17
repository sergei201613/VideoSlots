using UnityEngine;
using UnityEngine.UI;

public class ItemPoker : MonoBehaviour {
    public Image image;
	
    public void UpdateData(Sprite sp)
    {
        image.sprite = sp;
    }
}
