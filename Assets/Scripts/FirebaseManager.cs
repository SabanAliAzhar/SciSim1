using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    [Header("Registration UI Elements")]
    public TMP_InputField fullNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Dropdown qualificationDropdown;
    public TMP_Text registerStatusText;
    public Button registerButton;

    [Header("Login UI Elements")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginStatusText;
    public Button loginButton;

    private FirebaseAuth auth;
    private DatabaseReference dbReference;
    private FirebaseApp app;
    public Button forgotPasswordButton;

    [Header("Panels")] 
    public GameObject RegistrationPanel;
    public GameObject LoginPanel;
public Button registerFromLoginButton;
public Button continueButton;

    void Awake()
    {
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordButtonClick);
        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        registerStatusText.text = "Make sure you are connected to Internet";
        loginStatusText.text = "Make sure you are connected to Internet";
        InitializeFirebase();
    }

    void InitializeFirebase()
{
    Firebase.FirebaseApp.LogLevel = LogLevel.Debug;
    Debug.Log("Starting Firebase Initialization...");

    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        Debug.Log("Dependency check complete.");

        if (task.IsFaulted)
        {
            Debug.LogError("Firebase dependency check failed: " + task.Exception);
            return;
        }

        if (task.Result == DependencyStatus.Available)
        {
            Debug.Log("Firebase dependencies are available.");

            app = FirebaseApp.DefaultInstance;

            // Set the Realtime Database URL manually to avoid warning
            app.Options.DatabaseUrl = new System.Uri("https://scisim-vr-fyp-default-rtdb.firebaseio.com/");

            auth = FirebaseAuth.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            Debug.Log("Firebase Initialized Successfully");
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);

            if (registerStatusText != null)
                registerStatusText.text = "Please Check Your Internet Connection!";
            
            if (loginStatusText != null)
                loginStatusText.text = "Please Check Your Internet Connection!";
        }
    });
}


    // ---------------------------- Registration ----------------------------
    public void OnRegisterButtonClick()
    {
        string fullName = fullNameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        int selectedQualificationIndex = qualificationDropdown.value;
        string qualification = qualificationDropdown.options[selectedQualificationIndex].text;

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            registerStatusText.text = "All fields must be filled.";
            Debug.LogWarning("All fields must be filled.");
            return;
        }

        if (selectedQualificationIndex == 0)
        {
            registerStatusText.text = "Please select a valid qualification.";
            Debug.LogWarning("Invalid qualification selected.");
            return;
        }

        if (password != confirmPassword)
        {
            registerStatusText.text = "Passwords do not match.";
            Debug.LogWarning("Passwords do not match.");
            return;
        }

        registerStatusText.text = "Registering...";

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                string errorMsg = "Registration failed: " + task.Exception.InnerExceptions[0].Message;
                Debug.LogError(errorMsg);
                registerStatusText.text = errorMsg;
                return;
            }

            FirebaseUser newUser = task.Result.User;
            SaveUserData(newUser.UserId, fullName, email, qualification);
            registerStatusText.text = "Registered successfully as " + newUser.Email;
            RegistrationPanel.SetActive(false);
            LoginPanel.SetActive(true);
            Debug.Log("User Registered: " + newUser.Email);
        });
    }

    void SaveUserData(string userId, string fullName, string email, string qualification)
    {
        User newUser = new User(fullName, email, qualification);
        string json = JsonUtility.ToJson(newUser);

        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User data saved successfully.");
            }
            else
            {
                string errorMsg = "Failed to save user data: " + task.Exception.Message;
                Debug.LogError(errorMsg);
            }
        });
    }

    // ---------------------------- Login ----------------------------
    public void OnLoginButtonClick()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Please enter both email and password.";
            Debug.LogWarning("Login fields missing.");
            return;
        }

        loginStatusText.text = "Logging in...";

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                string errorMsg = "Login failed: " + task.Exception.InnerExceptions[0].Message;
                Debug.LogError(errorMsg);
                loginStatusText.text = errorMsg;
                return;
            }

            FirebaseUser user = task.Result.User;
            loginStatusText.text = "Logged in as " + user.Email + ". Please have a look at the panel on your left side and continue, thanks.";
    continueButton.interactable = true;

    Debug.Log("User Logged In: " + user.Email);

loginEmailInput.transform.parent.parent.gameObject.SetActive(false);
loginPasswordInput.transform.parent.parent.gameObject.SetActive(false);
registerFromLoginButton.transform.parent.parent.gameObject.SetActive(false);
loginButton.transform.parent.parent.gameObject.SetActive(false);
forgotPasswordButton.transform.parent.parent.gameObject.SetActive(false);

continueButton.interactable =true;
            Debug.Log("User Logged In: " + user.Email);
            SceneManager.LoadScene(1);
        });
    }

    // ---------------------------- Forgot Password ----------------------------
    public void OnForgotPasswordButtonClick()
    {
        string email = loginEmailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            loginStatusText.text = "Please enter your email to reset your password.";
            Debug.LogWarning("No email entered for password reset.");
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                string errorMsg = "Failed to send reset email: " + task.Exception.InnerExceptions[0].Message;
                Debug.LogError(errorMsg);
                loginStatusText.text = errorMsg;
                return;
            }

            loginStatusText.text = $"A password reset email has been sent to:\n<b>{email}</b>\n\nCheck your inbox or spam folder.\n\n<color=#00BFFF><i>– SciSim Security System</i></color>";
            Debug.Log("Password reset email sent to " + email);
        });
    }

    // ---------------------------- Data Class ----------------------------
    [System.Serializable]
    public class User
    {
        public string fullName;
        public string email;
        public string qualification;

        public User(string fullName, string email, string qualification)
        {
            this.fullName = fullName;
            this.email = email;
            this.qualification = qualification;
        }
    }
}
