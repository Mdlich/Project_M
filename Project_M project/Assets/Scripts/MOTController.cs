using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MOTController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public event Action<Vector2> ControllerPressed;
	public event Action ControllerReleased;

	[SerializeField] private Transform stick;
	[SerializeField] private float controllerThresholdY = 0.5f;
	[SerializeField] private Vector2 controllerSensitivity;
	[SerializeField] private float controllerDumping = 0.5f;
	[SerializeField] private float damping;
	[SerializeField] private float controllerRangeY;

	public Vector2 ControllerAxis { get; private set; }
	public float Damping => damping;
	public Vector2 ControllerPos => Camera.main.ScreenToWorldPoint( stick.position );
	private bool available = false;
	private bool pressed;

	private void Start()
	{
		if (!PlayerController.instance.MOTAvailable || Application.platform != RuntimePlatform.Android)
		{
			gameObject.SetActive( false );
			return;
		}
		//int index = SceneManager.GetActiveScene().buildIndex;
		/*if (Application.platform != RuntimePlatform.Android || (index != 3 && index != 6))
		{
			gameObject.SetActive( false );
		}*/
		FadeController();
	}
	private void Update()
	{
		if (pressed)
		{
			Process( InputManager.GetPointerScreenPos() );
		}
		var pos = stick.position;
		pos.x = Camera.main.WorldToScreenPoint( PlayerController.instance.transform.position ).x;
		stick.position = pos;
		ControllerAxis = Vector2.Lerp( ControllerAxis, Vector2.zero, controllerDumping );
		if (available && !PlayerController.instance.MassOfTentacles)
		{
			FadeController();
		}
		else if (!available && PlayerController.instance.MassOfTentacles)
		{
			EnableController();
		}
	}

	private void EnableController()
	{
		available = true;
		foreach (var image in GetComponentsInChildren<Image>())
		{
			var c = image.color;
			c.a = 0.5f;
			image.color = c;
		}
	}

	private void FadeController()
	{
		available = false;
		foreach (var image in GetComponentsInChildren<Image>())
		{
			var c = image.color;
			c.a = 0.1f;
			image.color = c;
		}
	}

	public void OnPointerDown( PointerEventData eventData )
	{
		Debug.Log( "pointer down" );
		if (!PlayerController.instance.MassOfTentacles)
			return;
		Debug.Log( "pointer processing" );
		Process( eventData.position );
		if (!pressed)
		{
			Debug.Log( "pointer first pressed" );
			pressed = true;
			ControllerPressed?.Invoke( eventData.position );
		}
	}

	private void Process(Vector2 pPos )
	{
		var stickPos = stick.position;
		var controllerDeltaY = pPos.y - transform.position.y;
		controllerDeltaY = Mathf.Clamp( controllerDeltaY, -controllerRangeY, controllerRangeY );
		var deltaY = controllerDeltaY / controllerRangeY;
		stickPos.y = transform.position.y + controllerDeltaY;
		stick.position = stickPos;
		Vector2 pos = Camera.main.ScreenToWorldPoint( transform.position );
		Vector2 targetrPos = Camera.main.ScreenToWorldPoint( stick.position );
		Vector2 pointerPos = Camera.main.ScreenToWorldPoint( pPos );
		//var directionY = targetrPos - pos;
		var directionX = pointerPos - targetrPos;
		/*if (Mathf.Abs(directionY.y) < controllerThresholdY)
			directionY.y = 0;*/
		if (Mathf.Abs( deltaY ) < controllerThresholdY)
			deltaY = 0;
		ControllerAxis = new Vector2( directionX.x * controllerSensitivity.x, deltaY * controllerSensitivity.y );
	}
	public void OnPointerUp( PointerEventData eventData )
	{
		pressed = false;
		ControllerReleased?.Invoke();
		var resetPos = stick.position;
		resetPos.y = transform.position.y;
		stick.position = resetPos;
	}
}
