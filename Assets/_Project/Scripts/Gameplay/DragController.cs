using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragController : MonoBehaviour
{
    public Transform chainHandle;
    public float handleYPosition = 0.1f;
    
    // UI that prompts user to drag
    public GameObject dragIndicatorUI;
    
    [Header("Base Limits")]
    public Transform chainBaseTransform;
    public float maxDistanceFromBase = 3.0f;
    
    [Header("Visuals - Range Indicator")]
    public bool showRangeIndicatorLine = true;
    public LineRenderer rangeIndicatorLine;
    public int rangeIndicatorSegments = 50;
    public Color rangeIndicatorColor = new Color(1f, 1f, 1f, 0.3f);
    public float rangeIndicatorWidth = 0.1f;
    
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
        
        SetupRangeIndicator();
    }

    private void SetupRangeIndicator()
    {
        if (showRangeIndicatorLine && rangeIndicatorLine == null)
        {
            GameObject lineObj = new GameObject("DragRangeIndicator");
            lineObj.transform.SetParent(transform);
            rangeIndicatorLine = lineObj.AddComponent<LineRenderer>();
            
            rangeIndicatorLine.startWidth = rangeIndicatorWidth;
            rangeIndicatorLine.endWidth = rangeIndicatorWidth;
            rangeIndicatorLine.useWorldSpace = true;
            rangeIndicatorLine.loop = true;
            
            // Materialと色を設定
            Material lineMat = new Material(Shader.Find("Sprites/Default"));
            rangeIndicatorLine.material = lineMat;
            rangeIndicatorLine.startColor = rangeIndicatorColor;
            rangeIndicatorLine.endColor = rangeIndicatorColor;
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
                float currentMaxDist = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(maxDistanceFromBase) : maxDistanceFromBase;

                Vector3 basePos = chainBaseTransform.position;
                Vector3 offset = targetPosition - basePos;
                offset.y = 0; // 平面(XZ)上の距離で制限
                
                if (offset.magnitude > currentMaxDist)
                {
                    targetPosition = basePos + offset.normalized * currentMaxDist;
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
        
        UpdateRangeIndicator();
    }

    private void UpdateRangeIndicator()
    {
        if (showRangeIndicatorLine && rangeIndicatorLine != null && chainBaseTransform != null)
        {
            rangeIndicatorLine.positionCount = rangeIndicatorSegments;
            float angle = 0f;
            Vector3 center = chainBaseTransform.position;
            center.y = handleYPosition; // ドラッグ操作平面の高さに合わせる
            
            float currentMaxDist = SpikeBall.Instance != null ? SpikeBall.Instance.GetScaledValue(maxDistanceFromBase) : maxDistanceFromBase;

            for (int i = 0; i < rangeIndicatorSegments; i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentMaxDist;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * currentMaxDist;
                
                Vector3 pos = center + new Vector3(x, 0, z);
                rangeIndicatorLine.SetPosition(i, pos);
                
                angle += (360f / rangeIndicatorSegments);
            }
        }
        else if (rangeIndicatorLine != null && rangeIndicatorLine.positionCount > 0)
        {
            rangeIndicatorLine.positionCount = 0;
        }
    }

    private void OnDrawGizmos()
    {
        // エディタ上でも分かりやすいようにGizmoを描画
        if (chainBaseTransform != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = chainBaseTransform.position;
            center.y = handleYPosition;
            
            float currentMaxDist = (Application.isPlaying && SpikeBall.Instance != null) ? SpikeBall.Instance.GetScaledValue(maxDistanceFromBase) : maxDistanceFromBase;

            int segments = 36;
            float angle = 0f;
            Vector3 lastPos = center + new Vector3(0, 0, currentMaxDist);
            for (int i = 1; i <= segments; i++)
            {
                angle += 360f / segments;
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentMaxDist;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * currentMaxDist;
                Vector3 newPos = center + new Vector3(x, 0, z);
                Gizmos.DrawLine(lastPos, newPos);
                lastPos = newPos;
            }
        }
    }
}