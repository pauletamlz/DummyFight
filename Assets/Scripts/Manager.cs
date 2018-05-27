﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public GameObject lThink, lWatch, lAttack, lDodge;
    private ArrayList gtDisplayList;
    private SubItem currentSelected;
    public Transform content;

    private void Awake()
    {
        gtDisplayList = new ArrayList();
		Session.actionList = new ArrayList();
		HistoryManager.instance.OnRedo += CheckActionAndExec;
		HistoryManager.instance.OnUndo += CheckActionAndExec;



    }

	void CheckActionAndExec( ActionType type )
	{
		switch( type )
		{
		case ActionType.AddAttack:
			AddAttack(true);
			break;
		case ActionType.AddDodge:
			AddDodge(true);
			break;
		case ActionType.AddThink:
			AddThink(true);
			break;
		case ActionType.AddWatch:
			AddWatch(true);
			break;
		case ActionType.RemoveAttack: 
			RemoveLink(false, Session.actionList.Count-1);
			break;
		case ActionType.RemoveDodge:
			RemoveLink(false, Session.actionList.Count-1);
			break;
		case ActionType.RemoveThink:  
			RemoveLink(true, Session.actionList.Count-1);
			break;
		case ActionType.RemoveWatch:
			RemoveLink(true, Session.actionList.Count-1);
			break;
		}
	}

    public void Highlight( SubItem item )
    {
        currentSelected = item;
        foreach ( SubItem i in gtDisplayList )
            i.Dark();
        item.Highlight();              

    }

	public void AddSubItem( Transform actBtn, Link link )
	{
		for ( int i = 0; i < 3; i++ )
		{
			GameObject go = actBtn.GetChild(i).gameObject;
			go.SetActive(true);
			Button btn = go.GetComponent<Button>();            
			SubItem item = go.GetComponent<SubItem>();
            item.index = gtDisplayList.Count;
			gtDisplayList.Add(item);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Highlight(item));

            go.GetComponent<SubItem>().link = link;
		}  
	}
    

    public void RemoveLink( bool hasDisplay, int index )
	{
		//index = Session.actionList.Count - 1;
		Destroy( content.GetChild(index).gameObject );
		content.GetComponent<RectTransform>().sizeDelta -= new Vector2(200, 0);

		if ( hasDisplay )
		{
			gtDisplayList.Remove( index );
			gtDisplayList.Remove( index - 1);
			gtDisplayList.Remove( index - 2);
		}

		Session.actionList.RemoveAt(index);
	}

	public void AddWatch( bool fromHistory )
    {
        GameObject act = Instantiate(lWatch, content);
        Button btn = act.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        int count = Session.actionList.Count;
        btn.onClick.AddListener(() => SelectLinkItem(count));
        content.GetComponent<RectTransform>().sizeDelta += new Vector2(200, 0);
        Transform actBtn = act.transform.GetChild(1);
        actBtn.gameObject.SetActive(true);	

		Link newLink = new Link(LinkType.Watch, true);
		Session.actionList.Add(newLink);

        AddSubItem(actBtn, newLink);

        if ( !fromHistory )
		{
			HistoryAction ha = new HistoryAction( ActionType.AddWatch, ActionType.RemoveWatch ); 
			HistoryManager.instance.AddHistory(ha);
		}
    }

    private void SelectLinkItem(int index)
    {
        for( int i = 0; i < content.childCount; i++ )        
            content.GetChild(i).GetComponent<Image>().color = Color.white;
        content.GetChild(index).GetComponent<Image>().color = Color.blue;

        for ( int i = 0; i < Session.actionList.Count; i++ )
        {
            Link l = ( Link )Session.actionList[ index ];
            l.selected = false;
        }

        Link link = (Link)Session.actionList[ index ];
        link.selected = true;

        Session.currentSelected = index;
    }

    public GameObject linkEmpty;
    private void RefreshUI()
    {
        gtDisplayList.Clear();
        for ( int i = 0; i < Session.actionList.Count; i++ )
        {
            GameObject go = content.GetChild(0).gameObject;
            go.transform.SetParent(null);
            Destroy(go);            
        }        

        for( int i = 0; i < Session.actionList.Count; i++ )
        {
            Link link = (Link)Session.actionList[ i ];
            switch ( link.type )
            {
                case LinkType.Attack:
                    GameObject attack = Instantiate(lAttack, content);
                    Button btna = attack.GetComponent<Button>();
                    btna.onClick.RemoveAllListeners();
                    int indexa = i;
                    btna.onClick.AddListener(() => SelectLinkItem(indexa));
                    break;
                case LinkType.Dodge:
                    GameObject dodge = Instantiate(lDodge, content);
                    Button btnd = dodge.GetComponent<Button>();
                    btnd.onClick.RemoveAllListeners();
                    int indexd = i;
                    btnd.onClick.AddListener(() => SelectLinkItem(indexd));
                    break;
                case LinkType.Think:
                    GameObject think = Instantiate(lThink, content);
                    Button btn = think.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    int index = i;
                    btn.onClick.AddListener(() => SelectLinkItem(index));
                    think.transform.GetChild(1).gameObject.SetActive(true);
                    think.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "" + link.caseIdle;
                    think.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = "" + link.caseAttack;
                    think.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = "" + link.caseDodge;
                    AddSubItem(think.transform.GetChild(1), link);
                    break;
                case LinkType.Watch:
                    GameObject watch = Instantiate(lWatch, content);
                    Button btnw = watch.GetComponent<Button>();
                    btnw.onClick.RemoveAllListeners();
                    int indexw = i;
                    btnw.onClick.AddListener(() => SelectLinkItem(indexw));
                    watch.transform.GetChild(1).gameObject.SetActive(true);
                    watch.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "" + link.caseIdle;
                    watch.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = "" + link.caseAttack;
                    watch.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = "" + link.caseDodge;
                    AddSubItem(watch.transform.GetChild(1), link);
                    break;
            }
        }

        content.GetChild(Session.currentSelected).GetComponent<Image>().color = Color.blue;
        
    }

    public void MoveLeft()
    {
        if ( Session.currentSelected - 1 == -1 )
        {
            Mathf.Clamp(Session.currentSelected, 0, Session.actionList.Count - 1);
            return;
        }

        Link pivot = ( Link )Session.actionList[ Session.currentSelected ];
        Session.actionList[ Session.currentSelected ] = Session.actionList[ Session.currentSelected - 1];
        Session.actionList[ Session.currentSelected - 1 ] = pivot;
        Session.currentSelected--;                   
        
        RefreshUI();
    }    

    public void MoveRight()
    {
        if ( Session.currentSelected + 1 == Session.actionList.Count )
        {
            Mathf.Clamp(Session.currentSelected, 0, Session.actionList.Count - 1);
            return;
        }

        Link pivot = ( Link )Session.actionList[ Session.currentSelected ];
        Session.actionList[ Session.currentSelected ] = Session.actionList[ Session.currentSelected + 1 ];
        Session.actionList[ Session.currentSelected + 1 ] = pivot;
        Session.currentSelected++;

        RefreshUI();
    }

    public void AddThink( bool fromHistory )
    {
		GameObject act = Instantiate(lThink, content);
        Button btn = act.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        int count = Session.actionList.Count;
        btn.onClick.AddListener(() => SelectLinkItem(count));
        content.GetComponent<RectTransform>().sizeDelta += new Vector2(200, 0);
		Transform actBtn = act.transform.GetChild(1);
		actBtn.gameObject.SetActive(true);		

		Link newLink = new Link(LinkType.Think, true);
		Session.actionList.Add(newLink);

        AddSubItem(actBtn, newLink);

        if (!fromHistory)
		{
			HistoryAction ha = new HistoryAction( ActionType.AddThink, ActionType.RemoveThink ); 
			HistoryManager.instance.AddHistory(ha);
		}
    }

	public void AddAttack(bool fromHistory)
    {
        GameObject act = Instantiate(lAttack, content);
		content.GetComponent<RectTransform>().sizeDelta += new Vector2(200, 0);
        Button btn = act.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        int count = Session.actionList.Count;
        btn.onClick.AddListener(() => SelectLinkItem(count));

        Link newLink = new Link(LinkType.Attack, true);
		Session.actionList.Add(newLink);        

        if (!fromHistory)
		{
			HistoryAction ha = new HistoryAction( ActionType.AddAttack, ActionType.RemoveAttack ); 
			HistoryManager.instance.AddHistory(ha);
		}
    }

	public void AddDodge(bool fromHistory)
    {
        GameObject act = Instantiate(lDodge, content);
		content.GetComponent<RectTransform>().sizeDelta += new Vector2(200, 0);
        Button btn = act.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        int count = Session.actionList.Count;
        btn.onClick.AddListener(() => SelectLinkItem(count));

        Link newLink = new Link(LinkType.Dodge, true);
		Session.actionList.Add(newLink);

		if (!fromHistory)
		{
			HistoryAction ha = new HistoryAction( ActionType.AddDodge, ActionType.RemoveDodge ); 
			HistoryManager.instance.AddHistory(ha);
		}
    }

    public void PlusClick( bool fromHistory )
    {
        currentSelected.Increase();
        if (!fromHistory)
        {
            HistoryAction ha = new HistoryAction(ActionType.IncreaseValue, ActionType.DecreaseValue);
            HistoryManager.instance.AddHistory(ha);
        }
    }

    public void MinusSign( bool fromHistory )
    {
        currentSelected.Decrease();
        if ( !fromHistory )
        {
            HistoryAction ha = new HistoryAction(ActionType.DecreaseValue, ActionType.IncreaseValue);
            HistoryManager.instance.AddHistory(ha);
        }
    }

}
