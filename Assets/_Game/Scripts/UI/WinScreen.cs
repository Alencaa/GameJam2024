using Cysharp.Threading.Tasks;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

public class WinScreen : UIScreen
{
    [SerializeField] private Button _homeButton;

    private void Start()
    {
        _homeButton.onClick.AddListener(OnHomeButtonClick);
    }
    
    private void OnHomeButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Hide();
        SceneLoader.Instance.LoadScene(Define.SceneName.TITLE).Forget();
        UIManager.Instance.GetScreen<TitleScreen>().Show();
    }
}
