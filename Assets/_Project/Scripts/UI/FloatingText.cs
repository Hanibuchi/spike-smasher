using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 1.5f;
    public Vector3 floatDirection = Vector3.up;
    public float floatSpeed = 2f;

    private TextMeshPro textMesh;

    private void Awake()
    {
        // Using 3D TextMeshPro for floating texts in world space
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }
        
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
        
        // Always face camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}