using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    public Transform chainHandle;
    public float handleYPosition = 1.0f;
    
    // UI that prompts user to drag
    public GameObject dragIndicatorUI;
    
    [Header("Base Limits")]
    public Transform chainBaseTransform;
    public float maxDistanceFromBase = 5.0f;
    
    private Camera mainCamera;
    private Plane dragPlane;
    private bool isDragging = false;
    private Rigidbody handleRb;

    private void Start()
    {
        mainCamera = Camera.main;
        dragPlane = new Plane(Vector3.up, new Vector3(0, handleYPosition, 0));
        
        if (chainHandle != null)
        {
            handleRb = chainHandle.GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            isDragging = false;
            UpdateIndicator();
            return;
        }

        HandleInput();
        UpdateIndicator();
    }
    
    private void FixedUpdate()
    {
        if (isDragging && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            MoveHandleToInput();
        }
    }

    private void HandleInput()
    {
        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return; // Clicked on UI

            isDragging = true;
        }
        else if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void MoveHandleToInput()
    {
        if (handleRb == null) return;
        if (Pointer.current == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance);
            
            // Limit distance from ChainBase
            if (chainBaseTransform != null)
            {
                Vector3 basePos = chainBaseTransform.position;
                Vector3 offset = targetPosition - basePos;
                offset.y = 0; // 平面(XZ)上の距離で制限
                
                if (offset.magnitude > maxDistanceFromBase)
                {
                    targetPosition = basePos + offset.normalized * maxDistanceFromBase;
                    targetPosition.y = handleYPosition; // 高さを元に戻す
                }
            }

            // Move using MovePosition to respect physics
            handleRb.MovePosition(targetPosition);
        }
    }

    private void UpdateIndicator()
    {
        if (dragIndicatorUI != null)
        {
            dragIndicatorUI.SetActive(!isDragging && GameManager.Instance.CurrentState == GameManager.GameState.Playing);
            
            if (chainHandle != null && dragIndicatorUI.activeSelf)
            {
                // Simple follow logic, assuming UI is world space canvas or an object with sprite
                dragIndicatorUI.transform.position = chainHandle.position;
            }
        }
    }
}