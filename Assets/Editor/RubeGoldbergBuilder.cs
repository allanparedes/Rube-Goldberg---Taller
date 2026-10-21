using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Construye la escena de la máquina de Rube Goldberg y permite probarla simulando la física desde el editor.
// Menú: Rube Goldberg > Construir escena / Simular y capturar
public static class RubeGoldbergBuilder
{
    const string ScenePath = "Assets/Scenes/RubeGoldberg.unity";
    const string MatFolder = "Assets/Materials";
    const float Ancho = 1.2f;

    static Transform raiz;

    [MenuItem("Rube Goldberg/Construir escena")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory(MatFolder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        raiz = new GameObject("Maquina").transform;

        // Materiales visuales
        var mSuelo = Mat("Suelo", new Color(0.35f, 0.38f, 0.42f));
        var mMadera = Mat("Madera", new Color(0.62f, 0.42f, 0.24f));
        var mRampa = Mat("Rampa", new Color(0.20f, 0.55f, 0.85f));
        var mRiel = Mat("Riel", new Color(0.85f, 0.85f, 0.88f), 0.6f);
        var mBola1 = Mat("Bola1", new Color(0.90f, 0.20f, 0.20f), 0.7f);
        var mBola2 = Mat("Bola2", new Color(0.95f, 0.75f, 0.10f), 0.7f);
        var mPendulo = Mat("Pendulo", new Color(0.25f, 0.25f, 0.28f), 0.9f);
        var mMeta = Mat("Meta", new Color(0.20f, 0.75f, 0.35f));
        var mDominoA = Mat("DominoA", new Color(0.95f, 0.95f, 0.95f));
        var mDominoB = Mat("DominoB", new Color(0.12f, 0.12f, 0.14f));

        // Materiales físicos
        var pBola = PhysMat("BolaFisica", 0.4f, 0.4f, 0.2f, PhysicsMaterialCombine.Average);
        var pBolaLisa = PhysMat("BolaLisaFisica", 0f, 0f, 0.1f, PhysicsMaterialCombine.Minimum);
        var pDomino = PhysMat("DominoFisica", 0.8f, 0.9f, 0f, PhysicsMaterialCombine.Maximum);

        // Suelo
        var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.SetParent(raiz);
        suelo.transform.localScale = new Vector3(6, 1, 6);
        suelo.GetComponent<Renderer>().sharedMaterial = mSuelo;

        // --- Etapa 1: péndulo que golpea la primera bola ---
        var plataforma = Box("PlataformaInicio", new Vector3(-12f, 6f, 0), new Vector3(2f, 0.2f, Ancho), 0, mMadera);
        Pilar(new Vector3(-12f, 0, 0), 5.9f, mMadera);

        var pivote = new Vector3(-12.15f, 9.4f, 0);
        var soporte = Box("SoportePendulo", pivote + Vector3.up * 0.1f, new Vector3(0.3f, 0.2f, 1.6f), 0, mMadera);
        Pilar(new Vector3(-12.15f, 0, 0.8f), 9.4f, mMadera, 0.15f);
        Pilar(new Vector3(-12.15f, 0, -0.8f), 9.4f, mMadera, 0.15f);

        var pendulo = new GameObject("Pendulo");
        pendulo.transform.SetParent(raiz);
        pendulo.transform.position = pivote;
        var varilla = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        varilla.name = "Varilla";
        Object.DestroyImmediate(varilla.GetComponent<Collider>());
        varilla.transform.SetParent(pendulo.transform, false);
        varilla.transform.localPosition = new Vector3(0, -1.35f, 0);
        varilla.transform.localScale = new Vector3(0.06f, 1.35f, 0.06f);
        varilla.GetComponent<Renderer>().sharedMaterial = mRiel;
        var masa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        masa.name = "Masa";
        masa.transform.SetParent(pendulo.transform, false);
        masa.transform.localPosition = new Vector3(0, -3f, 0);
        masa.transform.localScale = Vector3.one * 0.6f;
        masa.GetComponent<Renderer>().sharedMaterial = mPendulo;
        pendulo.transform.rotation = Quaternion.Euler(0, 0, -60f);
        var rbPendulo = pendulo.AddComponent<Rigidbody>();
        rbPendulo.mass = 3f;
        rbPendulo.isKinematic = true;
        rbPendulo.interpolation = RigidbodyInterpolation.Interpolate;
        var bisagra = pendulo.AddComponent<HingeJoint>();
        bisagra.anchor = Vector3.zero;
        bisagra.axis = Vector3.forward;

        var bola1 = Bola("Bola1", new Vector3(-11.6f, 6.35f, 0), 0.5f, 1f, mBola1, pBola);

        // --- Etapa 2: rampas con un salto entre ellas ---
        Rampa("Rampa1", new Vector2(-11f, 6.1f), new Vector2(-5.5f, 3.8f), mRampa, mRiel);
        Pilar(new Vector3(-5.8f, 0, 0), 3.8f, mMadera);
        Rampa("Rampa2", new Vector2(-4.6f, 3.0f), new Vector2(0f, 1.2f), mRampa, mRiel);
        Pilar(new Vector3(-4.3f, 0, 0), 2.8f, mMadera);

        // --- Etapa 3: mesa con fila de dominós ---
        Box("Mesa", new Vector3(4.5f, 0.6f, 0), new Vector3(9f, 1.2f, Ancho + 0.3f), 0, mMadera);
        var dominos = new GameObject("Dominos").transform;
        dominos.SetParent(raiz);
        for (int i = 0; i < 14; i++)
        {
            var d = Box($"Domino{i + 1:00}", new Vector3(1.5f + i * 0.5f, 1.2f + 0.45f, 0), new Vector3(0.12f, 0.9f, 0.5f), 0,
                i % 2 == 0 ? mDominoA : mDominoB, dominos);
            d.GetComponent<Collider>().sharedMaterial = pDomino;
            var rb = d.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // --- Etapa 4: la segunda bola baja hasta la meta ---
        // La bola descansa en una repisa a la altura de la punta del último dominó, que la empuja al caer.
        Box("Repisa", new Vector3(8.575f, 1.7f, 0), new Vector3(0.65f, 0.1f, 0.6f), 0, mMadera);
        Box("PosteRepisa", new Vector3(8.8f, 1.425f, 0), new Vector3(0.1f, 0.45f, 0.1f), 0, mMadera);
        var bola2 = Bola("Bola2", new Vector3(8.5f, 2.0f, 0), 0.5f, 0.5f, mBola2, pBolaLisa);
        Rampa("Rampa3", new Vector2(9f, 1.2f), new Vector2(13f, 0f), mRampa, mRiel);

        var meta = new GameObject("Meta").transform;
        meta.SetParent(raiz);
        Box("ParedFondo", new Vector3(17f, 0.4f, 0), new Vector3(0.2f, 0.8f, 2f), 0, mMeta, meta);
        Box("ParedIzq", new Vector3(15.75f, 0.2f, 1f), new Vector3(2.5f, 0.4f, 0.2f), 0, mMeta, meta);
        Box("ParedDer", new Vector3(15.75f, 0.2f, -1f), new Vector3(2.5f, 0.4f, 0.2f), 0, mMeta, meta);

        var zona = new GameObject("ZonaMeta");
        zona.transform.SetParent(meta);
        zona.transform.position = new Vector3(15.75f, 0.5f, 0);
        var trigger = zona.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2.4f, 1f, 1.8f);

        var luz = new GameObject("LuzMeta").AddComponent<Light>();
        luz.transform.SetParent(meta);
        luz.transform.position = new Vector3(15.75f, 3f, -1f);
        luz.type = LightType.Point;
        luz.color = new Color(0.3f, 1f, 0.4f);
        luz.intensity = 4f;
        luz.range = 6f;
        luz.enabled = false;

        var goal = zona.AddComponent<GoalTrigger>();
        goal.confeti = Confeti(meta, new Vector3(15.75f, 0.5f, 0));
        goal.luzMeta = luz;

        // Etiquetas de cada etapa
        Etiqueta("1. Péndulo", new Vector3(-13.5f, 10.4f, 0));
        Etiqueta("2. Rampas", new Vector3(-6.5f, 6.5f, -0.5f));
        Etiqueta("3. Dominós", new Vector3(4.5f, 3f, 0));
        Etiqueta("4. Meta", new Vector3(15.5f, 2f, 0));

        // Lógica de juego y cámara
        var control = new GameObject("Control");
        control.AddComponent<MachineHUD>();
        control.AddComponent<MachineStarter>().pendulo = rbPendulo;

        var cam = Camera.main;
        cam.transform.position = new Vector3(1.5f, 5.5f, -27f);
        cam.transform.rotation = Quaternion.Euler(5f, 0, 0);
        cam.fieldOfView = 45f;

        var sol = Object.FindFirstObjectByType<Light>();
        if (sol != null && sol.type == LightType.Directional)
        {
            sol.transform.rotation = Quaternion.Euler(45f, -30f, 0);
            sol.shadows = LightShadows.Soft;
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[RubeGoldberg] Escena construida en " + ScenePath);
    }

    // Simula la máquina sin entrar en Play, informa el resultado y guarda capturas en la carpeta Screenshots.
    [MenuItem("Rube Goldberg/Simular y capturar")]
    public static void SimulateAndCapture()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var modoAnterior = Physics.simulationMode;
        Physics.simulationMode = SimulationMode.Script;

        try
        {
            var bola1 = GameObject.Find("Bola1").transform;
            var bola2 = GameObject.Find("Bola2").transform;
            var pendulo = GameObject.Find("Pendulo").GetComponent<Rigidbody>();
            var dominos = GameObject.Find("Dominos").GetComponentsInChildren<Rigidbody>();
            var zona = GameObject.Find("ZonaMeta").GetComponent<BoxCollider>().bounds;
            var cam = Camera.main;

            Directory.CreateDirectory("Screenshots");
            Capturar(cam, "Screenshots/01_inicio.png");
            pendulo.isKinematic = false;

            const float dt = 0.02f;
            var log = new StringBuilder();
            float metaEn = -1f;
            bool capturaDominos = false;
            for (int paso = 1; paso <= 1500; paso++)
            {
                Physics.Simulate(dt);
                float t = paso * dt;
                int caidos = 0;
                foreach (var d in dominos) if (d.transform.up.y < 0.7f) caidos++;

                if (paso % 25 == 0)
                    log.AppendLine($"t={t:F1} bola1={bola1.position:F2} bola2={bola2.position:F2} dominos caidos={caidos}/{dominos.Length}");
                if (!capturaDominos && caidos >= dominos.Length / 2)
                {
                    capturaDominos = true;
                    Capturar(cam, "Screenshots/02_dominos.png");
                }
                if (metaEn < 0 && zona.Contains(bola2.position))
                {
                    metaEn = t;
                    Capturar(cam, "Screenshots/03_meta.png");
                }
            }
            if (metaEn < 0) Capturar(cam, "Screenshots/03_final.png");

            Debug.Log("[RubeGoldberg] Simulación:\n" + log);
            Debug.Log(metaEn >= 0
                ? $"[RubeGoldberg] RESULTADO: OK, la bola 2 llegó a la meta en {metaEn:F2} s"
                : "[RubeGoldberg] RESULTADO: FALLO, la bola 2 no llegó a la meta");
        }
        finally
        {
            Physics.simulationMode = modoAnterior;
            // Recargar para descartar los cambios que hizo la simulación
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
        }
    }

    static void Capturar(Camera cam, string ruta)
    {
        var rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
    }

    // ---------- utilidades de construcción ----------

    static GameObject Box(string nombre, Vector3 centro, Vector3 tam, float rotZ, Material mat, Transform padre = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre != null ? padre : raiz);
        go.transform.SetPositionAndRotation(centro, Quaternion.Euler(0, 0, rotZ));
        go.transform.localScale = tam;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    // Rampa cuya superficie superior va del punto a al punto b (en el plano XY), con rieles laterales.
    static void Rampa(string nombre, Vector2 a, Vector2 b, Material mat, Material matRiel)
    {
        const float grosor = 0.2f;
        var dir = b - a;
        float largo = dir.magnitude;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var normal = new Vector2(-dir.y, dir.x) / largo;
        var medio = (a + b) / 2f;

        var grupo = new GameObject(nombre).transform;
        grupo.SetParent(raiz);
        var c = medio - normal * grosor / 2f;
        Box("Superficie", new Vector3(c.x, c.y, 0), new Vector3(largo + 0.05f, grosor, Ancho), ang, mat, grupo);
        var r = medio + normal * 0.15f;
        Box("RielIzq", new Vector3(r.x, r.y, Ancho / 2f + 0.05f), new Vector3(largo, 0.3f, 0.1f), ang, matRiel, grupo);
        Box("RielDer", new Vector3(r.x, r.y, -Ancho / 2f - 0.05f), new Vector3(largo, 0.3f, 0.1f), ang, matRiel, grupo);
    }

    // Columna decorativa (sin colisionador) desde la base hasta la altura indicada.
    static void Pilar(Vector3 basePos, float alto, Material mat, float grosor = 0.25f)
    {
        var go = Box("Pilar", basePos + Vector3.up * alto / 2f, new Vector3(grosor, alto, grosor), 0, mat);
        Object.DestroyImmediate(go.GetComponent<Collider>());
    }

    static GameObject Bola(string nombre, Vector3 pos, float diametro, float masa, Material mat, PhysicsMaterial fisica)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = nombre;
        go.transform.SetParent(raiz);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * diametro;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.GetComponent<Collider>().sharedMaterial = fisica;
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = masa;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        return go;
    }

    static void Etiqueta(string texto, Vector3 pos)
    {
        var go = new GameObject("Etiqueta " + texto);
        go.transform.SetParent(raiz);
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.text = texto;
        tm.fontSize = 64;
        tm.characterSize = 0.08f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(0.1f, 0.1f, 0.15f);
        var fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = fuente;
        go.GetComponent<MeshRenderer>().sharedMaterial = fuente.material;
    }

    static ParticleSystem Confeti(Transform padre, Vector3 pos)
    {
        var go = new GameObject("Confeti");
        go.transform.SetParent(padre);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(-90f, 0, 0));
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = 3f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.gravityModifier = 0.8f;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.yellow, 0.33f), new GradientColorKey(Color.cyan, 0.66f), new GradientColorKey(Color.magenta, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        main.startColor = new ParticleSystem.MinMaxGradient(grad) { mode = ParticleSystemGradientMode.RandomColor };

        var emision = ps.emission;
        emision.rateOverTime = 0;
        emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 200) });

        var forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 30f;
        forma.radius = 0.3f;

        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.sharedMaterial = Mat("Confeti", Color.white, 0, "Particles/Standard Unlit");
        return ps;
    }

    static Material Mat(string nombre, Color color, float brillo = 0.3f, string shader = "Standard")
    {
        string ruta = $"{MatFolder}/{nombre}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find(shader));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.color = color;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", brillo);
        EditorUtility.SetDirty(m);
        return m;
    }

    static PhysicsMaterial PhysMat(string nombre, float dinamica, float estatica, float rebote, PhysicsMaterialCombine combinar)
    {
        Directory.CreateDirectory("Assets/PhysicsMaterials");
        string ruta = $"Assets/PhysicsMaterials/{nombre}.physicMaterial";
        var p = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(ruta);
        if (p == null)
        {
            p = new PhysicsMaterial(nombre);
            AssetDatabase.CreateAsset(p, ruta);
        }
        p.dynamicFriction = dinamica;
        p.staticFriction = estatica;
        p.bounciness = rebote;
        p.frictionCombine = combinar;
        EditorUtility.SetDirty(p);
        return p;
    }
}
