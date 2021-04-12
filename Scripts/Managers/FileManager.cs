using UnityEngine;
using System.Collections;
using System.IO;
using Geogram;

public class FileManager : MonoBehaviour
{

    public void SavePicture(Texture2D picture)
    {
        SavePicture(picture.EncodeToPNG());
    }

    public void SavePicture(byte[] bytes)
    {
        //string ALBUMN_NAME = "Geogram";
        //string FILE_NAME = "photo.png";
        //return;
#if UNITY_EDITOR
        //System.IO.File.WriteAllBytes(Utils.PHOTO_PATH + FILE_NAME, bytes);
#else
        Debug.Log("Trying to Save");
        NativeGallery.SaveImageToGallery(bytes, Utils.ALBUMN_NAME, Utils.FILE_NAME);
#endif
    }

    public static void WriteStringToFile( string str, string filename )
    {
#if !WEB_BUILD
        string path = pathForDocumentsFile( filename );
        FileStream file = new FileStream (path, FileMode.Create, FileAccess.Write);
        
        StreamWriter sw = new StreamWriter( file );
        sw.WriteLine( str );
        
        sw.Close();
        file.Close();
        Debug.Log("Created");
#endif
    }
    
    public static string ReadStringFromFile( string filename)//, int lineIndex )
    {
#if !WEB_BUILD
        string path = pathForDocumentsFile( filename );
        
        if (File.Exists(path))
        {
            FileStream file = new FileStream (path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader( file );
            
            string str = null;
            str = sr.ReadLine ();
            
            sr.Close();
            file.Close();
            
            return str;
        }
        
        else
        {
            return null;
        }
#else
        return null;
#endif
    }
    
    public static string pathForDocumentsFile( string filename ) 
    { 
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring( 0, Application.dataPath.Length - 5 );
            path = path.Substring( 0, path.LastIndexOf( '/' ) );
            return Path.Combine( Path.Combine( path, "Documents" ), filename );
        }
        
        else if(Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;   
            path = path.Substring(0, path.LastIndexOf( '/' ) ); 
            return Path.Combine (path, filename);
        }   
        
        else 
        {
            string path = Application.dataPath; 
            path = path.Substring(0, path.LastIndexOf( '/' ) );
            return Path.Combine (path, filename);
        }
    }
}
