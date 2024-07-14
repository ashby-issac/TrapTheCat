using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

enum Direction
{
    left,
    right,
    up,
    down,
}

public class CatMover : MonoBehaviour
{
    private Vector3 trackPos = Vector3.zero;
    private List<Direction> directions = new List<Direction>();

    private Dictionary<Direction, bool> blockedDirections = new Dictionary<Direction, bool>();
    private Dictionary<Direction, float> directionLengths = new Dictionary<Direction, float>();

    private bool canMove = false;
    private bool catEscaped = false;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 gridStartPos = Vector3.zero;
    private Vector3 gridEndPos = Vector3.zero;
    private Vector3 lastMovedDir = Vector3.zero;

    public void UpdateCatPosition()
    {
        ResetData();

        var catPos = transform.position;
        gridStartPos = GridManager.Instance.StartPos;
        gridEndPos = GridManager.Instance.EndPos;

        trackPos = catPos;
        while (true)
        {
            CheckAvailableDirections(catPos, gridStartPos, gridEndPos);
            if (HasReachedEdge())
            {
                catEscaped = true;
                break;
            }
            else if ((directions.Contains(Direction.left) && directions.Contains(Direction.right)
                && directions.Contains(Direction.up) && directions.Contains(Direction.down)))
            {
                break;
            }
        }

        if (catEscaped)
        {
            moveDirection += lastMovedDir * 5f;
            return;
        }

        float minDirLength = 999;
        minDirLength = FindMinDirectionalLength(minDirLength);
        moveDirection = catPos;

        if (minDirLength != 0)
        {
            if (AnyUnblockedDirection())
            {
                foreach (var dir in directionLengths)
                {
                    if (!blockedDirections[dir.Key] && dir.Value == minDirLength)
                    {
                        MoveCatPosition(dir.Key);
                        break;
                    }
                }
            }
            else
            {
                foreach (var dir in directionLengths)
                {
                    if (dir.Value == minDirLength && dir.Value > 0)
                    {
                        MoveCatPosition(dir.Key);
                        break;
                    }
                }
            }
        }
    }

    private bool HasReachedEdge()
    {
        return trackPos.y >= gridEndPos.y ||
               trackPos.y <= gridStartPos.y ||
               trackPos.x <= gridStartPos.x ||
               trackPos.x >= gridEndPos.x;
    }

    private void MoveCatPosition(Direction dir)
    {
        UpdateMoveDirection(dir);
        canMove = true;
    }

    private float FindMinDirectionalLength(float minDirLength)
    {
        if (AnyUnblockedDirection())
        {
            foreach (var dirLength in directionLengths)
                if (!blockedDirections[dirLength.Key] && dirLength.Value < minDirLength)
                    minDirLength = dirLength.Value;
        }
        else
        {
            foreach (var dirLength in directionLengths)
                if (dirLength.Value < minDirLength && dirLength.Value > 0)
                    minDirLength = dirLength.Value;
        }

        return minDirLength;
    }

    private bool AnyUnblockedDirection() => directionLengths.Any(direction => !blockedDirections[direction.Key]);

    private void UpdateMoveDirection(Direction dirKey)
    {
        switch (dirKey)
        {
            case Direction.left:
                lastMovedDir = Vector3.left;
                moveDirection += Vector3.left;
                break;
            case Direction.right:
                lastMovedDir = Vector3.right;
                moveDirection += Vector3.right;
                break;
            case Direction.up:
                lastMovedDir = Vector3.up;
                moveDirection += Vector3.up;
                break;
            case Direction.down:
                lastMovedDir = Vector3.down;
                moveDirection += Vector3.down;
                break;
        }
    }

    private void CheckAvailableDirections(Vector3 catPos, Vector3 gridStartPos, Vector3 gridEndPos)
    {
        if (trackPos.y < gridEndPos.y && !blockedDirections[Direction.up] && !directions.Contains(Direction.up))
        {
            CheckIfPathIsBlocked(offsetDir: Vector3.up, Direction.up);
            if (blockedDirections[Direction.up] || trackPos.y >= gridEndPos.y)
            {
                SetPathData(offsetDir: Vector3.up, trackPos.y, catPos.y, catPos, exploredDir: Direction.up);
            }
        }
        else if (trackPos.y > gridStartPos.y && !blockedDirections[Direction.down] && !directions.Contains(Direction.down))
        {
            CheckIfPathIsBlocked(offsetDir: Vector3.down, Direction.down);
            if (blockedDirections[Direction.down] || trackPos.y <= gridStartPos.y)
            {
                SetPathData(offsetDir: Vector3.down, trackPos.y, catPos.y, catPos, exploredDir: Direction.down);
            }
        }
        else if (trackPos.x > gridStartPos.x && !blockedDirections[Direction.left] && !directions.Contains(Direction.left))
        {
            CheckIfPathIsBlocked(offsetDir: Vector3.left, Direction.left);
            if (blockedDirections[Direction.left] || trackPos.x <= gridStartPos.x)
            {
                SetPathData(offsetDir: Vector3.left, trackPos.x, catPos.x, catPos, exploredDir: Direction.left);
            }
        }
        else if (trackPos.x < gridEndPos.x && !blockedDirections[Direction.right] && !directions.Contains(Direction.right))
        {
            CheckIfPathIsBlocked(offsetDir: Vector3.right, Direction.right);
            if (blockedDirections[Direction.right] || trackPos.x >= gridEndPos.x)
            {
                SetPathData(offsetDir: Vector3.right, trackPos.x, catPos.x, catPos, exploredDir: Direction.right);
            }
        }
    }

    private void SetPathData(Vector3 offsetDir, float trackAxisVal, float catAxisVal,
                                                   Vector3 catPos, Direction exploredDir)
    {
        // negate the offsetDir axis value and add it to the trackAxisVal 
        if (blockedDirections[exploredDir])
        {
            var negatedOffsetVal = -(offsetDir.x == 0 ? offsetDir.y : offsetDir.x);
            trackAxisVal += negatedOffsetVal;
            directionLengths[exploredDir] = Mathf.Abs(Mathf.Round(trackAxisVal - catAxisVal));
        }
        else
        {
            directionLengths[exploredDir] = Mathf.Abs(Mathf.Round(trackAxisVal - catAxisVal));
        }

        // Resetting trackPos for checking in other directions
        trackPos = catPos;
        directions.Add(exploredDir); // avoid looping for same direction
    }

    private void CheckIfPathIsBlocked(Vector3 offsetDir, Direction direction)
    {
        trackPos += offsetDir;
        trackPos = new Vector3(Mathf.Round(trackPos.x), MathF.Round(trackPos.y));
        var nodePoint = GridManager.Instance.Nodes.Find(nodePoint => nodePoint.Coordinates.x == trackPos.x &&
                                                                     nodePoint.Coordinates.y == trackPos.y);
        if (nodePoint != null)
            blockedDirections[direction] = nodePoint.IsBlocked;
    }

    private void Start()
    {
        InitDirectionProps();
        GridManager.Instance.OnNodeClick += UpdateCatPosition;
    }

    private void Update()
    {
        if (canMove && IsReachedDest())
        {
            canMove = false;
        }
        else if (catEscaped && IsReachedDest())
        {
            catEscaped = false;
        }
    }

    private bool IsReachedDest()
    {
        transform.position = Vector3.Lerp(transform.position, moveDirection, Time.deltaTime * 10f);
        return Vector3.Distance(transform.position, moveDirection) <= 0;
    }

    private void ResetData()
    {
        blockedDirections.Clear();
        directionLengths.Clear();
        directions.Clear();
        InitDirectionProps();
    }

    private void InitDirectionProps()
    {
        blockedDirections.Clear();
        blockedDirections.Add(Direction.left, false);
        blockedDirections.Add(Direction.right, false);
        blockedDirections.Add(Direction.up, false);
        blockedDirections.Add(Direction.down, false);

        directionLengths.Clear();
        directionLengths.Add(Direction.left, 0f);
        directionLengths.Add(Direction.right, 0f);
        directionLengths.Add(Direction.up, 0f);
        directionLengths.Add(Direction.down, 0f);
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
