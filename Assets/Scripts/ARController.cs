using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using Unity.VisualScripting;

public class ARController : MonoBehaviour
{
    [SerializeField] protected ARRaycastManager arRaycastManager;
    [SerializeField] protected Camera arCamera;

    [SerializeField] protected GameObject[] placeablePrefabs;
    [SerializeField] protected GameObject pointerPrefab;

    [SerializeField] private Material[] swappableMaterials;

    [SerializeField] private float rotationStep = 15f;
    [SerializeField] private float scaleStep = 0.1f;
    [SerializeField] private float minScale = 0.05f;
    [SerializeField] private float maxScale = 3f;

    private int _selecterPrefabIndex;
    private PlaceObjects _selectedObject;

    private readonly List<ARRaycastHit> arHits = new();
    private readonly List<PlaceObjects> _placeObjects = new();

    public PlaceObjects SelectedObject => _selectedObject;

    public int PlacedCount => _placeObjects.Count;

    public Material[] SwappableMaterials => swappableMaterials;

    public event System.Action<PlaceObjects> OnObjectSelected;
    public event System.Action OnSelectionCleared;
    public event System.Action OnPlacedObjectsChanged;


    private void Update()
    {
        UpdatePointer();
        if (TryGetPointerPlaced(out Vector2 screenPos) && !IsPointerOverUI())
            HandleInput(screenPos);
    }

    protected void UpdatePointer()
    {
        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        bool valid = arRaycastManager.Raycast(screenCenter, arHits, TrackableType.PlaneWithinPolygon);

        pointerPrefab.SetActive(valid);

        if (valid)
        {
            Pose pose = arHits[0].pose;
            pointerPrefab.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    private void HandleInput(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            PlaceObjects placed = hitInfo.collider.GetComponentInParent<PlaceObjects>();
            if (placed != null)
            {
                SelectObject(placed);
                return;
            }
        }
        if (arRaycastManager.Raycast(screenPos, arHits, TrackableType.PlaneWithinPolygon))
        {
            ClearSelection();
            PlaceObjects(arHits[0].pose);
        }
    }

    private void PlaceObjects(Pose pose)
    {
        GameObject go = Instantiate(placeablePrefabs[_selecterPrefabIndex], pose.position, pose.rotation);

        PlaceObjects placed = go.GetComponent<PlaceObjects>();

        if (placed == null)
        {
            placed = go.AddComponent<PlaceObjects>();
        }

        _placeObjects.Add(placed);
        OnPlacedObjectsChanged?.Invoke();
    }

    private void ClearSelection()
    {
        if (_selectedObject != null)
        {
            _selectedObject.SetSelected(false);
            _selectedObject = null;
            OnSelectionCleared.Invoke();
        }
    }

    private void SelectObject(PlaceObjects placed)
    {
        if (_selectedObject == placed) return;

        ClearSelection();

        _selectedObject = placed;
        _selectedObject.SetSelected(true);
        OnObjectSelected.Invoke(_selectedObject);

    }

    public void ClearAll()
    {
        foreach (PlaceObjects obj in _placeObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }

        _placeObjects.Clear();
        _selectedObject = null;
        OnSelectionCleared.Invoke();
        OnPlacedObjectsChanged.Invoke();
    }

    protected bool TryGetPointerPlaced(out Vector2 position)
    {
        Pointer pointer = Pointer.current;

        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            position = pointer.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    public void RotateSelected(float direction)
    {
        if(_selectedObject != null)
            _selectedObject.transform.Rotate(Vector3.up, direction * rotationStep);
    }

    public void SetSelectedMaterial(int materialIndex)
    {
        if (_selectedObject != null && materialIndex >= 0 && materialIndex < swappableMaterials.Length)
        {
            _selectedObject.SetMaterial(swappableMaterials[materialIndex]); 
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetSelectedPrefab(int index)
    {
        _selecterPrefabIndex = Mathf.Clamp(index, 0, placeablePrefabs.Length - 1 );
    }

    public void DeleteSelected()
    {
        if (_selectedObject == null) return;

        _placeObjects.Remove(_selectedObject);
        Destroy(_selectedObject.gameObject);
        _selectedObject = null;
        OnSelectionCleared.Invoke();
        OnPlacedObjectsChanged.Invoke();
    }
}
