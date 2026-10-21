using UnityEngine;

// Zona final de la máquina: cuando entra una bola se lanza el confeti y se marca como completada.
[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    public ParticleSystem confeti;
    public Light luzMeta;

    bool activada;

    void OnTriggerEnter(Collider other)
    {
        if (activada || other.attachedRigidbody == null || !(other is SphereCollider)) return;

        activada = true;
        if (confeti != null) confeti.Play();
        if (luzMeta != null) luzMeta.enabled = true;
        MachineHUD.Completada = true;
        Debug.Log($"¡Meta alcanzada en {Time.timeSinceLevelLoad:F2} s!");
    }
}
