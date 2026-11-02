using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RunInfinity.Adicional
{
    // Arranca antes de los scripts de practica para evitar controles duplicados.
    [DefaultExecutionOrder(-10000)]
    public sealed class RIGame : MonoBehaviour
    {
        public enum Estado { Menu, Jugando, Pausa, Derrota }
        [Header("Referencias asignadas por el instalador")]
        public RIPlayer jugador;
        public BoxCollider2D suelo;
        public GameObject plantillaObstaculo;
        public GameObject obstaculoDePrueba;
        public Camera camara;
        public RIUI interfaz;
        public RIAudio sonido;
        public MonoBehaviour[] componentesDePractica;
        [Header("Dificultad")]
        public float intervaloMinimo = 1.7f;
        public float intervaloMaximo = 2.3f;
        public float segundosHastaDificultadMaxima = 90f;

        public int Lives { get; private set; } = 2;
        private float proteccionRestante;
        public bool IsInvulnerable => proteccionRestante > 0;

        public Estado State { get; private set; }
        public bool IsPlaying => State == Estado.Jugando;
        public int Score => Mathf.FloorToInt(tiempo * 10f);
        public int BestScore => PlayerPrefs.GetInt("RI.Record", 0);
        public RIAudio Audio => sonido;
        public float LeftEdge => camara.transform.position.x - camara.orthographicSize * camara.aspect;
        public float RightEdge => camara.transform.position.x + camara.orthographicSize * camara.aspect;
        public float Speed => Mathf.Max(4f, (RightEdge - LeftEdge) * Mathf.Lerp(0.30f, 0.45f,
            Mathf.Clamp01(tiempo / Mathf.Max(1, segundosHastaDificultadMaxima))));
        private readonly List<RIObstacle> obstaculos = new List<RIObstacle>();
        private readonly List<RaycastResult> impactosUI = new List<RaycastResult>();
        private float tiempo, siguienteObstaculo;
        private int frameInicio, ancho, alto;
        private Vector3 inicioJugador;
        private bool preparado;
        private Transform contenedorObstaculos;

        private void Awake()
        {
            // Solo desactiva componentes durante Play; no cambia sus archivos.
            if (componentesDePractica != null)
                foreach (MonoBehaviour component in componentesDePractica)
                    if (component != null) component.enabled = false;
            if (obstaculoDePrueba != null) obstaculoDePrueba.SetActive(false);
            if (plantillaObstaculo != null) plantillaObstaculo.SetActive(false);
        }

        private void Start()
        {
            if (jugador == null || suelo == null || plantillaObstaculo == null ||
                camara == null || interfaz == null || sonido == null)
            {
                Debug.LogError("RunInfinity: faltan referencias. Ejecuta RunInfinity > Instalar gameplay adicional.");
                enabled = false;
                return;
            }
            Time.timeScale = 1;
            inicioJugador = jugador.transform.position;
            jugador.suelo = suelo;
            jugador.Initialize(this);
            sonido.Initialize();
            interfaz.Initialize(this);
            contenedorObstaculos = new GameObject("Obstaculos de esta partida").transform;
            contenedorObstaculos.SetParent(transform, false);
            AjustarCamara();
            preparado = true;
            GoToMenu();
        }

        private void AjustarCamara()
        {
            ancho = Screen.width; alto = Screen.height;
            camara.orthographic = true;
            camara.orthographicSize = 5f;
            float mitadAncho = camara.orthographicSize * camara.aspect;
            float piso = suelo.bounds.max.y;
            // Personaje al 20% del ancho y piso al 22% de la altura.
            camara.transform.position = new Vector3(inicioJugador.x + mitadAncho * 0.6f, piso + 5f, -10);
            float anchoSuelo = suelo.bounds.size.x;
            if (anchoSuelo > 0.001f)
            {
                Vector3 escala = suelo.transform.localScale;
                escala.x *= (mitadAncho * 2f + 4f) / anchoSuelo;
                suelo.transform.localScale = escala;
            }
            Vector3 posicion = suelo.transform.position;
            posicion.x = camara.transform.position.x;
            suelo.transform.position = posicion;
        }

        private void Update()
        {
            if (!preparado) return;
            if (ancho != Screen.width || alto != Screen.height)
            {
                if (IsPlaying) Pause();
                ClearObstacles();
                siguienteObstaculo = 1.5f;
                AjustarCamara();
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                interfaz.Back();
                return;
            }
            if (!IsPlaying || Time.frameCount <= frameInicio) return;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jugador.RequestJump();
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 p = Touchscreen.current.primaryTouch.position.ReadValue();
                if (!SobreControlUI(p)) jugador.RequestJump();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                !SobreControlUI(Mouse.current.position.ReadValue())) jugador.RequestJump();
            interfaz.UpdateScore(Score);
        }

        private bool SobreControlUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            impactosUI.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, impactosUI);
            foreach (RaycastResult hit in impactosUI)
                if (hit.gameObject.GetComponentInParent<Selectable>() != null) return true;
            return false;
        }

        private void FixedUpdate()
        {
            if (!preparado || !IsPlaying) return;
            proteccionRestante = Mathf.Max(0, proteccionRestante - Time.fixedDeltaTime);
            tiempo += Time.fixedDeltaTime;
            siguienteObstaculo -= Time.fixedDeltaTime;
            if (siguienteObstaculo <= 0)
            {
                Spawn();
                siguienteObstaculo = Random.Range(Mathf.Max(1.5f, intervaloMinimo), Mathf.Max(intervaloMinimo, intervaloMaximo));
            }
        }

        private void Spawn()
        {
            GameObject go = Instantiate(plantillaObstaculo, contenedorObstaculos);
            go.name = "Obstaculo";
            // La plantilla es inactiva, por lo que se prepara antes de entrar en fisica.
            BoxCollider2D caja = go.GetComponent<BoxCollider2D>();
            float altura = Mathf.Abs(caja.size.y * go.transform.lossyScale.y);
            float anchoObjeto = Mathf.Abs(caja.size.x * go.transform.lossyScale.x);
            go.transform.position = new Vector3(RightEdge + anchoObjeto,
                suelo.bounds.max.y + altura * 0.5f - 0.2f, 0);
            RIObstacle obstacle = go.GetComponent<RIObstacle>();
            obstacle.Initialize(this);
            obstaculos.Add(obstacle);
            go.SetActive(true);
        }

        public void RemoveObstacle(RIObstacle obstacle) { obstaculos.Remove(obstacle); }
        private void ClearObstacles()
        {
            foreach (RIObstacle obstacle in obstaculos)
            {
                if (obstacle == null) continue;
                obstacle.gameObject.SetActive(false);
                Destroy(obstacle.gameObject);
            }
            obstaculos.Clear();
        }

        public void StartGame()
        {
            ClearObstacles();
            tiempo = 0; siguienteObstaculo = 1.5f;
            Lives = 2; proteccionRestante = 0;
            interfaz.UpdateLives(Lives);
            State = Estado.Jugando; Time.timeScale = 1;
            frameInicio = Time.frameCount;
            jugador.gameObject.SetActive(true);
            jugador.ResetPlayer(true);
            interfaz.ShowGameplay();
            sonido.PlayGame();
        }
        public void Pause()
        {
            if (!IsPlaying) return;
            State = Estado.Pausa; Time.timeScale = 0;
            interfaz.ShowPause(); sonido.PauseGame();
        }
        public void Resume()
        {
            if (State != Estado.Pausa) return;
            State = Estado.Jugando; Time.timeScale = 1;
            frameInicio = Time.frameCount;
            interfaz.ShowGameplay(); sonido.ResumeGame();
        }
        public void TakeHit(RIObstacle obstacle)
        {
            if (!IsPlaying || obstacle == null) return;
            // Retira tambien el obstaculo durante la proteccion: nunca vuelve a golpear.
            RemoveObstacle(obstacle);
            obstacle.gameObject.SetActive(false);
            Destroy(obstacle.gameObject);
            if (IsInvulnerable) return;
            Lives = Mathf.Max(0, Lives - 1);
            interfaz.UpdateLives(Lives);
            if (Lives == 0) Lose();
            else proteccionRestante = 1.2f;
        }

        public void Lose()
        {
            if (!IsPlaying) return;
            State = Estado.Derrota; Time.timeScale = 0;
            jugador.StopPlayer();
            if (Score > BestScore) { PlayerPrefs.SetInt("RI.Record", Score); PlayerPrefs.Save(); }
            interfaz.ShowGameOver(Score); sonido.PlayLose();
        }
        public void GoToMenu()
        {
            State = Estado.Menu; Time.timeScale = 1;
            ClearObstacles(); jugador.ResetPlayer(false);
            interfaz.ShowMenu(); sonido.PlayMenu();
        }
        private void OnApplicationPause(bool paused) { if (preparado && paused) { Pause(); PlayerPrefs.Save(); } }
        private void OnApplicationFocus(bool focus) { if (preparado && !focus) Pause(); }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
