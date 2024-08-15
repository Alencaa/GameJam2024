using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private TitleScreenView _view;

    private void Start()
    {
        _view.OnStartButtonClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        SceneLoader.Instance.LoadScene(Define.SceneName.GAME).Forget();
    }
}
