using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
  private string loginEmail;
  private string loginPassword;
  private string signUpEmail;
  private string signUpPassword;
  private string signUpConfirmPassword;
  private string signupName;

  private InputField loginEmailInputField;
  private InputField signUpScreenNameInputField;
  private InputField loginPasswordInputField;
  private InputField signUpEmailInputField;
  private InputField signUpPasswordInputField;
  private InputField signUpConfirmPasswordInputField;

  private DatabaseManager db_manager;
  private bool db_ready;

  public FirebaseAuth auth;
  public FirebaseUser user;
  public UserData userData;

  public AnimatorController AnimControl;
  // Start is called before the first frame update
  async void Start()
  {
    
    loginEmailInputField = GameObject.Find("LoginScreenEmailInput").GetComponent<InputField>();
    loginPasswordInputField = GameObject.Find("LoginScreenPasswordInput").GetComponent<InputField>();
    signUpEmailInputField = GameObject.Find("SignUpScreenEmailInputField").GetComponent<InputField>();
    signUpPasswordInputField = GameObject.Find("SignUpScreenPasswordInputField").GetComponent<InputField>();
    signUpConfirmPasswordInputField = GameObject.Find("SignUpScreenConfirmPasswordInputField").GetComponent<InputField>();
    signUpScreenNameInputField = GameObject.Find("SignUpScreenNameInputField").GetComponent<InputField>();

    db_manager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
    db_ready = await db_manager.Init();

    Debug.Log("Setting up Firebase Auth");
    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    auth.StateChanged += AuthStateChanged;
    AuthStateChanged(this, null);
    print (auth);
  }

  // Update is called once per frame
  void Update()
  {
    loginEmail = loginEmailInputField.text;
    loginPassword = loginPasswordInputField.text;
    signupName = signUpScreenNameInputField.text;
    signUpEmail = signUpEmailInputField.text;
    signUpPassword = signUpPasswordInputField.text;
    signUpConfirmPassword = signUpConfirmPasswordInputField.text;
  }

  public void BeginLogin()
  {
    bool success=true;
    print("Submit Login Form!");
    auth.SignInWithEmailAndPasswordAsync(loginEmail, loginPassword).ContinueWithOnMainThread(async task =>
    {
      if (task.IsCanceled)
      {
        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
        AnimControl.showpopup("Oops", "SignInWithEmailAndPasswordAsync  was canceled");
        success=false;
        return;
      }
      if (task.IsFaulted)
      {
        AnimControl.showpopup("Oops", "Wrong Username or Password");
        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
        success=false;
        return;
      }

      if (!success) return;
      Firebase.Auth.FirebaseUser newUser = task.Result;
      // AnimControl.showpopup("YAYYYYY", "Welcome\n" + ""+"!");
      changethescene(1);

    });

  }

  public void BeginSignUp()
  {
    print (auth);
    if (signUpPassword == signUpConfirmPassword)
    {
      auth.CreateUserWithEmailAndPasswordAsync(signUpEmail, signUpPassword).ContinueWithOnMainThread(task =>
      {
        if (task.IsCanceled)
        {
          Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
          AnimControl.showpopup("Oops", "CreateUserWithEmailAndPasswordAsync was canceled");

          return;
        }
        if (task.IsFaulted)
        {
          AnimControl.showpopup("Oops", "One of the things you entered was incorrect.");
          Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

          return;
        }

        // Firebase user has been created.
        Firebase.Auth.FirebaseUser newUser = task.Result;
        UserData user = new UserData
        {
            Email = signUpEmail,
            Name = signupName,
            CurrentChapter = new int[11] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        };
        if (db_ready)
        {
          db_manager.SetUserData(user);
        }
          AnimControl.showpopup("YAYYY!", "Welcome On board, " + user.Name+"!");
          AnimControl.AnimCont.SetTrigger("back");
        Debug.LogFormat("Firebase user created successfully: {0} ({1})",
            user.Name, newUser.UserId);
        
      });
    }
  }

  async void AuthStateChanged(object sender, System.EventArgs eventArgs)
  {
    if (auth.CurrentUser != user)
    {
      bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
      if (!signedIn && user != null)
      {
        Debug.Log("Signed out " + user.UserId);
        // AnimControl.showpopup("Bye...", "Signed out successfully");
        changethescene(0);
      }
      user = auth.CurrentUser;
      if (signedIn)
      {
        Debug.Log("Signed in " + user.UserId);
        userData = await db_manager.GetUserData(user.Email);
        //AnimControl.showpopup("Hello!", "Welcome, "+ user.DisplayName);
        AnimControl.directSignin = signedIn;
        // changethescene(1);
      }
    }
  }

  void OnDestroy()
  {
    auth.StateChanged -= AuthStateChanged;
    auth = null;
  }

  void changethescene(int n){
    GameObject.Find("SceneOpener").GetComponent<SceneOpener>().SceneChange(n);
  }

  public void SignOut()
  {
    auth.SignOut();
    userData = null;
  }
}
