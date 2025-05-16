using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    private bool isPlaced = false;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Initially freeze the block in place
        FreezeBlock();
    }
    
    public void FreezeBlock()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        isPlaced = true;
    }
    
    public void UnfreezeBlock()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
        isPlaced = false;
    }
    
    // This method can be called to check if the block is misplaced
    public bool CheckMisplacement(Vector3 targetPosition, float threshold)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        return distance > threshold;
    }
}