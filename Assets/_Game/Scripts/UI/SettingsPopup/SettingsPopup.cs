using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JSAM;
using Redcode.Extensions;
using Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : UIScreen
{
    [Header("CONTENT PANEL")]
    [SerializeField] private RectTransform _contentPanel;
    [SerializeField] private Button _closeButton;

    [Header("SETTINGS PANEL")]
    [SerializeField] private SwitchButton _soundSwitch;
    [SerializeField] private SwitchButton _musicSwitch;
    
    #region UNITY METHODS

    private void OnEnable()
    {
        _closeButton.onClick.AddListener(OnCloseButtonClick);
        _soundSwitch.onClick.AddListener(OnSoundSwitchClick);
        _musicSwitch.onClick.AddListener(OnMusicSwitchClick);
    }

    private void OnDisable()
    {
        _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        _soundSwitch.onClick.RemoveListener(OnSoundSwitchClick);
        _musicSwitch.onClick.RemoveListener(OnMusicSwitchClick);
    }

    #endregion

    #region OVERRIDES

    public override void Show()
    {
        base.Show();
        _contentPanel.DOKill();
        _contentPanel.SetLocalScale(1);
        _contentPanel.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        _soundSwitch.IsOn = !AudioManager.SoundMuted;
        _musicSwitch.IsOn = !AudioManager.MusicMuted;
    }

    public override void Hide()
    {
        DataManager.Instance.Save();
        base.Hide();
    }

    #endregion

    #region EVENT LISTENERS

    private void OnCloseButtonClick()
    {
        AudioManager.PlaySound(ESound.Click);
        Hide();
        DataManager.Instance.Save();
    }

    private void OnSoundSwitchClick()
    {
        AudioManager.PlaySound(ESound.Click);
        AudioManager.SoundMuted = !AudioManager.SoundMuted;
        _soundSwitch.IsOn = !AudioManager.SoundMuted;
        DataManager.Instance.GameData.IsSoundOn = !AudioManager.SoundMuted;
    }

    private void OnMusicSwitchClick()
    {
        AudioManager.PlaySound(ESound.Click);
        AudioManager.MusicMuted = !AudioManager.MusicMuted;
        _musicSwitch.IsOn = !AudioManager.MusicMuted;
        DataManager.Instance.GameData.IsMusicOn = !AudioManager.MusicMuted;
    }

    #endregion
}