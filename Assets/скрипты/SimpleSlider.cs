using UnityEngine;
using UnityEngine.UI;

public class SimpleSlider : MonoBehaviour
{
    [SerializeField] RectTransform content;   // контейнер со слайдами
    [SerializeField] float slideWidth = 1080f;

    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject prevButton;

    int currentIndex = 0;
    int maxIndex;

    void Start()
    {
        maxIndex = content.childCount - 1;
        UpdateSlidePosition();
        UpdateButtons();
    }

    public void NextSlide()
    {
        if (currentIndex >= maxIndex) return;

        currentIndex++;
        UpdateSlidePosition();
        UpdateButtons();
    }

    public void PrevSlide()
    {
        if (currentIndex <= 0) return;

        currentIndex--;
        UpdateSlidePosition();
        UpdateButtons();
    }

    void UpdateSlidePosition()
    {
        content.anchoredPosition = new Vector2(-currentIndex * slideWidth, content.anchoredPosition.y);
    }

    void UpdateButtons()
    {
        // Кнопка "назад" скрыта на первом слайде
        prevButton.SetActive(currentIndex > 0);

        // Кнопка "вперёд" скрыта на последнем слайде
        nextButton.SetActive(currentIndex < maxIndex);
    }
}