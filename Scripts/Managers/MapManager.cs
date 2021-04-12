using System;
using System.Collections;
using System.Collections.Generic;
using Geogram;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform userPin;
    public Pin mementoPinPrefab;
    public Camera mapCamera;
    public Transform interactionRadius;
    public TouchController touchController;
    private float currentZoom;
    private GameObject mementoPinParent;
    private DeviceLocationProvider location;
    private AbstractMap map;
    private float currentPositionX, currentPositionY;

    private List<Pin> pins;

    private bool isReady;

    public bool SetTouchController { set { touchController.enabled = value; }}
    public bool IsReady { get { return isReady; } }
    public string MyLocation { get { return location.CurrentLocation.LatitudeLongitude.x + ", " + location.CurrentLocation.LatitudeLongitude.y; } }
    public Vector2d MyCoordinates { get { return location.CurrentLocation.LatitudeLongitude; } }// Use this for initialization
    public Vector3 MyCoordinates3d { get { return MyCoordinates.ToVector3xz(); } }// Use this for initialization
    public float MyOrientation {get {return location.CurrentLocation.DeviceOrientation;}}// N: 0f E: 90f S: 180f W: 270f
    void Start()
    {
        currentZoom = 17f;
        pins = new List<Pin>();
        location = FindObjectOfType<DeviceLocationProvider>();
        map = FindObjectOfType<AbstractMap>();
        map.OnInitialized += () =>
        {
            isReady = true; Debug.Log("Map ready");
            interactionRadius.localScale = Vector3.one * map.WorldRelativeScale * Utils.PIN_INTERACT_RANGE;
        };
        map.OnUpdated += UpdatePins;

        mementoPinParent = new GameObject("Memento Pins");
        mementoPinParent.transform.parent = transform;

        touchController.Init();
        touchController.OnTouchDown = OnTouchDown;
        touchController.OnDoubleTouch = OnDoubleTouch;
        touchController.OnMove = OnDrag;
        touchController.OnTap = OnTap;

    }

    private void OnEnable()
    {
        //map.Initialize(MyCoordinates, (int)currentZoom);
    }

    // Update is called once per frame
    void Update()
    {
        userPin.rotation = Quaternion.RotateTowards(userPin.rotation, Quaternion.Euler(0f, MyOrientation, 0f), 60f * Time.deltaTime);
        /*
        for (int i = 0; i < pins.Count; i++)
        {
            if (IsClose(MyCoordinates, pins[i].coordinates, Utils.PIN_INTERACT_RANGE))
                pins[i].IsClose();
            else
                pins[i].IsFar();
        }
        */
    }

    public void CreateMementoPin(Memento memento)
    {
        Vector2d coordinates = Conversions.StringToLatLon(memento.coordinates);
        AddMarker(mementoPinPrefab, coordinates, PinType.MEMENTO, memento).parent = mementoPinParent.transform;
    }

    public Transform AddMarker(Pin pinPrefab, Vector2d coordinates, PinType type, Memento memento = null)
    {
        Pin instance = Instantiate(pinPrefab) as Pin;
        instance.SetData(type, coordinates, memento);
        instance.CheckOwner(AppManager.Instance.User);
        instance.transform.localPosition = map.GeoToWorldPosition(coordinates, true);
        //AppManager.Instance.Debug("Pin created at:" + instance.transform.position);
        //AppManager.Instance.Debug("Pin created at:" + map.WorldToGeoPosition(instance.transform.position));
        //AppManager.Instance.Debug("I am at:" + map.GeoToWorldPosition(location.CurrentLocation.LatitudeLongitude));
        //AppManager.Instance.Debug("I am at:" + location.CurrentLocation.LatitudeLongitude);
        pins.Add(instance);
        return instance.transform;
    }

    private void UpdatePins()
    {
        for (int i = 0; i < pins.Count; i++)
        {
            pins[i].transform.localPosition = map.GeoToWorldPosition(pins[i].coordinates, true);
        }
        //interactionRadius.localScale = Vector3.one * map.WorldRelativeScale * Utils.PIN_INTERACT_RANGE;
    }

    private List<Memento> GetMementosAround(Vector2d coordinates, float maxRange)
    {
        List<Memento> closeMementos = new List<Memento>();
        for (int i = 0; i < pins.Count; i++)
        {
            if (IsClose(coordinates, pins[i].coordinates, maxRange))
                closeMementos.Add(pins[i].memento);
        }
        return closeMementos;
    }

    private bool IsClose(Vector2d coord1, Vector2d coord2, float maxRange)
    {
        float distance = (float)Vector2d.Distance(Conversions.LatLonToMeters(coord1), Conversions.LatLonToMeters(coord2));
        return distance < maxRange;
    }

    public void ClearPins()
    {
        for (int i = 0; i < pins.Count; i++)
        {
            Destroy(pins[i].gameObject);
        }
        pins.Clear();
    }

    #region TOUCH CONTROL
    private void OnTap(Vector3 touchPosition)
    {
        Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        if (Physics.Raycast(ray, out rayHit, Utils.MAP_LAYER_MASK))
        {
            Pin pin = rayHit.transform.GetComponent<Pin>();
            if (pin)
            {
                if (IsClose(MyCoordinates, pin.coordinates, Utils.PIN_INTERACT_RANGE))
                    AppManager.Instance.ShowCameraScan(GetMementosAround(MyCoordinates, Utils.PIN_INTERACT_RANGE));
            }
        }
    }

    private void OnTouchDown(Vector3 touchPosition)
    {
        currentPositionX = touchPosition.x;
        currentPositionY = touchPosition.y;
    }

    private void OnDrag(Vector3 touchPosition)
    {
        float offsetX = currentPositionX - touchPosition.x;
        float offsetY = currentPositionY - touchPosition.y;

        if (map)
        {
            mapCamera.transform.Translate( Utils.MAP_MOVE_SENSITIVITY * new Vector3(offsetX,offsetY,0));

            currentPositionX = touchPosition.x;
            currentPositionY = touchPosition.y;
        }
    }

    public void OnDoubleTouch(float deltaDist, float deltaAngle)
    {
        if (map)
        {
            Zoom(deltaDist);
        }
    }

    #endregion

    private void Zoom(float deltaDist)
    {
        RecenterCamera();

        // To make zoom-in feel less sensitive
        if (deltaDist > 0)
        {
            deltaDist = deltaDist * Utils.MAP_ZOOM_SENSITIVITY * (deltaDist > 0 ? 5f : 1f);
        }

        currentZoom += deltaDist;
        map.SetZoom(currentZoom);
        map.UpdateMap();
        interactionRadius.localScale = Vector3.one * map.WorldRelativeScale * Utils.PIN_INTERACT_RANGE;
    }

    private void RecenterCamera()
    {
        map.SetCenterLatitudeLongitude(map.WorldToGeoPosition(mapCamera.transform.position));
        Vector3 position = Vector3.zero;
        position.y = mapCamera.transform.position.y;
        mapCamera.transform.position = position;
    }

}