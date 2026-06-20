using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaneRotationChecker : ARController
{
    [SerializeField] private ARPlaneManager _ARPlaneManager;
    [SerializeField] private TextMeshProUGUI _objectText;


    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        UpdatePointer();
        if (TryGetPointerPlaced(out Vector2 screenPos)){
            HandleTouch(screenPos);
        }
        else
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            CheckPlaneRotation(screenCenter, false);
        }
    }

    private void HandleTouch(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider.GetComponentInParent<PlaceObjects>())
            {
                Destroy(hitInfo.collider.gameObject);
                return;
            }
        }

        CheckPlaneRotation(screenPos, true);
    }

    private void CheckPlaneRotation(Vector2 screenPos, bool touchedThisFrame)
    {
        if (arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];

            ARPlane hitPlane = _ARPlaneManager.GetPlane(hit.trackableId);

            if (hitPlane.alignment == PlaneAlignment.Vertical)
            {
                if(touchedThisFrame) PlaceObjects(hit.pose, placeablePrefabs[0]);

                _objectText.SetText("Painting");
            }
            else if (hitPlane.alignment == PlaneAlignment.HorizontalDown)
            {
                if (touchedThisFrame) PlaceObjects(hit.pose, placeablePrefabs[1]);

                _objectText.SetText("Chandelier");
            }
            else if (hitPlane.alignment == PlaneAlignment.HorizontalUp)
            {
                if (touchedThisFrame) PlaceObjects(hit.pose, placeablePrefabs[2]);

                _objectText.SetText("Vase");
            }

        }
    }

    private void PlaceObjects(Pose pose, GameObject objectToSpawn)
    {
        GameObject go = Instantiate(objectToSpawn, pose.position, pose.rotation);

        PlaceObjects placed = go.GetComponent<PlaceObjects>();

        if (placed == null)
        {
            placed = go.AddComponent<PlaceObjects>();
        }
    }
}
