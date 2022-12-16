using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // instance
    public static GameManager Instance;

    // camera
    private Camera2DFollow cam;

    // player info
    public string currentPlayer { get; private set; } = "Axis";
    public Transform currentPlayerTransform { get; private set; }
    private PlayerState playerLoadState;
    public float maxHP { get; set; } = 100f;
    public float maxMP { get; set; } = 100f;
    private float _playerHP;
    private float _playerMP;
    public float playerHP { get { return _playerHP; } set { _playerHP = Mathf.Clamp(value, 0, maxHP); } }
    public float playerMP { get { return _playerMP; } set { _playerMP = Mathf.Clamp(value, 0, maxMP); } }

    // game state info
    public string gameState { get; set; } = "play";
    private Dictionary<string, bool> sceneFlags = new Dictionary<string, bool>();
    private bool canRepress = true;
    private int framesSinceLastClick = 0;

    // other references
    GameObject savePointUI;
    GameObject axisObj;
    GameObject mystObj;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneFlags.Add("Start_Helipad-collectible1", false);
        sceneFlags.Add("SampleScene-collectible1", false);
        sceneFlags.Add("SampleScene-collectible2", false);
        sceneFlags.Add("Start_Hotel-collectible1", false);
        sceneFlags.Add("Start_Hotel-FakeWall1", false);
        playerHP = maxHP;
        playerMP = maxMP;
        ConfigRefs();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (playerLoadState != null) {
            ConfigRefs();
            if (currentPlayer == "Axis") {
                currentPlayerTransform = GameObject.Find("Axis").transform;
                GameObject.Find("Axis").GetComponent<AxisPlayerController>().LoadFromPlayerState(playerLoadState);
            } else {
                currentPlayerTransform = GameObject.Find("Myst_Character").transform;
                GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().LoadFromPlayerState(playerLoadState);
                GameObject.Find("Myst_Sword").GetComponent<MystSwordController>().ResetPosition();
            }
            cam = GameObject.Find("Main Camera").GetComponent<Camera2DFollow>();
            float camX = Mathf.Clamp(playerLoadState.position.x, cam.sceneLeftEdge + Constants.CAM_VIEWPORT_WIDTH/2, cam.sceneLeftEdge + cam.sceneWidth - Constants.CAM_VIEWPORT_WIDTH/2);
            float camY = Mathf.Clamp(playerLoadState.position.y, cam.sceneBottomEdge + Constants.CAM_VIEWPORT_HEIGHT/2, cam.sceneBottomEdge + cam.sceneHeight - Constants.CAM_VIEWPORT_HEIGHT/2);
            GameObject.Find("Main Camera").transform.position = new Vector3(camX, camY, -15);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentPlayerTransform = GameObject.Find("Axis").transform;
    }

    void LateUpdate() {
        if (gameState == "saving" && Input.GetButtonDown("Cancel") && canRepress) {
            LeaveSavePoint();
            canRepress = false;
        } else if (Input.GetButtonUp("Cancel")) {
            canRepress = true;
        }
        framesSinceLastClick++;
    }

    void ConfigRefs() {
        savePointUI = GameObject.Find("SavePointOptions");
        axisObj = GameObject.Find("Axis");
        mystObj = GameObject.Find("Myst");
        if (currentPlayer == "Axis") {
            mystObj.SetActive(false);
        } else {
            axisObj.SetActive(false);
        }
        GameObject.Find("SwapChars").GetComponent<Button>().onClick.AddListener(this.SwapCharacters);
        GameObject.Find("SaveQuit").GetComponent<Button>().onClick.AddListener(this.QuitToMenu);
        savePointUI.SetActive(false);
    }

    public void SwapCharacters() {
        if (framesSinceLastClick < 5) {
            return;
        }
        if (currentPlayer == "Axis")
        {
            currentPlayer = "Myst";
            mystObj.SetActive(true);
            GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().LoadFromPlayerState(new PlayerState(axisObj.GetComponent<AxisPlayerController>()));
            currentPlayerTransform = GameObject.Find("Myst_Character").transform;
            GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().moveMode = "normal";
            GameObject.Find("Myst_Sword").GetComponent<MystSwordController>().ResetPosition();
            axisObj.SetActive(false);
        } else {
            currentPlayer = "Axis";
            axisObj.SetActive(true);
            axisObj.GetComponent<AxisPlayerController>().LoadFromPlayerState(new PlayerState(GameObject.Find("Myst_Character").GetComponent<MystPlayerController>()));
            axisObj.GetComponent<AxisPlayerController>().moveMode = "normal";
            currentPlayerTransform = GameObject.Find("Axis").transform;
            mystObj.SetActive(false);
        }
        framesSinceLastClick = 0;
    }

    public void ChangeScene(Transition t, PlayerState ps)
    {
        if (t.GetTargetTransitionType() == "B") {
            if (currentPlayer == "Axis") {
                GameObject.Find("Axis").GetComponent<AxisPlayerController>().StopFloat();
            } else {
                GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().StopFloat();
            }
        }
        SceneManager.LoadScene(t.targetScene);
        playerLoadState = ps;
        float dist = currentPlayer == "Axis" ? GameObject.Find("Axis").GetComponent<AxisPlayerController>().DistanceToGround() : GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().DistanceToGround();
        if (t.GetTargetTransitionType() == "B") {
            playerLoadState.position = new Vector3(playerLoadState.facingRight ? t.targetX - 2 : t.targetXFacingLeft + 2, t.targetY+dist-2, 0);
            if (currentPlayer == "Myst" && playerLoadState.moveMode == "Float") {
                GameObject.Find("Myst_Character").GetComponent<MystPlayerController>().StopFloat();
            }
            playerLoadState.moveMode = "up-transition";
        } else {
            playerLoadState.position = new Vector3(t.targetX, t.targetY+dist, 0);
        }
        if (t.GetTargetTransitionType() != "E") {
            Input.ResetInputAxes();
        }
    }

    public bool GetSceneFlag(string flag) {
        return sceneFlags[flag];
    }

    public void SetSceneFlag(string flag, bool value) {
        if (sceneFlags.ContainsKey(flag)) {
            sceneFlags[flag] = value;
        }
    }

    public void GetCollectible(string name)
    {
        Debug.Log("Got collectible - "+name+"!");
        SetSceneFlag(name, true);
    }

    public void DestroyTerrain(GameObject obj)
    {
        SetSceneFlag(obj.name, true);
        obj.SetActive(false);
    }

    public void UseSavePoint()
    {
        gameState = "saving";
        savePointUI.SetActive(true);
    }

    public void LeaveSavePoint() {
        gameState = "play";
        savePointUI.SetActive(false);
    }

    public void QuitToMenu() {
        SceneManager.LoadScene("MenuMain");
    }
}
