using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//无限数量的滚动列表
//注：
//  content的锚点需设为（0，1）
//  listItem的锚点需设为（0，1）
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
    private Vector2 prevItemPos;
    //滚动容器初始位置
    private Vector3 contentStartPos;
    //内容的transform属性
    private RectTransform contentRectTf;
    //滚动组件
    private ScrollRect sr;
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
		this.totalCount = -1;
        if (count < 0) count = 0;
        if (this.scroll == null) return;
        if (this.content == null) return;
        this.m_updateItem = updateItem;
        this.isHorizontal = isHorizontal;
        //设置组件横向纵向滚动
        this.sr = this.scroll.GetComponent<ScrollRect>();
        this.sr.horizontal = this.isHorizontal;
        this.sr.vertical = !this.isHorizontal;
        this.gapH = gap;
        this.gapV = gap;
        this.listWidth = this.scroll.GetComponent<RectTransform>().sizeDelta.x;
        this.listHeight = this.scroll.GetComponent<RectTransform>().sizeDelta.y;
        this.itemWidth = this.itemPrefab.GetComponent<RectTransform>().sizeDelta.x;
        this.itemHeight = this.itemPrefab.GetComponent<RectTransform>().sizeDelta.y;

        //this.scroll.transform.localPosition = new Vector3(-this.listWidth / 2, this.listHeight / 2);
        this.contentRectTf = this.content.GetComponent<RectTransform>();
        this.contentRectTf.sizeDelta = new Vector2(listWidth, listHeight);
        this.contentRectTf.localPosition = new Vector3(0, 0);
        this.prevItemPos = new Vector2();
        this.contentStartPos = this.scroll.transform.localPosition;
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
            item.transform.localScale = new Vector3(1, 1, 1);
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
            Transform itemTf = item.transform;
            if(!this.isHorizontal)
            {
                //获取item相对于scroll的坐标
                float posY = scroll.transform.InverseTransformPoint(itemTf.position).y;
                if (posY > this.top && this.curIndex < this.totalCount - this.showCount)
                {
                    //往上拖动时
                    //如果第一个位置超过顶部范围，并且不是滚动到最后一个，则重新设置位置。 
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject lastItem = this.itemList[this.itemList.Count - 1];
                        itemTf.localPosition = new Vector3(itemTf.localPosition.x,
                                                            lastItem.transform.localPosition.y - this.itemHeight - this.gapV);
                        this.itemList.Add(item);
                        this.curIndex++;
                    }
                    else
                    {
                        itemTf.localPosition = new Vector3(itemTf.localPosition.x, -this.itemHeight - this.gapV);
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
                        itemTf.localPosition = new Vector3(itemTf.localPosition.x,
                                                            firstItem.transform.localPosition.y + this.itemHeight + this.gapV);
                        this.itemList.Insert(0, item);
                        this.curIndex--;
                    }
                    else
                    {
                        itemTf.localPosition = new Vector3(itemTf.localPosition.x, -this.itemHeight - this.gapV);
                        this.curIndex = 0;
                    }
                    break;
                }
            }
            else
            {
                //获取item相对于scroll的坐标
                float posX = scroll.transform.InverseTransformPoint(itemTf.position).x;
                if (posX < this.left && this.curIndex < this.totalCount - this.showCount)
                {
                    //往上拖动时
                    //如果第一个位置超过顶部范围，并且不是滚动到最后一个，则重新设置位置。
                    if (this.itemList.Count > 1)
                    {
                        this.itemList.RemoveAt(i);
                        GameObject lastItem = this.itemList[this.itemList.Count - 1];
                        itemTf.localPosition = new Vector3(lastItem.transform.localPosition.x + this.itemWidth + this.gapH,
                                                            itemTf.localPosition.y);
                        this.itemList.Add(item);
                        this.curIndex++;
                    }
                    else
                    {
                        itemTf.localPosition = new Vector3(this.itemWidth + this.gapH, itemTf.localPosition.y);
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
                        itemTf.localPosition = new Vector3(firstItem.transform.localPosition.x - this.itemWidth - this.gapH,
                                                            itemTf.localPosition.y);
                        this.itemList.Insert(0, item);
                        this.curIndex--;
                    }
                    else
                    {
                        itemTf.localPosition = new Vector3(this.itemWidth + this.gapH, itemTf.localPosition.y);
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
            this.prevItemPos.x = item.transform.localPosition.x;
            this.prevItemPos.y = item.transform.localPosition.y;
        }
        //判断 当前删除的index 是否在 this.curIndex , this.curIndex + this.showCount 之间。
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
            if (!isHorizontal)
                this.prevItemPos.y += (this.itemHeight + this.gapV) * overCount;
            else
                this.prevItemPos.x -= (this.itemWidth + this.gapH) * overCount;
            //防止去除溢出后 索引为负数。
            if (this.curIndex < 0) this.curIndex = 0;
        }

        //保存上一次显示的数量
        int prevShowCount = this.showCount;
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
        //创建数量
        int creatCount = this.showCount - prevShowCount;
        if (creatCount < 0) creatCount = 0;
        //根据显示数量创建item
        this.createItem(this.itemPrefab, creatCount);
        //获取最新边界
        this.updateBorder();
        //总数更新content大小
        this.updateContentSize();
        //修正content的位置
        this.fixContentPos();
        //布局
        this.layoutItem();
        //重新调用回调
        this.reloadItem(true);
        this.isReload = true;
    }

    /// <summary>
    /// 根据创建的总数更新content的大小
    /// </summary>
    private void updateContentSize()
    {
        if (!this.isHorizontal)
            this.contentRectTf.sizeDelta = new Vector2(this.contentRectTf.sizeDelta.x, this.totalCount * (this.itemHeight + this.gapV));
        else
            this.contentRectTf.sizeDelta = new Vector2(this.totalCount * (this.itemWidth + this.gapH), this.contentRectTf.sizeDelta.y);
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
            Transform prevItemTf = prevItem.transform;
            if (this.curIndex == 0) prevItemTf.localPosition = new Vector3(0, 0);
            for (int i = 1; i < this.itemList.Count; ++i)
            {
                GameObject item = this.itemList[i];
                Transform itemTf = item.transform;
                if (!this.isHorizontal)
                {
                    itemTf.localPosition = new Vector3(itemTf.localPosition.x,
                                                        prevItemTf.localPosition.y - this.itemHeight - this.gapV);
                }
                else
                {
                    itemTf.localPosition = new Vector3(prevItemTf.localPosition.x + this.itemWidth + this.gapH,
                                                        itemTf.localPosition.y);
                }
                prevItem = this.itemList[i];
                prevItemTf = prevItem.transform;
            }
        }
    }

    /// <summary>
    /// 修正content的位置
    /// </summary>
    private void fixContentPos()
    {
        if (!this.isHorizontal)
        {
            //防止数量减少后content的位置在遮罩上面
            if (this.contentRectTf.sizeDelta.y <= this.listHeight)
            {
                //如果高度不够但content顶部超过scroll的顶部则content顶部归零对齐
                if (this.contentRectTf.localPosition.y > 0)
                    this.contentRectTf.localPosition = new Vector3(this.contentRectTf.localPosition.x, 0);
            }
            else
            {
                //如果高度足够但content底部超过scroll的底部则content底部对齐scroll的底部
                if (this.contentRectTf.localPosition.y - this.contentRectTf.sizeDelta.y > -this.listHeight)
                    this.contentRectTf.localPosition = new Vector3(this.contentRectTf.localPosition.x,
                                                                    -this.listHeight + this.contentRectTf.sizeDelta.y);
            }
        }
        else
        {
            //防止数量减少后content的位置在遮罩左面
            if (this.contentRectTf.sizeDelta.x <= this.listWidth)
            {
                if (this.contentRectTf.localPosition.x < 0)
                    this.contentRectTf.localPosition = new Vector3(0, this.contentRectTf.localPosition.y);
            }
            else
            {
                if (this.contentRectTf.localPosition.x + this.contentRectTf.sizeDelta.x < this.listWidth)
                    this.contentRectTf.localPosition = new Vector3(this.listWidth - this.contentRectTf.sizeDelta.x,
                                                                    this.contentRectTf.localPosition.y);
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
                item.transform.localPosition = new Vector3(0, this.prevItemPos.y - (this.itemHeight + this.gapV) * i);
            else
                item.transform.localPosition = new Vector3(this.prevItemPos.x + (this.itemWidth + this.gapH) * i, 0);
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
        this.bottom = -(this.itemHeight + this.gapV) * (this.showCount - 1);
        //左右
        this.left = -this.itemWidth - this.gapH;
        this.right = (this.itemWidth + this.gapH) * (this.showCount - 1);
    }

    /// <summary>
    /// 根据索引滚动到相应位置
    /// </summary>
    /// <param name=index>item索引</param>
    /// <returns></returns>
    public void rollPosByIndex(int targetIndex)
    {
        if (this.itemList == null || 
            this.itemList.Count == 0) return;
        this.isReload = false;
        this.sr.StopMovement();
        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex > this.totalCount - 1) targetIndex = this.totalCount - 1;
        Vector3 contentPos = this.contentRectTf.localPosition;
        this.curIndex = targetIndex;
        //计算出第一个索引是多少， 因为第一个curIndex不一定是targetIndex 
        if (targetIndex + this.showCount > this.totalCount)
            this.curIndex -= targetIndex + this.showCount - this.totalCount;
        float gap;
        if(!this.isHorizontal)
        {
            gap = this.itemHeight + this.gapV;
            this.prevItemPos.y = -gap * this.curIndex; //算出移动的距离
            contentPos.y = gap * targetIndex;
        }
        else
        {
            gap = this.itemWidth + this.gapH;
            this.prevItemPos.x = gap * this.curIndex; //算出移动的距离
            contentPos.x = -gap * targetIndex;
        }
        this.contentRectTf.localPosition = contentPos;
        this.layoutItem();
        this.reloadItem(true);
        this.fixContentPos();
        this.isReload = true;
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
