using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    bool tracking = false;

    RaycastHit hit;
    Ray ray;
    private guiManager gui_manager;
    private AudioManager audio_manager;
    private DatabaseManager database_manager;
    private StorageManager storage_manager;
    private AuthManager auth_manager;
    public int chapterNumber;
    public int chapterRequest;

    List<Chapter> chapterData;
    public Text userNameText;

    MetaData server_data;

    // private float sizemin, sizemax;

    async void Start()
    {
        gui_manager = GameObject.Find("GUIManager").GetComponent<guiManager>();
        audio_manager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        database_manager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        storage_manager = GameObject.Find("StorageManager").GetComponent<StorageManager>();
        auth_manager = GameObject.Find("AuthManager").GetComponent<AuthManager>();
        chapterNumber = -1;
        chapterRequest = -1;

        chapterData = new List<Chapter>();

        gui_manager.init();
        gui_manager.setAudioButtions(false);

        audio_manager.Init();

        bool dbReady = this.database_manager.ready;
        storage_manager.Init();

        server_data = await this.database_manager.GetMetaData();

        // Check if assets_version.txt exists

        string version_file = Path.Combine(Application.persistentDataPath, "Content/assets_version.txt");

        if (File.Exists(version_file))
        {
            // If it exists, read contents
            string line = System.IO.File.ReadAllLines(version_file)[0];
            int local_version_number = -1;
            int.TryParse(line, out local_version_number);
            if (local_version_number != -1)
            {
                // Successfully read file. Query DB to check version number
                if (dbReady)
                {
                    print(string.Format("Log > Server asset version: {0}", server_data.assets_version));
                    if (server_data.assets_version > local_version_number)
                    {
                        // Delete everything
                        print("Log > I'm about to set fire to everything!");
                        string data_path = Path.Combine(Application.persistentDataPath, "Content");
                        print(string.Format("Log > Deleting {0}", data_path));
                        System.IO.Directory.Delete(data_path, true);

                        // Download everything
                        Directory.CreateDirectory(Path.GetDirectoryName(version_file));
                        await this.DownloadEverything();
                        File.WriteAllText(version_file, string.Format("{0}", server_data.assets_version));
                    }
                }
            }
        }
        else
        {
            // If it doesn't exist, download everything
            if (dbReady)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(version_file));
                MetaData server_data = await this.database_manager.GetMetaData();
                await this.DownloadEverything();
                File.WriteAllText(version_file, string.Format("{0}", server_data.assets_version));
            }
        }

        await GetAllChapterData();
        gui_manager.setAudioButtions(true);
        gui_manager.LoadingBar.gameObject.SetActive(false);
        audio_manager.UpdateUserProgressUI();
        userNameText.text = "Hi, " + auth_manager.userData.Name;
    }

    public async Task GetAllChapterData()
    {
        for (int chapterNumber = 1; chapterNumber <= server_data.number_of_chapters; chapterNumber++)
        {
            Chapter chapter = await this.database_manager.GetChapter(chapterNumber);
            chapterData.Add(chapter);
        }
    }

    public async Task DownloadEverything()
    {
        print("Log > Downloading everything");
        await GetAllChapterData();
        
        int total_files = 0;
        int files_downloaded = 0;

        chapterData.ForEach(chapter => {
            total_files = total_files + (chapter.Content.Count * 2) + 1;
        });

        for (int chapterNumber = 1; chapterNumber <= server_data.number_of_chapters; chapterNumber++)
        {
            Chapter chapter = chapterData[chapterNumber - 1];
            gui_manager.SetLoadingBar(files_downloaded, total_files);
            
            await storage_manager.DownloadFile(StorageManager.FileType.AssetBundle, chapterNumber, chapter.AssetBundleName);
            files_downloaded++;
            gui_manager.SetLoadingBar(files_downloaded, total_files);

            foreach (var obj in chapter.Content)
            {
                await storage_manager.DownloadFile(StorageManager.FileType.Audio, chapterNumber, obj.AudioPath);
                files_downloaded++;
                gui_manager.SetLoadingBar(files_downloaded, total_files);

                await storage_manager.DownloadFile(StorageManager.FileType.Subtitle, chapterNumber, obj.SubtitlePath);
                files_downloaded++;
                gui_manager.SetLoadingBar(files_downloaded, total_files);
            }
        }
    }

    private void _CreateChapter(string documentID, string assetBundleName, int contentLength)
    {
        List<ContentData> content = new List<ContentData>();

        for (int i = 1; i <= contentLength; i++)
        {
            ContentData contentData = new ContentData
            {
                Index = i - 1,
                AudioPath = string.Format("{0}.mp3", i),
                ModelPath = "",
                SubtitlePath = string.Format("{0}.mp3.srt", i),
            };

            content.Add(contentData);
        }

        database_manager.CreateChapter(documentID, assetBundleName, content);
    }

    void Update()
    {
        //    print("Time:" + Time.time);;
        touchInput();
        if (tracking)
        {
            gui_manager.showGUI();
            // need to do this in input manager
            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    gui_manager.setLabel(hit.transform.name.ToString());
                    if (hit.transform.name.ToString() == "atom")
                    {
                        TogglePlay();
                    }
                }
            }

        }
        else
        {
            gui_manager.hideGUI();
        }

    }

    public void setTracking(bool a)
    {
        print("tracking called");
        tracking = a;
    }

    public void TogglePlay()
    {
        if (audio_manager.paused)
        {
            // audio_manager.Play();
        }
        else
        {
            audio_manager.Pause();
        }

    }

    void touchInput()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Debug.Log(difference);
            changeSize(GameObject.Find("Cube"), difference);
        }
    }

    void changeSize(GameObject obj, float size)
    {
        float a = Mathf.Clamp(obj.transform.localScale.x + size, 20, 200);
        obj.transform.localScale = new Vector3(a, a, a);
    }

    public void setChapter(int chap)
    {
        chapterRequest = chap;
        if (this.chapterNumber == -1)
        {
            gui_manager.showpopup("Load Chapter " + chap.ToString(), "Are you sure?");    
            return;
        }
        gui_manager.showpopup("Switch Chapter", "Are you sure?");
    }

    public void Play()
    {
        audio_manager.Play();
        gui_manager.togglePlayPauseButton();
    }

    public void Stop()
    {
        audio_manager.StopAllPlay();
        gui_manager.togglePlayPauseButton();
    }

    public void SignOut()
    {
        auth_manager.SignOut();
    }

    public void resolveChapterRequest()
    {
        Stop();
        if (this.chapterNumber != -1)
        {
            // Delete existing data for chapterNumber
            Debug.Log("Deleting data for chapter " + this.chapterNumber);
            this.audio_manager.deleteChildren(this.chapterNumber);
        }

        // Update chapterNumber
        this.chapterNumber = chapterRequest;


        // Load Data for new chapterNumber
        Debug.Log("Loading Data for chapter " + this.chapterNumber);
        this.audio_manager.Init();
        this.audio_manager.load(chapterNumber, chapterData[chapterNumber - 1]);
    }

    public void rejectChapterRequest()
    {
        this.chapterRequest = -1;
    }
}
