using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TetrisRandomizer : MonoBehaviour
{
    public int bagSize = 7;
    public List<GameObject> tetrominoes;

    private List<GameObject> currentBag;
    private int currentPieceIndex = 0;

    public Transform blockHolder;
    private BoardManager boardManager;

    private void Start()
    {
        InitializeBag();
        SpawnNewTetromino();
        boardManager = BoardManager.instance;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && validMove(new Vector3(1, 0, 0)))
        {
            transform.position += new Vector3(1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && validMove(new Vector3(-1, 0, 0)))
        {
            transform.position += new Vector3(-1, 0, 0);
        }
    }
    private bool validMove(Vector2 direction)
    {
        int roundedX = Mathf.RoundToInt(transform.position.x + direction.x);
        int roundedY = Mathf.RoundToInt(transform.position.y + direction.y);

        if (roundedX < 3 || roundedX >= boardManager.grid.GetLength(0) - 3)
            return false;

        if (boardManager.grid[roundedX, roundedY] != null)
            return false;
        return true;
    }
    // Get the next tetromino from the bag
    public GameObject GetNextTetroMino()
    {
        if (currentPieceIndex >= bagSize)
        {
            InitializeBag();
            currentPieceIndex = 0;
        }

        GameObject nextPiece = currentBag[currentPieceIndex];
        currentPieceIndex++;
        //UpdateNextPieces();
        return nextPiece;
    }

    private void InitializeBag()
    {
        currentBag = tetrominoes.OrderBy(x => Random.value).ToList();
    }
    //private void UpdateNextPieces()
    //{
    //    int nextPiece2Index = (currentPieceIndex + 1) % bagSize;
    //    GameObject nextPiece2 = Instantiate(currentBag[nextPiece2Index], blockHolder.position, Quaternion.identity);
    //}
    public GameObject SpawnNewTetromino()
    {
        GameObject tetromino = Instantiate(GetNextTetroMino(), transform.position, Quaternion.identity);
        tetromino.transform.SetParent(blockHolder);
        return tetromino;
    }

}
