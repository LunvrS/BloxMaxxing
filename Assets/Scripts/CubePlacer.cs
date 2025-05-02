using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CubePlacer : MonoBehaviour
{
    public GameObject baseBlockPrefab;
    public GameObject woodenBoxPrefab;
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject lastPlacedBlock = null;
    private bool basePlaced = false;

    public void PlaceBlock()
    {
        // Raycast from the center of the screen in world space
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (!basePlaced)
            {
                lastPlacedBlock = Instantiate(baseBlockPrefab, hitPose.position, Quaternion.identity);
                basePlaced = true;
            }
            else
            {
                Vector3 lastPos = lastPlacedBlock.transform.position;
                // The new block's position depends on the last block's position and some offset (height of block)
                Vector3 newPos = new Vector3(lastPos.x, lastPos.y + lastPlacedBlock.transform.localScale.y, lastPos.z);
                lastPlacedBlock = Instantiate(woodenBoxPrefab, newPos, Quaternion.identity);
            }
        }
    }

}
