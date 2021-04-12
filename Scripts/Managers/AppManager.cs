using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geogram;
using System;

public class AppManager : MonoBehaviour
{
    public List<Memento> mementos;
    public List<Memento> myMemento;

    public List<Memento> closeMementos;
    private AppState currentState;
    private UIManager uiManager;
    private MapManager mapManager;
    private CameraManager cameraManager;
    private FileManager fileManager;
    private WebServerManager webServerManager;

    private string user;

    private static AppManager instance;
    internal object map;

    public string User { get { return user; }}
    public Vector3 UserCoordinates{get {return mapManager.MyCoordinates3d;}}
    public Quaternion UserOrientation { get {return Quaternion.Euler(0f, - mapManager.MyOrientation, 0f);}} 
    public AppState CurrentState { get { return currentState; }}
    public Texture2D GetPhoto { get { return cameraManager.Photo; }}
    public static AppManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Initialize();
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Initialize()
    {
        uiManager       = FindObjectOfType<UIManager>();
        mapManager      = FindObjectOfType<MapManager>();
        cameraManager   = FindObjectOfType<CameraManager>();
        fileManager     = FindObjectOfType<FileManager>();
        webServerManager= FindObjectOfType<WebServerManager>();

        uiManager.Initialize(this);
        cameraManager.Initialize(uiManager.ShowScanMenu, uiManager.HideAllCameraCreateScreen, uiManager.ScanGroundScreen, uiManager.SceneMenuScreen, uiManager.ScenePageantryScreen, uiManager.SceneConfirmScreen);
        webServerManager.ServerIcon = uiManager.ShowServerIcon;
        //GET IG PHOTOS
        webServerManager.GetRecentMedia(CreateInstaPins, ShowErrorMessage);

    }

    // Use this for initialization
    void Start()
    {
        ShowSplashScreen();
        Invoke("ShowMap", 1f); //Simulate Splash Screen
#if UNITY_EDITOR
        cameraManager.gameObject.SetActive(false);
        if (mementos.Count > 0)
            cameraManager.CreateARMemento(mementos);
#endif
    }

    #region PUBLIC

    //---------DEBUG---------//

    public void Debug(string text)
    {
        UnityEngine.Debug.Log(text);
        //uiManager.DebugLog(text);
    }

    public void ToggleMap()
    {
        if(mapManager.gameObject.activeInHierarchy)
            mapManager.gameObject.SetActive(false);
        else
            mapManager.gameObject.SetActive(true);
    }

    public void ToggleCamera()
    {
        if (cameraManager.gameObject.activeInHierarchy)
            cameraManager.gameObject.SetActive(false);
        else
            cameraManager.gameObject.SetActive(true);
    }

    //---------CHANGE STATE---------//

    public void ShowSplashScreen()
    {
        ChangeState(AppState.SPLASH);
    }

    public void ShowLogIn()
    {
        ChangeState(AppState.LOG_IN);
    }

    public void ShowMap()
    {
        ChangeState(AppState.MAP);
        AuthenticateUser("gabe", "123");
    }

    public void ShowCameraScan(List<Memento> mementos)
    {
        if (!cameraManager.IsReady || mementos.Count < 1)
            return;

        closeMementos = mementos;
        ChangeState(AppState.AR_CAMERA_SCAN);
    }

    public void ShowCameraCreate()
    {
        ChangeState(AppState.AR_CAMERA_CREATE);
    }

    //---------LOG IN---------//

    public void AuthenticateUser(string user, string pw)
    {
        webServerManager.AuthenticateUser(user, pw, OnAuthenticationSucess, OnAuthenticationFailure);
    }

    public void CreateUser(string user, string pw)
    {
        webServerManager.CreateUser(user, pw, OnCreateUserSucess, OnCreateUserFailure);
    }

    //---------MAP VIEW---------//

    public void AddNewMemento(params Memento[] newMementos)
    {
        for (int i = 0; i < newMementos.Length; i++)
        {
            //Debug("memento scene: " + newMementos[i].sceneObjects);
            mementos.Add(newMementos[i]);
        }
        if(currentState == AppState.MAP)
            StartCoroutine(CreateMementosOnMap(newMementos));
    }

    public void LoadMementos()
    {
        webServerManager.LoadAllMementos(
            onSucess: 
            (List<Memento> m) => 
        {
            myMemento = m;

            AddNewMemento(mementos.ToArray());
        },
            onFailure: Debug);
    }

    //---------CAMERA COMMON---------//

    public void TakePicture(bool isScan = false)
    {
        if (isScan)
            cameraManager.TakePicture(ShowPhoto, false);
        else
            cameraManager.TakePicture(ShowPageantry, true);
    }

    public void DownloadPicture()
    {
        fileManager.SavePicture(cameraManager.Photo);
    }

    //---------CAMERA SCAN-----------//


    public void ShowMementoUIHint(MementoAR[] mementos, Camera camera)
    {
        uiManager.ShowMementoHints(mementos, camera);
    }

    private void ShowPhoto(Texture2D photo)
    {
        uiManager.ShowPhoto(photo);
    }

    //---------CAMERA CREATE---------//

    public void RotateCamera()
    {
        cameraManager.RotateCamera();
    }

    public void ChangeFrame(int frameCode)
    {
        cameraManager.ChangeFrame(frameCode);
    }

    public void ShowPageantry(Texture2D photo)
    {
        cameraManager.CreateARPreviews(photo);
        uiManager.PolaroidPreviewScreen();
        //uiManager.ShowPageantryMenu(true);
    }

    public void SetPhotoFromGallery(Texture2D texture)
    {
        ShowPageantry(texture);
    }

    public void StartCreatingScene()
    {
        cameraManager.StartSceneCreation();
    }

    public void ShowSceneObject(int arCode)
    {
        cameraManager.ShowSceneObject(arCode);
    }

    public void ConfirmSceneObject()
    {
        cameraManager.ConfirmSceneObject();
    }

    public void HidePolaroidPreviews()
    {
        cameraManager.HidePreviews();
    }

    public void DeleteSceneObject()
    {
        cameraManager.DeleteSceneObject();
    }

    public void SendMemento(int pageantry, PrivacySetting privacySetting, string friends = "")
    {
        Memento memento = Utils.CreateMemento(User, mapManager.MyLocation, cameraManager.Photo, cameraManager.Comment, pageantry, cameraManager.SceneObjects);
        webServerManager.SaveMemento(memento, null, null);

        //mementos.Add(memento);
        //mapManager.CreateMementoPin(memento);
        Debug("Memento created at: " + memento.coordinates);
        ShowMap();
    }

    #endregion

    #region PRIVATE


    #endregion

    #region STATE

    private void ChangeState(AppState newState)
    {
        OnExit(currentState);
        currentState = newState;
        OnEnter(currentState);
    }

    private void OnEnter(AppState state)
    {
        switch (state)
        {
            case AppState.SPLASH:
                //Load image
                //mapManager.gameObject.SetActive(false);
                //cameraManager.gameObject.SetActive(false);
                uiManager.ShowSplashScreen(true);
                break;
            case AppState.LOG_IN:
                uiManager.ShowLogInScreen(true);
                AuthenticateUser(PlayerPrefs.GetString(Utils.KEY_USER, "-1"), PlayerPrefs.GetString(Utils.KEY_PW,""));
                break;
            case AppState.MAP:
                mapManager.gameObject.SetActive(true);
                mapManager.SetTouchController = true;
                uiManager.ShowMainMenuUI(true);

                mapManager.ClearPins();
                StartCoroutine(CreateMementosOnMap(mementos.ToArray()));
                //Go user location
                break;
            case AppState.AR_CAMERA_SCAN:
                cameraManager.StartScanCamera();

                cameraManager.CreateARMemento(closeMementos);
                //cameraManager.CreateARMemento(mementos);

                uiManager.ShowScanMessage();
                uiManager.ShowCameraScanUI(true);
                //Get device orientation
                //Start scan
                break;
            case AppState.AR_CAMERA_CREATE:
                cameraManager.StartCreateCamera();
                uiManager.ShowCameraCreatePanel(true);
                uiManager.TakePictureScreen();
                break;
            default:
                break;
        }
    }

    private void OnExit(AppState state)
    {
        switch (state)
        {
            case AppState.SPLASH:
                //Unload image
                //
                uiManager.ShowSplashScreen(false);
                break;
            case AppState.LOG_IN:
                uiManager.ShowLogInScreen(false);
                break;
            case AppState.MAP:
                //mapManager.gameObject.SetActive(false);
                mapManager.SetTouchController = false;
                uiManager.ShowMainMenuUI(false);
                mapManager.ClearPins();
                break;
            case AppState.AR_CAMERA_SCAN:
                cameraManager.DestroyAllAR();
                cameraManager.gameObject.SetActive(false);
                //uiManager.ShowCameraScanMessage(false); //unnecessary?
                uiManager.ShowCameraScanUI(false);
                uiManager.SetHints(false);
                break;
            case AppState.AR_CAMERA_CREATE:
                cameraManager.DisableCreateCamera();
                uiManager.ShowCameraCreatePanel(false);
                break;
            default:
                break;
        }
    }

    #endregion

    #region SERVER_RESPONSE

    private void CreateInstaPins(string response)
    {
        StartCoroutine(LoadInstaPosts(response));
    }

    private void ShowErrorMessage(string response)
    {
        UnityEngine.Debug.LogError(response);
    }

    private void OnAuthenticationSucess(string result)
    {
        user = result;
        PlayerPrefs.SetString(Utils.KEY_USER, user);

        //LoadMementos();
        //webServerManager.LoadUserMementos(user);
    }

    private void OnAuthenticationFailure(string result)
    {

    }

    private void OnCreateUserSucess(string result)
    {

    }

    private void OnCreateUserFailure(string result)
    {

    }

    #endregion

    #region COROUTINE

    private IEnumerator CreateMementosOnMap(params Memento[] newMementos)
    {
        yield return new WaitForSeconds(1f); // NOT NECESSARY

        while(!mapManager.IsReady) //Wait for the map to be ready
            yield return 0;

        //mementos = LoadMementos();
        for (int i = 0; i < newMementos.Length; i++)
        {
            mapManager.CreateMementoPin(newMementos[i]);
            yield return new WaitForSeconds(Utils.PINS_SHOW_DURATION/newMementos.Length);
        }
    }

    private IEnumerator LoadInstaPosts(string response)
    {
        InstaPicJSON[] pics = InstaPicJSON.Array(response, "data");
        List<Memento> instaMementos = new List<Memento>();
        for (int i = 0; i < pics.Length; i++)
        {
            if (pics[i].Location != null)
            {
                //UnityEngine.Debug.Log(pics[i].Image.url);
                WWW picture = new WWW(pics[i].Image.url);
                yield return picture;

                //UnityEngine.Debug.Log("Got picture: " + instaMementos.Count);
                Memento instaMemento = Utils.CreateMemento(pics[i], picture.texture);
                instaMementos.Add(instaMemento);
            }
        }
        StartCoroutine(CreateMementosOnMap(instaMementos.ToArray()));
    }

    #endregion
}
