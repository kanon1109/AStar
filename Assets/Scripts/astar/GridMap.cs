using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GridMap
{
    //地图列表
    private List<List<Node>> mapList;
    //行数
    private int _rows = 0;
    public int rows { get { return _rows; } }
    //列数
    private int _columns = 0;
    public int columns { get { return _columns; } }
    //网格的宽度
    private float _gridWidth;
    public float gridWidth { get { return _gridWidth; } }
    //网格的高度
    private float _gridHeight;
    public float gridHeight { get { return _gridHeight; } }
    //网格横向间隔
    private float _gapH;
    public float gapH { get { return _gapH; } }
    //网格纵向间隔
    private float _gapV;
    public float gapV { get { return _gapV; } }
    /// <summary>
    /// 创建地图
    /// </summary>
    /// <param name="rows">行数</param>
    /// <param name="columns">列数</param>
    /// <param name="gridWidth">网格的宽度</param>
    /// <param name="gridHeight">网格的高度</param>
    /// <param name="gapH">网格横向间隔</param>
    /// <param name="gapV">网格纵向间隔</param>
    public void create(int rows, 
                       int columns, 
                       float gridWidth = 1, 
                       float gridHeight = 1,
                       float gapH = 0, 
                       float gapV = 0)
    {
        this.clear();
        this._rows = rows;
        this._columns = columns;
        this._gridWidth = gridWidth;
        this._gridHeight = gridHeight;
        this._gapH = gapH;
        this._gapV = gapV;
        if (this.mapList == null) this.mapList = new List<List<Node>>();
        for (int i = 0; i < rows; i++)
        {
            List<Node> list = new List<Node>();
            for (int j = 0; j < columns; j++)
            {
                Node node = new Node();
                node.x = j;
                node.y = i;
                list.Add(node);
            }
            mapList.Add(list);
        }
    }

    /// <summary>
    /// 根据坐标获取节点
    /// </summary>
    /// <param name="row">列号</param>
    /// <param name="column">行号</param>
    /// <returns></returns>
    public Node getNode(int row, int column)
    {
        if (this.mapList == null) return null;
        if (row < 0 ||
            row >= this._rows ||
            column < 0 ||
            column > this._columns) return null;
        return this.mapList[row][column];
    }

    /// <summary>
    /// 清除
    /// </summary>
    public void clear()
    {
        if (this.mapList != null)
            this.mapList.Clear();
    }

    /// <summary>
    /// 根据坐标获取节点
    /// </summary>
    /// <param name="x">x坐标</param>
    /// <param name="y">y坐标</param>
    /// <returns></returns>
    public Node getNodeByPos(float x, float y)
    {
        int column = Mathf.FloorToInt(x / (this._gridWidth + this._gapH));
        int row = Mathf.FloorToInt(y / (this._gridHeight + this._gapV));
        return getNode(row, column);
    }

    /// <summary>
    /// 根据行列算出网格实际的坐标位置（2d位置）
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列</param>
    /// <returns></returns>
    public Vector2 getPosByNodeIndex(int row, int column)
    {
        return new Vector2(column * (this._gridWidth + this._gapH), row * (this._gridHeight + this._gapV));
    }
}
