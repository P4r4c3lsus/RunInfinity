using System.Collections.Generic;
using UnityEngine;

namespace RunInfinity.Adicional
{
    public sealed class RIAudio : MonoBehaviour
    {
        [Header("Opcionales: arrastra tus clips. Si estan vacios se usa audio provisional.")]
        public AudioClip musicaMenu;
        public AudioClip musicaJuego;
        public AudioClip sonidoSalto;
        public AudioClip sonidoDerrota;
        public AudioClip sonidoBoton;
        private AudioSource musica, efectos;
        private readonly List<AudioClip> generados = new List<AudioClip>();
        public float Volume => PlayerPrefs.GetFloat("RI.Volume", 0.7f);

        public void Initialize()
        {
            musica = gameObject.AddComponent<AudioSource>();
            efectos = gameObject.AddComponent<AudioSource>();
            musica.playOnAwake = efectos.playOnAwake = false;
            musica.spatialBlend = efectos.spatialBlend = 0;
            musica.loop = true;
            musica.volume = 0.16f; efectos.volume = 0.35f;
            if (musicaMenu == null) musicaMenu = Notes("Menu", new float[] { 261.63f, 329.63f, 392, 329.63f }, 0.5f);
            if (musicaJuego == null) musicaJuego = Notes("Carrera", new float[] { 261.63f, 329.63f, 392, 523.25f, 440, 392, 329.63f, 293.66f }, 0.25f);
            if (sonidoSalto == null) sonidoSalto = Notes("Salto", new float[] { 440, 660 }, 0.07f);
            if (sonidoDerrota == null) sonidoDerrota = Notes("Derrota", new float[] { 392, 293.66f, 196 }, 0.15f);
            if (sonidoBoton == null) sonidoBoton = Notes("Boton", new float[] { 600 }, 0.04f);
            AudioListener.volume = Mathf.Clamp01(Volume);
        }

        private AudioClip Notes(string name, float[] frequencies, float duration)
        {
            const int rate = 22050;
            int length = Mathf.RoundToInt(rate * duration);
            float[] samples = new float[length * frequencies.Length];
            for (int note = 0; note < frequencies.Length; note++)
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / rate;
                    float fade = Mathf.Min(1, t / 0.01f) * Mathf.Clamp01((duration - t) / 0.04f);
                    samples[note * length + i] = Mathf.Sin(2 * Mathf.PI * frequencies[note] * t) * fade;
                }
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, rate, false);
            clip.SetData(samples, 0); generados.Add(clip);
            return clip;
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("RI.Volume", AudioListener.volume);
            PlayClick();
        }
        public void PlayMenu() { efectos.Stop(); musica.clip = musicaMenu; musica.Play(); }
        public void PlayGame() { efectos.Stop(); musica.clip = musicaJuego; musica.Play(); }
        public void PauseGame() { musica.Pause(); efectos.Stop(); }
        public void ResumeGame() { musica.UnPause(); }
        public void PlayJump() { efectos.PlayOneShot(sonidoSalto); }
        public void PlayLose() { musica.Stop(); efectos.PlayOneShot(sonidoDerrota); }
        public void PlayClick() { if (efectos != null) efectos.PlayOneShot(sonidoBoton); }
        private void OnApplicationQuit() { PlayerPrefs.Save(); }
        private void OnDestroy() { foreach (AudioClip clip in generados) Destroy(clip); }
    }
}
