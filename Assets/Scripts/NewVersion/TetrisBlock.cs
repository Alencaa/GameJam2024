using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public Vector3 rotationPoint;
    private float previousTime;
    private int _height;
    private int _width;
    BoardManager board;
    private Transform ghostTileHolderTransform;
    GameObject ghostTile;

    void Start()
    {
        //player = FindObjectOfType<PlayerController>();
        board = BoardManager.instance;
        _width = board.grid.GetLength(0);
        _height = board.grid.GetLength(1);
        //ghostTile = Instantiate(gameObject);

    }
    private void Update()
    {
        //PrintCell();
        //SpawnGhostTile();
        Movement();

        // nếu nhấn key xuống thì rớt nhanh hơn gấp 10 lần
        Gravity();

    }

    private void Gravity()
    {
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
            }
        }
        if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? board.fallTime / 10 : board.fallTime))
        {
            foreach (Transform children in transform)
            {
                int roundedXDown = Mathf.RoundToInt(children.transform.position.x);
                int roundedYDown = Mathf.RoundToInt(children.transform.position.y - 1);
                if (roundedYDown >= 0)
                {
                    if (board.grid[roundedXDown, roundedYDown] != null && board.grid[roundedXDown, roundedYDown] != board.player.transform)
                    {
                        previousTime = Time.time;

                        break;
                    }
                    if (board.grid[roundedXDown, roundedYDown] == board.player.transform)
                    {
                        previousTime = Time.time;
                        Debug.Log("RIHGT");
                        return;
                    }
                }
            }
            base.transform.position += new Vector3(0, -1, 0);
            //AddToGrid();

            if (!ValidMove())
            {
                ShiftDown();
            }
            previousTime = Time.time;
        }
    }

    private void ShiftDown()
    {
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

    void AddToGrid()
    {

        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            if (roundedX < 0 || roundedX >= _width || roundedY < 0 || roundedY >= _height)
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
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedX < 0 || roundedX >= _width || roundedY < 0 || roundedY >= _height + 2)
                return false;

            if (board.grid[roundedX, roundedY])
                return false;

        }
        return true;
    }
    bool IsPlayerUnder()
    {
        foreach (Transform child in transform)
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
        return false;
    }
    private void SpawnGhostTile()
    {
        foreach (Transform child in ghostTileHolderTransform)
        {
            int roundedX = Mathf.RoundToInt(child.transform.position.x);
            for (int i = 0; i < _height; i++)
            {
                if (board.grid[roundedX, i] == null && ValidMove())
                {
                    child.position += new Vector3(0, -1, 0);
                }
                else
                {
                    child.position -= new Vector3(0, -1, 0);
                    Debug.Log("Dung lai");
                }
            }
        }
    }
    
    
}