using DG.Tweening;
using NaughtyAttributes;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [SerializeField] private RectTransform _canvasRectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _hideOnStart;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    [SerializeField] private Ease showEase= Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        if (_hideOnStart)
        {
            Hide();
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
            RectTransform rectTransform = _canvasGroup.GetComponent<RectTransform>();
            rectTransform.DOScale(Vector3.one, duration:0.5f).SetEase(Ease.OutBack);
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
            rectTransform.DOScale(Vector3.zero, duration: 0.5f).SetEase(Ease.InBack);
        }
    }
}
