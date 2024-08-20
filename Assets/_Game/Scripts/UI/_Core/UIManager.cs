using System.Collections.Generic;
using System.Linq;
using Toolkit.UI;
using UnityEngine;

public class UIManager : SingletonMono<UIManager>
{
    [SerializeField] private List<UIScreen> _showOnStartViews;
    [SerializeField] private List<UIScreen> _views;
    private readonly Dictionary<string, UIScreen> _viewsDic = new();
    
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

    public T GetScreen<T>() where T : UIScreen
    {
        var key = typeof(T).FullName;
        return _viewsDic.TryGetValue(key, out var value) ? value as T : null;
    }

    public void HideAllScreens()
    {
        foreach (var view in _views)
        {
            view.Hide();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _views = gameObject.GetComponentsInChildren<UIScreen>().ToList();
    }
#endif
}