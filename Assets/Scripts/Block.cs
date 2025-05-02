using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private float stabilizationTime = 2f;
    private bool isStabilized = false;
    private float stabilizationTimer = 0f;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Rigidbody rb;
    private BlockProperties blockProperties;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        blockProperties = GetComponent<BlockProperties>();
        
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        
        // Start stabilization check for non-kinematic objects
        if (!rb.isKinematic)
        {
            StartCoroutine(CheckStability());
        }
        else
        {
            // Concrete base is already stabilized
            isStabilized = true;
        }
    }
    
    IEnumerator CheckStability()
    {
        // Wait a short time for initial physics settling
        yield return new WaitForSeconds(0.5f);
        
        while (!isStabilized)
        {
            // Check if block has moved significantly
            if (Vector3.Distance(transform.position, lastPosition) < 0.001f &&
                Quaternion.Angle(transform.rotation, lastRotation) < 0.1f)
            {
                stabilizationTimer += Time.deltaTime;
                if (stabilizationTimer >= stabilizationTime)
                {
                    StabilizeBlock();
                    break;
                }
            }
            else
            {
                // Reset timer if block moved
                stabilizationTimer = 0f;
            }
            
            // Update last known position/rotation
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            
            yield return null;
        }
    }
    
    void StabilizeBlock()
    {
        isStabilized = true;
        
        // Notify the block properties component
        if (blockProperties != null)
        {
            blockProperties.OnStabilized();
        }
        else
        {
            // Fall back to simple visual change if no BlockProperties component
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = new Color(
                    originalColor.r,
                    originalColor.g,
                    originalColor.b,
                    0.8f);
            }
        }
    }
}