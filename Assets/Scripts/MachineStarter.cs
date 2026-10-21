using UnityEngine;

// Suelta el péndulo después de un pequeño retraso para que la máquina arranque sola.
public class MachineStarter : MonoBehaviour
{
    public Rigidbody pendulo;
    public float retraso = 1.5f;

    void Start()
    {
        Invoke(nameof(Soltar), retraso);
    }

    public void Soltar()
    {
        if (pendulo != null)
        {
            pendulo.isKinematic = false;
            pendulo.WakeUp();
        }
        MachineHUD.Iniciada = true;
    }
}
