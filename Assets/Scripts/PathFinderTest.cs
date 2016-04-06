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
    //地图
    private GridMap gm;
    //障碍物的材质
    private Material blockMat = null;
    //默认材质
    private Material defaultMat = null;
    //蓝色材质
    private Material blueMat = null;
    //网格的列表
    private List<GameObject> gridList;
    //寻路
    private PathFinder pf;
    //创建按钮
    public Button createBtn;
    //是否平滑按钮
    public Button floydBtn;
    //起始位置
    private Vector2 startPos;
    //寻找到的路径节点列表
    private List<Node> pathList;
    //角色
    private GameObject role;
    //速度
    private float speed = .2f;
    //路径索引
    private int pathIndex = 1;
    //是否平滑
    private bool isFloyd = true;
    //起始节点
    void Start()
    {
        this.gm = new GridMap();
        this.pf = new PathFinder();
        this.createBtn.onClick.AddListener(onCreateBtnHandler);
        this.floydBtn.onClick.AddListener(onFloydBtnHandler);
    }

    private void onFloydBtnHandler()
    {
        string str = "平滑路径";
        this.isFloyd = !this.isFloyd;
        if (!this.isFloyd) str = "不平滑路径";
        floydBtn.GetComponentInChildren<Text>().text = str;
    }

    /// <summary>
    /// 创建地图
    /// </summary>
    private void createMap()
    {
        this.clear();

        if (this.role == null)
        {
            GameObject roleGo = Resources.Load("prefabs/role") as GameObject;
            this.role = MonoBehaviour.Instantiate(roleGo, new Vector3(0, 0), new Quaternion()) as GameObject;
            this.role.transform.SetParent(this.gridContainer.transform);
        }

        this.gm.create(20, 30, 1,  1, .1f, .1f);
        if (this.gridList == null)
            this.gridList = new List<GameObject>();
        for (int i = 0; i < this.gm.rows; i++)
        {
            for (int j = 0; j < this.gm.columns; j++)
            {
                Node node = this.gm.getNode(i, j);

                if (i > 0 && j >0 && Random.Range(0f, 1f) < .2f)
                    node.isBlock = true;

                GameObject gridGo = Resources.Load("prefabs/Grid") as GameObject;
                gridGo = MonoBehaviour.Instantiate(gridGo, new Vector3(0, 0), new Quaternion()) as GameObject;
                gridGo.transform.SetParent(this.gridContainer.transform);
                gridGo.transform.localPosition = new Vector3(this.startX + (j * (this.gm.gridWidth + this.gm.gapH)), 0,
                                                             this.startY + (i * (this.gm.gridHeight + this.gm.gapV)));
                if (this.defaultMat == null)
                    this.defaultMat = gridGo.GetComponent<MeshRenderer>().material;

                if (this.blockMat == null)
                    this.blockMat = Resources.Load("Material/blockMat") as Material;

                if (this.blueMat == null)
                    this.blueMat = Resources.Load("Material/roleMat") as Material;

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
        }
        this.startPos = this.gm.getPosByNodeIndex(0, 0);
        this.role.transform.localPosition = new Vector3(startPos.x, 
                                                        this.role.transform.localPosition.y, 
                                                        startPos.y);
        this.pf.init(this.gm);
    }

    /// <summary>
    /// 清理
    /// </summary>
    private void clear()
    {
        this.gm.clear();
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
        if (this.pathList != null) 
            this.pathList.Clear();
    }

    private void onCreateBtnHandler()
    {
        this.createMap();
    }

    /// <summary>
    /// 角色移动
    /// </summary>
    private void roleMove()
    {
        if (this.gm == null) return;
        if (this.pathList == null || this.pathList.Count == 0) return;
        if (this.pathIndex <= this.pathList.Count - 1)
        {
            Node node = this.pathList[this.pathIndex];
            Vector2 v2 = this.gm.getPosByNodeIndex(node.y, node.x);
            Debug.Log(this.role.transform.localPosition);
            Vector2 roleV2 = new Vector2(this.role.transform.localPosition.x, 
                                         this.role.transform.localPosition.z);
            float dis = Vector2.Distance(roleV2, v2);

            if (dis < this.speed)
                this.speed = dis;
            else 
                this.speed = .1f;

            Debug.Log("dis " + dis);
            if (dis > this.speed)
            {
                //求出角度
                float angle = Mathf.Atan2(v2.y - this.startPos.y, 
                                          v2.x - this.startPos.x);

                float vx = Mathf.Cos(angle) * this.speed;
                float vy = Mathf.Sin(angle) * this.speed;

                this.role.transform.localPosition = new Vector3(this.role.transform.localPosition.x + vx,
                                                                this.role.transform.localPosition.y,
                                                                this.role.transform.localPosition.z + vy);


            }
            else
            {
                this.startPos = v2;
                this.role.transform.localPosition = new Vector3(v2.x,
                                                                this.role.transform.localPosition.y,
                                                                v2.y);
                this.pathIndex++;
            }
        }
    }

    void Update()
    {
        this.roleMove();
        //鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) 
            {
                //根据点击的grid 获取对应的node 
                Debug.Log(hit.transform.localPosition);

                this.startPos = new Vector2(this.role.transform.localPosition.x,
                                            this.role.transform.localPosition.z);

                Vector3 pos = hit.transform.localPosition;
                Vector2 targetPos = new Vector2(pos.x, pos.z);

                //寻路
                long now = DateUtil.getCurTimeStamp();
                List<Node> list = this.pf.findPath(this.startPos, targetPos, this.isFloyd);
                if (list == null) return;
                Node node;
                int length;
                if (this.pathList != null)
                {
                    length = pathList.Count;
                    for (int i = 0; i < length; i++)
                    {
                        node = pathList[i];
                        if (node.userData != null)
                            node.userData.GetComponent<MeshRenderer>().material = this.defaultMat;
                    }
                }
                this.pathList = list;
                this.pathIndex = 1;
                long time = DateUtil.getCurTimeStamp() - now;
                Debug.Log("用时：" + time + "毫秒");
                length = this.pathList.Count;
                for (int i = 0; i < length; i++)
                {
                    node = pathList[i];
                    node.userData.GetComponent<MeshRenderer>().material = this.blueMat;
                }
            }
        }
    }

}
