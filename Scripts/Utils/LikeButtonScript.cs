using System.Collections;
using System.Collections.Generic;
using Geogram;
using TMPro;
using UnityEngine;

public class LikeButtonScript : MonoBehaviour
{
    public TextMeshPro likeCounter;
    public SpriteRenderer heartRenderer;
    public Sprite emptyLikedIcon;
    public Sprite filledLikedIcon;
  
    private int LikeCounterInt;
    private bool IsButtonEnabled;
    private bool IsLiked;
    private Vector3 originalHeartSize;
    private float timer;
    private float scale = 1.5f;
    
    private const float Pace = 4f;
    private const float MaxHeartScale = 2.2f;
    

    public bool getIsLiked { get { return IsLiked; } }
    
    // Start is called before the first frame update
    void Start()
    {
        timer = 1f;
        LikeCounterInt = 1; // initialized to 1
        likeCounter.text = LikeCounterInt.ToString();
        likeCounter.color = new Color(0.0f, 0.0f, 0.0f, 0f);
        heartRenderer.sprite = emptyLikedIcon;
        heartRenderer.color = Color.black;
        IsLiked = false;
        IsButtonEnabled = false;
        originalHeartSize = heartRenderer.transform.localScale;
    }

    // Update is called once per frame
    void Update() 
    {
        if (AppManager.Instance.CurrentState == AppState.AR_CAMERA_CREATE)
        {
            
            likeCounter.color = new Color(0.0f, 0.0f, 0.0f, 0f);
            IsButtonEnabled = false;
        } 
        else
        {
            likeCounter.color = Color.black;
            IsButtonEnabled = true;
        }
        if (timer < 1f)
        {
            timer += Time.deltaTime * Pace;
            scale = Mathf.Lerp(MaxHeartScale, 1f, timer);
            heartRenderer.transform.localScale = originalHeartSize * scale;
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            if (!IsLiked)
            {
                IncrementLikeCounter();
            }
            else
            {
                DecrementLikeCounter();
            }
        }
    }
    

    public void IncrementLikeCounter()
    {
        if (IsButtonEnabled)
        {
            LikeCounterInt++;
            likeCounter.text = LikeCounterInt.ToString();
            heartRenderer.sprite = filledLikedIcon; 
            heartRenderer.color = Color.red;
            IsLiked = true;
            timer = 0f;
        }
    }
    
    public void DecrementLikeCounter()
    {
        if (IsButtonEnabled)
        {
            LikeCounterInt--;
            likeCounter.text = LikeCounterInt.ToString();
            heartRenderer.sprite = emptyLikedIcon;
            heartRenderer.color = Color.black;
            IsLiked = false;
            heartRenderer.transform.localScale = originalHeartSize;
        }
    }

    public void HideCounter()
    {
        likeCounter.text = "";
    }
}
