using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;

# region strict functions and classes
public class Subtitle
{
    public string text = "";
    public float duration = 0;//seconds;

    public Subtitle()
    {
        text = "";
        duration = 0;
    }

    public Subtitle(string t, float dur)
    {
        text = t;
        duration = dur;
    }
}

public class Audiopkg
{
    public List<Subtitle> _subs;
    public AudioClip _AudioClip;
    public String myName;


    public Audiopkg()
    {
        _subs = new List<Subtitle>();
        _AudioClip = null;
        myName = null;
    }

    public Audiopkg(string Name, Subtitle subt, AudioClip aud)
    {
        _subs = new List<Subtitle>();
        _subs.Add(subt);
        myName = Name;
        _AudioClip = aud;
    }

    public Audiopkg(string Name, AudioClip aud)
    {
        _subs = new List<Subtitle>();
        myName = Name;
        _AudioClip = aud;
    }

}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}
#endregion

public class AudioManager : MonoBehaviour
{
    public GameObject[] chapterparents;
    //manager Vars
    public bool paused;
    //Audio Vars
    private List<AudioClip> audioClips;
    private AudioSource audioSource;
    public int currentTrack;
    public bool stop;
    public guiManager guimanager;
    public DatabaseManager db_manager;
    public AuthManager auth_manager;
    // private AudioClip selectedClip;

    // //Subtitle Vars
    public Text Textholder;
    public GameObject subbg;

    public List<Audiopkg> packages;

    // public List<GameObject> models;

    //Animator vars
    public RuntimeAnimatorController modelAnimatorController;

    private int currentChapter;

    public GameObject ChapterTexts;

    void Start()
    {
        //initializing components and variables;

        audioSource = GameObject.FindGameObjectWithTag("_Audiosource").GetComponent<AudioSource>();
        db_manager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
        auth_manager = GameObject.Find("AuthManager").GetComponent<AuthManager>();
        paused = true;
        // subs = new List<Subtitle>();
        // audioClips = new List<AudioClip>();
        packages = new List<Audiopkg>();
        currentTrack = 0;
        stop = true;
        // load();
        // getSubtitles();
        subbg.SetActive(false);
    }

    public void Init()
    {
        paused = true;
        packages = new List<Audiopkg>();
        currentTrack = 0;
        stop = true;

    }

    public void UpdateUserProgressUI() {
        int i = 0;
        foreach (Transform child in ChapterTexts.transform)
        {
            child.gameObject.GetComponent<Text>().text = "Chapter " + (i + 1);
            string _text = child.gameObject.GetComponent<Text>().text;
            child.gameObject.GetComponent<Text>().text = _text + (auth_manager.userData.CurrentChapter[i] == 1 ? "*" : "");
            // child.gameObject.SetActive(auth_manager.userData.CurrentChapter[i] == 1);
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            paused = true;
        }
    }

    IEnumerator PlayCoroutine(int from)
    {
        for (int i = from; i < packages.Count; i++)
        {
            if (stop)
            {
                break;
            }
            currentTrack = i;
            audioSource.clip = packages[i]._AudioClip;
            StartCoroutine("showSubs", packages[i]._subs);
            audioSource.Play(0);
            showModel(currentTrack, currentChapter);
            yield return new WaitForSeconds(audioSource.clip.length);
            hidemodel(currentTrack, currentChapter);
            yield return new WaitForSeconds(1f);
            paused = false;
        }
        stop = true;
        guimanager.togglePlayPauseButton();
        StopAllPlay();

        if (currentChapter > 0)
        {
            AddChapterToProgress(currentChapter);
        }
    }

    public void Play()
    {
        stop = false;
        StartCoroutine("PlayCoroutine", currentTrack);
    }

    public void StopAllPlay()
    {
        stop = true;
        subbg.SetActive(false);
        StopCoroutine("PlayCoroutine");
        StopCoroutine("showSubs");
        Textholder.text = "";
        audioSource.Stop();
    }

    public void rewind()
    {
        StopAllPlay();
        currentTrack -= 1;
        if (currentTrack <= 0) currentTrack = 0;
        Play();
    }

    public void forward()
    {
        StopAllPlay();
        currentTrack += 1;
        if (currentTrack>=(packages.Count-1)){
            currentTrack = packages.Count-1;
        }
        Play();
    }

    public void Pause()
    {
        audioSource.Pause();
        paused = true;
    }

    void addSubtitle(List<Subtitle> list, string txt, float dur)
    {
        list.Add(new Subtitle(txt, dur));
    }


    IEnumerator showSubs(List<Subtitle> subtitles)
    {
        subbg.SetActive(true);
        for (int i = 0; i < subtitles.Count; i++)
        {
            //print(subtitles[i].duration);
            Textholder.text = subtitles[i].text;
            yield return new WaitForSeconds(subtitles[i].duration + 0.2f);
        }
        Textholder.text = "";
        subbg.SetActive(false);
    }

    public void load(int i, Chapter chapterData)
    {
        //need to add individual subtitles when fetching audios
        //string text = File.ReadAllText(Path.Combine(Application.persistentDataPath, "Content/Subtitles/8.mp3.srt").ToString());
        currentChapter = i;
        StartCoroutine(getAudios(i, chapterData));

    }

    public void PlayAudio(int i)
    {
        // audioSource.clip = audioClips[i];
        audioSource.Play();
    }

    private IEnumerator LoadFile(string fullpath)
    {

        // print("LOADING CLIP " + fullpath);

        if (!System.IO.File.Exists(Path.Combine(fullpath + "")))
        {
            print("DIDN'T EXIST: " + fullpath);
            yield break;
        }

        ;
        AudioClip temp = null;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + Path.Combine(fullpath, ""), AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                print("NAHI MILA BOSS!");
            }
            else
            {
                temp = DownloadHandlerAudioClip.GetContent(www);
            }
        }
        yield return temp;
    }

    IEnumerator getAudios(int chapterNum, Chapter chapterData)
    {
        guimanager.setAudioButtions(false);
        AudioClip selectedClip;
        // audioClips.Clear();
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Path.Combine(Application.persistentDataPath, "Content/Audios/" + "Chapter "+chapterNum.ToString()));
        foreach (var obj in chapterData.Content)
        {
            string audioPath = Path.Combine(Application.persistentDataPath, string.Format("Content/Audios/Chapter {0}/{1}", chapterNum, obj.AudioPath));
            string subtitlePath = Path.Combine(Application.persistentDataPath, string.Format("Content/Subtitles/Chapter {0}/{1}", chapterNum, obj.SubtitlePath));
            
            CoroutineWithData cd = new CoroutineWithData(this, LoadFile(audioPath));
            yield return cd.coroutine;
            selectedClip = cd.result as AudioClip;
            selectedClip.name = obj.AudioPath;
            Audiopkg temp = new Audiopkg(selectedClip.name, selectedClip);
            fetchNadd(subtitlePath, temp);
            packages.Add(temp);
        }
        loadmodels(chapterNum);
        guimanager.setAudioButtions(true);
    }

    void fetchNadd(string path, Audiopkg _pkg)
    {
        List<String> strLines = System.IO.File.ReadLines(path).ToList<String>();
        string expression = "\\d\\d:\\d\\d:\\d\\d,\\d\\d\\d";
        double difference = 0;
        foreach (string item in strLines)
        {
            if (Regex.IsMatch(item, "^\\d+$"))
            {
                Console.WriteLine(Regex.Match(item, "^\\d+$"));
                continue;
            }
            // MatchCollection mc = Regex.Matches(item, expression);
            string[] mc = Regex.Matches(item, expression).OfType<Match>().Select(match => match.Value).ToArray<string>();

            if (mc.Length > 0)
            {
                // Console.WriteLine (mc[0].Substring(3,2) + " - " + mc[1].Substring(3,2) );// minutes
                // Console.WriteLine (mc[0].Substring(6,2) + " - " + mc[1].Substring(6,2) );// seconds
                // Console.WriteLine (mc[0].Substring(9,3) + " - " + mc[1].Substring(9,3) );// miliseconds
                DateTime dt1 = new DateTime(2000, 10, 10, 1, int.Parse(mc[0].Substring(3, 2)), int.Parse(mc[0].Substring(6, 2)), int.Parse(mc[0].Substring(9, 3)));
                DateTime dt2 = new DateTime(2000, 10, 10, 1, int.Parse(mc[1].Substring(3, 2)), int.Parse(mc[1].Substring(6, 2)), int.Parse(mc[1].Substring(9, 3)));
                difference = (dt2 - dt1).TotalSeconds;
                continue;
            }

            if (!Regex.IsMatch(item, "^\\s*$"))
            {
                // print(item + " Float: " + ((float)difference).ToString() + " Double: " + difference);
                addSubtitle(_pkg._subs, item, (float)difference);
                // Console.WriteLine("differnece " + (dt2 - dt1).ToString());
                // Console.WriteLine(item);
            }
        }
    }

    public void testfunc()
    {
        print(packages[2]._subs[1].text);
        print(packages[2]._subs[1].duration);

    }

    void loadmodels(int chapternumber)
    {
        var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.persistentDataPath,"Content/AssetBundles", "chapter"+chapternumber.ToString()));
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle! Chapter " + chapternumber);
            return;
        }

        // models = new List<GameObject>();

        for (int i = 0; i < packages.Count; i++)
        {
            GameObject go = myLoadedAssetBundle.LoadAsset<GameObject>((i + 1).ToString());
            go.AddComponent<Animator>();
            go.GetComponent<Animator>().runtimeAnimatorController = modelAnimatorController;
            // models.Add(go);

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            
            GameObject temp = Instantiate(go);
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.identity;
            
            temp.transform.SetParent(chapterparents[chapternumber - 1].transform);
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.identity;
        }

        /* foreach (GameObject item in models)
        {
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            GameObject temp = Instantiate(item);
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.identity;
            temp.transform.SetParent(chapterparents[chapternumber - 1].transform);
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localRotation = Quaternion.identity;

        } */

        for (int i = 0; i < chapterparents[chapternumber - 1].transform.childCount; i++)
        {
            chapterparents[chapternumber - 1].transform.GetChild(i).gameObject.transform.localPosition = Vector3.zero;
            chapterparents[chapternumber - 1].transform.GetChild(i).gameObject.transform.localRotation = Quaternion.identity;
            chapterparents[chapternumber - 1].transform.GetChild(i).gameObject.SetActive(false);
        }

        // myLoadedAssetBundle.Unload(false);

    }

    void showModel(int n, int chap)
    {

        for (int i = 0; i < chapterparents[chap - 1].transform.childCount; i++)
        {
            chapterparents[chap - 1].transform.GetChild(i).gameObject.SetActive(false);
        }

        chapterparents[chap - 1].transform.GetChild(n).gameObject.SetActive(true);
        chapterparents[chap - 1].transform.GetChild(n).GetComponent<Animator>().SetTrigger("intro");
    }

    void hidemodel(int n, int chap)
    {
        chapterparents[chap - 1].transform.GetChild(n).GetComponent<Animator>().SetTrigger("outtro");
    }

    public void deleteChildren(int chapter)
    {
        AssetBundle.UnloadAllAssetBundles(true);
        string chapterName = "chap" + (chapter).ToString();

        Transform trs = GameObject.Find(chapterName).transform;

        Debug.Log(trs.childCount);
        int i = 0;

        //Array to hold all child obj
        GameObject[] allChildren = new GameObject[trs.childCount];

        //Find all child obj and store to that array
        foreach (Transform child in trs)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        //Now destroy them
        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child.gameObject);
        }

        Debug.Log(trs.childCount);
    }

    private async void AddChapterToProgress(int chapterNumber)
    {
        UserData newData = auth_manager.userData;
        newData.CurrentChapter[chapterNumber - 1] = 1;
        await db_manager.SetUserData(newData);
        auth_manager.userData = newData;
        Debug.Log("Added Chapter to Progress" + chapterNumber);
        UpdateUserProgressUI();
    }
}

