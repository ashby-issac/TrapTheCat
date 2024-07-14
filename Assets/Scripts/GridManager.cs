using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private NodePoint nodePoint;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private int rows, cols;

    private List<NodePoint> nodes = new List<NodePoint>();
    public List<NodePoint> Nodes => nodes;

    public static GridManager Instance;

    public Vector3 StartPos => startPos;
    public Vector3 EndPos => nodes[nodes.Count - 1].Coordinates;

    public Action OnNodeClick;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitGrid();
    }

    private void InitGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var pos = new Vector3(startPos.x + j, startPos.y + i, 0);
                var nodeInstance = Instantiate(nodePoint, pos, Quaternion.identity);

                nodeInstance.Init(coordinates: pos, isBlocked: false);
                nodes.Add(nodeInstance);
            }
        }
    }

    public void UpdateBlockedState(Vector3 coordinates)
    {
        var node = nodes.Find(node => node.Coordinates == coordinates);
        node?.EnableBlockState();
    }
}
