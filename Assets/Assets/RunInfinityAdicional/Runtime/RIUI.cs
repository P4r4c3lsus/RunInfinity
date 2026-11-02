using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RunInfinity.Adicional
{
// Controla las ventanas por referencias sin modificar los scripts originales.
public sealed class RIUI : MonoBehaviour
{
    [System.Serializable]
    public class Window
    {
        public string id;
        public GameObject root;
        public CanvasGroup group;
    }
    public Window[] ventanas;
    private RIGame game;
    private readonly Dictionary<string, Window> windows = new Dictionary<string, Window>();
    private readonly Stack<string> history = new Stack<string>();
    private string current;
    private GameObject hud, backButton, records;
    private RectTransform safeArea;
    private TMP_Text scoreText, recordsText;
    private TMP_FontAsset font;
    private readonly List<GameObject> hearts = new List<GameObject>();
    private GameObject pauseContainer;
    private Image pauseBackground;
    private SpriteRenderer forest;
    private bool originalHUD;


    public void Initialize(RIGame owner)
    {
        game = owner;
        foreach (Window window in ventanas)
        {
            if (window == null || window.root == null) continue;
            // Permite volver a mostrar las UIs si se ocultaron para probar la fisica.
            Transform ancestor = window.root.transform.parent;
            while (ancestor != null)
            {
                ancestor.gameObject.SetActive(true);
                ancestor = ancestor.parent;
            }
            windows[window.id] = window;
            foreach (Canvas canvas in window.root.GetComponentsInChildren<Canvas>(true))
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 20;
            }
            if (font == null)
            {
                TMP_Text text = window.root.GetComponentInChildren<TMP_Text>(true);
                if (text != null) font = text.font;
            }
        }
        BuildHUD();
        if (windows.TryGetValue("settingsui", out Window settings))
        {
            Slider slider = settings.root.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
                slider.SetValueWithoutNotify(game.Audio.Volume);
                slider.onValueChanged = new Slider.SliderEvent();
                slider.onValueChanged.AddListener(game.Audio.SetVolume);
            }
        }
        PrepareOriginalHUD();
        Bind("mainmenuui", "LongButton_Blue_1", game.StartGame);
        Bind("mainmenuui", "Settings", () => Navigate("settingsui"));
        Bind("mainmenuui", "LongButton_Orange_1", ShowRecords);
        Bind("mainmenuui", "LongButton_Red_Pressed_1", () => Navigate("popui"));
        Bind("pauseui", "LongButton_Blue_1", game.Resume);
        Bind("pauseui", "Pause_Button", () => { if (game.IsPlaying) game.Pause(); else game.Resume(); });
        Bind("pauseui", "LongButton_Green_1", () => Navigate("settingsui"));
        Bind("pauseui", "LongButton_Red_1", () => Navigate("popui"));
        Bind("gameoverui", "LongButton_Blue_1", game.StartGame);
        Bind("gameoverui", "LongButton_Red_1", game.GoToMenu);
        Bind("settingsui", "CreditsButton", () => Navigate("gamecreditsui"));
        Bind("gamecreditsui", "LongButton_White_3", Back);
        Bind("popui", "Yes_Button", ConfirmExit);
        Bind("popui", "No_Button", Back);
    }

    private void Bind(string windowId, string objectName, UnityAction action)
    {
        if (!windows.TryGetValue(windowId, out Window window))
        {
            Debug.LogError("Falta ventana: " + windowId);
            return;
        }
        foreach (Button button in window.root.GetComponentsInChildren<Button>(true))
        {
            if (button.name.Trim() != objectName) continue;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { game.Audio.PlayClick(); action(); });
            // Evita que Espacio active un boton que quedo seleccionado.
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return;
        }
        Debug.LogError("Falta boton: " + windowId + "/" + objectName);
    }

    private void SetVisible(Window window, bool visible)
    {
        if (window.group != null)
        {
            window.group.alpha = visible ? 1 : 0;
            window.group.interactable = visible;
            window.group.blocksRaycasts = visible;
        }
        window.root.SetActive(visible);
    }

    private void Display(string id)
    {
        if (current == "settingsui") PlayerPrefs.Save();
        foreach (Window window in windows.Values) SetVisible(window, false);
        current = id;
        hud.SetActive(id == "" && !originalHUD);
        if (originalHUD)
        {
            bool playing = id == "";
            pauseContainer.SetActive(!playing);
            pauseBackground.enabled = !playing;
            if (playing) SetVisible(windows["pauseui"], true);
        }
        if (forest != null) forest.gameObject.SetActive(id == "");
        records.SetActive(id == "records");
        backButton.SetActive(id == "settingsui" || id == "records");
        if (windows.TryGetValue(id, out Window target)) SetVisible(target, true);
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    private void Navigate(string id)
    {
        history.Push(current);
        Display(id);
    }

    public void ShowGameplay()
    {
        history.Clear();
        Display("");
        UpdateScore(game.Score);
    }

    public void ShowMenu() { history.Clear(); Display("mainmenuui"); }
    public void ShowPause() { history.Clear(); SetWindowScore("pauseui", game.Score); Display("pauseui"); }
    public void ShowGameOver(int score) { history.Clear(); SetWindowScore("gameoverui", score); Display("gameoverui"); }
    public void UpdateScore(int score)
    {
        scoreText.text = "PUNTOS  " + score.ToString("00000");
        SetWindowScore("pauseui", score);
    }

    public void UpdateLives(int count)
    {
        for (int i = 0; i < hearts.Count; i++) hearts[i].SetActive(i < count);
    }

    private void PrepareOriginalHUD()
    {
        if (!windows.TryGetValue("pauseui", out Window pause)) return;
        foreach (Transform child in pause.root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("Heart_")) hearts.Add(child.gameObject);
            if (child.name == "Container") pauseContainer = child.gameObject;
            if (child.name == "Background_Rectangle_1") pauseBackground = child.GetComponent<Image>();
        }
        hearts.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        UpdateLives(game.Lives);
        originalHUD = pauseContainer != null && pauseBackground != null && pauseBackground.sprite != null;
        if (!originalHUD)
        {
            Debug.LogError("RunInfinity: revisa Container y Background_Rectangle_1 en PauseUI.");
            return;
        }
        // La imagen UI opaca taparia al Runner. El mismo sprite se dibuja detras del mundo.
        GameObject background = new GameObject("Bosque gameplay");
        background.transform.SetParent(transform, false);
        forest = background.AddComponent<SpriteRenderer>();
        forest.sprite = pauseBackground.sprite;
        forest.color = pauseBackground.color;
        forest.sortingLayerID = game.jugador.GetComponent<SpriteRenderer>().sortingLayerID;
        forest.sortingOrder = -1000;
        // Los graficos decorativos no deben bloquear el boton de pausa ni los toques.
        foreach (Graphic graphic in pause.root.GetComponentsInChildren<Graphic>(true))
            if (graphic.GetComponentInParent<Selectable>() == null) graphic.raycastTarget = false;
        FitForest();
    }

    private void FitForest()
    {
        if (forest == null || game.camara == null) return;
        Camera camera = game.camara;
        Vector3 size = forest.sprite.bounds.size;
        if (size.x <= 0 || size.y <= 0) return;
        float height = camera.orthographicSize * 2;
        forest.transform.localScale = new Vector3(height * camera.aspect / size.x, height / size.y, 1);
        Vector3 center = camera.transform.position;
        center.z = 5;
        forest.transform.position = center - Vector3.Scale(forest.sprite.bounds.center, forest.transform.lossyScale);
    }

    private void SetWindowScore(string id, int score)
    {
        if (!windows.TryGetValue(id, out Window window)) return;
        foreach (TMP_Text text in window.root.GetComponentsInChildren<TMP_Text>(true))
            if (text.name == "00000") text.text = score.ToString("00000");
    }

    private void ShowRecords()
    {
        recordsText.text = "MEJOR PUNTUACION\n\n" + game.BestScore.ToString("00000") + "\n\nSupera tu record en la siguiente partida.";
        Navigate("records");
    }

    public void Back()
    {
        if (history.Count > 0) { Display(history.Pop()); return; }
        if (game.State == RIGame.Estado.Jugando) game.Pause();
        else if (game.State == RIGame.Estado.Pausa) game.Resume();
        else if (game.State == RIGame.Estado.Derrota) game.GoToMenu();
        else Navigate("popui");
    }

    private void ConfirmExit()
    {
        if (game.State != RIGame.Estado.Menu) { game.GoToMenu(); return; }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BuildHUD()
    {
        GameObject canvasObject = new GameObject("Runner HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 1600);
        scaler.matchWidthOrHeight = 0.5f;
        safeArea = Panel("Safe Area", canvasObject.transform);
        hud = Panel("Gameplay", safeArea).gameObject;
        scoreText = Label("Score", hud.transform, "PUNTOS  00000", new Vector2(0.04f, 0.89f), new Vector2(0.65f, 0.98f), 34);
        scoreText.alignment = TextAlignmentOptions.Left;
        ButtonAt("Pausa", hud.transform, new Vector2(0.73f, 0.9f), new Vector2(0.97f, 0.98f), game.Pause);
        Label("Instructions", hud.transform, "TOCA PARA SALTAR\nEspacio o clic en computadora", new Vector2(0.04f, 0.02f), new Vector2(0.65f, 0.13f), 26);
        ButtonAt("Saltar", hud.transform, new Vector2(0.7f, 0.03f), new Vector2(0.97f, 0.14f), game.jugador.RequestJump);
        records = Panel("Records", safeArea).gameObject;
        Image background = records.AddComponent<Image>();
        background.color = new Color(0.07f, 0.12f, 0.18f, 0.98f);
        recordsText = Label("Best Score", records.transform, "", new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.8f), 40);
        backButton = ButtonAt("Volver", safeArea, new Vector2(0.03f, 0.9f), new Vector2(0.3f, 0.98f), Back).gameObject;
        records.SetActive(false);
        backButton.SetActive(false);
        ApplySafeArea();
    }

    private RectTransform Panel(string label, Transform parent)
    {
        RectTransform rect = new GameObject(label, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private TMP_Text Label(string label, Transform parent, string content, Vector2 min, Vector2 max, float size)
    {
        RectTransform rect = Panel(label, parent);
        rect.anchorMin = min; rect.anchorMax = max;
        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = content;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14; text.fontSizeMax = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private Button ButtonAt(string label, Transform parent, Vector2 min, Vector2 max, UnityAction action)
    {
        RectTransform rect = Panel(label, parent);
        rect.anchorMin = min; rect.anchorMax = max;
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.13f, 0.32f, 0.45f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        Label("Label", rect, label, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), 30);
        return button;
    }

    private void LateUpdate() { if (safeArea != null) ApplySafeArea(); FitForest(); }
    private void ApplySafeArea()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        safeArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
    }
}

}
