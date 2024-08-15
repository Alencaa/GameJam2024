using UnityEngine;

public class DataManager : SingletonMono<DataManager>
{
    [SerializeField] private GameData _gameData;
    
    private const string DATA_KEY = "GAME_DATA";
    public GameData GameData => _gameData;

    private void Start() => Init();

    public void Init()
    {
        if (PlayerPrefs.HasKey(DATA_KEY))
        {
            UpdateData();
        }
        else
        {
            CreateNewData();
        }
    }
    
    public void UpdateData()
    {
        var dataString = PlayerPrefs.GetString(DATA_KEY);
        var oldData = JsonUtility.FromJson<GameData>(dataString);
        _gameData = oldData;
    }

    public void CreateNewData()
    {
        _gameData = new();
    }

    public void Save()
    {
        var dataString = JsonUtility.ToJson(_gameData);
        PlayerPrefs.SetString(DATA_KEY, dataString);
    }
}