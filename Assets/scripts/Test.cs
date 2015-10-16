using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button btn;
    public Button addBtn;
    public GameObject list;
    private List<TestVo> datalist;
	// Use this for initialization
	void Start ()
    {
        this.addBtn.onClick.AddListener(addBtnHandler);
        this.btn.onClick.AddListener(btnHandler);
        this.datalist = new List<TestVo>();
        for (int i = 0; i < 30; ++i)
        {
            TestVo tVo = new TestVo();
            tVo.name = "name" + i;
            //this.datalist.Add(tVo);
        }
        this.list.GetComponent<ListRenderer>().init(false, datalist.Count, 10, updateListItem);
	}

    private void addBtnHandler()
    {
        TestVo tVo = new TestVo();
        tVo.name = "name" + this.datalist.Count;
        this.datalist.Add(tVo);

        tVo = new TestVo();
        tVo.name = "name" + this.datalist.Count;
        this.datalist.Add(tVo);

        this.list.GetComponent<ListRenderer>().reloadData(this.datalist.Count);
    }

    void btnHandler()
    {
        int index = Random.Range(0, this.datalist.Count - 1);
        print("跳转到index : " + index);
        this.list.GetComponent<ListRenderer>().rollPosByIndex(index);
        return;
        if (this.datalist.Count > 0) 
        {
            index = Random.Range(0, this.datalist.Count - 1);
            TestVo tVo = this.datalist[this.datalist.Count - 1];
            print("remove tVo: " + tVo.name);
            this.datalist.RemoveAt(this.datalist.Count - 1);
            index = Random.Range(0, this.datalist.Count - 1);
            this.datalist.RemoveAt(this.datalist.Count - 1);
            this.list.GetComponent<ListRenderer>().reloadData(this.datalist.Count);
        }
    }

    void updateListItem(GameObject item, int index, bool isReload)
    {
        //print("updateListItem index: " + index + "  this.datalist.Count: " + this.datalist.Count);
        if(index < this.datalist.Count)
        {
            TestVo tVo = this.datalist[index];
            item.GetComponent<ListItem>().txt.text = tVo.name;
        }
    }

	// Update is called once per frame
	void Update () 
    {
	
	}
}

public class TestVo:Object
{
    public string name;
    public TestVo()
    {

    }
}
