using System.Collections.Generic;
using System.Linq;
using Toolkit.UI;
using UnityEngine;

public class UIManager : SingletonMono<UIManager>
{
    [SerializeField] private List<UIView> _showOnStartViews;
    [SerializeField] private List<UIView> _views;
    private readonly Dictionary<string, UIView> _viewsDic = new();

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        foreach (var view in _views)
        {
            _viewsDic.Add(view.Key, view);
            view.Init();
        }
        
        foreach (var view in _showOnStartViews)
        {
            view.Show();
        }
    }

    public T GetView<T>() where T : UIView
    {
        var key = typeof(T).FullName;
        return _viewsDic.ContainsKey(key) ? _viewsDic[key] as T : null;
    }

    public void CloseAllViews()
    {
        foreach (var view in _views)
        {
            view.Hide();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _views = gameObject.GetComponentsInChildren<UIView>().ToList();
    }
#endif
}