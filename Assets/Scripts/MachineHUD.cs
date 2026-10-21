using UnityEngine;
using UnityEngine.SceneManagement;

// Interfaz sencilla: título, cronómetro, estado y botón para reiniciar la máquina.
public class MachineHUD : MonoBehaviour
{
    public static bool Iniciada;
    public static bool Completada;

    float tiempo;

    void Awake()
    {
        Iniciada = false;
        Completada = false;
    }

    void Update()
    {
        if (Iniciada && !Completada) tiempo += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.R)) Reiniciar();
    }

    void OnGUI()
    {
        var titulo = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
        var texto = new GUIStyle(GUI.skin.label) { fontSize = 18 };

        GUI.Label(new Rect(20, 15, 600, 40), "Máquina de Rube Goldberg", titulo);
        GUI.Label(new Rect(20, 55, 400, 30), $"Tiempo: {tiempo:F2} s", texto);

        string estado = Completada ? "¡Meta alcanzada!" : Iniciada ? "En marcha..." : "Preparando...";
        GUI.Label(new Rect(20, 80, 400, 30), estado, texto);

        if (GUI.Button(new Rect(20, 115, 140, 35), "Reiniciar (R)")) Reiniciar();
    }

    void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
