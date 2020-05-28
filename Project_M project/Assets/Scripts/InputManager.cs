using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    public enum PointerState { Idle, Down, Held, Up };

    private readonly float pointerHoldTime;
    private float timeSincePointerDown = 0;
    private PointerState pointerCurrentState;
    private Vector3? firstPos;
    public Vector3? CurrentPos { private set; get; }
    public bool PointerMovedWhileHeld { get
        {
            if (Input.touchCount == 0)
                return false;
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
                return true;
            if (Input.GetMouseButton(0))
            {
                return (firstPos.HasValue && Input.mousePosition != firstPos.Value);
            }
            return false;
        } }
    public Vector3? Delta { get 
        {
            if (CurrentPos.HasValue && firstPos.HasValue)
                return (CurrentPos - firstPos).Value.normalized;
            else
                return null;
        } }
    public bool PointerHeld { private set; get; }
    public Action<Vector3> PointerDown;
    public Action<Vector3> PointerFirstheld;
    public Action<Vector3> PointerUp;

    public InputManager(float holdTime )
    {
        pointerHoldTime = holdTime;
    }

    public void ProcessInput()
    {
        if (pointerCurrentState == PointerState.Up || pointerCurrentState == PointerState.Idle)
        {
            if (GetPointerDown())
            {
                timeSincePointerDown += Time.deltaTime;
                firstPos = CurrentPos = GetPointerScreenPos();
                PointerDown?.Invoke( firstPos.Value );
                pointerCurrentState = PointerState.Down;
                PointerHeld = false;
                //Debug.Log( "pointer down" );
            }
        }
        if (pointerCurrentState == PointerState.Down)
        {
            if (GetPointerHeld() && firstPos != null)
            {
                timeSincePointerDown += Time.deltaTime;
                if (timeSincePointerDown >= pointerHoldTime)
                {
                    CurrentPos = GetPointerScreenPos();
                    PointerHeld = true;
                    pointerCurrentState = PointerState.Held;
                    PointerFirstheld?.Invoke( CurrentPos.Value );
                    //Debug.Log( "pointer held" );
                }
            }
            if (GetPointerUp())
            {
                CurrentPos = GetPointerScreenPos();
                pointerCurrentState = PointerState.Up;
                PointerUp?.Invoke( CurrentPos.Value );
            }
        }
        if (pointerCurrentState == PointerState.Held)
        {
            CurrentPos = GetPointerScreenPos();
            if (GetPointerUp())
            {
                CurrentPos = GetPointerScreenPos();
                pointerCurrentState = PointerState.Up;
                PointerUp?.Invoke( CurrentPos.Value );
            }
        }
        if (pointerCurrentState == PointerState.Up)
        {
            //Debug.Log( "pointer up" );
            timeSincePointerDown = 0;
            CurrentPos = null;
            firstPos = null;
            pointerCurrentState = PointerState.Idle;
        }
    }
    private bool GetPointerDown()
    {
        if (Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began)
            return true;
        if (Input.GetMouseButtonDown( 0 ))
            return true;

        return false;
    }

    private Vector2 GetPointerScreenPos()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch( 0 );
            return touch.position;
        }
        return Input.mousePosition;
    }

    private bool GetPointerUp()
    {
        if (Input.GetMouseButtonUp( 0 ) || (Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Ended))
        {
            timeSincePointerDown = 0f;
            return true;
        }

        return false;
    }

    private bool GetPointerHeld()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch( 0 );
            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                return true;
            }
        }
        if (Input.GetMouseButton( 0 ))
        {
            return true;
        }

        return false;
    }
}
