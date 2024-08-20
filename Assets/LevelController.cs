using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public int startingLevelIndex = 0;
    public int currentLevelIndex;

    private void Awake()
    {
        currentLevelIndex = 0;
    }
    public void IncreaseLevel()
    {
        currentLevelIndex++;
    }
    public int CurrentLevel()
    {
        return currentLevelIndex;
    }
}
