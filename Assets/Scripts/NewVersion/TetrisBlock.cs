using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    normal,
    obstacle,
    oil
}
public class TetrisBlock : MonoBehaviour
{
    public Vector3 rotationPoint;
    private float previousTime;
    private int _height;
    private int _width;
    BoardManager board;
    private Transform ghostTileHolderTransform;
    GameObject ghostTile;
    private int toCheckHitPlayerAndOther = 0;

    //screenShake
    public float shakeDuration = 0.1f;
    public float shakeStrength = 0.3f;
    public int shakeVibrato = 0;
    public float shakeRandomness = 90f;

    public TileType tileType;

    private List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
        //player = FindObjectOfType<PlayerController>();
        board = BoardManager.instance;
        _width = board.grid.GetLength(0);
        _height = board.grid.GetLength(1);
        //ghostTile = Instantiate(gameObject);
        ChangeTileTypeBasedOnLevel(board.levelController.CurrentLevel());
        SetupTile(tileType);

    }
    private void Update()
    {
        //PrintCell();
        //SpawnGhostTile();
        Movement();

        // nếu nhấn key xuống thì rớt nhanh hơn gấp 10 lần
        Gravity();

    }

    public void ChangeTileTypeBasedOnLevel(int level)
    {
        switch (level)
        {
            case 0:
                tileType = TileType.normal;
                break;
            case 1:
                tileType = TileType.obstacle;
                break;
            case 2:
                tileType = TileType.oil;
                break;
        }
    }

    private void SetupTile(TileType type)
    {
        switch (type)
        {
            case TileType.normal:
                break;
            case TileType.obstacle:
                SetupObstacleTile();
                break;
            case TileType.oil:
                SetupOilTile();
                break;
        }
    }
    private void SetupObstacleTile()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name.Contains("Obstacle"))
            {
                obstacles.Add(child.gameObject);
            }
        }
        RandomObstacleAppear();
        Debug.Log(obstacles.Count);
    }
    private void RandomObstacleAppear()
    {
        float randomValue = Random.value;
        // tỷ lệ có bẫy
        if (randomValue < 0.45f)
            return;
        int numObjectsToActivate = Random.Range(1, 3);

        for (int i = 0; i < numObjectsToActivate; i++)
        {
            int randomIndex = Random.Range(0, obstacles.Count);
            // nếu đã tạo sẵn thì bỏ qua
            if (obstacles[randomIndex].activeInHierarchy)
            {
                continue;
            }

            // Activate the chosen object
            obstacles[randomIndex].SetActive(true);
        }
    }
    private void SetupOilTile()
    {

    }

    #region GRAVITY
    private void Gravity()
    {
        if (board.isWon) return;
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (IsPlayerUnder())
            {
                board.player.ChangePlayerMaterialColor();
                return;
            }
            while (ValidMove())
            {
                base.transform.position += new Vector3(0, -1, 0);
            }
            //check if it hit player
            
            //AddToGrid();

            if (!ValidMove())
            {
                ShiftDown();
                ShakeCamera();
            }
        }
        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? board.fallTime / 10 : board.fallTime))
        {
            foreach (Transform children in transform)
            {
                if (children.gameObject.activeInHierarchy)
                {
                    int roundedXDown = Mathf.RoundToInt(children.transform.position.x);
                    int roundedYDown = Mathf.RoundToInt(children.transform.position.y - 1);
                    if (roundedYDown >= 0)
                    {
                        if (toCheckHitPlayerAndOther >= 2)
                            break;
                        if (board.grid[roundedXDown, roundedYDown] == board.player.transform)
                        {
                            previousTime = Time.time;
                            toCheckHitPlayerAndOther++;
                            Debug.Log("RIHGT");
                            return;
                        }
                        //if (board.grid[roundedXDown, roundedYDown] != null)
                        //{
                        //    toCheckHitPlayerAndOther++;
                        //    previousTime = Time.time;

                        //    break;
                        //}

                    }
                }
            }
            base.transform.position += new Vector3(0, -1, 0);
            //AddToGrid();

            if (!ValidMove())
            {
                ShiftDown();
                ShakeCamera();
            }
            previousTime = Time.time;
        }
    }
    void AddToGrid()
    {
        if (board.isWon) return;
        foreach (Transform children in transform)
        {
            if (children.gameObject.activeInHierarchy)
            {
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);
                if (roundedY <= _height)
                {
                    if (roundedX < 0 || roundedX >= _width || roundedY < 0 || roundedY >= _height + 4)
                    {
                        board.canSpawn = false;
                        Debug.Log("GAMEOVER");
                        return;
                    }
                    if (board.grid[roundedX, roundedY] == null)
                    {
                        board.grid[roundedX, roundedY] = children;
                    }
                    else
                    {
                        Debug.Log("GameOVER");
                        board.canSpawn = false;
                        return;
                    }

                    board.grid[roundedX, roundedY] = children;
                }
               
            }

        }
    }
    #endregion
    #region MOVEMENT
    private void Movement()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            base.transform.position += new Vector3(1, 0, 0);
            if (!ValidMove())
            {
                base.transform.position -= new Vector3(1, 0, 0);
            }

        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            base.transform.position += new Vector3(-1, 0, 0);
            if (!ValidMove())
            {
                base.transform.position -= new Vector3(-1, 0, 0);
            }

        }
        // rotation movement
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            base.transform.RotateAround(base.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidMove())
                base.transform.RotateAround(base.transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
        }
    }
    #endregion

    #region COLLISION
    private void ShiftDown()
    {
        toCheckHitPlayerAndOther = 0;
        base.transform.position -= new Vector3(0, -1, 0);
        AddToGrid();
        CheckForLine();
        this.enabled = false;
        if (board.canSpawn)
        {
           FindObjectOfType<TetrisRandomizer>().SpawnNewTetromino();
        }
        if (base.transform.hierarchyCount == 0)
            Destroy(this.gameObject);
    }

    private void CheckForLine()
    {
        for (int i = _height - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
            }
        }
    }
    private bool HasLine(int i)
    {
        for (int j = 0; j < _width; j++)
        {
            if (board.grid[j, i] == null)
                return false;
        }
        return true;
    }
    private void DeleteLine(int i)
    {
        for (int j = 0; j < _width; j++)
        {
            if (board.grid[j, i] != board.player.transform)
            {
                Destroy(board.grid[j, i].gameObject);
                board.grid[j, i] = null;
            }
        }
    }
    private void RowDown(int i)
    {
        for (int y = i; y < _height; y++)
        {
            for (int j = 0; j < _width; j++)
            {
                if (board.grid[j, y] != null)
                {
                    board.grid[j, y - 1] = board.grid[j, y];
                    board.grid[j, y] = null;
                    board.grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }
    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            if (children.gameObject.activeInHierarchy)
            {
                int roundedX = Mathf.RoundToInt(children.transform.position.x);
                int roundedY = Mathf.RoundToInt(children.transform.position.y);
                if (roundedY < _height)
                {
                    if (roundedX < 0 || roundedX >= _width || roundedY < 0 || roundedY >= _height + 4)
                    return false;

                if (board.grid[roundedX, roundedY])
                    return false;
                }
                
            }
        }
        return true;
    }
    bool IsPlayerUnder()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                int roundedX = Mathf.RoundToInt(child.transform.position.x);
                int roundedY = Mathf.RoundToInt(child.transform.position.y);
                for (int i = 0; i < _height; i++)
                {
                    if (board.grid[roundedX, i] == board.player.transform)
                    {
                        Debug.Log("CO NGUOI CHOI");
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    public void ShakeCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Shake the camera using DOTween
            mainCamera.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness);
        }
    }

}