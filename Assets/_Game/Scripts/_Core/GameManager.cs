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
        // Wait for all controllers complete init
        await UniTask.DelayFrame(5);
        await SceneLoader.Instance.LoadScene(Define.SceneName.TITLE);
    }
}
