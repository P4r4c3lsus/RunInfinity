using Dino.Utility.Audio;
using UnityEngine;
using UnityEngine.UI;
using AudioType = Dino.Utility.Audio.AudioType;

public class SettingsUI : UIWindow
{
    [Header("SettingsUI")]
    [SerializeField] private Slider _volumeMusicSlider;

    public override void Initialize()
    {
        base.Initialize();
        _volumeMusicSlider.onValueChanged.AddListener(OnVolumeMusicChange);
    }

    private void OnVolumeMusicChange(float volume)
    {
        Debug.Log($"Volume changed to: {volume}");
        AudioManager.Instance.UpdateAudioMixerGroupVolume(volume, Dino.Utility.Audio.AudioType.Music);
    }
}
