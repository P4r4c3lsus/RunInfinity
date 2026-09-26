using DG.Tweening;
using NaughtyAttributes;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _id;
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _hideOnStart;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    [SerializeField] private Ease showEase= Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    public CanvasGroup CanvasGroup => _canvasGroup;
    public RectTransform CanvasRectTransform => _canvasRectTransform;
    public string Id => _id;
    void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        if (_hideOnStart)
        {
            Hide(true);
        }
    }

    [Button]
    public virtual void Show(bool instant=false)
    {
        if (instant)
        {
            _canvasRectTransform.gameObject.SetActive(true);
        }
        else
        {
            _canvasRectTransform.gameObject.SetActive(true);
            RectTransform rectTransform = _canvasGroup.GetComponent<RectTransform>();
            rectTransform.DOScale(Vector3.one, showDuration).SetEase(showEase);
        }
    }

    [Button]
    public virtual void Hide(bool instant=false)
    {
        if (instant)
        {
            _canvasRectTransform.gameObject.SetActive(false);
        }
        else
        {
            RectTransform rectTransform = _canvasGroup.GetComponent<RectTransform>();
            rectTransform.DOScale(Vector3.zero, hideDuration).SetEase(hideEase).OnComplete(() =>
            {
                _canvasRectTransform.gameObject.SetActive(false);
            });
        }
    }
}
