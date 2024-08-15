using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMono<SceneLoader>
{
    public async UniTask LoadScene(string sceneName, float delayTime = 0.3f,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single, Action onComplete = null)
    {
        await UniTask.Yield();
        var async = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        if (async != null)
        {
            async.allowSceneActivation = false;
            while (async.progress < 0.9f)
            {
                await UniTask.Yield();
            }
            await UniTask.Yield();
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime), DelayType.DeltaTime);
            async.allowSceneActivation = true;
        }
        await UniTask.Yield();
        onComplete?.Invoke();
        await UniTask.CompletedTask;
    }

    public async UniTask UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            var async = SceneManager.UnloadSceneAsync(scene);
            if (async != null)
            {
                while (async.progress < 0.9f)
                {
                    await UniTask.Yield();
                }
            }
        }
    }
}