#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RunInfinity.Adicional.Editor
{
    public static class RIGooglePlayBuild
    {
        [MenuItem("RunInfinity/Google Play/Comprobar configuracion")]
        public static void Check()
        {
            var problems = Problems();
            EditorUtility.DisplayDialog("HobbitRun - Google Play", problems.Count == 0
                ? "Configuracion lista para exportar un AAB firmado."
                : string.Join("\n\n", problems), "Aceptar");
        }

        [MenuItem("RunInfinity/Google Play/Exportar AAB firmado")]
        public static void Export()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("HobbitRun", "Sal del modo Play antes de exportar.", "Aceptar");
                return;
            }
            var problems = Problems();
            if (problems.Count != 0)
            {
                EditorUtility.DisplayDialog("Falta completar la exportacion", string.Join("\n\n", problems), "Aceptar");
                return;
            }
            string directory = Path.GetFullPath("build/GooglePlay");
            Directory.CreateDirectory(directory);
            string output = EditorUtility.SaveFilePanel("Exportar HobbitRun para Google Play", directory,
                "HobbitRun-" + PlayerSettings.bundleVersion + "-" + PlayerSettings.Android.bundleVersionCode, "aab");
            if (string.IsNullOrEmpty(output)) return;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.development = false;
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled) scenes.Add(scene.path);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                target = BuildTarget.Android, scenes = scenes.ToArray(), locationPathName = output,
                options = BuildOptions.DetailedBuildReport | BuildOptions.CompressWithLz4HC
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("HobbitRun", "La compilacion fallo. Revisa la consola de Unity.", "Aceptar");
                return;
            }
            EditorUtility.RevealInFinder(output);
            Debug.Log("HobbitRun: AAB firmado exportado a " + output);
        }

        // Never log passwords, key contents or credential values.
        public static List<string> Problems()
        {
            var problems = new List<string>();
            var android = NamedBuildTarget.Android;
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                problems.Add("Activa Android en File > Build Profiles.");
            if (PlayerSettings.GetApplicationIdentifier(android) != "com.dbombstudios.hobbitrun")
                problems.Add("El identificador debe ser com.dbombstudios.hobbitrun para esta app nueva.");
            if (PlayerSettings.GetScriptingBackend(android) != ScriptingImplementation.IL2CPP ||
                (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
                problems.Add("Configura IL2CPP y ARM64 en Player > Other Settings.");
            if ((int)PlayerSettings.Android.targetSdkVersion < 36)
                problems.Add("Configura Target API Level en Android 16 / API 36 o superior.");
            bool sceneFound = false;
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled && scene.path == "Assets/Scenes/SampleScene.unity") sceneFound = true;
            if (!sceneFound) problems.Add("Incluye y activa Assets/Scenes/SampleScene.unity en Scene List.");
            if (!PlayerSettings.Android.useCustomKeystore || string.IsNullOrEmpty(PlayerSettings.Android.keystoreName))
                problems.Add("Crea o carga la clave de subida en Player > Android > Publishing Settings > Keystore Manager.");
            else if (string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName) ||
                string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
                string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
                problems.Add("Selecciona el alias e introduce las dos contrasenas en Publishing Settings. Unity no guarda las contrasenas entre sesiones.");
            return problems;
        }
    }
}
#endif
