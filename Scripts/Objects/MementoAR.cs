using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Geogram;
using UnityEngine;
using Mapbox.Unity.Utilities;
using TMPro;

public class MementoAR : MonoBehaviour
{
    public GameObject arPin;
    public GameObject arContent;
    public GameObject framePivot;
    public GameObject frame;
    public SpriteRenderer photoSprite;
    public MeshRenderer photoRenderer;
    public MeshRenderer frameRenderer;
    public TextMeshPro comment;
    public TextMeshPro charCount;
    public LikeButtonScript likeButton;
    private Transform lookTarget;
    private new SphereCollider collider;
    private float timer;
    private float pace;
    private float yRotation;
    private float targetRotation;
    private float yPosition = 0f;
    private float targetHeight;
    private float scale = 0.3f;
    private Vector3 frameStartPosition;
    private Vector3 pinStartPosition;
    private SceneObject[] sceneObjects;
    private bool isPinModeActive;
    private bool isFrameModeActive;
    private TouchScreenKeyboard keyboard;

    public Material Frame { set { frameRenderer.material = value; }}
    public bool getIsFrameModeActive { get { return isFrameModeActive; } }

    private Memento data;

    public Vector3 Coordinates { get { return Conversions.StringToLatLon(data.coordinates).ToVector3xz(); } }

    private void Awake()
    {
        timer = 1f;
        collider = GetComponent<SphereCollider>();
        arContent.SetActive(false);
        isPinModeActive = true;
        frameStartPosition = framePivot.transform.localPosition;
        pinStartPosition = arPin.transform.localPosition;
    }

    private void Start()
    {
        /*
        if (data)
            SetData(data, null);
*/
    }

    private void Update()
    {
        if (timer < 1f)
        {
            timer += Time.deltaTime * pace;
            yRotation = Mathf.Lerp(yRotation, targetRotation, timer);
            yPosition = Mathf.Lerp(yPosition, targetHeight, timer);
            if (isFrameModeActive)
            {
                scale = Mathf.Lerp(scale, 1.3f, 2 * timer);
                frame.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
                framePivot.transform.localPosition = /*createPosition*/ frameStartPosition + Vector3.up * yPosition;
                arPin.transform.localPosition = pinStartPosition + Vector3.up * yPosition * 2f;
                frame.transform.localScale = Vector3.one * scale;
            }
            else if (isPinModeActive)
            {
                arPin.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
                arPin.transform.localPosition = Vector3.Lerp(arPin.transform.localPosition, new Vector3(0f, 0.5f, 0f), timer);
                framePivot.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }
        else if (lookTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(framePivot.transform.position - lookTarget.position, Vector3.up);
            targetRotation.eulerAngles = new Vector3(0f, targetRotation.eulerAngles.y, 0f);
            Quaternion frameRotation = framePivot.transform.rotation;
            float deltaAngle = Mathf.Pow(frameRotation.eulerAngles.y - targetRotation.eulerAngles.y, 2);
            framePivot.transform.rotation = Quaternion.RotateTowards(frameRotation, targetRotation, deltaAngle * Utils.BILLBOARD_SPEED * Time.deltaTime);
        }

        if (TouchScreenKeyboard.visible)
        {
            if (keyboard == null)
            {
                keyboard = TouchScreenKeyboard.Open(comment.text);
                keyboard.text = comment.text;
            }
                

            charCount.gameObject.SetActive(true);
            
            if (keyboard.text.Length > Utils.MAX_NUMBER_CHARACTER)
            {
                charCount.color = Color.red;
            }
            else
            {
                charCount.color = Color.black;
                comment.text = keyboard.text;
            }

            charCount.text = keyboard.text.Length + "/" + Utils.MAX_NUMBER_CHARACTER;
        }
        else
        {
            charCount.gameObject.SetActive(false);
            keyboard = null;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
            OnClick();
#endif
    }

    public void SetData(Memento data, SceneObject[] sceneObjectsPrefabs)
    {
        this.data = data;
        //Create the Sprite component to show the picture
        Texture2D picture = data.picture;
        if (picture)
        {
            photoRenderer.material.SetTexture("_MainTex", picture);
            float pixelPerUnit = (picture.width < picture.height) ? picture.height : picture.width;
            photoSprite.sprite = Sprite.Create(data.picture, new Rect(0, 0, data.picture.width, data.picture.height), 0.5f * Vector2.one, pixelPerUnit);
        }
        //Write message behind picture
        comment.text = data.message;
        //Create scene
        if (sceneObjectsPrefabs != null)
            sceneObjects = SceneObject.InstatiateFromJSON(arContent.transform, sceneObjectsPrefabs, data.SceneObjectsJSON);
    }

    public void SetData(Texture2D photo, bool isPreview = false)
    {
        //Create the Sprite component to show the picture
        Texture2D picture = photo;
        if (picture)
        {
            photoRenderer.material.SetTexture("_MainTex", picture);
            float pixelPerUnit = (picture.width < picture.height) ? picture.height : picture.width;
            photoSprite.sprite = Sprite.Create(photo, new Rect(0, 0, photo.width, photo.height), 0.5f * Vector2.one, pixelPerUnit);
        }
        //Write message behind picture
        comment.text = "";

        if (isPreview)
        {
            //arPin.SetActive(false);
            //arContent.SetActive(true);
            //collider.center = new Vector3(0f, 1f, 0f);
            isPinModeActive = true;
            OnClick();
            likeButton.HideCounter();
            collider.enabled = false;
        }
    }

    public void OnClick()
    {
        // set this memento to active
        // (in camera mgr, loop through all mementoAR and fold up those that are inactive)
        yPosition = 0;
        if (isPinModeActive)
        {
            frame.transform.localScale = Vector3.one * scale;
            collider.center = new Vector3(0f, 1.4f, 0f);
            arPin.SetActive(false);
            arContent.SetActive(true);
            isFrameModeActive = true;
            isPinModeActive = false;
            //createPosition = transform.position;
            pace = 1f / 2f;
            timer = 0f;
            targetRotation = 720f;
            targetHeight = 0.4f;
        }
        else if (isFrameModeActive)
        {
            collider.center = new Vector3(0f, 0.75f, 0f);
            arPin.SetActive(true);
            arContent.SetActive(false);
            isFrameModeActive = false;
            isPinModeActive = true;
            pace = 1f / 2f;
            timer = 0f;
            targetRotation = 0f;
            targetHeight = 0.75f;

        }
        else
        {
            frame.transform.localScale = Vector3.one * scale;
            pace = 1f / Utils.FRAME_ROTATION_DURATION;
            timer = 0f;
            targetRotation += 180f;
        }
    }

    public void Follow(Transform target)
    {
        lookTarget = target;
    }
}

