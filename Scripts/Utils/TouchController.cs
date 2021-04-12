using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    private const float HOLD_DISTANCE_SENSITIVITY = 30f;
    private const float MAX_TAP_TIME = 0.1f;
    private const float MIN_HOLD_TIME = 0.8f;
    private const float MAX_SWIPE_TIME = 1.4f;
    private const float MIN_SWIPE_DISTANCE = 0.01f; //PERCENT OF SCREEN HEIGHT

    public Image holdFeedback;

	private enum SwipeDirection{
		UP, DOWN, RIGHT, LEFT
	}

	public bool debug;
	public bool swipe;
	public bool twoSided;
    public bool whileOnHold;
	[SerializeField]
	private bool swipeOnlyOnTouchUp;			//ONLY CHECK SWIPE WHEN TOUCH UP
	[SerializeField]
	private bool swipeTimeLimit;				//LIMIT TIME FOR SWIPE
	private bool check;						//CHECK INPUTS AND UPDATE TIMERS
	private bool touchChecked = false; 				//USED FOR DONT CHECK SWIPE TWICE IF "SWIPE ON TOUCH UP" IS FALSE
	private float touchTimer;				//Time the player is touching screen
    private bool isTap;
    private bool isMove;
    private bool isHold;
    private Vector3 touchDownPosition;
    private bool isDoubleTouch;
    private float doubleTouchFirstDistance;
    private float doubleTouchLastDistance;
    private float doubleTouchLastAngle;
    private Vector3 doubleTouchLastAvgPosition;
    //	private Vector3 screenDimention;

    private Action<Vector3> onTouchDown;	public Action<Vector3> OnTouchDown	{set {onTouchDown = value;}}
	private Action<Vector3> onTouchUp;		public Action<Vector3> OnTouchUp		{set {onTouchUp = value;}}
	private Action<Vector3> onTap;			public Action<Vector3> OnTap			{set {onTap = value;}}
	private Action onLeftTap;		public Action OnLeftTap		{set {onLeftTap = value;}}
	private Action onRightTap;		public Action OnRightTap	{set {onRightTap = value;}}
	private Action onSwipeUp;		public Action OnSwipeUp		{set {onSwipeUp = value;}}
	private Action onSwipeDown;		public Action OnSwipeDown	{set {onSwipeDown = value;}}
	private Action onSwipeLeft;		public Action OnSwipeLeft	{set {onSwipeLeft = value;} }
    private Action onSwipeRight;    public Action OnSwipeRight  {set { onSwipeRight = value; } }
    private Action<Vector3> onMove; public Action<Vector3> OnMove { set { onMove = value; } }
    private Action<Vector3> onHold; public Action<Vector3> OnHold { set { onHold = value; } }
    private Action<float, float> onDoubleTouch; public Action<float, float> OnDoubleTouch { set { onDoubleTouch = value; } }
    private Action onDoubleTouchDown; public Action OnDoubleTouchDown { set { onDoubleTouchDown = value; } }
    private Action<Vector3> onDoubleMove; public Action<Vector3> OnDoubleMove { set { onDoubleMove = value; } }

    public void Init () {
		onTouchDown 	= (Vector3 v) => {if(debug)Debug.Log("ON TOUCH DOWN");};
		onTouchUp 		= (Vector3 v) => {if(debug)Debug.Log("ON TOUCH UP");};
		onTap 			= (Vector3 v) => {if(debug)Debug.Log("ON TAP");};
		onLeftTap 		= () => {if(debug)Debug.Log("ON LEFT TAP");};
		onRightTap 		= () => {if(debug)Debug.Log("ON RIGHT TAP");};
		onSwipeUp 		= () => {if(debug)Debug.Log("ON SWIPE UP");};
		onSwipeDown 	= () => {if(debug)Debug.Log("ON SWIPE DOWN");};
		onSwipeLeft 	= () => {if(debug)Debug.Log("ON SWIPE LEFT");};
        onSwipeRight    = () => {if(debug)Debug.Log("ON SWIPE RIGHT");};
        onMove          = (Vector3 v) => { if (debug) Debug.Log("ON MOVE"); };
        onHold          = (Vector3 v) => { if (debug) Debug.Log("ON HOLD"); };
        onDoubleTouch   = (float d, float a) => { if (debug) Debug.Log("ON DOUBLE TOUCH"); };
        onDoubleTouchDown = () => {if(debug)Debug.Log("ON DOUBLE TOUCH DOWN");};
        onDoubleMove    = (Vector3 v) => { if (debug) Debug.Log("ON DOUBLE MOVE"); };
    }

	void Start () {
		check = true;
//		screenDimention = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
	}
	
	void Update () {
		if (check){
			checkInput();
			updateTimers();
		}
	}
	
	private void checkInput() {
#if UNITY_EDITOR
		if (Input.GetMouseButtonDown (0)) {
			touchDown(Input.mousePosition);
		}
		if (Input.GetMouseButtonUp (0)) {
			touchUp(Input.mousePosition);
		}
#endif

		if (Input.touchCount > 0){
            if(Input.touchCount == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (!isDoubleTouch)
                {
	                onDoubleTouchDown();
                    doubleTouchFirstDistance = GetTouchesDistance(touch1.position, touch2.position);
                    doubleTouchLastDistance = GetTouchesDistance(touch1.position, touch2.position) / doubleTouchFirstDistance;
                    doubleTouchLastAngle = GetTouchesAngle(touch1.position, touch2.position);
                    doubleTouchLastAvgPosition = GetTouchesAvgPosition(touch1.position, touch2.position);
                    isDoubleTouch = true;
                }
                Vector3 newAvgPosition = GetTouchesAvgPosition(touch1.position, touch2.position);
                float newDistance = GetTouchesDistance(touch1.position, touch2.position) / doubleTouchFirstDistance;
                float newAngle = GetTouchesAngle(touch1.position, touch2.position);
                onDoubleMove(newAvgPosition - doubleTouchLastAvgPosition);
                onDoubleTouch(newDistance - doubleTouchLastDistance, newAngle - doubleTouchLastAngle);

                doubleTouchLastAvgPosition = newAvgPosition;
                doubleTouchLastDistance = newDistance;
                doubleTouchLastAngle = newAngle;
            }
            else
            {
                if (isDoubleTouch)
                    return;

                Touch touch = Input.touches[0];
                if (touch.phase == TouchPhase.Began)
                    touchDown(touch.position);
                else if (touch.phase == TouchPhase.Ended)
                    touchUp(touch.position);
                else if (touch.phase == TouchPhase.Stationary)
                    touchHold(touch.position);
                else
                    touchMove(touch.position);
            }
		}
        else if(isDoubleTouch)
            isDoubleTouch = false;
	}

	private void updateTimers(){
		touchTimer += Time.deltaTime;
	}

	private void touchDown(Vector3 position) {
		touchDownPosition = position;
		touchTimer = 0;
        isTap = true;
        isMove = false;
        isHold = false;
		onTouchDown(position);

        if (holdFeedback)
            holdFeedback.rectTransform.position = position;
	}
	
	private void touchUp(Vector3 position)
    {
        if (!touchChecked)
			if(!checkSwipe(position))
                if(isTap)
                    CheckTap(position);

		touchChecked = false;
        onTouchUp(position);

        if (holdFeedback)
            holdFeedback.fillAmount = 0;
    }

    private void touchHold(Vector3 position)
    {
        if (holdFeedback && !isHold && !isMove && touchTimer > MAX_TAP_TIME)
            holdFeedback.fillAmount = (touchTimer - MAX_TAP_TIME) / (MIN_HOLD_TIME - MAX_TAP_TIME);
        else if (holdFeedback)
            holdFeedback.fillAmount = 0;

        if (touchTimer < MIN_HOLD_TIME || isMove)
            return;

        isTap = false;

        if(whileOnHold || !isHold)
            onHold(position);

        isHold = true;
    }

	private void touchMove(Vector3 position)
    {
        //Debug.Log("Touch Move Distace " + GetTouchesDistance(position, touchDownPosition));
        if (!isMove && GetTouchesDistance(position, touchDownPosition) > HOLD_DISTANCE_SENSITIVITY)
        {
            isMove = true;
            if (holdFeedback)
                holdFeedback.fillAmount = 0;
        }
        
        if (touchTimer < MAX_TAP_TIME)
            return;

        isTap = false;
        onMove(position);

		if (!swipeOnlyOnTouchUp && !touchChecked){
			touchChecked = checkSwipe(position);
		}
    }

	private bool checkSwipe(Vector2 end){
		if(!swipe)
			return false;

		float dist = Vector2.Distance (touchDownPosition, end);
		if (((swipeTimeLimit && touchTimer < MAX_SWIPE_TIME) || !swipeTimeLimit) && dist > MIN_SWIPE_DISTANCE * Screen.height){
			switch(checkSwipeDirection(end)){
			case SwipeDirection.RIGHT:
				onSwipeRight();
				break;
			case SwipeDirection.UP:
				onSwipeUp();
				break;
			case SwipeDirection.LEFT:
				onSwipeLeft();
				break;
			case SwipeDirection.DOWN:
				onSwipeDown();
				break;
			}
			Debug.Log("SWIPE!");
			return true;
		}
		return false;
	}
	
	private SwipeDirection checkSwipeDirection(Vector2 end){
		float angle = Mathf.Atan2((end.y - touchDownPosition.y), (end.x - touchDownPosition.x));
		if (Mathf.Abs (angle) < Mathf.PI * 0.25f) {
			return SwipeDirection.RIGHT;	
		}
		else if (Mathf.Abs (angle) > Mathf.PI * 0.75f){
			return SwipeDirection.LEFT;
		}
		else if (angle > 0){
			return SwipeDirection.UP;
		}
		else{
			return SwipeDirection.DOWN;
		}
	}

	private void CheckTap(Vector3 position){
		if(twoSided){
			if (position.x < Screen.width*0.5f)
				onLeftTap();
			else
				onRightTap();
		}
		else
            onTap(position);
	}

    private float GetTouchesDistance(Vector2 t1, Vector2 t2)
    {
        return Vector2.Distance(t1, t2);
    }

    private float GetTouchesAngle(Vector2 t1, Vector2 t2)
    {
        return Mathf.Atan2((t2.y - t1.y), (t2.x - t1.x));
    }

    private Vector3 GetTouchesAvgPosition(Vector2 t1, Vector2 t2)
    {
        return Vector3.Lerp(t1, t2, 0.5f);
    }
}
