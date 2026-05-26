using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    public Transform chainHandle;
    public float handleYPosition = 1.0f;
    
    // UI that prompts user to drag
    public GameObject dragIndicatorUI;
    
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