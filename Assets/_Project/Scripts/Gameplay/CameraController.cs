using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera vcam;
    public float baseOrthographicSize = 10f;
    public float baseDistance = 15f;
    public float growthFactor = 0.002f;
    public float followSpeed = 2f; // Used if we need custom follow logic, but cinemachine handles it mostly
    
    private void Start()
    {
        if (vcam == null)
            vcam = GetComponent<CinemachineCamera>();
            
        GameManager.Instance.OnScoreChanged += UpdateCameraSettings;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateCameraSettings;
        }
    }

    private void UpdateCameraSettings(int score)
    {
        if (vcam == null) return;
        
        if (vcam.Lens.Orthographic)
        {
            vcam.Lens.OrthographicSize = baseOrthographicSize + score * growthFactor;
        }
        else
        {
            vcam.Lens.FieldOfView = Mathf.Clamp(60f + score * growthFactor * 2f, 60f, 90f);
            
            // Also increase distance if using a framing transposer or similar component
            var framedTransposer = vcam.GetComponent<CinemachinePositionComposer>();
            if (framedTransposer != null)
            {
                // Note: Actual distance modification depends on exactly how the camera is set up.
                // Simple FOV adjustment often suffices for scale out feeling.
            }
        }
    }
}