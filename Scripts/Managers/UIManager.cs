using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geogram;
using System.IO;

public class UIManager : MonoBehaviour
{
    public UIPointer pointerPrefab;
    public Transform miscPanel;
    public GameObject splashScreenPanel;
    public GameObject logInPanel;
    public GameObject mainMenuPanel;
    public GameObject cameraScanUI;
    public GameObject cameraScanMessageUI;
    public GameObject cameraScanButtons;
    public Image cameraScanPhotoView;
    public GameObject cameraCreateUI;
    public GameObject cameraCreateScanUI;
    public GameObject cameraCreateSceneMenuUI;
    public GameObject cameraCreateScenePageantryUI;
    public GameObject cameraCreateSceneConfirmUI;
    public GameObject cameraCreatePageantryUI;
    public GameObject cameraCreatePgtryMenu;
    public GameObject cameraCreatePgtryBirthday;
    public GameObject cameraCreatePgtryFun;
    public GameObject serverIcon;
    [SerializeField]
    private Text debugLog;
    private AppManager appManager;
    private int currentGroup;
    private int currentPageantry;
    private int amountPointers;
    private UIPointer[] pointers;
    private UIFeedback uiFeedback;


    public void Initialize(AppManager appManager)
    {
        Vector2 referenceRes = GetComponent<CanvasScaler>().referenceResolution;
        referenceRes.x *= Screen.dpi / 420f;
        GetComponent<CanvasScaler>().referenceResolution = referenceRes;
        uiFeedback = FindObjectOfType<UIFeedback>();
        this.appManager = appManager;
        pointers = new UIPointer[40];
        for (int i = 0; i < pointers.Length; i++)
        {
            pointers[i] = Instantiate(pointerPrefab, miscPanel) as UIPointer;
        }
    }

    public void DebugLog(string text)
    {
        debugLog.text += "\n" + text;
    }

    #region BUTTONS

    public void BtnMmOpenCamera()
    {
        appManager.ShowCameraCreate();
    }

    public void BtnMmSettings()
    {

    }

    public void BtnMmFavorite()
    {

    }

    public void BtnMmGift()
    {

    }

    public void BtnMmProfile()
    {

    }
    #endregion

    #region BACK_BUTTON

    public void BackToMap()
    {
        appManager.ShowMap();
    }

    public void BackPageantry() //I created this function because the pageantry menu use the same Back Arrow, but does different things
    {
        if (cameraCreatePgtryMenu.activeInHierarchy)
            BackToTakePicture();
        else
            BackToPageantryGroupMenu();
    }

    public void BackToScanCamera()
    {
        ShowScanMenu();
        //cameraScanPhotoView.gameObject.SetActive(false);
        //cameraScanButtons.SetActive(true);
        //SetHints(true);
    }

    public void BackToTakePicture()
    {
        TakePictureScreen();
        appManager.HidePolaroidPreviews();
    }

    public void BackToPolaroidPreview()
    {
        PolaroidPreviewScreen();
        //appManager.HidePolaroidPreviews();
    }

    public void BackToPageantryGroupMenu()
    {
        //ShowPageantryGroupMenu(-1);
        cameraCreatePgtryMenu.SetActive(true);
    }

    public void BackToSceneMenu()
    {
        SceneMenuScreen();
    }

    #endregion

    public void ShowServerIcon(bool state)
    {
        serverIcon.SetActive(state);
    }

    #region SPLASH_SCREEN

    public void ShowSplashScreen(bool state)
    {
        splashScreenPanel.SetActive(state);
    }

    #endregion

    #region LOG_IN

    public void ShowLogInScreen(bool state)
    {
        logInPanel.SetActive(state);
    }

    #endregion

    #region MAP

    public void ShowMainMenuUI(bool state)
    {
        mainMenuPanel.SetActive(state);
    }

    #endregion

    #region CAMERA_SCAN

    public GameObject[] cameraScanScreens;

    private const int SCAN_MESSAGE = 0;
    private const int SCAN_MENU = 1;
    private const int PHOTO_VIEW = 2;

    public void ShowCameraScanUI(bool state)
    {
        cameraScanUI.SetActive(state);
    }

    private void ShowScanScreen(int screen)
    {
        for (int i = 0; i < cameraScanScreens.Length; i++)
        {
            cameraScanScreens[i].SetActive(i == screen);
        }
        SetHints(screen == SCAN_MENU);
    }

    public void ShowScanMessage()
    {
        ShowScanScreen(SCAN_MESSAGE);
    }

    public void ShowScanMenu()
    {
        ShowScanScreen(SCAN_MENU);
    }

    public void ShowPictureView()
    {
        ShowScanScreen(PHOTO_VIEW);
    }

    /*
    public void ShowCameraScanMessage(bool state)
    {
        cameraScanMessageUI.SetActive(state);
        cameraScanButtons.SetActive(!state);
        SetHints(state);
    }
    */

    public void BtnTakePictureScan()
    {
        ShowScanScreen(-1);
        //SetHints(false);
        //ShowCameraScanMessage(false);
        //cameraScanButtons.SetActive(false);
        appManager.TakePicture(true);
    }

    public void BtnDownloadPicture()
    {
        appManager.DownloadPicture();
        uiFeedback.ShowFeedback("SAVED");
    }

    public void ShowPhoto(Texture2D photo)
    {
        cameraScanPhotoView.sprite = Sprite.Create(photo, new Rect(0f, 0f, photo.width, photo.height), 0.5f * Vector2.one);
        ShowPictureView();
    }

    public void ShowMementoHints(MementoAR[] mementos, Camera camera)
    {
        amountPointers = mementos.Length;
        for (int i = 0; i < mementos.Length; i++)
        {
            Debug.Log("SETTING TARGET");
            pointers[i].SetTarget(mementos[i].transform, camera);
        }
    }

    public void SetHints(bool state)
    {
        for (int i = 0; i < amountPointers; i++)
        {
            pointers[i].gameObject.SetActive(state);
        }
    }

    #endregion

    #region CAMERA_CREATE

    public GameObject[] cameraCreateScreens;
    public Image[] sceneObjectsCategoriesMenu;
    public GameObject[] sceneObjectsCategoriesContent;

    private const int TAKE_PICTURE      = 0;
    private const int POLAROID_PREVIEW  = 1;
    private const int PICK_FRAME        = 2;
    private const int SCAN_GROUND       = 3;
    private const int SCENE_MENU        = 4;
    private const int SCENE_PAGEANTRY   = 5;
    private const int SCENE_CONFIRM     = 6;

    private void ShowCreateScreen(int screen)
    {
        for (int i = 0; i < cameraCreateScreens.Length; i++)
        {
            cameraCreateScreens[i].SetActive(i == screen);
        }
    }

    public void ShowCameraCreatePanel(bool state)
    {
        HideAllCameraCreateScreen();
        cameraCreateUI.SetActive(state);
    }

    public void HideAllCameraCreateScreen()
    {
        ShowCreateScreen(-1);
    }

    public void TakePictureScreen()
    {
        ShowCreateScreen(TAKE_PICTURE);
    }

    public void PolaroidPreviewScreen()
    {
        ShowCreateScreen(POLAROID_PREVIEW);
    }

    public void PickFrameScreen()
    {
        ShowCreateScreen(PICK_FRAME);
    }

    public void ScanGroundScreen()
    {
        ShowCreateScreen(SCAN_GROUND);
    }

    public void SceneMenuScreen()
    {
        ShowCreateScreen(SCENE_MENU);
    }

    public void ScenePageantryScreen()
    {
        ShowCreateScreen(SCENE_PAGEANTRY);
    }

    public void SceneConfirmScreen()
    {
        ShowCreateScreen(SCENE_CONFIRM);
    }

    public void SelectSceneObjectCategory(int index)
    {
        for (int i = 0; i < sceneObjectsCategoriesMenu.Length; i++)
        {
            sceneObjectsCategoriesMenu[i].color = (i == index) ? Utils.SCENE_CATEGORY_ACTIVE : Utils.SCENE_CATEGORY_DEACTIVE;
            sceneObjectsCategoriesContent[i].SetActive(i == index);
        }
    }

    public void BtnTakePicture()
    {
        HideAllCameraCreateScreen();
        appManager.TakePicture();
    }

    public void BtnPgtryMenu(int index)
    {
        //ShowPageantryGroupMenu(index);
    }

    public void BtnFrameOpt(int index)
    {
        currentPageantry = index;
        appManager.ChangeFrame(currentPageantry);
    }

    public void BtnStartCreatingScene()
    {
        ScanGroundScreen();
        appManager.StartCreatingScene();
    }

    public void BtnSendMemento()
    {
        //ShowPageantryGroupMenu(-1);
        //ShowPageantryMenu(false);

        PrivacySetting privacySetting = GetPrivacySetting();
        string friends = "";
        if (privacySetting == PrivacySetting.PRIVATE)
            friends = GetSelectedFriends();

        appManager.SendMemento(currentPageantry, privacySetting, friends);
    }

    public void BtnRotateCamera()
    {
        appManager.RotateCamera();
    }

    public void BtnOpenGallery()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(GetImageFromGallery);
        Debug.Log(permission);
        if (permission == NativeGallery.Permission.Denied)
            NativeGallery.RequestPermission();
    }

    public void BtnShare()
    {
        if (appManager.GetPhoto == null)
        {
            Debug.LogError("No pic");
            return;
        }

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, appManager.GetPhoto.EncodeToPNG());

        new NativeShare().AddFile(filePath).SetSubject("Geogram").Share();
        //uiFeedback.ShowFeedback("SHARED");
    }

    public void BtnSticker()
    {
        //ShowSceneMenu(false);
        //ShowScenePageantry(true);
        ScenePageantryScreen();
    }

    public void BtnText()
    {
        TouchScreenKeyboard.Open("");
    }

    public void BtnSelectSceneObject(int index)
    {
        appManager.ShowSceneObject(index);
    }

    public void BtnConfirmSceneObject()
    {
        appManager.ConfirmSceneObject();
        BackToSceneMenu();
    }

    public void BtnDeleteSceneObject()
    {
        appManager.DeleteSceneObject();
    }
    /*
        public void ShowCameraCreateScanUI(bool state)
        {
            cameraCreateScanUI.SetActive(state);
        }

        public void ShowPageantryMenu(bool state)
        {
            cameraCreatePageantryUI.SetActive(state);
        }

        private void ShowPageantryGroupMenu(int group)
        {
            cameraCreatePgtryMenu.SetActive(false);
            currentGroup = group;
            cameraCreatePgtryBirthday.SetActive(group == 0);
            cameraCreatePgtryFun.SetActive(group == 1);
        }


    public void ShowCameraCreateUI(bool state)
    {
        cameraCreateUI.SetActive(state);
    }

    public void ShowSceneMenu(bool state)
    {
        cameraCreateSceneMenuUI.SetActive(state);
    }

    public void ShowScenePageantry(bool state)
    {
        cameraCreateScenePageantryUI.SetActive(state);
    }

    public void ShowSceneConfirm(bool state)
    {
        cameraCreateSceneConfirmUI.SetActive(state);
    }
*/
    #endregion

    #region PRIVATE

    private PrivacySetting GetPrivacySetting()
    {
        return PrivacySetting.PUBLIC;
    }

    private string GetSelectedFriends()
    {
        return "";
    }

    private string GetComment()
    {
        return "I just created this one";
    }


    #endregion

    #region CALLBACK

    private void GetImageFromGallery(string path)
    {
        Debug.Log("Image path: " + path);
        if (path != null)
        {
            // Create Texture from selected image
            Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false);
            if (texture == null)
            {
                Debug.Log("Couldn't load texture from " + path);
                return;
            }

            int biggerSide = texture.width;
            int lowerSide = texture.height;

            if (lowerSide > biggerSide)
            {
                int aux = lowerSide;
                lowerSide = biggerSide;
                biggerSide = aux;
            }

            //RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            //camera.targetTexture = rt;
            //camera.Render();
            //RenderTexture.active = rt;
            Texture2D screenShot;
            screenShot = new Texture2D(lowerSide, lowerSide, TextureFormat.RGB24, false);
            Debug.Log("big side: " + biggerSide);
            Debug.Log("small side: " + lowerSide);
            Color[] pixels = texture.GetPixels(texture.width / 2 - lowerSide / 2, texture.height / 2 - lowerSide / 2, lowerSide, lowerSide);
            screenShot.SetPixels(pixels);
            screenShot.Apply();

            appManager.SetPhotoFromGallery(screenShot);
        }
    }

    #endregion
}
