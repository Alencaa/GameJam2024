using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
    [SerializeField] private bool _isOn;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;

    [HideInInspector] public UnityEvent onClick = new();

    private Image _image;
    private Button _button;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public bool IsOn
    {
        get => _isOn;
        set
        {
            _isOn = value;
            _image.sprite = _isOn ? _onSprite : _offSprite;
        }
    }

    private void OnClick()
    {
        //AudioManager.PlaySound(Sounds.Click);
        onClick?.Invoke();
    }
}