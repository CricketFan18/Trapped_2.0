using UnityEngine;
using System.Collections;

public class EscapeDoor : MonoBehaviour, IInteractable
{
    [Header("Explosion FX")]
    public ParticleSystem explosionEffect;
    public AudioSource explosionSound;
    public GameObject doorModel; // The physical door mesh to hide
    public GameObject c4;

    private bool isBlowingUp = false;

    public string InteractionPrompt
    {
        get
        {
            if (isBlowingUp) return "";

            // Check if they crafted the C4!
            if (InventorySystem.Instance != null && InventorySystem.Instance.CollectedItemsNames.Contains("Armed C4"))
            {
                return "Press E to Plant C4 and Escape!";
            }
            else
            {
                return "The door is sealed. You need explosives.";
            }
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (isBlowingUp) return false;

        // If they have the C4, start the win sequence
        if (InventorySystem.Instance != null && InventorySystem.Instance.CollectedItemsNames.Contains("Armed C4"))
        {
            c4.SetActive(true);
            GameManager.Instance.AddScore(100, "Bomb Planted");
            StartCoroutine(DetonateSequence());
            return true;
        }

        return false;
    }

    private IEnumerator DetonateSequence()
    {
        isBlowingUp = true;
        Debug.Log("C4 Planted! Detonating...");

        yield return new WaitForSeconds(5f); // Dramatic pause
        c4.SetActive(false);
        // 1. Play Effects
        if (explosionEffect != null) explosionEffect.Play();
        if (explosionSound != null) explosionSound.Play();

        // 2. Hide the door
        if (doorModel != null) doorModel.SetActive(false);

        yield return new WaitForSeconds(1.5f); // Let them watch the explosion

        // 3. Call your GameManager's Win function!
        GameManager.Instance.WinGame();
    }
}