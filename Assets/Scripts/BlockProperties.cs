using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockProperties : MonoBehaviour
{
    public enum BlockType
    {
        ConcreteBase,
        WoodenBox
    }
    
    [SerializeField] private BlockType type;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float drag = 0.5f;
    [SerializeField] private float angularDrag = 0.5f;
    [SerializeField] private PhysicsMaterial physicsMaterial;
    
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Apply properties based on type
        ApplyBlockProperties();
    }
    
    void ApplyBlockProperties()
    {
        // Set basic properties
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        
        // Apply physics material if assigned
        if (physicsMaterial != null)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.material = physicsMaterial;
            }
        }
        
        // Apply type-specific properties
        switch (type)
        {
            case BlockType.ConcreteBase:
                // Concrete is heavier and more stable
                rb.mass = mass * 3f;
                rb.isKinematic = true; // Doesn't move by physics
                break;
                
            case BlockType.WoodenBox:
                // Wooden boxes have normal physics
                rb.isKinematic = false;
                break;
        }
    }
    
    // Can be called when block stabilizes
    public void OnStabilized()
    {
        // Slightly change appearance when stabilized
        if (meshRenderer != null)
        {
            Color originalColor = meshRenderer.material.color;
            meshRenderer.material.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                0.9f);
        }
        
        // Additional visual effects can be added here
    }
}