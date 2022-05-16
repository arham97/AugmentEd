using UnityEngine;
using Firebase.Storage;
using Firebase.Extensions;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

public class StorageManager : MonoBehaviour
{
  private FirebaseStorage storage;
  private StorageReference audiosRef;
  private StorageReference assetBundleRef;
  private StorageReference subtitlesRef;
  private AudioSource audioSource;

  public void Init()
  {
    this.storage = FirebaseStorage.DefaultInstance;
    this.audiosRef = storage.GetReferenceFromUrl("gs://augmented-844f1.appspot.com/audios");
    this.assetBundleRef = storage.GetReferenceFromUrl("gs://augmented-844f1.appspot.com/assetBundles");
    this.subtitlesRef = storage.GetReferenceFromUrl("gs://augmented-844f1.appspot.com/subtitles");
    this.audioSource = GameObject.FindGameObjectWithTag("_Audiosource").GetComponent<AudioSource>();
  }

  public async Task DownloadFile(FileType type, int chapter, string file)
  {
    StorageReference fileRef = null;
    string directoryName = "";
    string filename = "";

    if (type == FileType.Audio)
    {
      fileRef = this.audiosRef.Child(String.Format("Chapter {0}/{1}", chapter, file));
      directoryName = "Audios";
      filename = String.Format("Content/{0}/Chapter {1}/{2}", directoryName, chapter, file);
    }
    else if (type == FileType.AssetBundle)
    {
      fileRef = this.assetBundleRef.Child(String.Format("{0}", file));
      directoryName = "AssetBundles";
      filename = String.Format("Content/{0}/{1}", directoryName, file);
    }
    else if (type == FileType.Subtitle)
    {
      fileRef = this.subtitlesRef.Child(String.Format("Chapter {0}/{1}", chapter, file));
      directoryName = "Subtitles";
      filename = String.Format("Content/{0}/Chapter {1}/{2}", directoryName, chapter, file);
    }

    string localFile = Path.Combine(Application.persistentDataPath, filename);

    // Make sure directory exists if user is saving to sub dir.
    Directory.CreateDirectory(Path.GetDirectoryName(localFile));

    Debug.Log("Downloading " + filename);


    Task task = fileRef.GetFileAsync(localFile,
    new StorageProgress<DownloadState>(state =>
    {
      long progress = state.BytesTransferred / state.TotalByteCount * 100;
    }), CancellationToken.None);

    await task;
    print(string.Format("Log > Downloaded {0}!", filename));
  }

  public enum FileType
  {
    Audio,
    AssetBundle,
    Subtitle
  }
}