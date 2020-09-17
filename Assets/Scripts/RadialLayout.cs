using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialLayout : LayoutGroup
{
    public float fDistance;
    [Range(0f, 360f)]
    public float MinAngle, MaxAngle, StartAngle;
    public bool AutoAngle = false;
    public bool AutoFlip = false;
    public bool AllowRotate = false;
    protected override void OnEnable() { base.OnEnable(); CalculateRadial(); }
    public override void SetLayoutHorizontal()
    {
    }
    public override void SetLayoutVertical()
    {
    }
    public override void CalculateLayoutInputVertical()
    {
        CalculateRadial();
    }
    public override void CalculateLayoutInputHorizontal()
    {
        CalculateRadial();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateRadial();
    }
#endif
    void CalculateRadial()
    {
        if (AutoAngle)
        {
            MinAngle = 0;
            MaxAngle = 360f - 360f / transform.childCount;
        }
        m_Tracker.Clear();
        if (transform.childCount == 0)
            return;
        float fOffsetAngle = ((MaxAngle - MinAngle)) / (transform.childCount - 1);

        float fAngle = StartAngle;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = (RectTransform)transform.GetChild(i);
            if (child != null)
            {
                //Adding the elements to the tracker stops the user from modifiying their positions via the editor.
                m_Tracker.Add(this, child,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.Pivot);
                Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                child.localPosition = vPos * fDistance;
                if (AutoFlip) child.localRotation = Quaternion.Euler(new Vector3(child.localRotation.x, child.localRotation.y, fAngle + 90));
                //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                fAngle += fOffsetAngle;
            }
        }

    }

    Vector2 startPos, endPos;
    public void StartDrag()
    {
        dragging = true;
        startPos = Input.mousePosition;
    }

    bool dragging = false;
    public void Dragging()
    {
        endPos = Input.mousePosition;
        transform.Rotate(0, 0, endPos.x - startPos.x, Space.Self);
        startPos = endPos;
    }

    public void EndDrag()
    { 
        dragging = false;
    }
}