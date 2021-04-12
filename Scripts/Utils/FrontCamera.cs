using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrontCamera : MonoBehaviour
{
    private  bool isCameraAvailable;
    private bool playCamera;
    private WebCamTexture frontCameraTexture;
    private Texture defaultBackground;

    public GameObject blackScreen;
    public RawImage background;

    public bool IsPlaying{ get { return playCamera; }}
    public WebCamTexture Texture { get { return frontCameraTexture; }}

    // Start is called before the first frame update
    void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] webCamDevices = WebCamTexture.devices;

        isCameraAvailable = true;
        if (webCamDevices.Length == 0)
        {
            isCameraAvailable = false;
            Debug.Log("No devices");
        }

        for (int i = 0; i < webCamDevices.Length; i++)
        {
            if (webCamDevices[i].isFrontFacing)
                frontCameraTexture = new WebCamTexture(webCamDevices[i].name, Screen.width, Screen.height);
        }
        if (frontCameraTexture)
        {
            background.texture = frontCameraTexture;
            float referenceWidth = 1440f;
            float aspectRatio = Screen.height / Screen.width;
            float scaleFactor = referenceWidth / Screen.width;
            //background.GetComponent<RectTransform>().anchorMin = 0.5f * Vector2.one;
            //background.GetComponent<RectTransform>().anchorMax = 0.5f * Vector2.one;
            Debug.Log("DPI: " + Screen.dpi);
            background.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height * scaleFactor, referenceWidth);
        }

#if UNITY_IOS || PLATFORM_IOS
        background.rectTransform.localScale = new Vector3(-1f, -1f, 0f);
#endif

    }

    void Update()
    {
        //Debug.Log(frontCameraTexture.isPlaying);
        //if (playCamera)
        //    frontCameraTexture.Play();
#if UNITY_EDITOR
        /*
        if(isCameraAvailable)
        {
            if (!playCamera)
                Play();
        }
        else
        {
            if (playCamera)
                Stop();
        }
        */
#endif
    }

    // Update is called once per frame
    public void SetBlackScreen(bool status)
    {
        blackScreen.SetActive(status);
    }

    public void Play()
    {
        Debug.Log("Play front camera");
        background.gameObject.SetActive(true);
        playCamera = true;
        frontCameraTexture.Play();
    }

    public void Stop()
    {
        background.gameObject.SetActive(false);
        playCamera = false;
        frontCameraTexture.Stop();
    }
}
