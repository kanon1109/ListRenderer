using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
//无限数量的滚动列表
public class ListRenderer : MonoBehaviour
{
    public GameObject content;
    public GameObject scroll;
    //item的预设
    public GameObject itemPrefab;
    //更新的回调方法
    public delegate void UpdateListItem(GameObject item, int index, bool isReload);
    //更新列表回调方法
    private UpdateListItem m_updateItem;  
    //元素宽度
    private float itemWidth;
    //元素高度
    private float itemHeight;
    //列表宽度
    private float listWidth;
    //列表高度
    private float listHeight;
    //显示的数量
    private int showCount;
    //总的数据数量
    private int totalCount;
    //存放item的列表
    private List<GameObject> itemList = null;
    //横向间隔
    private float gapH;
    //纵向间隔
    private float gapV;
    //是否是横向的
    private bool isHorizontal;
    //当前第一个item的索引
    private int curIndex = 0;
    //底部位置
    private float bottom;
    //顶部位置
    private float top;
    //左边位置
    private float left;
    //右边位置
    private float right;
    //是否重新加载
    private bool isReload;
    //上一个位置
    private Vector2 prevV2;
    /// <summary>
    /// 初始化滚动列表
    /// </summary>
    /// <param name="isHorizontal">是否是横向的列表</param>
    /// <param name="count">列表数量</param>
    /// <param name="gap">间隔</param>
    /// <param name="updateItem">每一项的回调</param>
    /// <returns></returns>
    public void init(bool isHorizontal = false,
                     int count = 0, 
                     float gap = 5,
                     UpdateListItem updateItem = null)
    {
        if (count < 0) count = 0;
        if (this.scroll == null) return;
        if (this.content == null) return;
        this.m_updateItem = updateItem;
        this.isHorizontal = isHorizontal;
        //设置组件横向纵向滚动
        this.scroll.GetComponent<ScrollRect>().horizontal = this.isHorizontal;
        this.scroll.GetComponent<ScrollRect>().vertical = !this.isHorizontal;
        this.gapH = gap;
        this.gapV = gap;
        this.listWidth = this.scroll.GetComponent<RectTransform>().sizeDelta.x;
        this.listHeight = this.scroll.GetComponent<RectTransform>().sizeDelta.y;
        this.itemWidth = this.itemPrefab.GetComponent<RectTransform>().sizeDelta.x;
        this.itemHeight = this.itemPrefab.GetComponent<RectTransform>().sizeDelta.y;

        this.prevV2 = new Vector2();
        this.reloadData(count);
        this.isReload = true;
    }

    /// <summary>
    /// 创建item
    /// </summary>
    /// <param name="prefab">item的预设</param>
    /// <returns></returns>
    void createItem(GameObject prefab, int count)
    {
        if (this.itemList == null) this.itemList = new List<GameObject>();
        if (count <= 0) return;
        for (int i = 0; i < count; ++i)
        {
            GameObject item = MonoBehaviour.Instantiate(prefab, new Vector3(0, 0), new Quaternion()) as GameObject;
            item.transform.SetParent(this.content.gameObject.transform);
            item.GetComponent<ListItem>().id = i + 1;
            this.itemList.Add(item);
        }
    }

    /// <summary>
    /// 更新item
    /// </summary>
    /// <returns></returns>
    void updateItem()
    {
        if (!this.isReload) return;
        //坐标系 上正下负
        for (int i = 0; i < this.itemList.Count; ++i)
        {
            GameObject item = this.itemList[i];
            if(!this.isHorizontal)
            {
                //获取item相对于scroll的坐标
                float posY = scroll.transform.InverseTransformPoint(item.transform.position).y;
                /*if(i == 0)
                {
                    print("first posY: " + posY);
                    print("this.top: " + this.top);
                    print("this.curIndex: " + this.curIndex);
                    print("this.totalCount - this.showCount: " + (this.totalCount - this.showCount));
                }
                else if (i == this.itemList.Count - 1)
                {
                    print("last posY: " + posY);
                    print("this.bottom: " + this.bottom);
                }*/
                if (posY > this.top && this.curIndex < this.totalCount - this.showCount)
                {
                    print("change：" + this.itemList.Count);
                    //往上拖动时
                    //如果第一个位置超过顶部范围，并且不是滚动到最后一个，则重新设置位置。 
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject lastItem = this.itemList[this.itemList.Count - 1];
                        item.transform.localPosition = new Vector3(item.transform.localPosition.x,
                                                                   lastItem.transform.localPosition.y - this.itemHeight - this.gapV);
                        this.itemList.Add(item);
                        this.curIndex++;
                    }
                    else
                    {
                        item.transform.localPosition = new Vector3(item.transform.localPosition.x, -this.itemHeight - this.gapV);
                        this.curIndex = 0;
                    }
                    break;
                }
                else if (posY < this.bottom && this.curIndex > 0)
                {
                    //往下拖动时
                    //如果底部位置超过范围,并且不是滚动到第一个位置，则重新设置位置。
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject firstItem = this.itemList[0];
                        item.transform.localPosition = new Vector3(item.transform.localPosition.x,
                                                                   firstItem.transform.localPosition.y + this.itemHeight + this.gapV);
                        this.itemList.Insert(0, item);
                        this.curIndex--;
                    }
                    else
                    {
                        item.transform.localPosition = new Vector3(item.transform.localPosition.x, -this.itemHeight - this.gapV);
                        this.curIndex = 0;
                    }
                    break;
                }
            }
            else
            {
                //获取item相对于scroll的坐标
                float posX = scroll.transform.InverseTransformPoint(item.transform.position).x;
                if (posX < this.left && this.curIndex < this.totalCount - this.showCount)
                {
                    //往上拖动时
                    //如果第一个位置超过顶部范围，并且不是滚动到最后一个，则重新设置位置。
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject lastItem = this.itemList[this.itemList.Count - 1];
                        item.transform.localPosition = new Vector3(lastItem.transform.localPosition.x + this.itemWidth + this.gapH,
                                                                   item.transform.localPosition.y);
                        this.itemList.Add(item);
                        this.curIndex++;
                    }
                    else
                    {
                        item.transform.localPosition = new Vector3(this.itemWidth + this.gapH,
                                                                   item.transform.localPosition.y);
                        this.curIndex = 0;
                    }
                    break;
                }
                else if (posX > this.right && this.curIndex > 0)
                {
                    //往下拖动时
                    //如果底部位置超过范围,并且不是滚动到第一个位置，则重新设置位置。
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject firstItem = this.itemList[0];
                        item.transform.localPosition = new Vector3(firstItem.transform.localPosition.x - this.itemWidth - this.gapH,
                                                                   item.transform.localPosition.y);
                        this.itemList.Insert(0, item);
                        this.curIndex--;
                    }
                    else
                    {
                        item.transform.localPosition = new Vector3(this.itemWidth + this.gapH,
                                                                   item.transform.localPosition.y);
                        this.curIndex = 0;
                    }
                    break;
                }
            }
        }

        //重新调用item回调
        this.reloadItem();
        this.fixItemPos();
    }

    /// <summary>
    /// 重新设置数据
    /// </summary>
    /// <param name="count">当前数据列表的数量</param>
    /// <returns></returns>
    public void reloadData(int count)
    {
        this.isReload = false;
        //保存上一次第一个item的位置
        if (this.itemList != null && 
            this.itemList.Count > 0)
        {
            GameObject item = this.itemList[0];
            this.prevV2.x = item.transform.localPosition.x;
            this.prevV2.y = item.transform.localPosition.y;
        }
        //判断 当前删除的index 是否在 this.curIndex , this.curIndex + this.showCount 之间。
        /*print("this.curIndex + this.showCount:" + (this.curIndex + this.showCount));
        print("count:" + count);
        print("this.curIndex:" + this.curIndex);*/
        //当前显示出来的最后一个item的index
        int curLastIndex = this.curIndex + this.showCount - 1;
        //总的最大index
        int lastIndex = count - 1;
        //防止当前显示的数量溢出
        if (this.curIndex > 0 && curLastIndex > lastIndex)
        {
            //获取溢出数量
            int overCount = curLastIndex - lastIndex;
            this.curIndex -= overCount;
            //补全位置
            this.prevV2.y += (this.itemHeight + this.gapV) * overCount;
            this.prevV2.x -= (this.itemWidth + this.gapH) * overCount;
            //防止去除溢出后 索引为负数。
            if (this.curIndex < 0) this.curIndex = 0;
        }

        //保存上一次显示的数量
        int prevShowCount = this.showCount;
        print("this.curIndex: " + this.curIndex);
        print("showCount: " + showCount);
        //判断当前多出来的数量，并删除。
        this.removeOverItem(count);
        if (!this.isHorizontal) //纵向
            this.showCount = (int)(Mathf.Ceil(this.listHeight / (this.itemHeight + this.gapV))); //计算应该显示的数量
        else
            this.showCount = (int)(Mathf.Ceil(this.listWidth / (this.itemWidth + this.gapH)));
        //需要创建的数量不大于实际数量
        if (this.showCount >= count)
            this.showCount = count; //取实际数据的数量
        else
            this.showCount += 1; //取计算的数量 + 1
        this.totalCount = count;
        print("count: " + count);
        print("prevShowCount: " + prevShowCount);
        print("this.showCount - prevShowCount: " + (this.showCount - prevShowCount));
        //根据显示数量创建item
        this.createItem(this.itemPrefab, this.showCount - prevShowCount);
        //获取最新边界
        this.updateBorder();
        //设置layout可滚动范围的高宽
        if (!this.isHorizontal)
            this.content.GetComponent<RectTransform>().sizeDelta = new Vector2(this.content.GetComponent<RectTransform>().sizeDelta.x, this.totalCount * (this.itemHeight + this.gapV));
        else
            this.content.GetComponent<RectTransform>().sizeDelta = new Vector2(this.totalCount * (this.itemWidth + this.gapH), this.content.GetComponent<RectTransform>().sizeDelta.y);
        //布局
        this.layoutItem();
        //重新调用回调
        this.reloadItem(true);
        this.isReload = true;
    }

    /// <summary>
    /// 重新调用item的回调
    /// </summary>
    /// <returns></returns>
    private void reloadItem(bool isReload = false)
    {
        if (this.itemList.Count > 0)
        {
            int index = 0;
            //print("范围:" + this.curIndex + "--------" + (this.curIndex + this.showCount));
            for (int i = this.curIndex; i < this.curIndex + this.showCount; ++i)
            {
                if (this.itemList[index] != null)
                {
                    GameObject item = this.itemList[index];
                    if (this.m_updateItem != null)
                        this.m_updateItem.Invoke(item, i, isReload);
                    index++;
                }
            }
        }
    }

    /// <summary>
    /// 拖动时修正位置
    /// </summary>
    /// <returns></returns>
    private void fixItemPos()
    {
        //拖动时修正位置
        if (this.itemList.Count > 0)
        {
            GameObject prevItem = this.itemList[0];
            if (this.curIndex == 0) prevItem.transform.localPosition = new Vector3(0, 0);
            for (int i = 1; i < this.itemList.Count; ++i)
            {
                GameObject item = this.itemList[i];
                if (!this.isHorizontal)
                {
                    item.transform.localPosition = new Vector3(item.transform.localPosition.x,
                                                               prevItem.transform.localPosition.y - this.itemHeight - this.gapV);
                }
                else
                {
                    item.transform.localPosition = new Vector3(prevItem.transform.localPosition.x + this.itemWidth + this.gapH,
                                                               item.transform.localPosition.y);
                }
                prevItem = this.itemList[i];
            }
        }
    }

    /// <summary>
    /// item布局
    /// </summary>
    /// <returns></returns>
    void layoutItem()
    {
        if (this.itemList == null) return;
        int count = this.itemList.Count;
        for (int i = 0; i < count; ++i)
        {
            GameObject item = this.itemList[i];
            if (!this.isHorizontal)
                item.transform.localPosition = new Vector3(0, this.prevV2.y - (this.itemHeight + this.gapV) * i);
            else
                item.transform.localPosition = new Vector3(this.prevV2.x + (this.itemWidth + this.gapH) * i, 0);
        }
    }

    //更新
    void Update()
    {
        this.updateItem();
    }

    /// <summary>
    /// 删除多余的item
    /// </summary>
    /// <param name="count">当前应该显示的数量</param>
    /// <returns></returns>
    void removeOverItem(int count)
    {
        if (this.itemList != null && 
            this.itemList.Count > 0)
        {
            //删除多余的item
            if (count < this.showCount)
            {
                //删除 this.showCount - count 个 item
                for (int i = this.showCount - 1; i >= count; --i)
                {
                    GameObject item = this.itemList[i];
                    if (item != null)
                    {
                        GameObject.Destroy(item);
                        this.itemList.RemoveAt(i);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新边界
    /// </summary>
    /// <returns></returns>
    void updateBorder()
    {
        //上下
        this.top = this.itemHeight + this.gapV;
        this.bottom = -(this.itemHeight + this.gapV) * (this.itemList.Count - 1);
        //左右
        this.left = -this.itemWidth - this.gapH;
        this.right = (this.itemWidth + this.gapH) * (this.itemList.Count - 1);
    }

    /// <summary>
    /// 删除所有item
    /// </summary>
    /// <returns></returns>
    public void removeAllItem()
    {
        if (this.itemList == null) return;
        int count = this.itemList.Count;
        for (int i = 0; i < count; ++i)
        {
            GameObject item = this.itemList[i];
            if (item != null) MonoBehaviour.Destroy(item);
        }
    }
}
