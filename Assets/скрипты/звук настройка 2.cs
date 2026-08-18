using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private const string VOLUME_KEY = "Volume";

    void Start()
    {
        // Загружаем сохранённую громкость, если нет — ставим 0.5 по умолчанию
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.5f);

        slider.value = savedVolume;
        AudioListener.volume = savedVolume;

        slider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;

        // Сохраняем каждый раз когда меняем громкость
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(SetVolume);
    }
}