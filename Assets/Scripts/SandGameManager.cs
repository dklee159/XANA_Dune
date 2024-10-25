using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnumCheck;
using Invector;
using Invector.vCharacterController;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

public class SandGameManager : MonoBehaviour
{
    private static SandGameManager instance = null;

    [SerializeField] CrabSpawner crabSpr;
    [SerializeField] SandUIManager uiMgr;

    [SerializeField] Transform startPoint;
    [SerializeField] Transform resetPoint;
    [SerializeField] Transform player;
    [SerializeField] SkinnedMeshRenderer[] playerRenderers;
    [SerializeField] Transform mark;
    [SerializeField] Transform board;

    [SerializeField] ParticleSystem finishParticle1;
    [SerializeField] ParticleSystem finishParticle2;
    [SerializeField] AudioSource rewardSound;
    [SerializeField] Button resetBtn;

    InputManager input;

    Animator animator;

    vThirdPersonInput playerInput;
    vThirdPersonCamera playerCamera;

    private string url = "https://7cjaa2ckmj.execute-api.ap-northeast-1.amazonaws.com/default/Lambda-XANA";

    Dictionary<string, string[]> rankList = new Dictionary<string, string[]>();
    public Transform Player
    {
        get
        {
            return player;
        }
    }

    const int startTime = 3;
    bool isStart = false;
    float timer = 0;
    float limitTime = 60;
    float record = 0;

    public float Timer
    {
        get
        {
            return Mathf.Floor(timer);
        }

        set
        {
            timer = value;
        }
    }

    public string id = "88";

    public Localization local = Localization.Jp;

    public static SandGameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        switch (Application.systemLanguage)
        {
            case SystemLanguage.Japanese:
                local = Localization.Jp;
                break;
            case SystemLanguage.English:
                local = Localization.En;
                break;
            default:
                local = Localization.Jp;
                break;
        }
    }

    void Start()
    {
        playerInput = player.GetComponent<vThirdPersonInput>();
        playerCamera = Camera.main.GetComponent<vThirdPersonCamera>();
        input = board.GetComponent<InputManager>();
        animator = player.GetComponent<Animator>();

        uiMgr.AddCallback(Des.SandInform, () => { StartBoarding(); });
        resetBtn.onClick.AddListener(() => { ResetPlayer(); });
        resetBtn.gameObject.SetActive(false);

        //TODO
        //You guy should link to these functions with your XANA userId
        StartCoroutine(CheckPoint());
        SetRanking();
    }

    IEnumerator CheckPoint()
    {
        WWWForm form = new WWWForm();
        form.AddField("command", "getPoint");
        form.AddField("id", id);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        string point = www.downloadHandler.text;
        if (point == "Register complete") point = "0";
        uiMgr.SetPointUI(point);

        www.Dispose();
    }

    public void TeleportSand()
    {
        uiMgr.DescriptionStart(Des.StartInform);
    }

    public void StartBoarding()
    {
        isStart = true;
        animator.SetBool("IsStart", true);
        SetPlayerStartPos();
        resetBtn.gameObject.SetActive(true);
        uiMgr.TimerStart();
        uiMgr.TimerOn(true);
        StartCoroutine(StartBoardingControl());
    }

    IEnumerator StartBoardingControl()
    {
        uiMgr.SetTimerText("00.00");
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.freezeRotation = false;

        input.enabled = true;
        input.force = 250;
        input.StopMove();

        board.gameObject.SetActive(true);
        int i = 0;
        Debug.Log("Boarding ready set");

        while (i < startTime)
        {
            i++;

            yield return new WaitForSeconds(1);
        }

        input.canRotate = true;
        input.force = input.startForce;
        animator.SetTrigger("BoardOn");
        Debug.Log("Boarding Start");
        crabSpr.OnCrabStart();

        while (isStart)
        {
            timer += Time.deltaTime;
            string timerFormat = string.Format("{0:00.00}", timer);
            uiMgr.SetTimerText(timerFormat);
            yield return null;
        }
    }

    public void GameOver()
    {
        if (!isStart) return;
        record = timer;
        timer = 0;
        isStart = false;
        crabSpr.OnCrabStop();
        Debug.Log(record);
        input.canRotate = false;

        rewardSound.Play();
        finishParticle1.Play();
        finishParticle2.Play();
        uiMgr.TimerOn(false);
        resetBtn.gameObject.SetActive(false);
        StartCoroutine(OnGameOver());
    }


    IEnumerator OnGameOver()
    {
        string recordString = string.Format("{0:00.00}", record);
        string playRecordString = recordString;

        WWWForm formSave = new WWWForm();
        formSave.AddField("command", "saveRecord");
        formSave.AddField("id", id);
        formSave.AddField("record", recordString);
        UnityWebRequest wwwSave = UnityWebRequest.Post(url, formSave);
        wwwSave.SendWebRequest();
        yield return new WaitUntil(() => wwwSave.isDone);
        Debug.Log(wwwSave.downloadHandler.text);

        WWWForm formPersonalRank = new WWWForm();
        formPersonalRank.AddField("command", "getPersonalRank");
        formPersonalRank.AddField("id", id);
        // formPersonalRank.AddField("record", recordString);

        UnityWebRequest wwwPersonalRank = UnityWebRequest.Post(url, formPersonalRank);
        wwwPersonalRank.SendWebRequest();
        yield return new WaitUntil(() => wwwPersonalRank.isDone);
        string personalRank = wwwPersonalRank.downloadHandler.text;
        string unit = "";

        int rankReward = 0;
        int playReward = 0;

        switch (int.Parse(personalRank))
        {
            case 1:
                rankReward = 100;
                unit = "st";
                break;
            case 2:
                rankReward = 70;
                unit = "nd";
                break;
            case 3:
                rankReward = 50;
                unit = "rd";
                break;
            default:
                rankReward = 0;
                unit = "th";
                break;
        }

        if (record < 14)
        {
            playReward = 150;
        }
        else if (record < 20)
        {
            playReward = 120;
        }
        else if (record < 30)
        {
            playReward = 100;
        }
        else if (record >= 30)
        {
            playReward = 10;
        }

        string rewardString = rankReward != 0 ? $"{playReward}(+{rankReward})" : $"{playReward}";
        int totalReward = rankReward == 0 ? playReward : playReward + rankReward;
        WWWForm formReward = new WWWForm();
        formReward.AddField("command", "savePoint");
        formReward.AddField("id", id);
        formReward.AddField("point", totalReward);

        UnityWebRequest wwwReward = UnityWebRequest.Post(url, formReward);
        wwwReward.SendWebRequest();
        yield return new WaitUntil(() => wwwReward.isDone);
        Debug.Log(wwwReward.downloadHandler.text);
        StartCoroutine(CheckPoint());
        if (int.Parse(personalRank) <= 10)
        {
            SetRanking();
        }
        wwwSave.Dispose();
        wwwPersonalRank.Dispose();
        wwwReward.Dispose();


        uiMgr.ShowResult(playRecordString, personalRank + unit, rewardString);
    }

    private void SetRanking()
    {
        StartCoroutine(SetRankingData());
    }

    IEnumerator SetRankingData()
    {
        WWWForm form = new WWWForm();
        form.AddField("command", "getRank");

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        string rank = www.downloadHandler.text;

        string[] ranks = rank.Split("\n");
        for (int i = 0; i < ranks.Length; i++)
        {
            string[] _ranks = ranks[i].Split(",");

            string[] _rank = { _ranks[1], _ranks[3], (i + 1).ToString() };
            rankList.Add(_ranks[0], _rank);
        }

        uiMgr.SetRankingBoard(rankList);
        rankList.Clear();

        www.Dispose();
    }

    void SetPlayerStartPos()
    {
        Quaternion currentRot = playerInput.cameraMain.transform.rotation;
        float offset = 30;

        float y = currentRot.eulerAngles.x;
        if (y > 180)
        {
            Debug.Log(y);
            Debug.Log(offset);
            y -= 360;
        }
        playerCamera.RotateCamera(-currentRot.eulerAngles.y / 3, (y - offset) / 3);

        SetPlayerPosition(startPoint.position);
    }

    void SetPlayerPosition(Vector3 pos)
    {
        playerInput.enabled = false;
        Player.position = pos;
        Player.rotation = Quaternion.Euler(0, 30, 0);
    }

    public void ResetPlayer()
    {
        StartCoroutine(OnResetPlayer());
    }

    IEnumerator OnResetPlayer()
    {
        input.force = 0;
        input.StopMove();
        Vector3 currPos = player.position;
        Vector3 newPos = currPos + new Vector3(0, 1f, 0);
        Quaternion initRot = Quaternion.Euler(20, 30, 6);
        player.position = newPos;
        player.rotation = initRot;

        yield return new WaitForSeconds(1);

        input.force = input.startForce;
    }

    public void ResetPlayerPos()
    {
        StartCoroutine(ReturnPlayer());
    }

    IEnumerator ReturnPlayer()
    {
        SetPlayerPosition(resetPoint.position);

        yield return new WaitForSeconds(1);

        playerInput.enabled = true;
    }

    public void SetBoardOff()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.mass = 50f;
        rb.freezeRotation = true;

        board.gameObject.SetActive(false);
        playerInput.enabled = true;
        animator.SetBool("IsStart", false);
        uiMgr.OpenPointUI();
    }

    public void MarkOn()
    {
        StartCoroutine(Mark());
    }

    // caution before claw up
    IEnumerator Mark()
    {
        mark.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        mark.gameObject.SetActive(false);
    }
}

