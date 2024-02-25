using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private List<PathNode> openList;
    private List<PathNode> closedList;
    private int tileLayerMask;
    private Bot myBot;

    public PathFinding(Bot bot)
    {
        tileLayerMask = 1 << LayerMask.NameToLayer("Tile");
        myBot = bot;
    }

    public List<PathNode> FindPath(PathNode startNode, PathNode endNode)
    {
        if (startNode == null || endNode == null)
        {
            // Invalid Path
            return null;
        }

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        startNode.GCost = 0;
        startNode.HCost = CalculateDistanceCost(startNode, endNode);

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                foreach (PathNode node in openList)
                {
                    node.ResetValues();
                }

                return CalculatePath(endNode);
            }

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.IsWalkable || neighbourNode.MyTile.BaseTowers.Contains(myBot.BaseTower) 
                    || (neighbourNode.MyTile.MyTileObject != null && !neighbourNode.Equals(endNode)))
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.GCost + 1;
                neighbourNode.CameFromNode = currentNode;
                neighbourNode.GCost = tentativeGCost;
                neighbourNode.HCost = CalculateDistanceCost(neighbourNode, endNode);

                if (!openList.Contains(neighbourNode))
                {
                    openList.Add(neighbourNode);
                }
            }
        }

        return null;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(b.X - a.X);
        int yDistance = Mathf.Abs(b.Y - a.Y);
        return xDistance + yDistance;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.CameFromNode != null)
        {
            path.Add(currentNode.CameFromNode);
            currentNode = currentNode.CameFromNode;
        }

        foreach (PathNode node in closedList)
        {
            node.ResetValues();
        }

        path.Reverse();
        return path;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        RaycastHit hit;
        Vector3 castDir = Vector3.forward;
        List<PathNode> neighbourList = new List<PathNode>();

        // Cast ray to forward, right, back and left (4 directions)
        for (int i = 0; i < 4; i++)
        {
            if (Physics.Raycast(currentNode.transform.position, castDir, out hit, 10f, tileLayerMask))
            {
                if (hit.collider.GetComponentInParent<PathNode>())
                    neighbourList.Add(hit.collider.GetComponentInParent<PathNode>());
            }

            castDir = Quaternion.AngleAxis(90f, Vector3.up) * castDir;
        }

        return neighbourList;
    }
} 
