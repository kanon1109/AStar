using System.Collections.Generic;
using UnityEngine;
public class PathFinder
{
    //是否检查对角线
    public bool checkDiagonals = true;
    //地图节点列表
    private List<List<Node>> mapList = null;
    //开放列表
    private List<Node> openList = null;
    //关闭列表
    private List<Node> closeList = null;
    //h值的启发式方法
    private delegate int Heuristic(Node node, Node endNode);
    private Heuristic heuristic = null;
    //直线代价     
    private int straightCost = 2;
    //对角线代价
    private int diagonalCost = 4;    

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="mapList"></param>
    public void init(List<List<Node>> mapList)
    {
        this.mapList = mapList;
        this.openList = new List<Node>();
        this.closeList = new List<Node>();
        this.heuristic = this.manhattan;
    }

    /// <summary>
    /// 寻路
    /// </summary>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">终点节点</param>
    /// <returns>最终路径列表</returns>
    public List<Node> findPath(Node startNode, Node endNode)
    {
        if (startNode.compare(endNode)) return null;
        if (startNode.isBlock || endNode.isBlock) return null;

        //清空开放关闭列表
        this.openList.Clear();
        this.closeList.Clear();

        //放入起始节点
        this.openList.Add(startNode);

        startNode.g = 0;
        startNode.f = this.heuristic(startNode, endNode);
        startNode.h = startNode.g + startNode.f;

        Node curNode = startNode;
        while (curNode != endNode)
        {
            List<Node> roundNodeList = this.getRoundNode(curNode);
            int length = roundNodeList.Count;
            for (int i = 0; i < length; ++i)
            {
                Node roundNode = roundNodeList[i];
                int cost = this.straightCost;  
                //如果是对象线，则使用对角代价
                if (!((curNode.x == roundNode.x) || (curNode.y == roundNode.y)))
                    cost = this.diagonalCost;
                //计算test节点的总代价                      
                int g = curNode.g + cost;
                int h = this.heuristic(roundNode, endNode);
                int f = g + h;

                //如果该点在open或close列表中
                if (this.isOpen(roundNode) || 
                    this.isClosed(roundNode))
                {
                    //如果本次计算的代价更小，则以本次计算为准
                    if (f < roundNode.f)
                    {
                        roundNode.f = f;
                        roundNode.g = g;
                        roundNode.h = h;
                        //重新指定该点的父节点为本轮计算中心点
                        roundNode.parentNode = curNode;
                    }
                }
                else
                {
                    //如果还不在open列表中，则除了更新代价以及设置父节点，还要加入open数组
                    roundNode.f = f;
                    roundNode.g = g;
                    roundNode.h = h;
                    roundNode.parentNode = curNode;
                    this.openList.Add(roundNode);
                }
            }

            //把处理过的本轮中心节点加入close节点
            this.closeList.Add(curNode);

            if (this.openList.Count == 0)
            {
                //循环开放列表直到开放列表数量为空
                //没找到路径
                return null;
            }

            //按总代价从小到大排序
            this.openList.Sort(new NodeComparer());
            //从open数组中删除代价最小的结节，同时把该节点赋值为node，做为下次的中心点
            curNode = this.openList[0];
            this.openList.RemoveAt(0);
        }

        List<Node> path = new List<Node>();
        Node node = endNode;
        path.Add(node);
        while (node != startNode)
        {
            node = node.parentNode;
            path.Add(node);
        }
        return path;
    }

    /// <summary>
    /// 节点是否在关闭列表中
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns></returns>
    private bool isClosed(Node node)
    {
        return this.closeList.IndexOf(node) != -1;
    }

    /// <summary>
    /// 节点是否在开放列表中
    /// </summary>
    /// <param name="node">节点</param>
    /// <returns></returns>
    private bool isOpen(Node node)
    {
        return this.openList.IndexOf(node) != -1;
    }

    /// <summary>
    /// 获取周围的节点（4个或者8个） 
    /// </summary>
    /// <param name="centerNode">节点</param>
    /// <returns></returns>
    public List<Node> getRoundNode(Node centerNode)
    {
        int startX = Mathf.Max(0, centerNode.x - 1);
        int startY = Mathf.Max(0, centerNode.y - 1);
        int endX = Mathf.Min(centerNode.x + 1, this.mapList[0].Count - 1);
        int endY = Mathf.Min(centerNode.y + 1, this.mapList.Count - 1);
        List<Node> roundNodeList = new List<Node>();
        //检查对角线的节点
        for (int i = startY; i <= endY; ++i) //行
        {
            for (int j = startX; j <= endX; ++j) //列
            {
                Node roundNode = this.mapList[i][j];
                if (roundNode == centerNode) continue; //排除自己
                if (roundNode.isBlock) continue;
                if ((roundNode.x == centerNode.x) || (roundNode.y == centerNode.y))
                {
                    //上下左右四个
                    roundNodeList.Add(roundNode);
                }
                else
                {
                    if (this.checkDiagonals)
                    {
                        //如果相邻的4个中一个是障碍的话，那么就把node所在的斜角节点去掉不放进 周围数组中
                        if (this.mapList[roundNode.y][centerNode.x].isBlock ||
                            this.mapList[centerNode.y][roundNode.x].isBlock) continue;
                        
                        roundNodeList.Add(roundNode);
                    }
                }
            }
        }
        return roundNodeList;
    }
    
    /// <summary>
    /// 弗洛伊德路径平滑处理 form http://wonderfl.net/c/aWCe
    /// </summary>
    /// <param name="floydPath"></param>
    /// <returns></returns>
    public List<Node> floyd(List<Node> floydPath)
    {
        if (floydPath == null) return null;
        floydPath.Reverse();
        int len = floydPath.Count;
        if (len > 2)
        {
            /*PathFinderNode vector = new PathFinderNode();
            PathFinderNode tempVector = new PathFinderNode();

            //遍历路径数组中全部路径节点，合并在同一直线上的路径节点
            //假设有1,2,3,三点，若2与1的横、纵坐标差值分别与3与2的横、纵坐标差值相等则
            //判断此三点共线，此时可以删除中间点2
            FloydVector(vector, _floydPath[len - 1], _floydPath[len - 2]);

            for (int i = _floydPath.Count - 3; i >= 0; i--)
            {
                FloydVector(tempVector, _floydPath[i + 1], _floydPath[i]);
                if (vector.PX == tempVector.PX && vector.PY == tempVector.PY)
                {
                    _floydPath.RemoveAt(i + 1);
                }
                else
                {
                    vector.PX = tempVector.PX;
                    vector.PY = tempVector.PY;
                }
            }*/
        }

        floydPath.Reverse();

        //合并共线节点后进行第二步，消除拐点操作。算法流程如下：
        //如果一个路径由1-10十个节点组成，那么由节点10从1开始检查
        //节点间是否存在障碍物，若它们之间不存在障碍物，则直接合并
        //此两路径节点间所有节点。
        //len = floydPath.Count;
        //for (int i = len - 1; i >= 0; i--)
        //{
        //    for (int j = 0; j <= i - 2; j++)
        //    {
        //        if ( _grid.hasBarrier(_floydPath[i].X, _floydPath[i].Y, _floydPath[j].X, _floydPath[j].Y) == false )
        //        {
        //            for (int k = i - 1; k > j; k--)
        //            {
        //                _floydPath.RemoveAt(k);
        //            }
        //            i = j;
        //            len = _floydPath.Count;
        //            break;
        //        }
        //    }
        //}

        return floydPath;
    }

    /// <summary>
    /// 曼哈顿估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private int manhattan(Node node, Node endNode)
    {
        return Mathf.Abs(node.x - endNode.x) * this.straightCost + Mathf.Abs(node.y - endNode.y) * this.straightCost;
    }
 
    /// <summary>
    /// 几何估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private int euclidian(Node node, Node endNode)
    {
        int dx = node.x - endNode.x;
        int dy = node.y - endNode.y;
        return (int)Mathf.Sqrt(dx * dx + dy * dy) * this.straightCost;
    }
 
    /// <summary>
    /// 对角线估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private int diagonal(Node node, Node endNode)
    {
        int dx = Mathf.Abs(node.x - endNode.x);
        int dy = Mathf.Abs(node.y - endNode.y);
        int diag = Mathf.Min(dx, dy);
        int straight = dx + dy;
        return (this.diagonalCost * 2) * diag + this.straightCost * (straight - 2 * diag);
    }
}

public class NodeComparer : IComparer<Node>
{
    //实现按年龄升序排列
    public int Compare(Node node1, Node node2)
    {
        return node1.f.CompareTo(node2.f);
    }
}