using UnityEngine;
using WyrmTale;
using Geogram;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Data", menuName = "Inventory/List", order = 1)]
public class Memento : ScriptableObject
{
    public string objectName = "Memento";
    public string user_id = "some_id";
    public string coordinates;
    public Texture2D picture;
    [TextArea]
    public string message;
    public int pageantryCode;
    public string sceneObjects;

    public JSON[] SceneObjectsJSON { get { return JSONArray(sceneObjects); }}

    public static implicit operator JSON(Memento value)
    {
        byte[] imageByte = value.picture.EncodeToJPG();
        JSON js = new JSON();
        js["user_id"] = value.user_id;
        js["coordinates"] = value.coordinates;
        js["message"] = value.message;
        js["image"] = System.Convert.ToBase64String(imageByte);
        js["pageantry_code"] = value.pageantryCode.ToString();
        js["scene_objects"] = value.sceneObjects;
        return js;
    }

    public static explicit operator Memento(JSON value)
    {
        checked
        {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(System.Convert.FromBase64String(value.ToString("image")));
            return Utils.CreateMemento(value.ToString("user_id"), value.ToString("coordinates"),texture2D, value.ToString("message"), int.Parse(value.ToString("pageantry_code")), value.ToString("scene_objects"));
        }
    }

    public static List<Memento> List(JSON[] array)
    {
        List<Memento> tc = new List<Memento>();
        for (int i = 0; i < array.Length; i++)
            tc.Add((Memento)array[i]);
        return tc;
    }

    public static List<Memento> List(string rawJSON, string field)
    {
        JSON json = new JSON();
        json.serialized = rawJSON;
        JSON[] jsonArray = json.ToArray<JSON>(field);
        return List(jsonArray);
    }

    public static Memento[] Array(JSON[] array)
    {
        return List(array).ToArray();
    }

    public static Memento[] Array(string rawJSON, string field)
    {
        return List(rawJSON, field).ToArray();
    }

    public static Memento GetInstaPicJSON(string rawString)
    {
        JSON userJSON = new JSON();
        userJSON.serialized = rawString;
        return (Memento)userJSON;
    }

    public static Memento FromString(string rawJSON)
    {
        JSON json = new JSON();
        json.serialized = rawJSON;
        return (Memento)json;
    }

    public override string ToString()
    {
        return string.Format("[Memento: ID={0}, Location={1}]", user_id, coordinates);
    }

    private static string RawJSON(JSON[] json)
    {
        if (json.Length < 1) return "";

        string result = json[0].serialized;
        for (int i = 0; i < json.Length; i++)
        {
            result = "," + json[i].serialized;
        }
        return result;
    }

    private static JSON[] JSONArray(string raw)
    {
        string newRaw = "{\"test\":[" + raw + "]}";
        JSON json = new JSON();
        json.serialized = newRaw;
        return json.ToArray<JSON>("test");
    }
}