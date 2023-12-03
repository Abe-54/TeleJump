using UnityEngine;

[ExecuteInEditMode]
public class ForceIndicatorEditor : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float forceRadius = 1.0f;
    public GameObject forceIndicator;

    public bool isCursorVisible = false;
    public bool isCrosshairVisible = false;


    void Update()
    {
        gameObject.transform.localScale = new Vector3(forceRadius, forceRadius, 1);

        Cursor.visible = isCursorVisible;

        forceIndicator.SetActive(isCrosshairVisible);

    }
}
