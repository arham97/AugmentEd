using UnityEngine;
using UnityEngine.UI;

public class guiManager : MonoBehaviour
{
  // Start is called before the first frame update


  //Audio buttons
  public GameObject playButton;
  public GameObject stopButton;
  public GameObject rewindButton;
  public GameObject forwardButton;
  public GameObject signOutButton;
  public Slider LoadingBar;

  public AudioManager audioManager;
  //private GameObject button;
  private GameManager game_manager;

  public GameObject popup;
  public Text PopupText;
  public Text PopupHeading;

  void Start()
  {
    game_manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    LoadingBar.gameObject.SetActive(false);
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void init()
  {
    // label = GameObject.Find("Label").GetComponent<Text>();
    // label.text = "";

    // label_bg = GameObject.Find("bgimage");
    // label_bg.SetActive(false);

    // button = GameObject.Find("Button");
    // button.SetActive(false);
  }

  public void showGUI()
  {
    // label_bg.SetActive(true);
    // button.SetActive(true);
  }

  public void hideGUI()
  {
    // label.text = "";
    // label_bg.SetActive(false);
    // button.SetActive(false);
  }

  public void setLabel(string labelText)
  {
    // label.text = labelText;
  }

  public void onButtonClick()
  {
    Debug.Log("Button Clicked!");
    game_manager.TogglePlay();
  }

  public void setAudioButtions(bool state)
  {
    print(string.Format("Setting UI Controls to: {0}", state));
    if (!state)
    {
      playButton.SetActive(state);
      stopButton.SetActive(state);
      rewindButton.SetActive(state);
      forwardButton.SetActive(state);
      signOutButton.SetActive(state);
      return;
    }

    if (audioManager.stop)
    {
      playButton.SetActive(true);
      stopButton.SetActive(false);
    }
    else
    {
      playButton.SetActive(false);
      stopButton.SetActive(true);
    }

    rewindButton.SetActive(true);
    forwardButton.SetActive(true);
    signOutButton.SetActive(true);

  }

  public void togglePlayPauseButton()
  {
    if (audioManager.stop)
    {
      playButton.SetActive(true);
      stopButton.SetActive(false);
    }
    else
    {
      playButton.SetActive(false);
      stopButton.SetActive(true);
    }
  }

  public void nextButton(){
    audioManager.forward();
  }

  public void SetLoadingBar(float val, float max ){
    LoadingBar.gameObject.SetActive(true);
    LoadingBar.maxValue = max;
    LoadingBar.value = val;
  }

  public void showpopup(string heading, string txt)
  {
        PopupHeading.text = heading;
        PopupText.text = txt;
        popup.SetActive(true);
  }
}
