using UnityEngine;
using System.Collections.Generic;
using WyrmTale;
using System;

public class InstaPicJSON
{
    private string id;
    private string user_name;
    private InstaImage image;
    private int cretedTime;
    private string caption;
    private InstaLocation location;
    //private string[] tagged_friends;
    //private int likes;

    public string ID { get { return id; } }
    public string Name { get { return user_name; } }
    public InstaImage Image { get { return image; }}
    public int CreatedTime { get { return cretedTime; }}
    public string Caption { get { return caption; }}
    public InstaLocation Location { get { return location; } }
    //public string[] TaggedFriends { get { return tagged_friends; }}

    public InstaPicJSON(string pId, string pName, InstaImage pImage, int pCreatedTime, JSON pCaption, InstaLocation pLocation)
    {
        id = pId;
        user_name = pName;
        image = pImage;
        cretedTime = pCreatedTime;
        caption = ((pCaption != null) ? pCaption.ToString("text") : "");
        location = pLocation;
        //tagged_friends = pTaggedFriends;
    }
    /*
    public static implicit operator JSON(InstaPicJSON value)
    {
        JSON js = new JSON();
        js["id"] = value.id;
        js["name"] = value.name;
        js["location"] = value.location;
        js["score"] = value.highscore;
        return js;
    }
    */
    // JSON to class conversion
    public static explicit operator InstaPicJSON(JSON value)
    {
        checked
        {
            return new InstaPicJSON(
                value.ToString("id"),
                value.ToJSON("user").ToString("full_name"),
                new InstaImage(value.ToJSON("images").ToJSON("standard_resolution")),
                value.ToInt("createdTime"),
                value.ToJSON("caption"),
                (value.ToJSON("location") == null) ? null : new InstaLocation(value.ToJSON("location"))
            );
        }
    }

    // convert a JSON array to a MyClass Array
    public static InstaPicJSON[] Array(JSON[] array)
    {
        List<InstaPicJSON> tc = new List<InstaPicJSON>();
        for (int i = 0; i < array.Length; i++)
            tc.Add((InstaPicJSON)array[i]);
        return tc.ToArray();
    }

    public static InstaPicJSON[] Array(string rawJSON, string field)
    {
        JSON json = new JSON();
        json.serialized = rawJSON;
        JSON[] jsonArray = json.ToArray<JSON>(field);
        return Array(jsonArray);
    }

    public static InstaPicJSON GetInstaPicJSON(string rawString)
    {
        JSON userJSON = new JSON();
        userJSON.serialized = rawString;
        return (InstaPicJSON)userJSON;
    }

    public override string ToString()
    {
        return string.Format("[InstaPicJSON: ID={0}, Name={1}, Location={2}]", ID, Name, Location.ToString());
    }

    public class InstaImage
    {
        public int width;
        public int height;
        public string url;

        public InstaImage(JSON imageJSON)
        {
            width = imageJSON.ToInt("width");
            height = imageJSON.ToInt("height");
            url = imageJSON.ToString("url");
        }
    }

    public class InstaLocation
    {
        public float latitude;
        public float longitude;
        string name;

        public string Coordinates { get { return latitude + "," + longitude; }}

        public InstaLocation(JSON locationJSON)
        {
            if (locationJSON == null) return;
            latitude = locationJSON.ToInt("latitude");
            longitude = locationJSON.ToInt("longitude");
            name = locationJSON.ToString("name");
        }

        public override string ToString()
        {
            return name;
        }
    }
}
