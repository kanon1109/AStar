using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PathFinderTest:MonoBehaviour
{
    //格子容器
    public GameObject gridContainer;
    //起始位置
    private float startX = 0;
    private float startY = 0;
    //间隔
    private float gapH = 0.1f;
    private float gapV = 0.1f;
    //格子高宽
    private float gridWidth = 1;
    private float gridHeight = 1;
    //行数
    private float rowNum = 25;
    //列数
    private float columnNum = 25;
    //障碍物的材质
    private Material blockMat = null;
    //默认材质
    private Material defaultMat = null;
    //地图列表
    private List<List<Node>> mapList;
    //网格的列表
    private List<GameObject> gridList;
    //寻路
    private PathFinder pf;
    //创建按钮
    public Button createBtn;
    //起始节点
    private Node startNode;
    //起始节点
    void Start()
    {
        this.pf = new PathFinder();
        this.createBtn.onClick.AddListener(onCreateBtnHandler);
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    private void createMap()
    {
        this.clear();
        if (this.gridList == null)
            this.gridList = new List<GameObject>();
        if (this.mapList == null)
            this.mapList = new List<List<Node>>();
        for (int i = 0; i < this.rowNum; i++)
        {
            List<Node> list = new List<Node>();
            for (int j = 0; j < this.columnNum; j++)
            {
                Node node = new Node();
                node.y = i;
                node.x = j;
                list.Add(node);
                if (Random.RandomRange(0f, 1f) < .2f)
                    node.isBlock = true;

                GameObject gridGo = Resources.Load("prefabs/Grid") as GameObject;
                gridGo = MonoBehaviour.Instantiate(gridGo, new Vector3(0, 0), new Quaternion()) as GameObject;
                gridGo.transform.SetParent(this.gridContainer.transform);
                gridGo.transform.localPosition = new Vector3(this.startX + (j * (this.gridWidth + this.gapH)), 0,
                                                             this.startY + (i * (this.gridHeight + this.gapV)));
                if (this.defaultMat == null)
                    this.defaultMat = gridGo.GetComponent<MeshRenderer>().material;

                if (this.blockMat == null)
                    this.blockMat = Resources.Load("Material/blockMat") as Material;

                node.userData = gridGo;
                if (node.isBlock)
                {
                    //如果是障碍
                    gridGo.transform.localScale = new Vector3(1, 1, 1);
                    gridGo.GetComponent<MeshRenderer>().material = this.blockMat;
                }
                else
                {
                    gridGo.transform.localScale = new Vector3(1, .1f, 1);
                    gridGo.GetComponent<MeshRenderer>().material = this.defaultMat;
                }
                this.gridList.Add(gridGo);
            }
            mapList.Add(list);
        }
        this.startNode = this.mapList[0][0];
        this.pf.init(this.mapList);
    }

    /// <summary>
    /// 清理
    /// </summary>
    private void clear()
    {
        if (this.mapList != null)
            this.mapList.Clear();

        if (this.gridList != null)
        {
            int count = this.gridList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameObject gridGo = this.gridList[i];
                GameObject.Destroy(gridGo);
            }
            this.gridList.Clear();
        }
    }

    private void onCreateBtnHandler()
    {
        this.createMap();
    }

    void Update()
    {
        //鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) 
            {
                //TODO 根据点击的grid 获取对应的node 
                Debug.Log(hit.transform.localPosition);
                Vector3 pos = hit.transform.localPosition;
                int x = Mathf.FloorToInt(pos.x / (this.gridWidth + this.gapH));
                int y = Mathf.FloorToInt(pos.z / (this.gridHeight + this.gapV));
                Debug.Log("c" + x + " r" + y);
                Node node = this.mapList[y][x];
                if (node.isBlock) return;
                Debug.Log("node.x " + node.x + " node.y " + node.y);
                //寻路
                long now = DateUtil.getCurTimeStamp();
                List<Node> pathList = this.pf.findPath(node, this.startNode);
                long time = DateUtil.getCurTimeStamp() - now;
                Debug.Log("time: " + time);

                if (pathList != null)
                {
                    int length = pathList.Count;
                    for (int i = 0; i < length; i++)
                    {
                        node = pathList[i];
                        node.userData.GetComponent<MeshRenderer>().material = this.blockMat;
                    }
                }
            }
        }
    }

}
