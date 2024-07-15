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

enum NodeState
{
    None,
    Blocked,
    UnBlocked
}

public class CatMover : MonoBehaviour
{
    private Vector3 trackPos = Vector3.zero;
    private List<Direction> directions = new List<Direction>();

    private Dictionary<Direction, NodeState> blockedDirections = new Dictionary<Direction, NodeState>();
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
            if ((directions.Contains(Direction.left) && directions.Contains(Direction.right) &&
                 directions.Contains(Direction.up) && directions.Contains(Direction.down)))
            {
                break;
            }
        }

        float minDirLength = 999;
        minDirLength = FindMinDirectionalLength(minDirLength);
        moveDirection = catPos;

        if (minDirLength > 0)
        {
            if (AnyDirectWayOut())
            {
                foreach (var dir in directionLengths)
                {
                    if (blockedDirections[dir.Key] == NodeState.UnBlocked && dir.Value == minDirLength)
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
        else // if negative it means there's a way out for the cat
        {
            var dirKey = directionLengths.FirstOrDefault(dirLength => dirLength.Value == -1).Key;
            MoveCatPosition(dir: dirKey);
        }
    }

    private bool HasReachedEdge(Direction currentDir)
    {
        bool hasReachedEdge = false;
        switch (currentDir)
        {
            case Direction.up:
                hasReachedEdge = transform.position.y >= gridEndPos.y;
                break;
            case Direction.down:
                hasReachedEdge = transform.position.y <= gridStartPos.y;
                break;
            case Direction.left:
                hasReachedEdge = transform.position.x <= gridStartPos.x;
                break;
            case Direction.right:
                hasReachedEdge = transform.position.x >= gridEndPos.x;
                break;
        }
        return hasReachedEdge;
    }

    private void MoveCatPosition(Direction dir)
    {
        UpdateMoveDirection(dir);
        canMove = true;
    }

    private float FindMinDirectionalLength(float minDirLength)
    {
        if (AnyDirectWayOut())
        {
            foreach (var dirLength in directionLengths)
            {
                if ((blockedDirections[dirLength.Key] == NodeState.UnBlocked || blockedDirections[dirLength.Key] == NodeState.None) && dirLength.Value < minDirLength)
                {
                    minDirLength = dirLength.Value;
                }
            }
        }
        else
        {
            foreach (var dirLength in directionLengths)
                if (dirLength.Value < minDirLength && dirLength.Value > 0)
                    minDirLength = dirLength.Value;
        }

        return minDirLength;
    }

    private bool AnyDirectWayOut() => directionLengths.Any(direction => blockedDirections[direction.Key] == NodeState.UnBlocked ||
                                                                          blockedDirections[direction.Key] == NodeState.None);

    private void UpdateMoveDirection(Direction dirKey)
    {
        switch (dirKey)
        {
            case Direction.left:
                moveDirection += Vector3.left;
                break;
            case Direction.right:
                moveDirection += Vector3.right;
                break;
            case Direction.up:
                moveDirection += Vector3.up;
                break;
            case Direction.down:
                moveDirection += Vector3.down;
                break;
        }
    }

    private void CheckAvailableDirections(Vector3 catPos, Vector3 gridStartPos, Vector3 gridEndPos)
    {
        trackPos = new Vector3(Mathf.Round(trackPos.x), Mathf.Round(trackPos.y));
        if (trackPos.y <= gridEndPos.y
            && blockedDirections[Direction.up] == NodeState.UnBlocked && !directions.Contains(Direction.up))
        {
            CheckIfNextNodeIsBlocked(currentOffsetDir: Vector3.up, Direction.up);
            if (blockedDirections[Direction.up] == NodeState.Blocked || trackPos.y > gridEndPos.y)
            {
                SetPathData(offsetDir: Vector3.up, trackPos.y, catPos.y, catPos, exploredDir: Direction.up);
            }
        }
        else if (trackPos.y >= gridStartPos.y
                 && blockedDirections[Direction.down] == NodeState.UnBlocked && !directions.Contains(Direction.down))
        {
            CheckIfNextNodeIsBlocked(currentOffsetDir: Vector3.down, Direction.down);
            if (blockedDirections[Direction.down] == NodeState.Blocked || trackPos.y < gridStartPos.y)
            {
                SetPathData(offsetDir: Vector3.down, trackPos.y, catPos.y, catPos, exploredDir: Direction.down);
            }
        }
        else if (trackPos.x >= gridStartPos.x
                 && blockedDirections[Direction.left] == NodeState.UnBlocked && !directions.Contains(Direction.left))
        {
            CheckIfNextNodeIsBlocked(currentOffsetDir: Vector3.left, Direction.left);
            if (blockedDirections[Direction.left] == NodeState.Blocked || trackPos.x < gridStartPos.x)
            {
                SetPathData(offsetDir: Vector3.left, trackPos.x, catPos.x, catPos, exploredDir: Direction.left);
            }
        }
        else if (trackPos.x <= gridEndPos.x &&
                 blockedDirections[Direction.right] == NodeState.UnBlocked && !directions.Contains(Direction.right))
        {
            CheckIfNextNodeIsBlocked(currentOffsetDir: Vector3.right, Direction.right);
            if (blockedDirections[Direction.right] == NodeState.Blocked || trackPos.x > gridEndPos.x)
            {
                SetPathData(offsetDir: Vector3.right, trackPos.x, catPos.x, catPos, exploredDir: Direction.right);
            }
        }
    }

    private void SetPathData(Vector3 offsetDir, float trackAxisVal, float catAxisVal,
                                                   Vector3 catPos, Direction exploredDir)
    {
        // negate the offsetDir axis value and add it to the trackAxisVal 
        InitDirectionLength(blockedDirections[exploredDir], offsetDir, trackAxisVal, catAxisVal, exploredDir);

        // Resetting trackPos for checking in other directions
        trackPos = catPos;
        directions.Add(exploredDir); // avoid looping for same direction
    }

    private void InitDirectionLength(NodeState nodeState, Vector3 offsetDir,
                                     float trackAxisVal, float catAxisVal, Direction exploredDir)
    {
        switch (nodeState)
        {
            case NodeState.None:
                directionLengths[exploredDir] = -1;
                break;
            case NodeState.UnBlocked:
                directionLengths[exploredDir] = Mathf.Abs(Mathf.Round(trackAxisVal - catAxisVal));
                break;
            case NodeState.Blocked:
                var negatedOffsetVal = -(offsetDir.x == 0 ? offsetDir.y : offsetDir.x);
                trackAxisVal += negatedOffsetVal;
                directionLengths[exploredDir] = Mathf.Abs(Mathf.Round(trackAxisVal - catAxisVal));
                break;
        }
    }

    private void CheckIfNextNodeIsBlocked(Vector3 currentOffsetDir, Direction currentDir)
    {
        trackPos += currentOffsetDir;
        trackPos = new Vector3(Mathf.Round(trackPos.x), MathF.Round(trackPos.y));
        var nodePoint = GridManager.Instance.Nodes.Find(nodePoint => nodePoint.Coordinates.x == trackPos.x &&
                                                                     nodePoint.Coordinates.y == trackPos.y);
        if (nodePoint)
        {
            blockedDirections[currentDir] = nodePoint.IsBlocked ? NodeState.Blocked : NodeState.UnBlocked;
        }
        else if (nodePoint == null)
        {
            blockedDirections[currentDir] = HasReachedEdge(currentDir) ? NodeState.None : NodeState.UnBlocked;
        }
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
        blockedDirections.Add(Direction.left, NodeState.UnBlocked);
        blockedDirections.Add(Direction.right, NodeState.UnBlocked);
        blockedDirections.Add(Direction.up, NodeState.UnBlocked);
        blockedDirections.Add(Direction.down, NodeState.UnBlocked);

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
