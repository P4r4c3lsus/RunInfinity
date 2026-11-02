#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RunInfinity.Adicional;

namespace RunInfinity.Adicional.Editor
{
    public static class RIInstaller
    {
        private static readonly string[] WindowIds = { "mainmenuui", "pauseui", "gameoverui", "settingsui", "gamecreditsui", "popui" };
        private static readonly string[,] ButtonNames = {
            { "mainmenuui", "LongButton_Blue_1" }, { "mainmenuui", "Settings" },
            { "mainmenuui", "LongButton_Orange_1" }, { "mainmenuui", "LongButton_Red_Pressed_1" },
            { "pauseui", "LongButton_Blue_1" }, { "pauseui", "Pause_Button" },
            { "pauseui", "LongButton_Green_1" }, { "pauseui", "LongButton_Red_1" },
            { "gameoverui", "LongButton_Blue_1" }, { "gameoverui", "LongButton_Red_1" },
            { "settingsui", "CreditsButton" }, { "gamecreditsui", "LongButton_White_3" },
            { "popui", "Yes_Button" }, { "popui", "No_Button" }
        };

        [MenuItem("RunInfinity/Instalar gameplay adicional")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Message("Sal del modo Play primero."); return; }
            Scene scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path)) { Message("Guarda y abre tu SampleScene antes de instalar."); return; }
            List<GameObject> objects = ObjectsIn(scene);
            GameObject runner = Named(objects, "Runner");
            GameObject ground = Named(objects, "Ground");
            GameObject obstacle = Named(objects, "Obstaculo");
            Camera camera = null;
            foreach (GameObject go in objects)
                if (go.CompareTag("MainCamera")) { camera = go.GetComponent<Camera>(); if (camera != null) break; }
            if (runner == null || ground == null || camera == null)
            { Message("Necesito Runner, Ground y una camara con tag MainCamera en la escena actual."); return; }
            if (runner.GetComponent<Rigidbody2D>() == null || runner.GetComponent<BoxCollider2D>() == null ||
                ground.GetComponent<BoxCollider2D>() == null)
            { Message("Runner necesita Rigidbody2D y BoxCollider2D. Ground necesita BoxCollider2D."); return; }
            if (ground.GetComponent<BoxCollider2D>().isTrigger)
            { Message("Desmarca Is Trigger en Ground y vuelve a instalar."); return; }
            if (runner.GetComponent<SpriteRenderer>() == null || runner.GetComponent<SpriteRenderer>().sprite == null)
            { Message("Asigna un sprite a Runner."); return; }

            var windows = new Dictionary<string, RIUI.Window>();
            var legacy = new List<MonoBehaviour>();
            foreach (GameObject go in objects)
            {
                foreach (MonoBehaviour component in go.GetComponents<MonoBehaviour>())
                {
                    if (component == null) continue;
                    string name = component.GetType().Name;
                    if (name == "InputManager" || name == "PlayerInput" || name == "SaltoRunner" ||
                        name == "MoverObstaculo" || name == "Player" || name == "AudioManager") legacy.Add(component);
                    if (!IsWindow(component)) continue;
                    SerializedObject data = new SerializedObject(component);
                    SerializedProperty id = data.FindProperty("_id");
                    SerializedProperty canvas = data.FindProperty("_canvasRectTransform");
                    SerializedProperty group = data.FindProperty("_canvasGroup");
                    if (id == null || canvas == null) continue;
                    RectTransform rect = canvas.objectReferenceValue as RectTransform;
                    if (rect == null) continue;
                    if (windows.ContainsKey(id.stringValue)) { Message("Hay ventanas con Id duplicado: " + id.stringValue); return; }
                    windows[id.stringValue] = new RIUI.Window {
                        id = id.stringValue, root = rect.gameObject,
                        group = group == null ? null : group.objectReferenceValue as CanvasGroup
                    };
                    legacy.Add(component);
                }
            }
            foreach (string id in WindowIds)
                if (!windows.ContainsKey(id)) { Message("No encontre la ventana " + id + ". Revisa su Id y Canvas Rect Transform."); return; }
            for (int i = 0; i < ButtonNames.GetLength(0); i++)
            {
                bool found = false;
                foreach (Button button in windows[ButtonNames[i, 0]].root.GetComponentsInChildren<Button>(true))
                    if (button.name.Trim() == ButtonNames[i, 1]) found = true;
                if (!found) { Message("No encontre el boton " + ButtonNames[i, 0] + "/" + ButtonNames[i, 1] + ". No cambie la escena."); return; }
            }
            if (windows["settingsui"].root.GetComponentInChildren<Slider>(true) == null)
            { Message("No encontre el slider de volumen de SettingsUI."); return; }
            bool eventSystem = false;
            foreach (GameObject go in objects)
                if (go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) eventSystem = true;
            if (!eventSystem) { Message("La escena necesita un EventSystem activo para sus botones."); return; }

            // Solo se realizan cambios despues de validar todos los objetos y botones.
            Undo.IncrementCurrentGroup();
            int undo = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Instalar gameplay adicional RunInfinity");
            RIGame game = null;
            foreach (GameObject go in objects)
                if (go.GetComponent<RIGame>() != null) { game = go.GetComponent<RIGame>(); break; }
            if (game == null)
            {
                GameObject root = new GameObject("RunInfinity - Gameplay adicional");
                Undo.RegisterCreatedObjectUndo(root, "Crear controlador");
                game = Undo.AddComponent<RIGame>(root);
            }
            Undo.RecordObject(game, "Asignar referencias");
            RIUI ui = game.GetComponent<RIUI>();
            if (ui == null) ui = Undo.AddComponent<RIUI>(game.gameObject);
            RIAudio audio = game.GetComponent<RIAudio>();
            if (audio == null) audio = Undo.AddComponent<RIAudio>(game.gameObject);
            RIPlayer player = runner.GetComponent<RIPlayer>();
            if (player == null) player = Undo.AddComponent<RIPlayer>(runner);
            Undo.RecordObject(ui, "Asignar ventanas");
            Undo.RecordObject(player, "Asignar suelo");
            var list = new List<RIUI.Window>();
            foreach (string id in WindowIds) list.Add(windows[id]);
            ui.ventanas = list.ToArray();
            player.suelo = ground.GetComponent<BoxCollider2D>();
            game.jugador = player; game.suelo = ground.GetComponent<BoxCollider2D>();
            game.camara = camera; game.interfaz = ui; game.sonido = audio;
            game.componentesDePractica = legacy.ToArray();
            game.obstaculoDePrueba = obstacle;

            if (game.plantillaObstaculo == null)
            {
                GameObject template = new GameObject("Plantilla de obstaculo (inactiva)");
                Undo.RegisterCreatedObjectUndo(template, "Crear plantilla");
                template.transform.SetParent(game.transform, false);
                template.transform.localScale = new Vector3(0.7f, 1f, 1f);
                SpriteRenderer image = template.AddComponent<SpriteRenderer>();
                SpriteRenderer original = obstacle == null ? null : obstacle.GetComponent<SpriteRenderer>();
                image.sprite = original != null && original.sprite != null ? original.sprite : runner.GetComponent<SpriteRenderer>().sprite;
                image.color = original != null ? original.color : new Color(0.85f, 0.2f, 0.2f);
                BoxCollider2D box = template.AddComponent<BoxCollider2D>();
                box.size = Vector2.one; box.offset = Vector2.zero; box.isTrigger = true;
                Rigidbody2D body = template.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0;
                template.AddComponent<RIObstacle>();
                template.SetActive(false);
                game.plantillaObstaculo = template;
            }
            EditorUtility.SetDirty(game); EditorUtility.SetDirty(ui); EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undo);
            Selection.activeGameObject = game.gameObject;
            Message("Instalacion terminada. Guarda con Ctrl+S, pulsa Play en Unity y Play en tu menu.\n\nNo se reemplazo ningun script original. Los componentes de practica se desactivan solamente durante Play.");
        }

        [MenuItem("RunInfinity/Poner escena actual primero en Build Settings")]
        public static void SetStartupScene()
        {
            string path = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path)) { Message("Guarda primero la escena."); return; }
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(path, true) };
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.path != path) scenes.Add(scene);
            EditorBuildSettings.scenes = scenes.ToArray();
            Message("Escena inicial: " + path + ". Revisa tambien Scene List si tu Build Profile utiliza una lista propia.");
        }

        private static bool IsWindow(MonoBehaviour component)
        {
            System.Type type = component.GetType();
            while (type != null) { if (type.Name == "UIWindow") return true; type = type.BaseType; }
            return false;
        }
        private static List<GameObject> ObjectsIn(Scene scene)
        {
            var result = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) result.Add(child.gameObject);
            return result;
        }
        private static GameObject Named(List<GameObject> objects, string name)
        {
            foreach (GameObject go in objects) if (go.name == name) return go;
            return null;
        }
        private static void Message(string text) { EditorUtility.DisplayDialog("RunInfinity", text, "Aceptar"); }
    }
}
#endif
