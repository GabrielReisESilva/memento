using UnityEngine;
using System.Collections;
using System;
using Geogram;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using WyrmTale;
using System.Collections.Generic;

public class WebServerManager : MonoBehaviour
{
    private const string ACCESS_TOKEN = "473895000.0932292.8890bbb960fe4547947f1738889176dd";

    private int serverCalls;
    private BoolController serverInUseIcon;
    private DatabaseReference dbReference;
    private bool useFirebase = false;

    public BoolController ServerIcon { set { serverInUseIcon = value; } }

    private void Start()
    {
#if UNITY_EDITOR
        useFirebase = false;
#else
        useFirebase = true;
#endif
        if(useFirebase)
        {
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Utils.URL_FIREBASE);
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            DatabaseReference mementoReference = FirebaseDatabase.DefaultInstance.GetReference(Utils.REFERENCE_MEMENTOS);
            mementoReference.ChildAdded += OnNewMementoAdd;
        }
    }

    public void AuthenticateUser(string user, string pw, ServerCallback onSucess, ServerCallback onFailure)
    {
        if(!useFirebase)
        {
            onSucess(user);
            return;
        }
            
        if (user == "-1")
        {
            onFailure(Utils.SERVER_MESSAGE_NO_ACCOUNT);
            return;
        }

        FirebaseDBSearch("users", "pw", user,
                         (string pass) =>
        {
            if (pass == pw) onSucess(user);
            else onFailure("Wrong Password: " + pass + " != " + pw);
        },
                         onFailure);
    }

    public void CreateUser(string userID, string pw, ServerCallback onSucess, ServerCallback onFailure)
    {
        if (!useFirebase) return;
        string userJSON = JsonUtility.ToJson(new User("newUser", "pass"));
        FirebaseDBCreate(Utils.REFERENCE_USERS, userID, userJSON,
                         onSucess,
                         onFailure);
    }

    public void LoadAllMementos(MementoCallback onSucess, ServerCallback onFailure)
    {
        if (!useFirebase) return;
        FirebaseDBSearchAll(Utils.REFERENCE_MEMENTOS,
                            (string json) =>
        {
            List<Memento> mementos = Memento.List(json, "mementos");
            onSucess(mementos);
        }, onFailure);
    }

    public void SaveMemento(Memento memento, ServerCallback onSucess, ServerCallback onFailure)
    {
        if (!useFirebase) return;
        string mementoJSON = ((JSON)memento).serialized;//JsonUtility.ToJson(new MementoJSON(memento));
        FirebaseDBCreate(Utils.REFERENCE_MEMENTOS, UnityEngine.Random.Range(0, 1000).ToString(), mementoJSON,
                         onSucess,
                         onFailure);
    }

    public void GetUserData(ServerCallback sucessCallback, ServerCallback errorCallback)
    {
        StartCoroutine(Query(
            url: Utils.URL_IG_API,
            method: "users/self/",
            parameters: new string[] { "access_token" },
            args: new string[] { ACCESS_TOKEN },
            callback: sucessCallback,
            errorCallback: errorCallback
            ));
    }

    public void GetRecentMedia(ServerCallback sucessCallback, ServerCallback errorCallback)
    {
        StartCoroutine(Query(
            url: Utils.URL_IG_API,
            method: "users/self/media/recent/",
            parameters: new string[] { "access_token" },
            args: new string[] { ACCESS_TOKEN },
            callback: sucessCallback,
            errorCallback: errorCallback
            ));
    }

    public void GetComments(string mediaId, ServerCallback sucessCallback, ServerCallback errorCallback)
    {
        StartCoroutine(Query(
            url: Utils.URL_IG_API,
            method: "media/" + mediaId + "/comments",
            parameters: new string[] { "access_token" },
            args: new string[] { ACCESS_TOKEN },
            callback: sucessCallback,
            errorCallback: errorCallback
            ));
    }

    private IEnumerator Query(string url, string method, string[] parameters, string[] args, ServerCallback callback, ServerCallback errorCallback)
    {
        serverCalls++;
        Debug.Log(serverCalls);
        CancelInvoke();
        if (serverInUseIcon != null)
        {
            serverInUseIcon(true);
        }
        // BUILD PARAMETERS
        string query = url + method;
        if (parameters != null && parameters.Length > 0)
        {
            query += "?" + parameters[0] + "=" + args[0];
            for (int i = 1; i < parameters.Length; i++)
                query += "&" + parameters[i] + "=" + args[i];
        }

        //Debug.Log(query);
        WWW service = new WWW(query);
        yield return service;

        if (service.error == null)
        {
            Debug.Log("Server: OK");
            if (callback != null)
                callback(service.text);

            if (serverInUseIcon != null)
            {
                serverInUseIcon(true);
            }
        }
        else
        {
            if (callback != null)
                errorCallback(service.error);
        }

        serverCalls--;

        if (serverCalls == 0)
            DisactiveIcon();
        //Invoke("DisactiveIcon", 2);
    }
    //.OrderByChild(key).EqualTo(value)
    private void FirebaseDBSearch(string reference, string key, string value, ServerCallback onSucess, ServerCallback onFailure)
    {
        FirebaseDatabase.DefaultInstance.GetReference(reference).GetValueAsync().ContinueWith(
            task =>
                {
                    if (task.IsFaulted)
                    {
                        onFailure("FIREBASE FAIL");
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        if (snapshot.ChildrenCount > 0 && snapshot.Child(value).Exists)
                        {
                            //Debug.Log(snapshot);
                            //Debug.Log(snapshot.Child(value));
                            //Debug.Log(snapshot.Child(value).Child(key));
                            //Debug.Log(snapshot.Child(value).Child(key).Value);
                            DataSnapshot db = snapshot.Child(value);
                            onSucess(db.Child("pw").Value.ToString());
                        }
                        else
                        {
                            onFailure("User not found");
                        }
                        // Do something with snapshot...
                    }
                }
        );
    }

    private void FirebaseDBSearchAll(string reference, ServerCallback onSucess, ServerCallback onFailure)
    {
        FirebaseDatabase.DefaultInstance.GetReference(reference).GetValueAsync().ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                {
                    onFailure("FIREBASE FAIL");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    string jsonArray = "{\"" + reference + "\":[";
                    bool first = false;
                    foreach (DataSnapshot item in snapshot.Children)
                    {
                        if (!first) first = true;
                        else jsonArray += ",";

                        //Debug.Log(item.GetRawJsonValue());
                        jsonArray += item.GetRawJsonValue();
                    }
                    jsonArray += "]}";
                    //Debug.Log(jsonArray);
                    onSucess(jsonArray);
                }
            }
        );
    }

    private void FirebaseDBCreate(string reference, string userId, string userJSON, ServerCallback onSucess, ServerCallback onFailure)
    {
        //Debug.Log(userJSON);
        dbReference.Child(reference).Child(userId).SetRawJsonValueAsync(userJSON);
    }

    private void DisactiveIcon()
    {
        if (serverInUseIcon != null)
            serverInUseIcon(false);
    }

#region EVENT_HANDLER

    private void OnNewMementoAdd(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Memento newMemento = Memento.FromString(args.Snapshot.GetRawJsonValue());
        Debug.Log("NEW MEMENTO ADDED: " + newMemento);
        AppManager.Instance.AddNewMemento(newMemento);
    }

#endregion
}

/* 
 * ------------------------------------------TEMPLATE QUERY
    public void CreateProfile(Profile user)
    {
        StartCoroutine(Query(
            method: "createUser.php",
            parameters: new string[] { "id", "name", "location" },
            args: new string[] { user.ID, user.Name.Replace(" ", "_"), user.Location },
            callback: null
            ));
    }

    public void GetUser(Profile user, Action<string> callback)
    {
        StartCoroutine(Query(
            method: "getUser.php",
            parameters: new string[] { "id" },
            args: new string[] { user.ID },
            callback: callback
        ));
    }

    public void UpdateScore(Profile user, string value)
    {
        StartCoroutine(Query(
            method: "updateUser.php",
            parameters: new string[] { "id", "score" },
            args: new string[] { user.ID, value },
            callback: null
        ));
    }

    public void GetHighscore(Profile user, Profile[] friends, Action<String> callback)
    {
        if (user == null || friends == null)
            return;

        string friendsId = user.ID;
        for (int i = 0; i < friends.Length; i++)
            friendsId += "_" + friends[i].ID;

        StartCoroutine(Query(
            method: "getHighscore.php",
            parameters: new string[] { "friends", "location" },
            args: new string[] { friendsId, user.Location },
            callback: callback
        ));
    }
    */
