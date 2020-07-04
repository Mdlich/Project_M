using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public enum PointerState { Idle, Down, Held, Up };

    private PointerState pointerCurrentState;
    private Vector3? firstPos;
    public static Vector3? CurrentPos { private set; get; }
    public bool PointerHeld { private set; get; }
    public static event Action<Vector3> PointerDown;
    public static event Action<Vector3> PointerFirstheld;
    public static event Action<Vector3> PointerUp;

    public void ProcessInput()
    {
        if (GameManager.GamePaused || EventSystem.current.IsPointerOverGameObject())
            return;

        if (pointerCurrentState == PointerState.Up || pointerCurrentState == PointerState.Idle)
        {
            if (GetPointerDown())
            {
                firstPos = CurrentPos = GetPointerScreenPos();
                PointerDown?.Invoke( firstPos.Value );
                pointerCurrentState = PointerState.Down;
                PointerHeld = false;
            }
        }
        if (pointerCurrentState == PointerState.Down)
        {
            if (GetPointerHeld() && firstPos != null)
            {
                CurrentPos = GetPointerScreenPos();
                PointerHeld = true;
                pointerCurrentState = PointerState.Held;
                PointerFirstheld?.Invoke( CurrentPos.Value );
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
            CurrentPos = null;
            firstPos = null;
            pointerCurrentState = PointerState.Idle;
        }
    }
    public static bool GetPointerDown()
    {
        if (Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Began)
            return true;
        if (Input.GetMouseButtonDown( 0 ))
            return true;

        return false;
    }

    public static Vector2 GetPointerScreenPos()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch( 0 );
            return touch.position;
        }
        return Input.mousePosition;
    }

    public static bool GetPointerUp()
    {
        if (Input.GetMouseButtonUp( 0 ) || (Input.touchCount > 0 && Input.GetTouch( 0 ).phase == TouchPhase.Ended))
        {
            return true;
        }

        return false;
    }

    public static bool GetPointerHeld()
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

	internal static bool PointInRange( Vector3 position )
	{
		throw new NotImplementedException();
	}
}
