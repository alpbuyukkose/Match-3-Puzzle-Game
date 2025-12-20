using System.Collections;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector] public Vector2Int posIndex { get; private set; }
    [HideInInspector] public Board board { get; private set; }

    // Gem Movement
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private bool isMousePressed;
    private float swipeAngle;
    private Gem otherGem;

    [HideInInspector] public Vector2Int previousPos;

    // Gem Types
    public enum GemType
    {
        blue,   // 0
        green,  // 1
        purple, // 2
        red,    // 3
        orange  // 4
    }
    public GemType type { get; private set; }

    public bool isMatched;

    // Optimization
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if ( Vector2.Distance(transform.position, posIndex) > .01f)
        {
            transform.position = Vector3.Lerp(transform.position, (Vector2)posIndex, board.gemSpeed * Time.deltaTime);
        } else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0f);
            board.allGems[posIndex.x, posIndex.y] = this;
        }
        

        if (isMousePressed && Input.GetMouseButtonUp(0))
        {
            isMousePressed = false;

            finalTouchPosition = _cam.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
            Debug.Log(swipeAngle);
        }
    }

    public void SetupGem(Vector2Int pos, Board theBoard, GemType newType)
    {
        posIndex = pos;
        board = theBoard;
        type = newType;
    }

    private void OnMouseDown()
    {
        firstTouchPosition = _cam.ScreenToWorldPoint(Input.mousePosition);
        isMousePressed = true;
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;

        if (swipeAngle < 0)
        {
            swipeAngle += 360;
        }

        if (Vector3.Distance(firstTouchPosition, finalTouchPosition) >= .5f)
        {
            MovePieces();
        }
    }

    private void MovePieces()
    {
        previousPos = posIndex;
        otherGem = null;

        if ( (swipeAngle >= 315 || swipeAngle <= 45) && posIndex.x < board.width - 1)
        {
            otherGem = board.allGems[posIndex.x + 1, posIndex.y];

            otherGem.posIndex = new Vector2Int(otherGem.posIndex.x - 1, otherGem.posIndex.y);
            posIndex = new Vector2Int(posIndex.x + 1, posIndex.y);
        }

        else if (swipeAngle >= 45 && swipeAngle <= 135 && posIndex.y < board.height - 1)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y + 1];

            otherGem.posIndex = new Vector2Int(otherGem.posIndex.x, otherGem.posIndex.y - 1);
            posIndex = new Vector2Int(posIndex.x, posIndex.y + 1);
        }

        else if (swipeAngle >= 135 && swipeAngle <= 225 && posIndex.x > 0)
        {
            otherGem = board.allGems[posIndex.x - 1, posIndex.y];

            otherGem.posIndex = new Vector2Int(otherGem.posIndex.x + 1, otherGem.posIndex.y);
            posIndex = new Vector2Int(posIndex.x - 1, posIndex.y);
        }

        else if (swipeAngle >= 225 && swipeAngle <= 315 && posIndex.y > 0)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y - 1];

            otherGem.posIndex = new Vector2Int(otherGem.posIndex.x, otherGem.posIndex.y + 1);
            posIndex = new Vector2Int(posIndex.x, posIndex.y - 1);
        }

        if (otherGem != null)
        {
            board.allGems[posIndex.x, posIndex.y] = this;
            board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;

            StartCoroutine(CheckMoveCo());
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.5f);

        board.matchFinder.FindAllMatches();

        if (otherGem != null)
        {
            if (!isMatched && !otherGem.isMatched)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                board.allGems[posIndex.x, posIndex.y] = this;
                board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;
            }
        }
    }
}
