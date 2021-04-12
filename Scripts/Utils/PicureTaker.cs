using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PicureTaker : MonoBehaviour{

    private new SpriteRenderer renderer;
	// Use this for initialization
	void Start () {
        renderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakePicture(WebCamTexture webCamTexture, Action<Texture2D> textureCallback, bool squareCrop)
    {
        /*
        WebCamTexture webCamTexture = new WebCamTexture();
        renderer.material.mainTexture = webCamTexture;
        webCamTexture.Play();
        */

        StartCoroutine(SavePicture(webCamTexture, textureCallback, squareCrop));
    }

    public void TakeScreenShot(Camera camera, Action<Texture2D> textureCallback, bool squareCrop)
    {
        StartCoroutine(SaveScreenShot(camera, textureCallback, squareCrop));
    }

    private IEnumerator SaveScreenShot(Camera camera, Action<Texture2D> textureCallback, bool squareCrop)
    {
        yield return new WaitForEndOfFrame();

        int resWidth = camera.pixelWidth;
        int resHeight = camera.pixelHeight;

        //RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        //camera.targetTexture = rt;
        //camera.Render();
        //RenderTexture.active = rt;
        Texture2D screenShot;
        if (squareCrop)
        {
            screenShot = new Texture2D(resWidth, resWidth, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, resHeight / 2 - resWidth / 2, resWidth, resWidth), 0, 0);
        }
        else
        {
            screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        }
        screenShot.Apply();
        ////camera.targetTexture = null;
        //RenderTexture.active = null;
        //Destroy(rt);

        textureCallback(screenShot);
    }

    private IEnumerator SavePicture(WebCamTexture webCamTexture, Action<Texture2D> textureCallback, bool squareCrop)
    {
#if UNITY_EDITOR
        yield return new WaitForSeconds(2f);
#elif UNITY_ANDROID
        yield return new WaitForSeconds(1f);
#endif
        yield return new WaitForEndOfFrame();

        int resWidth = webCamTexture.width;
        int resHeight = webCamTexture.height;

        if(resWidth > resHeight)
        {
            int aux = resHeight;
            resHeight = resWidth;
            resWidth = aux;
        }

        Texture2D photo;
        Debug.Log("Taking Front picture?");
        if (squareCrop)
        {
            photo = new Texture2D(resWidth, resWidth, TextureFormat.RGB24, false);
            photo.SetPixels(webCamTexture.GetPixels(0, resHeight / 2 - resWidth / 2, resWidth, resWidth));
        }
        else
        {
            photo = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            photo.SetPixels(webCamTexture.GetPixels());
        }
        //photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        byte[] bytes = photo.EncodeToPNG();
        Debug.Log("Photo Taken");
        textureCallback(photo);
    }
}
