using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TitleScreenView : MonoBehaviour
{
    [Header("UI OBJECTS")]
    [SerializeField] private Button _startButton;
    
    // Events
    [HideInInspector] public UnityEvent OnStartButtonClick;

    private void Start()
    {
        _startButton.onClick.AddListener(() => OnStartButtonClick?.Invoke());
    }
}