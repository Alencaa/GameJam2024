using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Toolkit.UI
{
    /// <summary>
    /// UIView included screen & popup
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        public virtual string Key => GetType().FullName;

        protected bool _isShowing;
        protected Canvas _canvas;
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas.enabled = false;
        }

        public virtual void Init()
        {
            _canvas.enabled = false;
            _isShowing = false;
        }

        public virtual void Show()
        {
            if (_isShowing) return;
            _isShowing = true;
            _canvas.enabled = true;
            _canvasGroup.interactable = true;
        }

        public virtual void Hide()
        {
            if(!_isShowing) return;
            _isShowing = false;
            _canvas.enabled = false;
            _canvasGroup.interactable = false;
        }

        public virtual void SetSortOrder(int order)
        {
            _canvas.sortingOrder = order;
        }
    }
}

