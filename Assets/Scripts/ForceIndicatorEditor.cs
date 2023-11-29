using UnityEngine;

[ExecuteInEditMode]
public class ForceIndicatorEditor : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float forceRadius = 1.0f;

    void Update()
    {
        gameObject.transform.localScale = new Vector3(forceRadius, forceRadius, 1);

    }
}
