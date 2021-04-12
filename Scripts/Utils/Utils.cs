using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geogram
{
    public enum AppState { SPLASH, LOG_IN, MAP, AR_CAMERA_SCAN, AR_CAMERA_CREATE };
    public enum PinType { MEMENTO };
    public enum PrivacySetting { PUBLIC, PRIVATE, FRIENDS }; //Public: Everyone, Private: 0+ friends, Friends: All friends
    public enum SceneStatus {OFF, SCAN_GROUND, PLACE_PICTURE, SELECT_PAGEANTRY, PLACE_OBJECT, TRANSFORM_OBJECT }; //Public: Everyone, Private: 0+ friends, Friends: All friends

    public delegate void BoolController(bool state);
    public delegate void ServerCallback(string result);
    public delegate void MementoCallback(List<Memento> result);

    public static class Utils
    {
        public const string KEY_USER    = "user_key";
        public const string KEY_PW      = "pw_key";

        public const string URL_FIREBASE = "https://geogram-pinnacle.firebaseio.com/";
        public const string URL_IG_API = "https://api.instagram.com/v1/";
        public const string URL_DB = "";

        public const string REFERENCE_USERS = "users";
        public const string REFERENCE_MEMENTOS = "mementos";

        public const string SERVER_MESSAGE_NO_ACCOUNT = "no account";

        public const float MAP_MOVE_SENSITIVITY = 0.07f;
        public const float MAP_ZOOM_SENSITIVITY = 0.07f;
        public const float PINS_SHOW_DURATION = 1.0f;
        public const float PIN_INTERACT_RANGE = 40f; //in meters;

        public static readonly int MAP_LAYER = LayerMask.NameToLayer("Map");
        public static readonly int MAP_LAYER_MASK = 1 << MAP_LAYER;

        public static readonly int PINS_LAYER = LayerMask.NameToLayer("Pins");
        public static readonly int PINS_LAYER_MASK = 1 << PINS_LAYER;

        public static readonly int SCENE_OBJECT_LAYER = LayerMask.NameToLayer("Scene Object");
        public static readonly int SCENE_OBJECT_LAYER_MASK = 1 << SCENE_OBJECT_LAYER;

        public static readonly int CIRCLE_LAYER = LayerMask.NameToLayer("Circle");
        public static readonly int CIRCLE_LAYER_MASK = 1 << CIRCLE_LAYER;

        public const float UI_MEMENTO_POINTER = 150f;

        public const string SHADER_OUTLINE = "Standard Outlined";
        public const string SHADER_OUTLINE_COLOR_ID = "_OutColor";
        public const string SHADER_OUTLINE_THICKNESS_ID = "_Outline";
        public const float SHADER_OUTLINE_SELECTED_THICKNESS = 0.05f;
        public static readonly Color OVERLAPPING_CIRCLE_COLOR = new Color(1.0f, 0.5f, 0.5f, 0.8f);
        public static readonly Color FREE_CIRCLE_COLOR = new Color(0.5f, 1.0f, 0.5f, 0.8f);
        public static readonly Color SELECTED_CIRCLE_COLOR = new Color(0.5f, 0.5f, 1.0f, 0.8f);
        public static readonly Color IDLE_CIRCLE_COLOR = new Color(0.8f, 0.8f, 0.4f, 0.4f);

        public const float SCREEN_DELTA_TO_WORLD = 0.005f;
        public const float SCENE_OBJECT_SCALE_RATE = 1f;
        public const float SCENE_OBJECT_ROTATE_RATE = -360f / Mathf.PI;

        public const string PHOTO_PATH = "Assets/Resources/Photo/";
        public const string FILE_NAME = "Memento.png";
        public const string ALBUMN_NAME = "Mementos";

        public const float FRAME_ROTATION_DURATION = 0.5f;
        public const float FRAME_ROTATION_STEP = 1f / FRAME_ROTATION_DURATION;
        public const float BILLBOARD_SPEED = 0.01f;

        public const float SCENE_CIRCLE_RADIUS = 1.0f;

        public const int MAX_NUMBER_CHARACTER = 50;

        //Every code must be related to the corresponding index under CameraManager.MementoPrefabs
        public const int AR_CODE_POLAROID = 0;
        public const int AR_CODE_BALLON = 0;
        public const int AR_CODE_BIRTHDAY_CAKE = 1;
        public const int AR_CODE_TREASURE_CHEST = 1;
        public const int AR_CODE_BANNER = 2;
        public const int AR_CODE_CONFETTI = 3;
        public const int AR_CODE_PARTY_HAT = 4;
        public const int AR_CODE_PARTY_HORN = 5;
        public const int AR_CODE_CANDLE = 6;
        public const int AR_CODE_GRAMOPHONE = 7;
        public const int AR_CODE_HEART = 8;
        public const int AR_CODE_FIRE = 9;
        public const int AR_CODE_SUN = 10;
        public const int AR_CODE_PIZZA = 11;
        public const int AR_CODE_THUMBS_UP = 12;
        public const int AR_CODE_100 = 13;

        //Every code must be related to the corresponding index under CameraManager.FrameMaterial and 
        // UIManager > Camera Panel > Create Panel > Pick Frame > Bottom Menu > Scroll View > Viewport > Content > <button>
        public const int FRAME_CODE_DEFAULT = 0;
        public const int FRAME_CODE_BANANA = 1;
        public const int FRAME_CODE_CDM = 2;
        public const int FRAME_CODE_FRIES = 3;
        public const int FRAME_CODE_DISNEY = 4;
        public const int FRAME_CODE_PIKACHU = 5;
        public const int FRAME_CODE_PINNAPLE = 6;
        public const int FRAME_CODE_PINNACLE_1 = 7;
        public const int FRAME_CODE_CAT = 8;
        public const int FRAME_CODE_PIZZA = 9;
        public const int FRAME_CODE_PRINCESS = 10;
        public const int FRAME_CODE_TSG = 11;
        public const int FRAME_CODE_BIRTHDAY = 12;
        public const int FRAME_CODE_SUMMER = 13;
        public const int FRAME_CODE_AVOCADO = 14;
        public const int FRAME_CODE_FLAMINGO = 15;

        public const int SCENE_OBJECT_CATEGORY_BIRTHDAY = 0;
        public const int SCENE_OBJECT_CATEGORY_FUN = 1;
        public const int SCENE_OBJECT_CATEGORY_MUSIC = 2;

        public static readonly Color SCENE_CATEGORY_ACTIVE = Color.white;
        public static readonly Color SCENE_CATEGORY_DEACTIVE = new Color(0.8f, 0.8f, 0.8f, 1.0f);

        public static readonly int[][] AR_CODE = new int[][]
        {
            new int[]{AR_CODE_BALLON, AR_CODE_TREASURE_CHEST,AR_CODE_BANNER,AR_CODE_CONFETTI,AR_CODE_PARTY_HAT,AR_CODE_PARTY_HORN,AR_CODE_CANDLE, AR_CODE_GRAMOPHONE},
            new int[]{AR_CODE_TREASURE_CHEST}
        };



        public static Vector3 GetRelativeDistanceInMeters(float originLat, float originLon, float targetLat, float targetLon)
        {
            //formula from https://stackoverflow.com/questions/639695/how-to-convert-latitude-or-longitude-to-meters
            float R = 6378.137f; // Radius of earth in KM
            float dLat = targetLat - originLat;
            float dLon = targetLon - originLon;
            float a = Mathf.Sin(dLat / 2f) * Mathf.Sin(dLat / 2f) +
                           Mathf.Cos(originLat) * Mathf.Cos(targetLat) *
                            Mathf.Sin(dLon / 2f) * Mathf.Sin(dLon / 2f);
            float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));
            float d = R * c;
            return d * 1000f * (new Vector3(dLon,0,dLat)).normalized; // meters
        }

        public static Memento CreateMemento(string id, string coordinates, Texture2D picture, string message, int pageantry, string sceneObjects)
        {
            Memento memento = ScriptableObject.CreateInstance<Memento>();
            memento.objectName = "New memento";
            memento.user_id = id;
            memento.coordinates = coordinates;
            memento.picture = picture;
            memento.message = message;
            memento.pageantryCode = pageantry;
            memento.sceneObjects = sceneObjects;
            return memento;
        }

        public static Memento CreateMemento(string id, string coordinates, Texture2D picture, string message, int pageantry, List<SceneObject> sceneObjects)
        {
            return CreateMemento(id, coordinates, picture, message, pageantry, SceneObject.GetRawSceneJSON(sceneObjects));
        }

        public static Memento CreateMemento(InstaPicJSON instaPicJSON, Texture2D picture)
        {
            return CreateMemento
            (
                id: instaPicJSON.ID,
                coordinates: instaPicJSON.Location.Coordinates,
                picture: picture,
                message: instaPicJSON.Caption,
                pageantry: AR_CODE_POLAROID,
                sceneObjects: "" 
            );
        }
    }

    public class User
    {
        public string user_name;
        public string pw;

        public User(string user_name, string pw)
        {
            this.user_name = user_name;
            this.pw = pw;
        }
    }

    public class MementoJSON
    {
        public string user_id;
        public string coordinates;
        public string message;
        public string image;
        public string pegeantry_code;

        public MementoJSON(Memento memento)
        {
            this.user_id = memento.user_id;
            this.coordinates = memento.coordinates;
            this.message = memento.message;
            byte[] imageByte = memento.picture.EncodeToJPG();
            this.image = System.Convert.ToBase64String(imageByte);
            this.pegeantry_code = memento.pageantryCode.ToString();
        }
    }
}
