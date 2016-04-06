using UnityEngine;
using System.Collections;

public class Node
{
    //列号
    public int x = 0;
    //行号
    public int y = 0;
    //权重
    public float f; // f = gone + heuristic
    public float g;
    public float h;
    //是否是障碍
    public bool isBlock = false;
    //显示对象
    public GameObject userData;
    //父级节点
    public Node parentNode;
    /// <summary>
    /// 比较node是否相同
    /// </summary>
    /// <param name="node">需要比较的node</param>
    /// <returns></returns>
    public bool compare(Node node)
    {
        return node.y == this.y && 
               node.x == this.x;
    }

    /// <summary>
    /// 输出字符串
    /// </summary>
    /// <returns></returns>
    public string toString()
    {
        return "x:" + this.x + " y:" + this.y;
    }
}
