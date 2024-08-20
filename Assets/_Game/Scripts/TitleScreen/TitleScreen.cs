using Cysharp.Threading.Tasks;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : UIScreen
{
    [Header("UI OBJECTS")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _startButton.onClick.AddListener(OnStartButtonClick);
        _settingsButton.onClick.AddListener(OnSettingsButtonClick);
        _quitButton.onClick.AddListener(OnQuitButtonClick);
    }

    private void OnStartButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Hide();
        SceneLoader.Instance.LoadScene(Define.SceneName.GAME).Forget();
    }
    
    private void OnSettingsButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        UIManager.Instance.GetScreen<SettingsPopup>().Show();;
    }

    private void OnQuitButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Application.Quit();
    }
}