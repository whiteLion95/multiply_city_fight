using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    private Grid _grid;
    private Tile _myTile;

    [field: SerializeField] public int X { get; private set; }
    [field: SerializeField] public int Y { get; private set; }
    public int GCost { get; set; } = 99999999;
    public int HCost { get; set; }
    public int FCost { get { return GCost + HCost; } }
    [field:SerializeField] public bool IsWalkable { get; set; } = true;
    public PathNode CameFromNode { get; set; }
    public Tile MyTile { get => _myTile; }

    private void Awake()
    {
        _grid = GetComponentInParent<Grid>();
        _myTile = GetComponent<Tile>();
    }

    private void Start()
    {
        X = _grid.LocalToCell(transform.localPosition).x;
        Y = _grid.LocalToCell(transform.localPosition).y;
        Invoke(nameof(SetWalkable), 0.1f);
    }

    private void SetWalkable()
    {
        if (_myTile.State == TileState.TileObject && _myTile.MyTileObject is Obstacle)
            IsWalkable = false;
    }

    public void ResetValues()
    {
        GCost = 99999999;
        HCost = 0;
        CameFromNode = null;
    }
}
