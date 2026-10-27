using System;
using UnityEngine;

public class Player : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    void Start()
    {
        InputManager.Instance.OnTapEvent += Tap;
        InputManager.Instance.OnContinuousEvent += HandleContinuousInput;
    }

    private void HandleContinuousInput(Vector2 pos)
    {
        Vector2 spritePos = Camera.main.ScreenToWorldPoint(pos);
        transform.position = new Vector3(spritePos.x, spritePos.y, 0);
    }

    private void Tap(bool isTapped)
    {
        spriteRenderer.color = isTapped ? Color.red : Color.white;
    }

    private void Update()
    {
        
    }
}
