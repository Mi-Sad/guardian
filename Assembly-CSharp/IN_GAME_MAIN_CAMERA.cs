using UnityEngine;

public class IN_GAME_MAIN_CAMERA : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY,
        MouseX,
        MouseY
    }

    public static GameType Gametype = GameType.Stop;
    public static int Difficulty;
    public static bool TriggerAutoLock;
    public static int Level;
    public static int Character = 1;
    public static bool IsPausing;
    public static bool IsTyping;
    public static float SensitivityMulti = 0.5f;
    public static int InvertY = 1;
    public static int CameraTilt = 1;
    public static STEREO_3D_TYPE StereoType;
    public static DayLight Time = DayLight.Dawn;
    public FengCustomInputs inputManager;
    public RotationAxes axes;
    public float minimumX = -360f;
    public float maximumX = 360f;
    public float minimumY = -60f;
    public float maximumY = 60f;
    public GameObject main_object;
    public bool gameOver;
    public AudioSource bgmusic;
    private float flashDuration;
    private bool needSetHUD;
    private GameObject lockTarget;
    public Material skyBoxDAY;
    public Material skyBoxDAWN;
    public Material skyBoxNIGHT;
    public bool spectatorMode;
    private GameObject locker;
    public static bool UsingTitan;
    public static float CameraDistance = 0.6f;
    private int currentPeekPlayerIndex;
    private Vector3 lockCameraPosition;
    private float R;
    private float duration;
    private float decay;
    private bool flip;
    private float closestDistance;
    private float distance = 10f;
    private Transform head;
    private float distanceMulti;
    private float distanceOffsetMulti;
    private float heightMulti;
    private bool lockAngle;
    private int snapShotDmg;
    private Texture2D snapshot1;
    private Texture2D snapshot2;
    private Texture2D snapshot3;
    private bool startSnapShotFrameCount;
    private float snapShotStartCountDownTime;
    private float snapShotInterval = 0.02f;
    private Vector3 snapShotTargetPosition;
    private GameObject snapShotTarget;
    private int snapShotCount;
    private float snapShotCountDown;
    private bool hasSnapShot;
    public GameObject snapShotCamera;
    public float timer;
    public static CAMERA_TYPE CameraMode;
    public Texture texture;
    public int score;
    public int lastScore;
    public float justHit;
    public static string SingleCharacter;
    public RenderTexture snapshotRT;

    private void Start()
    {
        GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().SetCamera(this);
        IsPausing = false;
        SensitivityMulti = PlayerPrefs.GetFloat("MouseSensitivity");
        InvertY = PlayerPrefs.GetInt("invertMouseY");
        inputManager = GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>();
        setDayLight(Time);
        locker = GameObject.Find("locker");
        if (PlayerPrefs.HasKey("cameraTilt"))
        {
            CameraTilt = PlayerPrefs.GetInt("cameraTilt");
        }
        else
        {
            CameraTilt = 1;
        }
        if (PlayerPrefs.HasKey("cameraDistance"))
        {
            CameraDistance = PlayerPrefs.GetFloat("cameraDistance") + 0.3f;
        }
        createSnapShotRT2();
    }

    public void setDayLight(DayLight val)
    {
        Time = val;
        switch (Time)
        {
            case DayLight.Day:
                RenderSettings.ambientLight = FengColor.AmbientDay;
                GameObject.Find("mainLight").GetComponent<Light>().color = FengColor.Day;
                base.gameObject.GetComponent<Skybox>().material = skyBoxDAY;
                break;
            case DayLight.Dawn:
                RenderSettings.ambientLight = FengColor.AmbientDawn;
                GameObject.Find("mainLight").GetComponent<Light>().color = FengColor.AmbientDawn;
                base.gameObject.GetComponent<Skybox>().material = skyBoxDAWN;
                break;
            case DayLight.Night:
                /* TODO: Flashlight is now handled in HERO#Start()
                GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("flashlight"));
                gameObject.transform.parent = base.transform;
                gameObject.transform.position = base.transform.position;
                gameObject.transform.rotation = Quaternion.Euler(353f, 0f, 0f);*/
                RenderSettings.ambientLight = FengColor.AmbientNight;
                GameObject.Find("mainLight").GetComponent<Light>().color = FengColor.Night;
                base.gameObject.GetComponent<Skybox>().material = skyBoxNIGHT;
                break;
        }
        snapShotCamera.gameObject.GetComponent<Skybox>().material = base.gameObject.GetComponent<Skybox>().material;
    }

    public void setHUDposition()
    {
        GameObject gameObject = GameObject.Find("Flare");
        gameObject.transform.localPosition = new Vector3((int)((float)(-Screen.width) * 0.5f) + 14, (int)((float)(-Screen.height) * 0.5f), 0f);
        gameObject = GameObject.Find("LabelInfoBottomRight");
        gameObject.transform.localPosition = new Vector3((int)((float)Screen.width * 0.5f), (int)((float)(-Screen.height) * 0.5f), 0f);
        gameObject.GetComponent<UILabel>().text = "Pause : " + GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().inputString[InputCode.Pause] + " ";
        gameObject = GameObject.Find("LabelInfoTopCenter");
        gameObject.transform.localPosition = new Vector3(0f, (int)((float)Screen.height * 0.5f), 0f);
        gameObject = GameObject.Find("LabelInfoTopRight");
        gameObject.transform.localPosition = new Vector3((int)((float)Screen.width * 0.5f), (int)((float)Screen.height * 0.5f), 0f);
        GameObject.Find("LabelNetworkStatus").transform.localPosition = new Vector3((int)((float)(-Screen.width) * 0.5f), (int)((float)Screen.height * 0.5f), 0f);
        gameObject = GameObject.Find("LabelInfoTopLeft");
        gameObject.transform.localPosition = new Vector3((int)((float)(-Screen.width) * 0.5f), (int)((float)Screen.height * 0.5f - 20f), 0f);
        gameObject = GameObject.Find("Chatroom");
        gameObject.transform.localPosition = new Vector3((int)((float)(-Screen.width) * 0.5f), (int)((float)(-Screen.height) * 0.5f), 0f);
        if ((bool)GameObject.Find("Chatroom"))
        {
            GameObject.Find("Chatroom").GetComponent<InRoomChat>().UpdatePosition();
        }
        if (!UsingTitan || Gametype == GameType.Singleplayer)
        {
            GameObject.Find("skill_cd_bottom").transform.localPosition = new Vector3(0f, (int)((float)(-Screen.height) * 0.5f + 5f), 0f);
            GameObject.Find("GasUI").transform.localPosition = GameObject.Find("skill_cd_bottom").transform.localPosition;
            GameObject.Find("stamina_titan").transform.localPosition = new Vector3(0f, 9999f, 0f);
            GameObject.Find("stamina_titan_bottom").transform.localPosition = new Vector3(0f, 9999f, 0f);
        }
        else
        {
            Vector3 localPosition = new Vector3(0f, 9999f, 0f);
            GameObject.Find("skill_cd_bottom").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_armin").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_eren").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_jean").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_levi").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_marco").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_mikasa").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_petra").transform.localPosition = localPosition;
            GameObject.Find("skill_cd_sasha").transform.localPosition = localPosition;
            GameObject.Find("GasUI").transform.localPosition = localPosition;
            GameObject.Find("stamina_titan").transform.localPosition = new Vector3(-160f, (int)((float)(-Screen.height) * 0.5f + 15f), 0f);
            GameObject.Find("stamina_titan_bottom").transform.localPosition = new Vector3(-160f, (int)((float)(-Screen.height) * 0.5f + 15f), 0f);
        }
        if (main_object != null && main_object.GetComponent<HERO>() != null)
        {
            if (Gametype == GameType.Singleplayer)
            {
                main_object.GetComponent<HERO>().setSkillHUDPosition2();
            }
            else if (main_object.GetPhotonView() != null && main_object.GetPhotonView().isMine)
            {
                main_object.GetComponent<HERO>().setSkillHUDPosition2();
            }
        }
        if (StereoType == STEREO_3D_TYPE.SIDE_BY_SIDE)
        {
            base.gameObject.GetComponent<Camera>().aspect = Screen.width / Screen.height;
        }
        createSnapShotRT2();
    }

    public void Flash()
    {
        GameObject gameObject = GameObject.Find("flash");
        gameObject.GetComponent<UISprite>().alpha = 1f;
        flashDuration = 2f;
    }

    private float GetSensitivityMultiWithTimeDelta()
    {
        return SensitivityMulti * UnityEngine.Time.deltaTime * 62f;
    }

    private float GetSensitivityMulti()
    {
        return SensitivityMulti;
    }

    private int GetReverse()
    {
        return InvertY;
    }

    private void Reset()
    {
        if (Gametype == GameType.Singleplayer)
        {
            GameObject.Find("MultiplayerManager").GetComponent<FengGameManagerMKII>().RestartGameSingle();
        }
    }

    public void SetSpectorMode(bool val)
    {
        spectatorMode = val;
        GameObject.Find("MainCamera").GetComponent<SpectatorMovement>().disable = !val;
        GameObject.Find("MainCamera").GetComponent<MouseLook>().disable = !val;
    }

    private void MoveCamera()
    {
        distanceOffsetMulti = CameraDistance * (200f - base.camera.fieldOfView) / 150f;
        base.transform.position = ((!(head != null)) ? main_object.transform.position : head.transform.position);
        base.transform.position += Vector3.up * heightMulti;
        base.transform.position -= Vector3.up * (0.6f - CameraDistance) * 2f;

        float dYaw = Input.GetAxis("Mouse X") * 10f * GetSensitivityMulti();
        float dPitch = (0f - Input.GetAxis("Mouse Y")) * 10f * GetSensitivityMulti() * (float)GetReverse();

        switch (CameraMode)
        {
            case CAMERA_TYPE.WOW:
                {
                    if (Input.GetKey(KeyCode.Mouse1))
                    {
                        base.transform.RotateAround(base.transform.position, Vector3.up, dYaw);
                        float currentPitch = base.transform.rotation.eulerAngles.x % 360f;
                        float newPitch = currentPitch + dPitch;
                        if ((!(dPitch > 0f) || ((!(currentPitch < 260f) || !(newPitch > 260f)) && (!(currentPitch < 80f) || !(newPitch > 80f)))) && (!(dPitch < 0f) || ((!(currentPitch > 280f) || !(newPitch < 280f)) && (!(currentPitch > 100f) || !(newPitch < 100f)))))
                        {
                            base.transform.RotateAround(base.transform.position, base.transform.right, dPitch);
                        }
                    }
                    break;
                }
            case CAMERA_TYPE.TPS:
                {
                    if (!inputManager.menuOn)
                    {
                        Screen.lockCursor = true;
                    }
                    base.transform.RotateAround(base.transform.position, Vector3.up, dYaw);
                    float currentPitch = base.transform.rotation.eulerAngles.x % 360f;
                    float newPitch = currentPitch + dPitch;
                    if ((!(dPitch > 0f) || ((!(currentPitch < 260f) || !(newPitch > 260f)) && (!(currentPitch < 80f) || !(newPitch > 80f)))) && (!(dPitch < 0f) || ((!(currentPitch > 280f) || !(newPitch < 280f)) && (!(currentPitch > 100f) || !(newPitch < 100f)))))
                    {
                        base.transform.RotateAround(base.transform.position, base.transform.right, dPitch);
                    }
                    break;
                }
            case CAMERA_TYPE.ORIGINAL:
                {
                    float num = 0f;
                    Vector3 mousePosition = Input.mousePosition;
                    if (mousePosition.x < (float)Screen.width * 0.4f)
                    {
                        num = (0f - ((Screen.width * 0.4f) - mousePosition.x) / (float)Screen.width * 0.4f) * GetSensitivityMultiWithTimeDelta() * 150f;
                        base.transform.RotateAround(base.transform.position, Vector3.up, num);
                    }
                    else
                    {
                        if (mousePosition.x > (float)Screen.width * 0.6f)
                        {
                            num = (mousePosition.x - (float)Screen.width * 0.6f) / (float)Screen.width * 0.4f * GetSensitivityMultiWithTimeDelta() * 150f;
                            base.transform.RotateAround(base.transform.position, Vector3.up, num);
                        }
                    }
                    float x = 140f * ((Screen.height * 0.6f) - mousePosition.y) / (float)Screen.height * 0.5f;
                    base.transform.rotation = Quaternion.Euler(x, base.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.z);
                    break;
                }
        }

        base.transform.position -= base.transform.forward * distance * distanceMulti * distanceOffsetMulti;
        if (CameraDistance < 0.65f)
        {
            base.transform.position += base.transform.right * Mathf.Max((0.6f - CameraDistance) * 2f, 0.65f);
        }
    }

    private void UpdateShake()
    {
        if (duration > 0f)
        {
            duration -= UnityEngine.Time.deltaTime;
            if (flip)
            {
                base.gameObject.transform.position += Vector3.up * R;
            }
            else
            {
                base.gameObject.transform.position -= Vector3.up * R;
            }
            flip = !flip;
            R *= decay;
        }
    }

    public void StartShake(float R, float duration, float decay = 0.95f)
    {
        if (this.duration < duration)
        {
            this.R = R;
            this.duration = duration;
            this.decay = decay;
        }
    }

    private GameObject findNearestTitan()
    {
        GameObject[] array = GameObject.FindGameObjectsWithTag("titan");
        GameObject result = null;
        float num = closestDistance = float.PositiveInfinity;
        Vector3 position = main_object.transform.position;
        foreach (GameObject gameObject in array)
        {
            float magnitude = (gameObject.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck").position - position).magnitude;
            if (magnitude < num && (!gameObject.GetComponent<TITAN>() || !gameObject.GetComponent<TITAN>().hasDie))
            {
                result = gameObject;
                num = (closestDistance = magnitude);
            }
        }
        return result;
    }

    public GameObject SetMainObject(GameObject obj, bool resetRotation = true, bool lockAngle = false)
    {
        main_object = obj;
        if (obj == null)
        {
            head = null;
            distanceMulti = (heightMulti = 1f);
        }
        else if ((bool)main_object.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head"))
        {
            head = main_object.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head");
            distanceMulti = ((!(head == null)) ? (Vector3.Distance(head.transform.position, main_object.transform.position) * 0.2f) : 1f);
            heightMulti = ((!(head == null)) ? (Vector3.Distance(head.transform.position, main_object.transform.position) * 0.33f) : 1f);
            if (resetRotation)
            {
                base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else if ((bool)main_object.transform.Find("Amarture/Controller_Body/hip/spine/chest/neck/head"))
        {
            head = main_object.transform.Find("Amarture/Controller_Body/hip/spine/chest/neck/head");
            distanceMulti = (heightMulti = 0.64f);
            if (resetRotation)
            {
                base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else
        {
            head = null;
            distanceMulti = (heightMulti = 1f);
            if (resetRotation)
            {
                base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        this.lockAngle = lockAngle;
        return obj;
    }

    public GameObject SetMainObjectTitan(GameObject obj)
    {
        main_object = obj;
        if ((bool)main_object.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head"))
        {
            head = main_object.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck/head");
            distanceMulti = ((!(head == null)) ? (Vector3.Distance(head.transform.position, main_object.transform.position) * 0.4f) : 1f);
            heightMulti = ((!(head == null)) ? (Vector3.Distance(head.transform.position, main_object.transform.position) * 0.45f) : 1f);
            base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        return obj;
    }

    public void snapShotUpdate()
    {
        if (startSnapShotFrameCount)
        {
            snapShotStartCountDownTime -= UnityEngine.Time.deltaTime;
            if (snapShotStartCountDownTime <= 0f)
            {
                snapShot2(1);
                startSnapShotFrameCount = false;
            }
        }
        if (!hasSnapShot)
        {
            return;
        }
        snapShotCountDown -= UnityEngine.Time.deltaTime;
        if (snapShotCountDown <= 0f)
        {
            GameObject.Find("UI_IN_GAME").GetComponent<UIReferArray>().panels[0].transform.Find("snapshot1").GetComponent<UITexture>().enabled = false;
            hasSnapShot = false;
            snapShotCountDown = 0f;
        }
        else if (snapShotCountDown < 1f)
        {
            GameObject.Find("UI_IN_GAME").GetComponent<UIReferArray>().panels[0].transform.Find("snapshot1").GetComponent<UITexture>().mainTexture = snapshot3;
        }
        else if (snapShotCountDown < 1.5f)
        {
            GameObject.Find("UI_IN_GAME").GetComponent<UIReferArray>().panels[0].transform.Find("snapshot1").GetComponent<UITexture>().mainTexture = snapshot2;
        }
        if (snapShotCount < 3)
        {
            snapShotInterval -= UnityEngine.Time.deltaTime;
            if (snapShotInterval <= 0f)
            {
                snapShotInterval = 0.05f;
                snapShotCount++;
                snapShot2(snapShotCount);
            }
        }
    }

    public void snapShot2(int index)
    {
        snapShotCamera.transform.position = ((!(head != null)) ? main_object.transform.position : head.transform.position);
        snapShotCamera.transform.position += Vector3.up * heightMulti;
        snapShotCamera.transform.position -= Vector3.up * 1.1f;
        Vector3 position;
        Vector3 a = position = snapShotCamera.transform.position;
        Vector3 vector = (a + snapShotTargetPosition) * 0.5f;
        snapShotCamera.transform.position = vector;
        a = vector;
        snapShotCamera.transform.LookAt(snapShotTargetPosition);
        if (index == 3)
        {
            snapShotCamera.transform.RotateAround(base.transform.position, Vector3.up, Random.Range(-180f, 180f));
        }
        else
        {
            snapShotCamera.transform.RotateAround(base.transform.position, Vector3.up, Random.Range(-20f, 20f));
        }
        snapShotCamera.transform.LookAt(a);
        snapShotCamera.transform.RotateAround(a, base.transform.right, Random.Range(-20f, 20f));
        float num = Vector3.Distance(snapShotTargetPosition, position);
        if (snapShotTarget != null && (bool)snapShotTarget.GetComponent<TITAN>())
        {
            float num2 = num;
            float num3 = index - 1;
            vector = snapShotTarget.transform.localScale;
            num = num2 + num3 * vector.x * 10f;
        }
        snapShotCamera.transform.position -= snapShotCamera.transform.forward * Random.Range(num + 3f, num + 10f);
        snapShotCamera.transform.LookAt(a);
        snapShotCamera.transform.RotateAround(a, base.transform.forward, Random.Range(-30f, 30f));
        Vector3 end = (!(head != null)) ? main_object.transform.position : head.transform.position;
        Vector3 vector2 = ((!(head != null)) ? main_object.transform.position : head.transform.position) - snapShotCamera.transform.position;
        end -= vector2;
        LayerMask mask = 1 << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = 1 << LayerMask.NameToLayer("EnemyBox");
        LayerMask mask3 = (int)mask | (int)mask2;
        RaycastHit hitInfo;
        if (head != null)
        {
            if (Physics.Linecast(head.transform.position, end, out hitInfo, mask))
            {
                snapShotCamera.transform.position = hitInfo.point;
            }
            else if (Physics.Linecast(head.transform.position - vector2 * distanceMulti * 3f, end, out hitInfo, mask3))
            {
                snapShotCamera.transform.position = hitInfo.point;
            }
        }
        else if (Physics.Linecast(main_object.transform.position + Vector3.up, end, out hitInfo, mask3))
        {
            snapShotCamera.transform.position = hitInfo.point;
        }
        switch (index)
        {
            case 1:
                snapshot1 = RTImage2(snapShotCamera.GetComponent<Camera>());
                SnapShotSaves.AddImage(snapshot1, snapShotDmg);
                break;
            case 2:
                snapshot2 = RTImage2(snapShotCamera.GetComponent<Camera>());
                SnapShotSaves.AddImage(snapshot2, snapShotDmg);
                break;
            case 3:
                snapshot3 = RTImage2(snapShotCamera.GetComponent<Camera>());
                SnapShotSaves.AddImage(snapshot3, snapShotDmg);
                break;
        }
        snapShotCount = index;
        hasSnapShot = true;
        snapShotCountDown = 2f;
        if (index == 1)
        {
            UITexture uiTexture1 = GameObject.Find("UI_IN_GAME").GetComponent<UIReferArray>().panels[0].transform.Find("snapshot1").GetComponent<UITexture>();
            uiTexture1.mainTexture = snapshot1;
            uiTexture1.transform.localScale = new Vector3((float)Screen.width * 0.4f, (float)Screen.height * 0.4f, 1f);
            uiTexture1.transform.localPosition = new Vector3((float)(-Screen.width) * 0.225f, (float)Screen.height * 0.225f, 0f);
            uiTexture1.transform.rotation = Quaternion.Euler(0f, 0f, 10f);
            if (PlayerPrefs.HasKey("showSSInGame") && PlayerPrefs.GetInt("showSSInGame") == 1)
            {
                uiTexture1.enabled = true;
            }
            else
            {
                uiTexture1.enabled = false;
            }
        }
    }

    private void Awake()
    {
        IsTyping = false;
        IsPausing = false;
        base.name = "MainCamera";
        if (PlayerPrefs.HasKey("GameQuality"))
        {
            if (PlayerPrefs.GetFloat("GameQuality") >= 0.9f)
            {
                GetComponent<TiltShift>().enabled = true;
            }
            else
            {
                GetComponent<TiltShift>().enabled = false;
            }
        }
        else
        {
            GetComponent<TiltShift>().enabled = true;
        }
        CreateMinimap();
    }

    private Texture2D RTImage2(Camera cam)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D texture2D = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        int num = (int)((float)cam.targetTexture.width * 0.04f);
        int num2 = (int)((float)cam.targetTexture.width * 0.02f);
        try
        {
            texture2D.SetPixel(0, 0, Color.white);
            texture2D.ReadPixels(new Rect(num, num, cam.targetTexture.width - num, cam.targetTexture.height - num), num2, num2);
            texture2D.Apply();
            RenderTexture.active = active;
        }
        catch
        {
            texture2D = new Texture2D(1, 1);
            texture2D.SetPixel(0, 0, Color.white);
            return texture2D;
        }
        return texture2D;
    }

    public void startSnapShot2(Vector3 p, int dmg, GameObject target, float startTime)
    {
        if (int.TryParse((string)FengGameManagerMKII.Settings[95], out int result))
        {
            if (dmg >= result)
            {
                snapShotCount = 1;
                startSnapShotFrameCount = true;
                snapShotTargetPosition = p;
                snapShotTarget = target;
                snapShotStartCountDownTime = startTime;
                snapShotInterval = 0.05f + Random.Range(0f, 0.03f);
                snapShotDmg = dmg;
            }
        }
        else
        {
            snapShotCount = 1;
            startSnapShotFrameCount = true;
            snapShotTargetPosition = p;
            snapShotTarget = target;
            snapShotStartCountDownTime = startTime;
            snapShotInterval = 0.05f + Random.Range(0f, 0.03f);
            snapShotDmg = dmg;
        }
    }

    public void update2()
    {
        if (flashDuration > 0f)
        {
            flashDuration -= UnityEngine.Time.deltaTime;
            if (flashDuration <= 0f)
            {
                flashDuration = 0f;
            }
            GameObject.Find("flash").GetComponent<UISprite>().alpha = flashDuration * 0.5f;
        }
        if (Gametype == GameType.Stop)
        {
            Screen.showCursor = true;
            Screen.lockCursor = false;
            return;
        }
        if (Gametype != GameType.Singleplayer && gameOver)
        {
            if (inputManager.isInputDown[InputCode.Attack1])
            {
                if (spectatorMode)
                {
                    SetSpectorMode(val: false);
                }
                else
                {
                    SetSpectorMode(val: true);
                }
            }
            if (inputManager.isInputDown[InputCode.Flare1])
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                currentPeekPlayerIndex = (currentPeekPlayerIndex + 1) % players.Length;
                if (players.Length > 0)
                {
                    SetMainObject(players[currentPeekPlayerIndex]);
                    SetSpectorMode(val: false);
                    lockAngle = false;
                }
            }
            if (inputManager.isInputDown[InputCode.Flare2])
            {
                currentPeekPlayerIndex--;
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                int num2 = players.Length;
                if (currentPeekPlayerIndex >= num2)
                {
                    currentPeekPlayerIndex = 0;
                }
                if (currentPeekPlayerIndex < 0)
                {
                    currentPeekPlayerIndex = num2 - 1;
                }
                if (num2 > 0)
                {
                    SetMainObject(players[currentPeekPlayerIndex]);
                    SetSpectorMode(val: false);
                    lockAngle = false;
                }
            }
            if (spectatorMode)
            {
                return;
            }
        }
        if (inputManager.isInputDown[InputCode.Pause])
        {
            if (IsPausing)
            {
                if (main_object != null)
                {
                    Vector3 position = base.transform.position;
                    position = ((head == null) ? main_object.transform.position : head.transform.position);
                    position += Vector3.up * heightMulti;
                    base.transform.position = Vector3.Lerp(base.transform.position, position - base.transform.forward * 5f, 0.2f);
                }
                return;
            }
            IsPausing = !IsPausing;
            if (IsPausing)
            {
                if (Gametype == GameType.Singleplayer)
                {
                    UnityEngine.Time.timeScale = 0f;
                }
                GameObject.Find("InputManagerController").GetComponent<FengCustomInputs>().menuOn = true;
                Screen.showCursor = true;
                Screen.lockCursor = false;
            }
        }
        if (needSetHUD)
        {
            needSetHUD = false;
            setHUDposition();
            Screen.lockCursor = !Screen.lockCursor;
            Screen.lockCursor = !Screen.lockCursor;
        }
        if (inputManager.isInputDown[InputCode.Fullscreen])
        {
            Screen.fullScreen = !Screen.fullScreen;
            if (Screen.fullScreen)
            {
                Screen.SetResolution(960, 600, fullscreen: false);
            }
            else
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen: true);
            }
            needSetHUD = true;
            Minimap.OnScreenResolutionChanged();
        }
        if (inputManager.isInputDown[InputCode.Restart])
        {
            Reset();
        }
        if (main_object == null)
        {
            return;
        }
        if (inputManager.isInputDown[InputCode.ChangeCamera])
        {
            switch (CameraMode)
            {
                case CAMERA_TYPE.TPS:
                    CameraMode = CAMERA_TYPE.ORIGINAL;
                    Screen.lockCursor = false;
                    break;
                case CAMERA_TYPE.ORIGINAL:
                    CameraMode = CAMERA_TYPE.WOW;
                    Screen.lockCursor = false;
                    break;
                case CAMERA_TYPE.WOW:
                    CameraMode = CAMERA_TYPE.TPS;
                    Screen.lockCursor = true;
                    break;
            }
            if ((int)FengGameManagerMKII.Settings[245] == 1 || main_object.GetComponent<HERO>() == null)
            {
                Screen.showCursor = false;
            }
        }
        if (inputManager.isInputDown[InputCode.ToggleCursor])
        {
            Screen.showCursor = !Screen.showCursor;
        }
        if (inputManager.isInputDown[InputCode.Focus])
        {
            TriggerAutoLock = !TriggerAutoLock;
            if (TriggerAutoLock)
            {
                lockTarget = findNearestTitan();
                if (closestDistance >= 150f)
                {
                    lockTarget = null;
                    TriggerAutoLock = false;
                }
            }
        }
        if (gameOver)
        {
            if (FengGameManagerMKII.InputRC.isInputHumanDown(InputCodeRC.LiveCamera))
            {
                if ((int)FengGameManagerMKII.Settings[263] == 0)
                {
                    FengGameManagerMKII.Settings[263] = 1;
                }
                else
                {
                    FengGameManagerMKII.Settings[263] = 0;
                }
            }
            HERO component = main_object.GetComponent<HERO>();
            if (component != null && (int)FengGameManagerMKII.Settings[263] == 1 && component.GetComponent<SmoothSyncMovement>().enabled && component.isPhotonCamera)
            {
                CameraMovementLive(component);
            }
            else if (lockAngle)
            {
                base.transform.rotation = Quaternion.Lerp(base.transform.rotation, main_object.transform.rotation, 0.2f);
                base.transform.position = Vector3.Lerp(base.transform.position, main_object.transform.position - main_object.transform.forward * 5f, 0.2f);
            }
            else
            {
                MoveCamera();
            }
        }
        else
        {
            MoveCamera();
        }
        if (TriggerAutoLock && lockTarget != null)
        {
            float z = base.transform.eulerAngles.z;
            Transform transform = lockTarget.transform.Find("Amarture/Core/Controller_Body/hip/spine/chest/neck");
            Vector3 a = transform.position - ((head == null) ? main_object.transform.position : head.transform.position);
            a.Normalize();
            lockCameraPosition = ((head == null) ? main_object.transform.position : head.transform.position);
            lockCameraPosition -= a * distance * distanceMulti * distanceOffsetMulti;
            lockCameraPosition += Vector3.up * 3f * heightMulti * distanceOffsetMulti;
            base.transform.position = Vector3.Lerp(base.transform.position, lockCameraPosition, UnityEngine.Time.deltaTime * 4f);
            if (head != null)
            {
                base.transform.LookAt(head.transform.position * 0.8f + transform.position * 0.2f);
            }
            else
            {
                base.transform.LookAt(main_object.transform.position * 0.8f + transform.position * 0.2f);
            }
            base.transform.localEulerAngles = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, z);
            Vector2 vector = base.camera.WorldToScreenPoint(transform.position - transform.forward * lockTarget.transform.localScale.x);
            locker.transform.localPosition = new Vector3(vector.x - (float)Screen.width * 0.5f, vector.y - (float)Screen.height * 0.5f, 0f);
            if (lockTarget.GetComponent<TITAN>() != null && lockTarget.GetComponent<TITAN>().hasDie)
            {
                lockTarget = null;
            }
        }
        else
        {
            locker.transform.localPosition = new Vector3(0f, (float)(-Screen.height) * 0.5f - 50f, 0f);
        }
        Vector3 end = (head == null) ? main_object.transform.position : head.transform.position;
        Vector3 normalized = (((head == null) ? main_object.transform.position : head.transform.position) - base.transform.position).normalized;
        end -= distance * normalized * distanceMulti;
        LayerMask mask = 1 << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = 1 << LayerMask.NameToLayer("EnemyBox");
        LayerMask mask3 = (int)mask | (int)mask2;
        RaycastHit hitInfo;
        if (head != null)
        {
            if (Physics.Linecast(head.transform.position, end, out hitInfo, mask))
            {
                base.transform.position = hitInfo.point;
            }
            else if (Physics.Linecast(head.transform.position - normalized * distanceMulti * 3f, end, out hitInfo, mask2))
            {
                base.transform.position = hitInfo.point;
            }
            Debug.DrawLine(head.transform.position - normalized * distanceMulti * 3f, end, Color.red);
        }
        else if (Physics.Linecast(main_object.transform.position + Vector3.up, end, out hitInfo, mask3))
        {
            base.transform.position = hitInfo.point;
        }
        UpdateShake();
    }

    private void CreateMinimap()
    {
        if (FengGameManagerMKII.Level != null)
        {
            Minimap minimap = base.gameObject.AddComponent<Minimap>();
            if (Minimap.Instance.myCam == null)
            {
                GameObject gameObject = new GameObject();
                Minimap.Instance.myCam = gameObject.AddComponent<Camera>();
                Minimap.Instance.myCam.nearClipPlane = 0.3f;
                Minimap.Instance.myCam.farClipPlane = 1000f;
                Minimap.Instance.myCam.enabled = false;
            }
            minimap.CreateMinimap(Minimap.Instance.myCam, 512, 0.3f, FengGameManagerMKII.Level.MinimapPreset);
            if ((int)FengGameManagerMKII.Settings[231] == 0 || RCSettings.GlobalDisableMinimap == 1)
            {
                minimap.SetEnabled(enabled: false);
            }
        }
    }

    public void createSnapShotRT2()
    {
        if (snapshotRT != null)
        {
            snapshotRT.Release();
        }
        if (QualitySettings.GetQualityLevel() > 3)
        {
            snapshotRT = new RenderTexture((int)((float)Screen.width * 0.8f), (int)((float)Screen.height * 0.8f), 24);
            snapShotCamera.GetComponent<Camera>().targetTexture = snapshotRT;
        }
        else
        {
            snapshotRT = new RenderTexture((int)((float)Screen.width * 0.4f), (int)((float)Screen.height * 0.4f), 24);
            snapShotCamera.GetComponent<Camera>().targetTexture = snapshotRT;
        }
    }

    public void CameraMovementLive(HERO hero)
    {
        float magnitude = hero.rigidbody.velocity.magnitude;
        if (magnitude > 10f)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, Mathf.Min(100f, magnitude + 40f), 0.1f);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 50f, 0.1f);
        }
        float d = hero.CameraMultiplier * (200f - Camera.main.fieldOfView) / 150f;
        base.transform.position = head.transform.position + Vector3.up * heightMulti - Vector3.up * (0.6f - CameraDistance) * 2f;
        base.transform.position -= base.transform.forward * distance * distanceMulti * d;
        if (hero.CameraMultiplier < 0.65f)
        {
            base.transform.position += base.transform.right * Mathf.Max((0.6f - hero.CameraMultiplier) * 2f, 0.65f);
        }
        base.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, hero.GetComponent<SmoothSyncMovement>().correctCameraRot, UnityEngine.Time.deltaTime * 5f);
    }
}
