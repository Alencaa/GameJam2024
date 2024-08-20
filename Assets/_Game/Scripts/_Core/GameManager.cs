using System;
using Cysharp.Threading.Tasks;

public class GameManager : SingletonMono<GameManager>
{
    private void Start()
    {
        InitializeGame().Forget();
    }

    private async UniTaskVoid InitializeGame()
    {
        await UniTask.Yield();
        // Wait for all controllers complete init
        UIManager.Instance.Initialize();
        await UniTask.DelayFrame(5);
        await SceneLoader.Instance.LoadScene(Define.SceneName.TITLE);
        UIManager.Instance.GetScreen<TitleScreen>().Show();
    }
}
