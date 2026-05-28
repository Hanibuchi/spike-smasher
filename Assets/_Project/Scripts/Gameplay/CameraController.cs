using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera vcam;
    private Vector3 baseFollowOffset;
    public float cameraScaleMultiplier = 0.5f;
    public float followSpeed = 2f; // Used if we need custom follow logic, but cinemachine handles it mostly

    private void Start()
    {
        if (vcam == null)
            vcam = GetComponent<CinemachineCamera>();

        if (vcam != null)
        {
            var follow = vcam.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                baseFollowOffset = follow.FollowOffset;
            }
        }

        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged += UpdateCameraSettings;
        }
    }

    private void OnDestroy()
    {
        if (SpikeBall.Instance != null)
        {
            SpikeBall.Instance.OnSizeLevelChanged -= UpdateCameraSettings;
        }
    }

    private void UpdateCameraSettings(float sizeLevel)
    {
        if (vcam == null) return;

        var follow = vcam.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            follow.FollowOffset = baseFollowOffset * ((1 - cameraScaleMultiplier) * sizeLevel + cameraScaleMultiplier);
        }
    }
}