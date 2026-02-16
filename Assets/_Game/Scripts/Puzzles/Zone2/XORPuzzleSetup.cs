using UnityEngine;

public class XORPuzzleSetup : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    public GameObject PlaneA; // The hidden message
    public GameObject PlaneB; // The revealer / filter / UV light
    
    [Header("Materials")]
    public Material PlaneAMaterial;
    public Material RevealerMaterial;

    private void Start()
    {
        SetupPuzzle();
    }

    [ContextMenu("Setup Puzzle")]
    public void SetupPuzzle()
    {
        // Setup Plane A (Hidden Message)
        if (PlaneA != null)
        {
            if (PlaneA.GetComponent<Collider>() == null) PlaneA.AddComponent<BoxCollider>();
            if (PlaneA.GetComponent<Rigidbody>() == null) PlaneA.AddComponent<Rigidbody>();
            
            var grab = PlaneA.GetComponent<GrabbableObject>();
            if (grab == null) grab = PlaneA.AddComponent<GrabbableObject>();
            grab.ObjectName = "Hidden Message Sheet";
            grab.UsePhysics = true;

            if (PlaneAMaterial != null)
            {
                var rend = PlaneA.GetComponent<Renderer>();
                if (rend != null) rend.material = PlaneAMaterial;
            }
        }

        // Setup Plane B (Revealer / UV Light)
        if (PlaneB != null)
        {
            if (PlaneB.GetComponent<Collider>() == null) PlaneB.AddComponent<BoxCollider>();
            if (PlaneB.GetComponent<Rigidbody>() == null) PlaneB.AddComponent<Rigidbody>();

            var grab = PlaneB.GetComponent<GrabbableObject>();
            if (grab == null) grab = PlaneB.AddComponent<GrabbableObject>();
            grab.ObjectName = "UV Filter Sheet";
            grab.UsePhysics = true;

            if (RevealerMaterial != null)
            {
                var rend = PlaneB.GetComponent<Renderer>();
                if (rend != null) rend.material = RevealerMaterial;
            }
        }
    }
}
