using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class Rules : MonoBehaviour
{
    DragAndDrop dad;
    Chess.Chess chess; 
    void Start()
    {

    }
    void Update()
    {
        dad.Action();
    }
    public Rules()
    {
        dad = new DragAndDrop();
        chess = new Chess.Chess();
    }

    void ShowFigures()
    {
        int nr = 0;
        for (int y = 0; y < 8; y++)                             //для каждой клетки на доске
            for (int x = 0; x < 8; x++)
            {
                string figure = chess.GetFigureAt(x, y).ToString();
                if (figure == ".") continue;
                PlaceFigure("box" + nr, figure, x, y);
                nr++;
            }
        //for (; nr < 32; nr++)
        // PlaceFigure("box" + nr, "q", 9, 9);                
    }

    void PlaceFigure(string box, string figure, int x, int y)
    {
        Debug.Log(box + " " + figure + " " + x + y);
        GameObject goBox = GameObject.Find(box);            //нашли бумажку, на которой будем рисовать
        GameObject goFigure = GameObject.Find(figure);      //K R P n b... имя фигуры
        GameObject goSquare = GameObject.Find("" + y + x);  //нашли квадрат, на котором будем рисовать

        var spriteFigure = goFigure.GetComponent<SpriteRenderer>(); //sprite фигуры 
        var spriteBox = goBox.GetComponent<SpriteRenderer>();       //sprite клетки
        spriteBox.sprite = spriteFigure.sprite;                     //передаем спрайт фигуры в клетку

        goBox.transform.position = goSquare.transform.position;     //рисуем клетку на нужном месте

    }

}

class DragAndDrop
{
    enum State
    {
        none,
        drag,
    }
    State state;
    GameObject item;

    public DragAndDrop()
    {
        state = State.none;
        item = null;

    }

    public bool Action()
    {
        switch (state)
        {
            case State.none:
                if (IsMouseButtonPressed())
                    PickUp();
                break;
            case State.drag:
                if (IsMouseButtonPressed())
                    Drag();
                else
                {
                    Drop();
                    return true;
                }
                break;
        }
        return false;
    }

    bool IsMouseButtonPressed()
    {
        return Input.GetMouseButton(0);
    }

    void PickUp()
    {
        Vector2 clickPosition = GetClickPosition();
        Transform clickedItem = GetItemAt(clickPosition);
        if(clickedItem == null)
        {
            Debug.Log("Clicked = NUll");
            return;

        }
        item = clickedItem.gameObject;
        state = State.drag;
        Debug.Log("Clicked " + item.name);
        state = State.drag;
    }

    Transform GetItemAt(Vector2 position)
    {
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);
        if (figures.Length == 0)
        {
            Debug.Log("No item ");
            return null;
        }
            return figures[0].transform;
    }

    Vector2 GetClickPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void Drag()
    {
        item.transform.position = GetClickPosition();
    }

    void Drop()
    {
        state = State.none;
        item = null;
    }

}
















