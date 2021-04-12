using System;
using System.Collections;
using System.Collections.Generic;
using Geogram;
using UnityEngine;
using UnityEngine.UI;

public class UIPointer : MonoBehaviour
{
    private RectTransform rect;
    private Image image;
    private Camera camera;
    private Vector2 screenDimension;
    private Transform target;
    private float leftOuterLimit, rightOuterLimit, topOuterLimit, bottomOuterLimit;
    private float leftInnerLimit, rightInnerLimit, topInnerLimit, bottomInnerLimit;

    private const float PinHeightOffset = 0.8f;
    private const float PinWidthLeftThreshold = 1300;
    private const float PinWidthRightThreshold = -150;
    private const float ScreenRenderScaleFactor = 4f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        screenDimension = new Vector2(Screen.width, Screen.height);
        leftOuterLimit = 0.5f * Utils.UI_MEMENTO_POINTER;
        rightOuterLimit = screenDimension.x - 0.5f * Utils.UI_MEMENTO_POINTER;
        bottomOuterLimit = 0.5f * Utils.UI_MEMENTO_POINTER + 0.1f * screenDimension.y;
        topOuterLimit = screenDimension.y - 0.5f * Utils.UI_MEMENTO_POINTER - 0.1f * screenDimension.y;

        leftInnerLimit = leftOuterLimit;// 1f * Utils.UI_MEMENTO_POINTER;
        rightInnerLimit = rightOuterLimit;//screenDimention.x - 1f * Utils.UI_MEMENTO_POINTER;
        bottomInnerLimit = - 2f * screenDimension.y; //1f * Utils.UI_MEMENTO_POINTER;
        topInnerLimit = 3f * screenDimension.y; //screenDimention.y - 1f * Utils.UI_MEMENTO_POINTER;

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (target)
        {
            Vector3 screenPosition = camera.WorldToScreenPoint(target.position + Vector3.up * PinHeightOffset);
            bool isInsideInnerLimit = screenPosition.x > leftInnerLimit && screenPosition.x < rightInnerLimit && screenPosition.y > bottomInnerLimit && screenPosition.y < topInnerLimit;
            bool isWithinMinimumWidthThreshold = PinWidthLeftThreshold > screenPosition.x &&  screenPosition.x > PinWidthRightThreshold;
            
            float deltaX = Mathf.Abs(screenPosition.x - screenDimension.x * 0.5f);
            float maximumRenderDistance = screenDimension.x * ScreenRenderScaleFactor;
           
            image.enabled = !isInsideInnerLimit && !isWithinMinimumWidthThreshold && (deltaX < maximumRenderDistance) && (screenPosition.z > 0);
            
            float scalingFactor = (maximumRenderDistance) / Utils.UI_MEMENTO_POINTER;
            image.rectTransform.sizeDelta = new Vector2((maximumRenderDistance - deltaX)/scalingFactor, (maximumRenderDistance - deltaX)/scalingFactor);
            float angle = Mathf.Atan2((screenDimension.y * 0.5f - screenPosition.y), (screenDimension.x * 0.5f - screenPosition.x));
            screenPosition.x = (screenPosition.x < leftOuterLimit) ? leftOuterLimit : (screenPosition.x > rightOuterLimit) ? rightOuterLimit : screenPosition.x;
            screenPosition.y = (screenPosition.y < bottomOuterLimit) ? bottomOuterLimit : (screenPosition.y > topOuterLimit) ? topOuterLimit : screenPosition.y;
            rect.position = screenPosition;
            rect.rotation = Quaternion.Euler(0f, 0f, angle/Mathf.PI * 180f - 90f);
        }
    }

    public void SetTarget(Transform target, Camera camera)
    {
        this.target = target;
        this.camera = camera;
        gameObject.SetActive(true);
    }
}
