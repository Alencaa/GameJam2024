using DG.Tweening;
using System.Collections;
using UnityEngine;

public enum MoveDirection
{
    Right,
    Up,
    Down,
    Left,
}
public class PlayerController : MonoBehaviour
{
    private BoardManager boardManager;
    private Rigidbody2D rb;

    private int width;
    private int height;

    //Jumping
    private Vector3 lastPosition;
    private float previousTime;
    public float fallTime = 0.4f;
    private bool canFall = true;
    private int heigthJumped = 0;
    public int jumpHeight = 3;
    public bool canJump = true;
    private int maxHorizontalMoveInAir = 3;
    private int horizontalMovedInAir = 0;

    //Moving
    private Vector3 targetPosition;
    private bool isMoving = false;
    float movementDuration = 0.05f;
    public float moveSpeed = 1f;
    public MoveDirection moveDirection;

    //Input buffer
    private Vector3 queuedInput = Vector3.zero;

    private SpriteRenderer spriteRenderer;
    private Color32 originalMaterialColor;

    Transform spriteTransform;

    public int heightClimbed = 0;

    private PlayerAnimationController animController;
    private bool animPlayed = false;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    public bool hasLanded;

    private int facingDir = 1;
    private bool facingRight = true;

    private bool isWon = false;


    void Start()
    {
        spriteTransform = transform.Find("Animator");
        animController = spriteTransform.GetComponent<PlayerAnimationController>();
        boardManager = BoardManager.instance;
        spriteRenderer = spriteTransform.gameObject.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); 

        width = boardManager.grid.GetLength(0);
        height = boardManager.grid.GetLength(1);

        // Set the player's initial position on the grid
        boardManager.grid[(int)RoundedPos(transform.position).x, (int)RoundedPos(transform.position).y] = this.transform;
        originalMaterialColor = spriteRenderer.material.color;

        animController.animator.Play("capyIdle", 0, 0);
    }
    private void Update()
    {
        if (!isWon)
        {
            addToGrid(lastPosition);

        }
        //prevent spawning block when the player can jump to victory, ensuring the player has touch the ground
        if (transform.position.y >= height - jumpHeight -3 && canJump && !isWon)
        {
            WIN();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            WIN();
        }
        HorizontalMovement();
        JumpAndFallMovement();
        HandleInputBuffering();

        // Update the grid every frame, even when not moving

        if (IsGrounded())
        {
            animPlayed = false;

        }
        else
        {
            hasLanded = false;
        }
        if (IsGrounded() && !hasLanded)
        {
            animController.animator.Play("capyLanding", 0, 0);
            hasLanded = true;
        }

    }

    private void WIN()
    {
        boardManager.canSpawn = false;
        Debug.Log("WONNN");
        boardManager.IsWonAnimation();
        transform.DOMoveY(transform.position.y + 30, 2).SetEase(Ease.InOutBack);
        rb.isKinematic = true;
        isWon = true;
    }
    public void GetToNextLevel()
    {
        int originalY = 0;
        transform.position = new Vector2(transform.position.x, - 4);
        transform.DOMoveY(originalY, 2).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            boardManager.levelController.IncreaseLevel();
            boardManager.canSpawn = true;
            rb.isKinematic = false;
            isWon = false;
            GetComponent<BoxCollider2D>().enabled = true;
            FindObjectOfType<TetrisRandomizer>().SpawnNewTetromino();
        });

    }

    #region FLIP
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
        {
            Flip();
        }
        if (_x < 0 && facingRight)
        {
            Flip();
        }
    }
    #endregion
    #region MOVEMENT
    private void JumpAndFallMovement()
    {
        if (transform.position.y == 0)
        {
            canFall = false;
            canJump = true;
            horizontalMovedInAir = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isMoving)
        {
            canFall = false;
            StartCoroutine(JumpUp());
        }
        if (Time.time - previousTime > fallTime && canFall && !isMoving)
        {
            if (validMove(new Vector3(0, -1, 0)))
            {
                canJump = false;
                targetPosition = transform.position + new Vector3(0, -1, 0) * moveSpeed;
                StartCoroutine(MoveForward(RoundedPos(targetPosition)));
                if (!animPlayed)
                {
                    animController.animator.Play("capyFall", 0, 0);
                    animPlayed = true;
                }
            }
            else
            {
                canJump = true;
                horizontalMovedInAir = 0;
            }
            previousTime = Time.time;
        }
    }



    private void HorizontalMovement()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving)
        {
            FlipController(1);
            if (!maxHorizontalMoveInAirAchieved())
            {
                targetPosition = transform.position + new Vector3(-1, 0, 0) * moveSpeed;
                //Debug.Log(validMove(new Vector3(-1, 0, 0)));
                if (validMove(new Vector3(-1, 0, 0)))
                {
                    animController.animator.Play("capyDash", 0, 0);
                    StartCoroutine(MoveForward(targetPosition));
                }
                if (!canJump)
                {
                    horizontalMovedInAir++;
                }
            }
            
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving )
        {
            FlipController(-1);
            if (!maxHorizontalMoveInAirAchieved())
            {
                targetPosition = transform.position + new Vector3(1, 0, 0) * moveSpeed;
                if (validMove(new Vector3(1, 0, 0)))
                {
                    animController.animator.Play("capyDash", 0, 0);
                    StartCoroutine(MoveForward(targetPosition));
                }
                if (!canJump)
                {
                    horizontalMovedInAir++;
                }

            }
                
        }
    }
    public bool maxHorizontalMoveInAirAchieved() => horizontalMovedInAir > maxHorizontalMoveInAir;
    private void HandleInputBuffering()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isMoving && !maxHorizontalMoveInAirAchieved())
        {
            queuedInput = new Vector3(-1, 0, 0);
            animController.animator.Play("capyDash", 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isMoving && !maxHorizontalMoveInAirAchieved())
        {
            animController.animator.Play("capyDash", 0, 0);
            queuedInput = new Vector3(1, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isMoving)
        {
            queuedInput = new Vector3(0, 1, 0);
            canFall = false;
            StartCoroutine(JumpUp());
        }

        // Execute queued input after movement is finished
        if (!isMoving && queuedInput != Vector3.zero)
        {
            if (validMove(queuedInput))
            {
                Vector3 targetPosition = transform.position + queuedInput * moveSpeed;
                StartCoroutine(MoveForward(RoundedPos(targetPosition)));
            }
            queuedInput = Vector3.zero; // Clear the queued input
        }
    }
    #endregion

    #region GRID LOGIC
    private void addToGrid(Vector3 lastPosition)
    {

        int roundedlastPosX = Mathf.RoundToInt(lastPosition.x);
        int roundedLastPosY = Mathf.RoundToInt(lastPosition.y);

        int roundedX = Mathf.RoundToInt(transform.position.x);
        int roundedY = Mathf.RoundToInt(transform.position.y);

        // If the player is not moving, update the grid
        if (lastPosition == transform.position)
        {
            if (boardManager.grid[roundedX, roundedY] == null)
            {
                // Update the grid with the player's position
                boardManager.grid[roundedX, roundedY] = transform;
                return;
            }
        }
        // If the player is moving, update the grid with the new position and clear the old position
        if (boardManager.grid[roundedX, roundedY] == null)
        {
            boardManager.grid[roundedX, roundedY] = transform;
            boardManager.grid[roundedlastPosX, roundedLastPosY] = null;
            //Debug.Log(new Vector2(roundedlastPosX, roundedLastPosY));
        }
    }
    private bool validMove(Vector2 direction)
    {
        int roundedX = Mathf.RoundToInt(transform.position.x + direction.x);
        int roundedY = Mathf.RoundToInt(transform.position.y + direction.y);

        if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
            return false;

        if (boardManager.grid[roundedX, roundedY] != null && !boardManager.grid[roundedX, roundedY].gameObject.CompareTag("Obstacle"))
            return false;
        return true;
    }
    #endregion

    #region ACTUAL MOVEMENT LOGIC
    IEnumerator MoveForward(Vector3 targetPosition)
    {
        isMoving = true;
        // Calculate the target position
        // Move the Rigidbody to the target position over one second
        float elapsedTime = 0f;
        lastPosition = transform.position;
        while (elapsedTime < movementDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / movementDuration;
            Vector2 roundedPos = RoundedPos(transform.position);
            Vector2 roundedTarget = RoundedPos(targetPosition);
            rb.MovePosition(Vector3.Lerp(roundedPos, roundedTarget, progress));
            // Update the grid during the movement
            addToGrid(lastPosition);
            yield return null;
        }

        // Ensure the player reaches the exact target position
        previousTime = Time.time;
        moveDirection = DetermineMoveDirection(RoundedPos(lastPosition), RoundedPos(targetPosition));
        rb.MovePosition(targetPosition);
        isMoving = false;
    }
    private Vector2 RoundedPos(Vector2 pos)
    {
        int roundedX = Mathf.RoundToInt(pos.x);
        int roundedY = Mathf.RoundToInt(pos.y);
        return new Vector2(roundedX, roundedY);
    }
    IEnumerator JumpUp()
    {
        animController.animator.Play("capyJump", 0, 0);
        while (heigthJumped < jumpHeight && validMove(new Vector3(0, 1, 0)))
        {
            canJump = false;
            targetPosition = transform.position + new Vector3(0, 1, 0) * moveSpeed;
            yield return StartCoroutine(MoveForward((RoundedPos(targetPosition))));
            heigthJumped++;
            if (targetPosition.y > heightClimbed)
            {
                heightClimbed++;
                UIController.instance.ChangeHeightClimbedText(heightClimbed);
            } 
        }
        heigthJumped = 0;
        canFall = true;
        //animController.SwitchAnimation("Fall");
    }
    #endregion

    public void ChangePlayerMaterialColor()
    {
        StartCoroutine(ChangeMaterialColorCoroutine());
    }
    private IEnumerator ChangeMaterialColorCoroutine()
    {
        spriteRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.material.color = originalMaterialColor;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Medal"))
        {
            Destroy(collision.gameObject);
            boardManager.IncreaseMedalScore();
        }
        if (collision.CompareTag("Obstacle"))
        {
            ChangePlayerMaterialColor();
            switch (moveDirection)
            {
                case (MoveDirection.Left):
                    StartCoroutine(MoveForward(DesiredHitPos(new Vector2(1, 0))));
                    previousTime = Time.time;
                    break;
                case (MoveDirection.Right):
                    StartCoroutine(MoveForward(DesiredHitPos(new Vector2(-1, 0))));
                    previousTime = Time.time;
                    break;
                case (MoveDirection.Up):
                    StartCoroutine(MoveForward(DesiredHitPos(new Vector2(0, -1))));
                    previousTime = Time.time;
                    break;
                case (MoveDirection.Down):
                    StartCoroutine(MoveForward(DesiredHitPos(new Vector2(0, 2))));
                    previousTime = Time.time;
                    break;
            }
        }
    }
    private Vector2 DesiredHitPos(Vector2 targetPos)
    {
        Vector2 afterHitPos = new Vector2(transform.position.x + targetPos.x, transform.position.y + targetPos.y);
        Vector2 rounded = RoundedPos(afterHitPos);
        return rounded;
    }
    private bool IsGrounded() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    private void OnDrawGizmos()
    {
        // Draw a debug ray to visualize the ground check distance
        Gizmos.color = Color.green;
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
    }

    private MoveDirection DetermineMoveDirection(Vector2 lastPos, Vector2 afterMovePos)
    {
        if (afterMovePos.x - lastPos.x > 0 && afterMovePos.y == lastPos.y)
        {
            return MoveDirection.Right;
        }
        if (afterMovePos.x - lastPos.x < 0 && afterMovePos.y == lastPos.y)
        {
            return MoveDirection.Left;
        }
        if (afterMovePos.y - lastPos.y < 0 && afterMovePos.x == lastPos.x)
        {
            return MoveDirection.Down;
        }
        if (afterMovePos.y - lastPos.y > 0 && afterMovePos.x == lastPos.x)
        {
            return MoveDirection.Up;
        }
        return MoveDirection.Left;
    }
}