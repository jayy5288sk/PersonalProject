using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettingsHUD : MonoBehaviour
{
    private const string BGM_VOLUME_PARAM = "BGMVolume";   // Mixer Exposed Parameter 이름이자 저장 키
    private const string SFX_VOLUME_PARAM = "SFXVolume";
    private const float MIN_VOLUME = 0.0001f;              // Log10(0) = 음수 무한 방지
    private const float DEFAULT_VOLUME = 1f;
    private const float DECIBEL_MULTIPLIER = 20f;          // 선형 값 → dB 변환 계수

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Button _openButton;
    [SerializeField] private Button _closeButton;

    // AudioMixer.SetFloat은 Awake에서 호출하면 적용되지 않으므로 Start에서 불러옴
    private void Start() => LoadVolumes();

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _openButton.onClick.AddListener(HandleOpenClicked);
        _closeButton.onClick.AddListener(HandleCloseClicked);
        _bgmSlider.onValueChanged.AddListener(HandleBgmChanged);
        _sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
    }

    private void UnbindEvents()
    {
        _openButton.onClick.RemoveListener(HandleOpenClicked);
        _closeButton.onClick.RemoveListener(HandleCloseClicked);
        _bgmSlider.onValueChanged.RemoveListener(HandleBgmChanged);
        _sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
    }

    // 저장값을 슬라이더와 Mixer에 반영. 슬라이더 이벤트가 다시 저장하지 않도록 WithoutNotify 사용
    private void LoadVolumes()
    {
        _settingsPanel.SetActive(false);

        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_PARAM, DEFAULT_VOLUME);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_PARAM, DEFAULT_VOLUME);

        _bgmSlider.SetValueWithoutNotify(bgmVolume);
        _sfxSlider.SetValueWithoutNotify(sfxVolume);

        ApplyVolume(BGM_VOLUME_PARAM, bgmVolume);
        ApplyVolume(SFX_VOLUME_PARAM, sfxVolume);
    }

    private void HandleOpenClicked()
    {
        _settingsPanel.SetActive(true);
    }

    // 닫을 때 디스크에 기록
    private void HandleCloseClicked()
    {
        PlayerPrefs.Save();
        _settingsPanel.SetActive(false);
    }

    private void HandleBgmChanged(float value)
    {
        ChangeVolume(BGM_VOLUME_PARAM, value);
    }

    private void HandleSfxChanged(float value)
    {
        ChangeVolume(SFX_VOLUME_PARAM, value);
    }

    private void ChangeVolume(string param, float value)
    {
        ApplyVolume(param, value);
        PlayerPrefs.SetFloat(param, value);
    }

    // Mixer는 dB(로그) 단위라 선형 슬라이더 값을 변환해 넣음 (1 → 0dB, 0.0001 → -80dB)
    private void ApplyVolume(string param, float value)
    {
        float clampedValue = Mathf.Max(value, MIN_VOLUME);
        _audioMixer.SetFloat(param, Mathf.Log10(clampedValue) * DECIBEL_MULTIPLIER);
    }
}
