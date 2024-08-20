using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

public class ReplayPopup : UIScreen
{
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _replayButton;

    private void Start()
    {
        _homeButton.onClick.AddListener(OnHomeButtonClick);
        _replayButton.onClick.AddListener(OnReplayButtonClick);
    }
    
    private void OnHomeButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Hide();
        SceneLoader.Instance.LoadScene(Define.SceneName.TITLE).Forget();
    }
    
    private void OnReplayButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Hide();
        SceneLoader.Instance.LoadScene(Define.SceneName.GAME).Forget();
    }
}
