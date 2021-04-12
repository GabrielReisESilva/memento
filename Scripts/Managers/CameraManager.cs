using System;
using System.Collections;
using System.Collections.Generic;
using Geogram;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARFoundation;

public class CameraManager : MonoBehaviour
{
    public SceneObject mementoParent;
    public Transform circleLimit;
    public MementoAR[] mementoPrefabs;
    public Material[] frameMaterials;
    public SceneObject[] sceneObjectPrefabs;
    public MementoAR mementoVideoPrefab;
    public ARSessionOrigin arOrigin;
    public ARPointCloudManager pointCloudManager;
    public ARPlaneManager planeManager;
    public PicureTaker picureTaker;
    public TouchController touchController;

    private ARSession session;
    private MementoAR polaroid;
    //private MementoAR[] mementoPreviewObjs;
    private MementoAR[] mementoObjs;
    private FrontCamera frontCamera;
    private Transform mementoARParent;
    private List<SceneObject> objectsOnScene;
    private MementoAR selectedObj;
    private bool showing;
    private bool creatingScene;
    private bool isReady;
    private Texture2D photo;
    private SceneManager sceneManager;

    public string Comment { get { return polaroid.comment.text; }}
    public bool IsReady { get { return isReady; } }
    public Action OnARPinsShow { get; set; }
    public BoolController SelectPageantryUI { get; set; }
    public Texture2D Photo { get { return photo;} }
    public List<SceneObject> SceneObjects { get { return objectsOnScene; } }
    private ARPlane basePlan;
    // Use this for initialization
    public void Initialize(Action OnARPinsShow, Action HideUI, Action OnPictureTaken, Action SceneMenuUI, Action ScenePageantryUI, Action ConfirmSceneUI)
    {
        objectsOnScene = new List<SceneObject>();
        session = FindObjectOfType<ARSession>();
        frontCamera = FindObjectOfType<FrontCamera>();
        mementoARParent = new GameObject("View Mode Parent").transform;
        mementoARParent.parent = transform;
        planeManager.planeAdded += OnPlanAdded;
        AppManager.Instance.Debug("Looking for a plane");
        this.OnARPinsShow = OnARPinsShow;
        //this.OnARPinsShow += (bool value) => { planeManager.enabled = value; pointCloudManager.enabled = value;};
        ARSubsystemManager.systemStateChanged += OnSessionStateChange;

        sceneManager = new SceneManager(
            hideUI: HideUI,
            scanUI: OnPictureTaken,
            sceneMenu: SceneMenuUI,
            scenePgtry: ScenePageantryUI,
            confirmScn: ConfirmSceneUI,
            planeMng: (bool b) => { planeManager.enabled = b; },
            pointCloudMng: (bool b) => { pointCloudManager.enabled = b;},
            touchCntl: (bool b) => { touchController.enabled = b; },
            mementoPrt: mementoParent,
            arSOrg: arOrigin,
            remove: DestroySceneObject
        );

        mementoParent.ARCode = -1;

        touchController.enabled = false;
    }

    private void Update()
    {
        if (AppManager.Instance.CurrentState == AppState.AR_CAMERA_SCAN)
        {
            
        }
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = arOrigin.camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit, 10f, Utils.PINS_LAYER_MASK))
                {
                    MementoAR mementoAR = rayHit.transform.GetComponent<MementoAR>();
                    if (mementoAR)
                    {
                        mementoAR.OnClick();
                    }

                    LikeButtonScript likeButtonScript = rayHit.transform.GetComponent<LikeButtonScript>();
                    if (likeButtonScript)
                    {

                        if (likeButtonScript.getIsLiked == false)
                        {
                            likeButtonScript.IncrementLikeCounter();
                        }
                        else
                        {
                            likeButtonScript.DecrementLikeCounter();
                        }
                    }
                }
                
                else if(Physics.Raycast(ray, out rayHit, 10f, Utils.SCENE_OBJECT_LAYER_MASK))
                {
                    rayHit.transform.GetComponent<SceneObject>().Interact();
                }
            }
        }

        /*
        if (Input.touchCount > 0)
        {
            //TAP:
            //SceneManager.PhotoPlaced? ChangeState->TRANSFORM_OBJECT : SELECT_PAGEANTRY
            Touch touch = Input.touches[0];
            if (arOrigin.Raycast(touch.position, arHits, TrackableType.Planes))
            {
                Pose hitPose = arHits[0].pose;
                AppManager.Instance.Debug("Touch at:" + hitPose.position);
                mementoParent.transform.LookAt(arOrigin.camera.transform.position, -Vector3.up);
                mementoParent.transform.localEulerAngles = new Vector3(0, mementoParent.transform.localEulerAngles.y, 0);
                mementoParent.transform.position = hitPose.position;
            }
        }
        */
    }
#region PUBLIC_SCAN

    public void StartScanCamera()
    {
        gameObject.SetActive(true);
        planeManager.enabled = true;
        pointCloudManager.enabled = false;
        touchController.enabled = true;
        basePlan = null;
        showing = false;
        session.Reset();
        SetScanController();
    }

    private void SetScanController()
    {
        touchController.Init();
        touchController.OnDoubleMove = MoveARParent;
    }

    public void CreateARMemento(List<Memento> mementos)
    {
        Debug.Log("Create Memento");
        //mementoParent = new GameObject("Memento Parent");
        mementoObjs = new MementoAR[mementos.Count];
        for (int i = 0; i < mementos.Count; i++)
        {
            mementoObjs[i] = Instantiate(mementoPrefabs[0],mementoARParent) as MementoAR;
            mementoObjs[i].SetData(mementos[i], sceneObjectPrefabs);
            mementoObjs[i].Frame = frameMaterials[mementos[i].pageantryCode];
            mementoObjs[i].gameObject.SetActive(false);
            mementoObjs[i].Follow(arOrigin.camera.transform);
        }
        //Temp - Video prefab
        //mementoObjs[mementos.Count] = Instantiate(mementoVideoPrefab) as MementoAR;
        //mementoObjs[mementos.Count].gameObject.SetActive(false);
    }

    public void DestroyAllAR()
    {
        //Destroy(mementoParent);
        if (mementoObjs != null)
        {
            for (int i = 0; i < mementoObjs.Length; i++)
            {
                Destroy(mementoObjs[i].gameObject);
            }
        }

        mementoObjs = null;
        showing = false;
        touchController.enabled = false;
        //session.Reset();
    }

    #endregion

#region PUBLIC_CREATE

    public void StartCreateCamera()
    {
        gameObject.SetActive(true);
        //planeManager.enabled = false;
        pointCloudManager.enabled = false;
        creatingScene = true;
        basePlan = null;
        mementoARParent.position = Vector3.zero;
        session.Reset();
        SetCreateController();
    }

    private void SetCreateController()
    {
        touchController.Init();
        touchController.OnTouchDown = sceneManager.OnTouchDown;
        touchController.OnTouchUp = sceneManager.OnTouchUp;
        touchController.OnTap = sceneManager.OnTap;
        touchController.OnMove = sceneManager.OnDrag;
        touchController.OnHold = sceneManager.OnHold;
        touchController.OnDoubleTouch = sceneManager.OnDoubleTouch;
        touchController.OnDoubleMove = sceneManager.OnDoubleMove;
        touchController.OnDoubleTouchDown = sceneManager.OnDoubleTouchDown;
    }

    public void TakePicture(Action<Texture2D> textureCallback, bool squareCrop)
    {
        textureCallback += (Texture2D tex) => { photo = tex; };
#if UNITY_EDITOR
        picureTaker.TakeScreenShot(Camera.main, textureCallback, squareCrop);
        //picureTaker.TakePicture(frontCamera.Texture, textureCallback, squareCrop);
#else
        if(frontCamera.IsPlaying)
            picureTaker.TakeScreenShot(Camera.main, textureCallback, squareCrop);
        else
            picureTaker.TakeScreenShot(arOrigin.camera, textureCallback, squareCrop);
#endif
    }

    public void RotateCamera()
    {
        //arOrigin.gameObject.SetActive(false);
        if(frontCamera.IsPlaying)
        {
            frontCamera.Stop();
            gameObject.SetActive(true);
            frontCamera.SetBlackScreen(false);
        }
        else
        {
            frontCamera.SetBlackScreen(true);
            gameObject.SetActive(false);
            frontCamera.Play();
        }
    }

    //THIS IS CALLED AFTER TAKING THE PICTURE
    //CREATE ALL FRAME MODELS
    public void CreateARPreviews(Texture2D photo)
    {
        if (frontCamera.IsPlaying)
            RotateCamera();

        //SET PARENT ON SCREEN
        SetParentOnScreen();
        this.photo = photo;

        //if(mementoPreviewObjs == null)
          //  mementoPreviewObjs = new MementoAR[mementoPrefabs.Length];
        /*
        for (int i = 0; i < mementoPreviewObjs.Length; i++)
        {
            if(mementoPreviewObjs[i] == null)
            {
                mementoPreviewObjs[i] = Instantiate(mementoPrefabs[i],mementoParent.transform) as MementoAR;
                mementoPreviewObjs[i].gameObject.SetActive(false);
                mementoPreviewObjs[i].transform.localScale = 1.0f * Vector3.one;
            }
            mementoPreviewObjs[i].SetData(photo, true);
        }
        */
        polaroid = Instantiate(mementoPrefabs[0], mementoParent.transform) as MementoAR;
        //polaroid.transform.localScale = 1.0f * Vector3.one;
        polaroid.SetData(photo, true);
        planeManager.enabled = true;
        ChangeFrame(0);

        //StartScanCamera();
    }

    //PICKING DIFFERENT FRAMES
    public void ChangeFrame(int arCode)
    {
        //for (int i = 0; i < mementoPreviewObjs.Length; i++)
          //  mementoPreviewObjs[i].gameObject.SetActive(i == arCode);

        polaroid.Frame = frameMaterials[arCode];
        Debug.Log("SHOW PREVIEW");
    }

    //FROM UI MANAGER
    //START SCANNING GROUND
    public void StartSceneCreation()
    {
        sceneManager.ChangeStatus(SceneStatus.SCAN_GROUND);
        if(basePlan)
        {
            SetParentOnGround(basePlan.transform.position);
            sceneManager.ChangeStatus(SceneStatus.TRANSFORM_OBJECT, mementoParent);
        }
    }

    //FROM UI MANAGER
    //CREATE A NEW SCENE OBJECT
    public void ShowSceneObject(int arCode)
    {
        SceneObject sceneObject = Instantiate(sceneObjectPrefabs[arCode], mementoParent.transform);
        sceneObject.ARCode = arCode;
        objectsOnScene.Add(sceneObject);

        sceneManager.ChangeStatus(SceneStatus.TRANSFORM_OBJECT, sceneObject); //SceneStatus.PLACE_OBJECT
    }

    //FROM UI MANAGER
    //CONFIRM OBJECT
    public void ConfirmSceneObject()
    {
        sceneManager.ChangeStatus(SceneStatus.SELECT_PAGEANTRY);
    }

    //FROM UI MANAGER
    //DELETE SCENE OBJECT
    public void DeleteSceneObject()
    {
        if(sceneManager.arObject != null)
            DestroySceneObject(sceneManager.arObject);
    }

    //FROM UI MANAGER
    //WHEN GOES BACK TO TAKINF PICTURE
    //HIDE THE POLAROID PREVIEW
    public void HidePreviews()
    {
        polaroid?.gameObject.SetActive(false);
        circleLimit.gameObject.SetActive(false);
        for (int i = 0; i < objectsOnScene.Count; i++)
        {
                Destroy(objectsOnScene[i].gameObject);
        }
        objectsOnScene.Clear();
        /*
        if (mementoPreviewObjs != null)
        {
            for (int i = 0; i < mementoPreviewObjs.Length; i++)
            {
                mementoPreviewObjs[i].gameObject.SetActive(false);
            }
        }
        */
    }

    //RESET EVERYTHING
    public void DisableCreateCamera()
    {
        creatingScene = false;
        sceneManager.ChangeStatus(SceneStatus.OFF);

        HidePreviews();

        for (int i = 0; i < objectsOnScene.Count; i++)
        {
            Destroy(objectsOnScene[i].gameObject);
        }

        objectsOnScene.Clear();
        circleLimit.gameObject.SetActive(false);
        sceneManager.ChangeStatus(SceneStatus.OFF);
        gameObject.SetActive(false);

        if(frontCamera.IsPlaying)
        {
            frontCamera.Stop();
            frontCamera.SetBlackScreen(false);
        }
    }

#endregion

#region PRIVATE

    private void MoveARObject(Transform obj, Vector3 position)
    {
        //Create AR Object in Unity world
        if (!obj)
            return;
        arOrigin.MakeContentAppearAt(obj, position);
    }

    private void MoveARParent(Vector3 deltaScreenPosition)
    {
        mementoARParent.Translate(Vector3.up * deltaScreenPosition.y * Utils.SCREEN_DELTA_TO_WORLD);
    }

    private void ShowPlanes(bool state)
    {
        List<ARPlane> planes = new List<ARPlane>();
        planeManager.GetAllPlanes(planes);
        for (int i = 0; i < planes.Count; i++)
        {
            planes[i].gameObject.SetActive(state);
        }
    }

    private void DestroySceneObject(SceneObject obj)
    {
        if(objectsOnScene.Remove(obj))
        {
            Destroy(obj.gameObject);
            sceneManager.ChangeStatus(SceneStatus.SELECT_PAGEANTRY);
        }
    }

#endregion

#region AR_EVENT_HANDLE

    private void OnSessionStateChange(ARSystemStateChangedEventArgs args)
    {
        Debug.Log("AR Camera state: " + args.state); 
        AppManager.Instance.Debug(args.state.ToString());
        switch (args.state)
        {
            case ARSystemState.Ready:
                if(!isReady)
                {
                    isReady = true;
                    AppManager.Instance.Debug("Disable Camera");
                    gameObject.SetActive(false);
                }
                //AppManager.Instance.ShowMap();
                break;
        }
    }

    private void OnPlanAdded(ARPlaneAddedEventArgs args)
    {
        AppManager.Instance.Debug("FOUND PLANE");
        if (creatingScene && sceneManager.status == SceneStatus.SCAN_GROUND)
        {
            basePlan = args.plane;
            SetParentOnGround(basePlan.transform.position);
            sceneManager.ChangeStatus(SceneStatus.PLACE_PICTURE, mementoParent);
        }
        else if (!showing)
        {
            basePlan = args.plane;
            ShowARMementosOnGround(basePlan.transform.position.y);
        }
        basePlan = args.plane;
        //ShowPlanes(!showing);
        ShowPlanes(true);
    }

    private Vector3 GetRelativePosition(Vector3 cameraPosition, float planeY, MementoAR mementoAR, Vector3 userCoordinates, Quaternion userOrientation)
    {
        Vector3 direction = mementoAR.Coordinates - userCoordinates;
        direction = cameraPosition + userOrientation * direction.normalized * 2f;
        if (Vector3.Distance(direction, cameraPosition) < 1.8f)
        {
            Vector2 unitCircle = 2f * UnityEngine.Random.insideUnitCircle.normalized;
            direction = cameraPosition + new Vector3(unitCircle.x, 0f, unitCircle.y);
        }

        direction.y = planeY;
        return direction;
    }

    private void CheckForOverlappingAndMove(MementoAR[] mementos, int currentIndex)
    {
        Vector3 pos = mementos[currentIndex].transform.position;
        bool isClose = true;
        int maxIteractions = 10;
        while (isClose && maxIteractions > 0)
        {
            maxIteractions--;
            isClose = false;

            for (int i = 0; i < currentIndex; i++)
            {
                isClose |= Vector3.Distance(mementos[i].transform.position, pos) < 1.0f;
                if (isClose) break;
            }
            if(isClose)
            {
                Vector2 unitCircle = UnityEngine.Random.insideUnitCircle.normalized;
                pos = mementos[currentIndex].transform.position + new Vector3(unitCircle.x, 0f, unitCircle.y);
            }
        }
        mementos[currentIndex].transform.position = pos;
    }

    private Vector3 GetPinPosition(Vector3 cameraPosition, float planeY, MementoAR[] mementos, int currentIndex)
    {
        Vector3 pos = Vector3.zero;
        bool isClose = true;
        int maxIteractions = 10;
        while (isClose && maxIteractions > 0)
        {
            maxIteractions--;
            isClose = false;
            pos = new Vector3(
                cameraPosition.x + UnityEngine.Random.Range(1.0f, 3.0f) * (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1),
                planeY,
                cameraPosition.z + UnityEngine.Random.Range(1.0f, 3.0f) * (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1)
            );
            for (int i = 0; i < currentIndex; i++)
            {
                isClose |= Vector3.Distance(mementos[i].transform.position, pos) < 1.0f;
                if (isClose) break;
            }
        };
        return pos;
    }

    private void ShowARMementosOnGround(float groundHeight)
    {
        if (mementoObjs != null && mementoObjs.Length > 0)
        {
            OnARPinsShow(); //disable UI
            showing = true;

            for (int i = 0; i < mementoObjs.Length; i++)
            {
                Debug.Log("Creating Pin: " + i);
                mementoObjs[i].transform.position = GetRelativePosition(arOrigin.camera.transform.position, groundHeight, mementoObjs[i], AppManager.Instance.UserCoordinates, AppManager.Instance.UserOrientation);
                CheckForOverlappingAndMove(mementoObjs, i);
                //GetPinPosition(arOrigin.camera.transform.position, basePlan.transform.position.y, mementoObjs, i);//arPosition;//basePlan.transform.position + 0.5f * i * new Vector3(Mathf.Sin(30 * i), 0f, Mathf.Cos(30 * i));

                mementoObjs[i].transform.LookAt(arOrigin.camera.transform.position, -Vector3.up);
                mementoObjs[i].transform.localEulerAngles = new Vector3(0, mementoObjs[i].transform.localEulerAngles.y + 180f, 0);
                mementoObjs[i].gameObject.SetActive(true);
                //AppManager.Instance.Debug("PIN AT: " + mementoObjs[i].transform.position);
            }

            AppManager.Instance.ShowMementoUIHint(mementoObjs, arOrigin.camera);
        }
    }

    private void SetParentOnScreen()
    {
        mementoParent.transform.SetParent(arOrigin.camera.transform);
        mementoParent.SetLocalPosition(new Vector3(0f, -1.4f, 0.8f));
        mementoParent.transform.localRotation = Quaternion.identity;
        circleLimit.gameObject.SetActive(false);
    }

    private void SetParentOnGround(Vector3 groundPosition)
    {
        mementoParent.transform.SetParent(transform);
        mementoParent.SetLocalPosition(groundPosition);
        mementoParent.transform.LookAt(arOrigin.camera.transform.position, -Vector3.up);
        mementoParent.transform.localEulerAngles = new Vector3(0, mementoParent.transform.localEulerAngles.y + 180f, 0);
        //mementoParent.transform.rotation = Quaternion.identity;
        circleLimit.gameObject.SetActive(true);
    }

#endregion
}

public class SceneManager
{
    public SceneStatus status;
    public SceneObject arObject;
    private Action HideUI;
    private Action ScanGroundUI;
    private Action SceneMenuUI;
    private Action ScenePageantryUI;
    private Action ConfirmSceneUI;
    private BoolController PlaneManagerComponent;
    private BoolController PointCloudManagerComponent;
    private BoolController TouchControllerComponent;
    private SceneObject mementoParent;
    private ARSessionOrigin arOrigin;
    private List<ARRaycastHit> arHits;
    private RaycastHit raycastHit;
    private Action<SceneObject> RemoveObject;

    private bool debug = false;
    private bool pitchToMoveARObjectsFlag = false; // toggle for alternate AR obj height controls
    private float prevDeltaDist = 9999;
    private float prevDeltaAngle = 9999;
    private float originalHeight;
    private bool baseLineFlag = false;
    private float baseLineOffset = 0;
    
    private const float HeightScaleFactor = -0.05f;


    public SceneManager(Action hideUI, Action scanUI, Action sceneMenu, Action scenePgtry, Action confirmScn, BoolController planeMng, BoolController pointCloudMng, BoolController touchCntl, SceneObject mementoPrt, ARSessionOrigin arSOrg, Action<SceneObject> remove)
    {
        arHits = new List<ARRaycastHit>();

        HideUI = hideUI;
        ScanGroundUI = scanUI;
        SceneMenuUI = sceneMenu;
        ScenePageantryUI = scenePgtry;
        ConfirmSceneUI = confirmScn;
        PlaneManagerComponent = planeMng;
        PointCloudManagerComponent = pointCloudMng;
        TouchControllerComponent = touchCntl;
        mementoParent = mementoPrt;
        arOrigin = arSOrg;
        RemoveObject = remove;
    }

    public void ChangeStatus(SceneStatus newStatus, SceneObject sceneObject = null)
    {
        if(sceneObject != null)
            SetARObject(sceneObject);

        Debug.Log("CHANGE STATUS >> Scene Manager: " + newStatus);
        OnExit(status);
        status = newStatus;
        OnEnter(status);
    }

    public void OnTouchDown(Vector3 touchPosition)
    {
        if (debug) Debug.Log("Touch Down");
        //SetObjectAlpha(arObject, 1.0f);
        switch (status)
        {
            case SceneStatus.SELECT_PAGEANTRY:
            case SceneStatus.PLACE_PICTURE:
            case SceneStatus.PLACE_OBJECT:
                //SelectObject(touchPosition);

                /*
                Ray ray = arOrigin.camera.ScreenPointToRay(touchPosition);
                if (Physics.Raycast(ray, out raycastHit, 10f, Utils.SCENE_OBJECT_LAYER_MASK))
                {
                    SetARObject(raycastHit.transform.GetComponent<SceneObject>());
                    
                    Debug.Log("New AR Object: " + arObject.name);
                }
                else if (Physics.Raycast(ray, out raycastHit, 10f, Utils.CIRCLE_LAYER_MASK))
                {
                    SetARObject(raycastHit.transform.GetComponent<SceneObject>());

                    Debug.Log("New AR Object: " + arObject.name);
                }
                */
                break;
        }
        //SetObjectAlpha(arObject, 0.3f);
    }

    public void OnTouchUp(Vector3 touchPosition)
    {
        if (debug) Debug.Log("Touch Up");
        /*
        if(status != SceneStatus.PLACE_PICTURE && arObject)
        {
            if (Vector3.Distance(raycastHit.point, mementoParent.transform.position) > 1.5f * Utils.SCENE_CIRCLE_RADIUS)
            {
                RemoveObject(arObject);
            }
        }
        */
        //SetARObject(null);

        switch (status)
        {
            case SceneStatus.PLACE_PICTURE:
            case SceneStatus.PLACE_OBJECT:
                ChangeStatus(SceneStatus.SELECT_PAGEANTRY);
                break;
            default:
                break;
        }
    }

    public void OnTap(Vector3 touchPosition)
    {
        if (debug) Debug.Log("On Tap");
        Ray ray = arOrigin.camera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out raycastHit, 10f, Utils.SCENE_OBJECT_LAYER_MASK))
        {
            raycastHit.transform.GetComponent<SceneObject>().Interact();
            Debug.Log("Interact");
        }
    }

    public void OnDrag(Vector3 touchPosition)
    {
        //if (debug) Debug.Log("On Drag");
        if (arObject)
        {
            switch(status)
            {
                case SceneStatus.PLACE_PICTURE:
                case SceneStatus.PLACE_OBJECT:
                case SceneStatus.TRANSFORM_OBJECT:
                    if (arObject == mementoParent)
                    {
                        MoveOnPlane(touchPosition);
                    }
                    else
                    {
                        MoveOnCircle(touchPosition);
                    }
                    break;
            }
        }
    }

    public void OnHold(Vector3 position)
    {
        //if (debug) Debug.Log("On Hold");
        if (SelectObject(position))
            ChangeStatus(SceneStatus.TRANSFORM_OBJECT);
    }

    public void OnDoubleTouchDown()
    {
        if (arObject)
        {
            if (pitchToMoveARObjectsFlag == true)
            {
                float adjustedAngleValue = AdjustedAngleValue();

                baseLineOffset = adjustedAngleValue;
                originalHeight = arObject.modelTransform.localPosition.y;
            }
        }
    }

    private float AdjustedAngleValue()
    {
        float adjustedAngleValue;
        if (arOrigin.camera.transform.eulerAngles.x > 180)
        {
            adjustedAngleValue = arOrigin.camera.transform.eulerAngles.x - 360;
        }
        else
        {
            adjustedAngleValue = arOrigin.camera.transform.eulerAngles.x;
        }

        return adjustedAngleValue;
    }

    public void OnDoubleTouch(float deltaDist, float deltaAngle)
    {
        if(arObject)
        {
            // Case: adjusting height of object
            if (deltaDist == prevDeltaDist && deltaAngle == prevDeltaAngle && pitchToMoveARObjectsFlag == true)
            {
                float adjustedAngleValue = AdjustedAngleValue();
                float newHeight = originalHeight + ((adjustedAngleValue - baseLineOffset) * HeightScaleFactor);
                
                arObject.Height(newHeight, true);

            }
            else
            {
                arObject.Scale(deltaDist * Utils.SCENE_OBJECT_SCALE_RATE);
                arObject.Rotate(deltaAngle * Utils.SCENE_OBJECT_ROTATE_RATE);
            }
            prevDeltaDist = deltaDist;
            prevDeltaAngle = deltaAngle;
        }

        

    }

    public void OnDoubleMove(Vector3 deltaScreenPosition)
    {
        if (arObject)
        {
            if (pitchToMoveARObjectsFlag == false)
            {
                arObject.Height(deltaScreenPosition.y * Utils.SCREEN_DELTA_TO_WORLD);
            }
        }
    }

    private void OnEnter(SceneStatus status)
    {
        switch (status)
        {
            case SceneStatus.OFF:
                HideUI();
                PlaneManagerComponent(false);
                PointCloudManagerComponent(false);
                TouchControllerComponent(false);
                //DESTROY ALL AR HERE?
                break;
            case SceneStatus.SCAN_GROUND:
                //Move AR Preview To Top Screen
                ScanGroundUI();//Show "Scan Ground" UI
                PlaneManagerComponent(true);//Enable PlaneManager <<< IS PLANE ALWAYS ON??
                //PointCloudManagerComponent(true);
                TouchControllerComponent(false);

                //--wait for plane detection
                break;
            case SceneStatus.PLACE_PICTURE:
                //--ui?
                // Show place picture UI
                HideUI();
                //--components?
                TouchControllerComponent(true);
                //--wait until tap
                break;
            case SceneStatus.SELECT_PAGEANTRY:
                //--ui?
                SceneMenuUI();// Show SelectPageantry UI
                //--components?
                //TouchControllerComponent(false); - Uses OnHold to select objects
                SetARObject(null);
                //--wait for button
                break;
            case SceneStatus.PLACE_OBJECT:
                //--components?
                TouchControllerComponent(true);
                //--wait until tap
                break;
            case SceneStatus.TRANSFORM_OBJECT:
                //--ui?
                ConfirmSceneUI();
                //--components?
                TouchControllerComponent(true);
                //wait until "ok" button
                break;
            default:
                break;
        }
    }

    private void OnExit(SceneStatus status)
    {
        switch (status)
        {
            case SceneStatus.SCAN_GROUND:
                PointCloudManagerComponent(false);
                break;
            case SceneStatus.PLACE_PICTURE:
                break;
            case SceneStatus.SELECT_PAGEANTRY:
                break;
            case SceneStatus.PLACE_OBJECT:
                break;
            case SceneStatus.TRANSFORM_OBJECT:
                break;
            default:
                break;
        }
    }

    private bool MoveOnPlane(Vector3 position)
    {
        if (debug) Debug.Log("Move On Plane");
        if (arOrigin.Raycast(position, arHits, TrackableType.Planes))
        {
            Pose hitPose = arHits[0].pose;
            arObject.transform.LookAt(arOrigin.camera.transform.position, -Vector3.up);
            arObject.transform.localEulerAngles = new Vector3(0, arObject.transform.localEulerAngles.y + 180, 0);
            arObject.Move(hitPose.position);
            return true;
        }
        return false;
    }

    private bool MoveOnCircle(Vector3 position)
    {
        if (debug) Debug.Log("Move On Circle");
        Ray ray = arOrigin.camera.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out raycastHit, 10f, Utils.CIRCLE_LAYER_MASK))
        {
            Debug.Log("Should be moving");
            if (Vector3.Distance(raycastHit.point, mementoParent.transform.position) < Utils.SCENE_CIRCLE_RADIUS)
            {
                arObject.Move(raycastHit.point);
            }
            return true;
        }
        return false;
    }

    private bool SelectObject(Vector3 position)
    {
        if (debug) Debug.Log("Select_Object");
        Ray ray = arOrigin.camera.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out raycastHit, 10f, Utils.SCENE_OBJECT_LAYER_MASK))
        {
            SetARObject(raycastHit.transform.GetComponent<SceneObject>());
            return true;
        }
        else if (Physics.Raycast(ray, out raycastHit, 10f, Utils.CIRCLE_LAYER_MASK))
        {
            SetARObject(raycastHit.transform.GetComponent<SceneObject>());
            return true;
        }
        return false;
    }

    private void SetARObject(SceneObject newArObject)
    {
        if (debug) Debug.Log("Set AR_Object");
        if (arObject)
            arObject.Deselect();//SetObjectAlpha(arObject, 1.0f);

        Debug.Log("New AR Object: " + newArObject);
        arObject = newArObject;

        if (arObject)
            arObject.Select();//SetObjectAlpha(arObject, 0.3f);
    }

    private void SetObjectAlpha(SceneObject sceneObject, float alpha)
    {
        if (sceneObject && sceneObject.GetComponentInChildren<Renderer>() != null)
        {
            Color color = sceneObject.GetComponentInChildren<Renderer>().material.color;
            color.a = alpha;
            sceneObject.GetComponentInChildren<Renderer>().material.color = color;
        }
    }
}