using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    float countdown;
    [Header("References")]
    public ParticleSystem explosionFX;

    [Header("Trigger Settings")]
    public string targetTag = "Item";
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
        countdown -= Time.deltaTime;
        hasExploded = true;
        explosionFX.Play();
        Destroy(gameObject);
        if(countdown <= 0)
            explosionFX.Stop();
    }


}
