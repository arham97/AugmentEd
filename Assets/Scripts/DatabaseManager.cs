using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DatabaseManager : MonoBehaviour
{
    private FirebaseFirestore db;
    // private ListenerRegistration _listener;
    public bool ready;

    void Start()
    {
        ready = false;
    }

    public async Task<bool> Init()
    {
        print("Log > Initializing DatabaseManager...");
        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        db = FirebaseFirestore.DefaultInstance;
                        ready = true;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(System.String.Format(
                          "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                        ready = false;
                    }
                });
        return ready;
    }

    public async Task<Chapter> GetChapter(int chapterNumber)
    {
        string ID = string.Format("Chapter {0}", chapterNumber);
        if (ready)
        {
            print("Log > Getting Chapter...");
            DocumentReference docRef = db.Collection("GameData").Document(ID);
            if (docRef != null)
            {
                return await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    DocumentSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // Write to struct
                        Chapter chapter = snapshot.ConvertTo<Chapter>();
                        return chapter;
                    }
                    else
                    {
                        Debug.LogError("Log > There was some issue getting data from the server.");
                        return null;
                    }
                });
            }
        }
        return null;
    }

    public async void CreateChapter(string title, string assetBundleName, List<ContentData> content)
    {
        if (ready)
        {
            DocumentReference docRef = db.Collection("GameData").Document(title);


            Chapter chapter = new Chapter
            {
                Title = title,
                Content = content,
                AssetBundleName = assetBundleName,
            };


            await docRef.SetAsync(chapter).ContinueWithOnMainThread(task =>
            {
                Debug.Log("Added data");
            });
        }
    }

    public async Task<MetaData> GetMetaData()
    {
        if (ready)
        {
            DocumentReference docRef = db.Collection("GameData").Document("MetaData");
            if (docRef != null)
            {
                return await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    DocumentSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // Write to struct
                        MetaData data = snapshot.ConvertTo<MetaData>();
                        return data;
                    }
                    else
                    {
                        Debug.LogError("Log > There was some issue getting data from the server.");
                        return null;
                    }
                });
            }
        }
        return null;
    }

    public async Task<UserData> GetUserData(string email)
    {
        if (ready)
        {
            DocumentReference docRef = db.Collection("UserData").Document(email);
            if (docRef != null)
            {
                return await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    DocumentSnapshot snapshot = task.Result;

                    if (snapshot.Exists)
                    {
                        // Write to struct
                        UserData data = snapshot.ConvertTo<UserData>();
                        return data;
                    }
                    else
                    {
                        Debug.LogError("DB: There was some issue getting data from the server.");
                        return null;
                    }
                });
            }
        }
        return null;
    }

    public async Task<bool> SetUserData(UserData input)
    {
        if (ready)
        {
            DocumentReference docRef = db.Collection("UserData").Document(input.Email);

			await docRef.SetAsync(input.ToDict()).ContinueWithOnMainThread(task =>
            {
                Debug.Log("DB: User Data set!");
            });
            return true;
        }
        return false;
    }

    void Update()
    {
        // Do nothing...yet.
    }

    void OnDestroy()
    {
        // _listener.Stop();
    }
}


[FirestoreData]
public class Chapter
{
    [FirestoreProperty]
    public string Title { get; set; }

    [FirestoreProperty]
    public List<ContentData> Content { get; set; }

    [FirestoreProperty]
    public string AssetBundleName { get; set; }

    public override string ToString()
    {
        return $"This is {this.Title}";
    }
}

[FirestoreData]
public class ContentData
{
    [FirestoreProperty]
    public int Index { get; set; }

    [FirestoreProperty]
    public string AudioPath { get; set; }

    [FirestoreProperty]
    public string ModelPath { get; set; }

    [FirestoreProperty]
    public string SubtitlePath { get; set; }
}

[FirestoreData]
public class MetaData
{
    [FirestoreProperty]
    public int assets_version { get; set; }

    [FirestoreProperty]
    public int number_of_chapters { get; set; }
}

[FirestoreData]
public class UserData
{
    [FirestoreProperty]
    public string Email { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public int[] CurrentChapter { get; set; }

    public Dictionary<string, object> ToDict()
    {
        Dictionary<string, object> res = new Dictionary<string, object>();
		res.Add("Name", this.Name);
        res.Add("Email", this.Email);
		res.Add("CurrentChapter", this.CurrentChapter);
        return res;
    }
}