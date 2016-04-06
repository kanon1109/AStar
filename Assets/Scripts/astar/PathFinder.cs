using System.Collections.Generic;
using UnityEngine;
public class PathFinder
{
    //地图
    private GridMap gm;
    //是否检查对角线
    public bool checkDiagonals = true;
    //开放列表
    private List<Node> openList = null;
    //关闭列表
    private List<Node> closeList = null;
    //h值的启发式方法
    private delegate float Heuristic(Node node, Node endNode);
    private Heuristic heuristic = null;
    //直线代价     
    private float straightCost = 1f;
    //对角线代价
    private float diagonalCost = 1.4f;    
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="gm">地图对象</param>
    public void init(GridMap gm)
    {
        this.gm = gm;
        this.openList = new List<Node>();
        this.closeList = new List<Node>();
        this.heuristic = this.manhattan;
    }

    /// <summary>
    /// 根据坐标点寻路
    /// </summary>
    /// <param name="pos1">起点坐标</param>
    /// <param name="pos2">终点坐标</param>
    /// <param name="floyd">是否用弗洛伊德算法平滑路径</param>
    /// <returns>最终路径列表</returns>
    public List<Node> findPath(Vector2 pos1, Vector2 pos2, bool floyd = true)
    {
        Node startNode = this.gm.getNodeByPos(pos1.x, pos1.y);
        Node endNode = this.gm.getNodeByPos(pos2.x, pos2.y);
        return this.findPath(startNode, endNode, floyd);
    }

    /// <summary>
    /// 寻路
    /// </summary>
    /// <param name="startNode">起始节点</param>
    /// <param name="endNode">终点节点</param>
    /// <param name="floyd">是否启用弗洛伊德平滑处理</param>
    /// <returns>最终路径列表</returns>
    public List<Node> findPath(Node startNode, Node endNode, bool floyd = true)
    {
        if (startNode == null || endNode == null) return null;
        if (startNode.isBlock || endNode.isBlock) return null;
        if (startNode.compare(endNode)) return null;

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
                float cost = this.straightCost;  
                //如果是对象线，则使用对角代价
                if (!((curNode.x == roundNode.x) || (curNode.y == roundNode.y)))
                    cost = this.diagonalCost;
                //计算test节点的总代价                      
                float g = curNode.g + cost;
                float h = this.heuristic(roundNode, endNode);
                float f = g + h;

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
        path.Reverse();
        //平衡处理
        if (floyd)
            return this.floyd(path);
        else
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
        int endX = Mathf.Min(centerNode.x + 1, this.gm.columns - 1);
        int endY = Mathf.Min(centerNode.y + 1, this.gm.rows - 1);
        List<Node> roundNodeList = new List<Node>();
        //检查对角线的节点
        for (int i = startY; i <= endY; ++i) //行
        {
            for (int j = startX; j <= endX; ++j) //列
            {
                Node roundNode = this.gm.getNode(i, j);
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
                        if (this.gm.getNode(roundNode.y, centerNode.x).isBlock ||
                            this.gm.getNode(centerNode.y, roundNode.x).isBlock) continue;
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
    /// <param name="pathList">路径</param>
    /// <returns></returns>
    public List<Node> floyd(List<Node> pathList)
    {
        if (pathList == null) return null;
        int len = pathList.Count;
        if (len > 2)
        {
            Node vector = new Node();
            Node tempVector = new Node();
            //遍历路径数组中全部路径节点，合并在同一直线上的路径节点
            //假设有1,2,3,三点，若2与1的横、纵坐标差值分别与3与2的横、纵坐标差值相等则
            //判断此三点共线，此时可以删除中间点2
            this.floydVector(vector, pathList[len - 1], pathList[len - 2]);
            for (int i = pathList.Count - 3; i >= 0; i--)
            {
                this.floydVector(tempVector, pathList[i + 1], pathList[i]);
                //如果有向量差相同的节点，说明在同一直线上。
                if (vector.x == tempVector.x && vector.y == tempVector.y)
                {
                    //删除该节点
                    pathList.RemoveAt(i + 1);
                }
                else
                {
                    //不再同一线上 将当前节点的向量差作为参考
                    vector.x = tempVector.x;
                    vector.y = tempVector.y;
                }
            }
        }

        //合并共线节点后进行第二步，消除拐点操作。算法流程如下：
        //如果一个路径由1-10十个节点组成，那么由节点10从1开始检查
        //节点间是否存在障碍物，若它们之间不存在障碍物，则直接合并
        //此两路径节点间所有节点。
        len = pathList.Count;
        for (int i = len - 1; i >= 0; i--)
        {
            for (int j = 0; j <= i - 2; j++)
            {
                if (this.floydCrossAble(pathList[i], pathList[j]))
                {
                    for (int k = i - 1; k > j; --k)
                    {
                        pathList.RemoveAt(k);
                    }
                    i = j;
                    len = pathList.Count;
                    break;
                }
            }
        }
        return pathList;
    }

    /// <summary>
    /// 判断2个节点间是否有和障碍物交叉
    /// </summary>
    /// <param name="n1">节点1</param>
    /// <param name="n2">节点2</param>
    /// <returns>是否有障碍物</returns>
    private bool floydCrossAble(Node n1, Node n2) 
    {
        List<Point> ps = this.bresenhamNodes(new Point(n1.x, n1.y), new Point(n2.x, n2.y));
        for (int i = ps.Count - 2; i > 0; --i)
        {
            if (ps[i].x >= 0 && 
                ps[i].y >= 0 && 
                ps[i].x < this.gm.columns &&
                ps[i].y < this.gm.rows &&
                this.gm.getNode(ps[i].y, ps[i].x).isBlock)
                return false;
        }
        return true;
    }

    /// <summary>
    /// bresenham算法
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private List<Point> bresenhamNodes(Point p1, Point p2) 
    {
        bool steep = Mathf.Abs(p2.y - p1.y) > Mathf.Abs(p2.x - p1.x);
        if (steep) 
        {
            int temp = p1.x;
            p1.x = p1.y;
            p1.y = temp;
            temp = p2.x;
            p2.x = p2.y;
            p2.y = temp;
        }
        int stepX = p2.x > p1.x ? 1 : (p2.x < p1.x ? -1 : 0);
        float deltay = (float)(p2.y - p1.y) / Mathf.Abs(p2.x - p1.x);

        List<Point> ret = new List<Point>();

        float nowX = p1.x + stepX;
        float nowY = p1.y + deltay;
        if (steep)
            ret.Add(new Point(p1.y, p1.x));
        else
            ret.Add(new Point(p1.x, p1.y));

        if (Mathf.Abs(p1.x - p2.x) == Mathf.Abs(p1.y - p2.y)) 
        {
            if (p1.x < p2.x && p1.y < p2.y)
            {
                ret.Add(new Point(p1.x, p1.y + 1));
                ret.Add(new Point(p2.x, p2.y - 1));
            }
            else if (p1.x > p2.x && p1.y > p2.y)
            {
                ret.Add(new Point(p1.x, p1.y - 1));
                ret.Add(new Point(p2.x, p2.y + 1));
            }
            else if (p1.x < p2.x && p1.y > p2.y)
            {
                ret.Add(new Point(p1.x, p1.y - 1));
                ret.Add(new Point(p2.x, p2.y + 1));
            }
            else if (p1.x > p2.x && p1.y < p2.y)
            {
                ret.Add(new Point(p1.x, p1.y + 1));
                ret.Add(new Point(p2.x, p2.y - 1));
            }
        }

        while (nowX != p2.x)
        {
            int fy = Mathf.FloorToInt(nowY);
            int cy = Mathf.CeilToInt(nowY);
            if (steep) 
                ret.Add(new Point(fy, (int)nowX));
            else
                ret.Add(new Point((int)nowX, fy));

            if (fy != cy)
            {
                if (steep)
                    ret.Add(new Point(cy, (int)nowX));
                else
                    ret.Add(new Point((int)nowX, cy));
            }
            else if (deltay != 0)
            {
                if (steep)
                {
                    ret.Add(new Point(cy + 1, (int)nowX));
                    ret.Add(new Point(cy - 1, (int)nowX));
                }
                else
                {
                    ret.Add(new Point((int)nowX, cy + 1));
                    ret.Add(new Point((int)nowX, cy - 1));
                }
            }
            nowX += stepX;
            nowY += deltay;
        }
        if (steep)
            ret.Add(new Point(p2.y, p2.x));
        else
            ret.Add(new Point(p2.x, p2.y));
        return ret;
    }

    /// <summary>
    /// 计算2个节点的向量差
    /// </summary>
    /// <param name="targetNode"></param>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    private void floydVector(Node targetNode, Node n1, Node n2)
    {
        targetNode.x = n1.x - n2.x;
        targetNode.y = n1.y - n2.y;
    }

    /// <summary>
    /// 曼哈顿估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private float manhattan(Node node, Node endNode)
    {
        return (Mathf.Abs(node.x - endNode.x) + Mathf.Abs(node.y - endNode.y)) * this.straightCost;
    }
 
    /// <summary>
    /// 几何估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private float euclidian(Node node, Node endNode)
    {
        int dx = node.x - endNode.x;
        int dy = node.y - endNode.y;
        return Mathf.Sqrt(dx * dx + dy * dy) * this.straightCost;
    }
 
    /// <summary>
    /// 对角线估价法
    /// </summary>
    /// <param name="node">当前节点</param>
    /// <param name="endNode">终点</param>
    /// <returns></returns>
    private float diagonal(Node node, Node endNode)
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

public class Point
{
    public int x = 0;
    public int y = 0;
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

