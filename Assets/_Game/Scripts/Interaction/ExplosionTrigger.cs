using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem explosionFX;

    [Header("Trigger Settings")]
    public string targetTag = "Player";
    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag(targetTag))
        {
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        hasExploded = true;
        explosionFX.Play();
        Destroy(gameObject, explosionFX.main.duration);
    }


}
