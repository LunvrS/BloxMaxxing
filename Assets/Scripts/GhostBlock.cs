using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBlock : MonoBehaviour
{
    public Material ghostMaterial;
    
    private void Start()
    {
        // Set up ghost material on all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material[] materials = new Material[r.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = ghostMaterial;
            }
            r.materials = materials;
        }
    }
}