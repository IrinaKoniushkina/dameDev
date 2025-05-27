using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SchemeViewer : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Canvas canvas;
    public Image smallSchemeImage;
    public Image fullSchemeImage;
    public GameObject schemePanel;
    public Button closeFullSchemeButton;
    public RectTransform schemeTargetPosition;

    [Header("Settings")]
    public float smallSchemeWidth = 300f;
    public float panelExpandedWidth = 350f;
    public float moveSpeed = 500f; // Скорость перемещения в пикселях/сек

    private bool hasSeenScheme = false;
    private RectTransform panelRect;
    private Vector2 originalPanelSize;
    private Vector3 originalSchemePosition;
    private bool isMoving = false;
    private RectTransform movingScheme;

    void Start()
    {
        panelRect = schemePanel.GetComponent<RectTransform>();
        originalPanelSize = panelRect.sizeDelta;
        originalSchemePosition = smallSchemeImage.rectTransform.localPosition;

        if (closeFullSchemeButton != null)
        {
            closeFullSchemeButton.onClick.AddListener(HideFullScheme);
        }

        smallSchemeImage.gameObject.SetActive(false);
        fullSchemeImage.gameObject.SetActive(false);
        schemePanel.SetActive(false);
    }

    void Update()
    {
        if (isMoving && movingScheme != null)
        {
            // Плавное перемещение к целевой позиции
            movingScheme.localPosition = Vector3.MoveTowards(
                movingScheme.localPosition, 
                schemeTargetPosition.localPosition, 
                moveSpeed * Time.deltaTime
            );

            // Проверка достижения цели
            if (Vector3.Distance(movingScheme.localPosition, schemeTargetPosition.localPosition) < 1f)
            {
                movingScheme.localPosition = schemeTargetPosition.localPosition;
                isMoving = false;
                movingScheme = null;
            }
        }
    }

    public void OnSchemeFound(Transform worldScheme)
    {
        if (hasSeenScheme) return;
        
        hasSeenScheme = true;
        schemePanel.SetActive(true);

        // Переносим схему из мира на канвас
        Image worldImage = worldScheme.GetComponent<Image>();
        if (worldImage != null)
        {
            // Настраиваем схему перед перемещением
            smallSchemeImage.sprite = worldImage.sprite;
            smallSchemeImage.gameObject.SetActive(true);
            smallSchemeImage.rectTransform.localPosition = worldScheme.localPosition;
            
            // Начинаем перемещение
            movingScheme = smallSchemeImage.rectTransform;
            isMoving = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasSeenScheme || isMoving) return;

        if (eventData.pointerCurrentRaycast.gameObject == smallSchemeImage.gameObject)
        {
            ShowFullScheme();
        }
    }

    private void ShowFullScheme()
    {
        fullSchemeImage.sprite = smallSchemeImage.sprite;
        fullSchemeImage.gameObject.SetActive(true);
        smallSchemeImage.gameObject.SetActive(false);
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
    }

    private void HideFullScheme()
    {
        fullSchemeImage.gameObject.SetActive(false);
        smallSchemeImage.gameObject.SetActive(true);
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
    }

    public void ExpandPanel()
    {
        if (hasSeenScheme && !isMoving)
        {
            panelRect.sizeDelta = new Vector2(panelExpandedWidth, originalPanelSize.y);
            smallSchemeImage.rectTransform.sizeDelta = new Vector2(smallSchemeWidth, smallSchemeWidth);
        }
    }

    public void CollapsePanel()
    {
        panelRect.sizeDelta = originalPanelSize;
        smallSchemeImage.rectTransform.sizeDelta = originalPanelSize;
    }
}