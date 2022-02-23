using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class ScenarioControl : MonoBehaviour
{
    private static ScenarioControl _instance;

    public static ScenarioControl Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public TrafficControl traffic;
    public WASDMovementProvider player;
    public Canvas transitionScreen;
    public GameObject playerSpawnPoint;
    public Canvas UI;
    public GameObject recticleSprite;
    public GameObject SpawnInfoText;
    public GameObject GoalInfoText;
    public GameObject FirstSpawnArea;
    public GameObject SecondSpawnArea;
    public AreaTimeMeasurement streetTrigger;
    public AreaTimeMeasurement firstSidewalkTrigger;
    public AreaTimeMeasurement secondSidewalkTrigger;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject playerHead;
    public GameObject particleSystem;
    private Image urgencyImage;
    public GameObject urgencySymbol;

    public const int RespawnTime = 4;
    public const int ManualHoldCooldown = 10;
    public const int UrgentTaskTime = 13;
    public const int NormalTaskTime = 23;
    public const int GeneralTaskTime = 120;
    public const int PolicePenaltyTime = 20;
    public const int Language = 1;// 0: german, 1: english

    public bool autonomous; //true = only AVs as gap-cars, false = only CVs as gap-cars

    public bool urgent; //true = scenario is urgent -> 13 seconds for bonus score, false = sceario is non-urgent -> 23 seconds for bonus score

    public bool NPC_mother; //true = mother and child will spawn in the scenario
    public bool NPC_crossingPerson; // true = a crossing person will spawn in the scenario
    public bool bustingEnabled = true;
    float chanceForHold = 1.0f;

    public bool NPC_police; // true = a police officer will spawn in the scenario
    //only one (or none) of these can be set to true simultaneously, if all are false, no NPC will spawn

    public GameObject CrossingPerson;
    public GameObject Mother;
    public GameObject Police;
    public GameObject HungryNPC01;
    public GameObject HungryNPC02;
    public GameObject HungryNPC03;

    public List<int> scenarioSequence = new List<int> { };
    public List<int> scoreLottery = new List<int> { };
    public int finalScore = 0;
    public int currentScenario = -1;
    public int scenarioScore = 0;
    public bool scenarioIsRunning = false;
    public bool goalSpawnTransition = false;
    public bool leftHandRaised = false;
    public bool rightHandRaised = false;
    public bool gameOver = false;
    int maxScenarios = 30;
    int maxTestScenarios = 5;

    public bool playerIsDead = false;
    public bool scenarioCompleted = false;
    public bool timeOver = false;
    public bool playerBusted = false;
    public bool respawnAfterPlayerGotBusted = false;

    public float elapsedTime = 0;
    public float timeUntilNextManualHold = 0;
    public float nextRespawnTime = 0;
    public int usedManualHolds = 0;
    public int usedAutonomousHolds = 0;
    public float lastCarPassedTime = -1;

    private string _loggingPath;
    public bool transition = false;
    private readonly GUIStyle _guiStyle = new GUIStyle();
    private float buttonPressedTime = -1;
    private float buttonPressedDuration = -1;

    private readonly List<Object> _manualHoldView = new List<Object>();
    private readonly List<Object> _generalTimerView = new List<Object>();
    private readonly List<Object> _bonusTimerView = new List<Object>();
    private Text _scenarioCountView;
    private readonly List<Object> _transitionInfoView = new List<Object>();
    private Image _blackScreen;
    private bool _firstTransitionTextChanged = false;

    public bool spawnEntered = true;
    public bool spawnExited = false;
    public bool readyTransition = true;

    private Sprite urgentSymbol, nonurgentSymbol;

    void Start()
    {

        //setup Log file
        var date = DateTime.Now;
        if (!Directory.Exists(Application.persistentDataPath + "/Log"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Log");
        }

        string day = date.Day > 9 ? "" + date.Day : "0" + date.Day;
        string month = date.Month > 9 ? "" + date.Month : "0" + date.Month;
        string hour = date.Hour > 9 ? "" + date.Hour : "0" + date.Hour;
        string minute = date.Minute > 9 ? "" + date.Minute : "0" + date.Minute;
        string second = date.Second > 9 ? "" + date.Second : "0" + date.Second;

        _loggingPath = Application.persistentDataPath + "/Log/" + day + "-" + month + "-" + date.Year + "_" +
                       hour + "-" + minute + "-" + second + ".csv";
        Debug.Log(_loggingPath);
        StreamWriter streamWriter = new StreamWriter(_loggingPath, true);
        streamWriter.WriteLine("Scenario Nr.," +
            "Scenario ID," +
            "Vehicle type," +
            "NPC type," +
            "Busting," + 
            "Yielding," +
            "Urgency type," +
            "Crossing Onset," +
            "Crossing Duration," +
            "Button press onset," +
            //"button press duration," +
            "Last car passed," +
            "Failure cause," +
            "earned points," +
            "Lottery Result");
        streamWriter.Close();

        //Setup UI
        _manualHoldView.Add(UI.transform.Find("ManualHoldCooldown").Find("Timer").GetComponent<Image>());
        _manualHoldView.Add(UI.transform.Find("ManualHoldCooldown").Find("Background").GetComponent<Image>());
        _manualHoldView.Add(UI.transform.Find("ManualHoldCooldown").Find("Status").GetComponent<Text>());

        _generalTimerView.Add(UI.transform.Find("GeneralTimer").Find("Timer").GetComponent<Image>());
        _generalTimerView.Add(UI.transform.Find("GeneralTimer").Find("Background").GetComponent<Image>());
        _generalTimerView.Add(UI.transform.Find("GeneralTimer").Find("Status").GetComponent<Text>());

        _bonusTimerView.Add(UI.transform.Find("BonusTimer").Find("Timer").GetComponent<Image>());
        _bonusTimerView.Add(UI.transform.Find("BonusTimer").Find("Background").GetComponent<Image>());
        _bonusTimerView.Add(UI.transform.Find("BonusTimer").Find("Status").GetComponent<Text>());

        _scenarioCountView = UI.transform.Find("ScenarioCount").GetComponent<Text>();
        _transitionInfoView.Add(transitionScreen.transform.Find("FirstInfoText").GetComponent<Text>());
        _transitionInfoView.Add(transitionScreen.transform.Find("SecondInfoText").GetComponent<Text>());
        _transitionInfoView.Add(transitionScreen.transform.Find("PoliceInfoText").GetComponent<Text>());

        _blackScreen = transitionScreen.transform.Find("BlackScreen").GetComponent<Image>();

        urgencyImage = urgencySymbol.GetComponentInChildren<Image>();

        nonurgentSymbol = Resources.Load<Sprite>("Sprites/noun_relaxing_2842804");
        urgentSymbol = Resources.Load<Sprite>("Sprites/noun_Running Man_429544");

        //Setup Scenario
        GenerateScenarioOrder();
        SetNextScenario(); //to start first scenario
        player.enabled = true;
        startWaitTransition(0);
    }

    void FixedUpdate()
    {
        if (!gameOver)
        {
            //dead or finished
            if ((playerIsDead) && !transition)
            {
                scenarioScore = 0;

                particleSystem.SetActive(true);
                GoalInfoText.SetActive(false);
                FirstSpawnArea.SetActive(true);
                SecondSpawnArea.SetActive(false);
                LogData();
                startWaitTransition(20); // walking back time 20s
                RespawnPlayer();
                SetNextScenario();
            }
            if ((scenarioCompleted) && !transition)
            {
                if (urgent)
                {
                    if (elapsedTime < UrgentTaskTime)
                    {
                        scenarioScore = 2;
                    }
                    else
                    {
                        scenarioScore = 1;
                    }
                }
                else
                {
                    if (elapsedTime < NormalTaskTime)
                    {
                        scenarioScore = 2;
                    }
                    else
                    {
                        scenarioScore = 1;
                    }
                }
                //only add to Lottery in non-Testscenario
                if (currentScenario < 100)
                {
                    scoreLottery.Add(scenarioScore);
                }

                GoalInfoText.SetActive(false);
                particleSystem.SetActive(true);
                FirstSpawnArea.SetActive(true);
                SecondSpawnArea.SetActive(false);
                LogData();
                startWaitTransition(RespawnTime);
                RespawnPlayer();
                SetNextScenario();
            }
            //times over
            else if (elapsedTime > GeneralTaskTime && !transition && scenarioIsRunning)
            {
                timeOver = true;
                scenarioScore = 0;
                particleSystem.SetActive(true);
                GoalInfoText.SetActive(false);
                FirstSpawnArea.SetActive(true);
                SecondSpawnArea.SetActive(false);
                LogData();
                startWaitTransition(RespawnTime);
                RespawnPlayer();
                SetNextScenario();
            }
            //player busted
            else if (playerBusted && !transition)
            {
                scenarioScore = 0;
                particleSystem.SetActive(true);
                GoalInfoText.SetActive(false);
                FirstSpawnArea.SetActive(true);
                SecondSpawnArea.SetActive(false);
                LogData();
                startWaitTransition(PolicePenaltyTime);
            }

            var currentTime = Time.fixedDeltaTime;
            elapsedTime += currentTime;

            //Respawn timer for busted transition
            if (nextRespawnTime > 0)
            {
                nextRespawnTime -= currentTime;
            }

            if (nextRespawnTime < 0)     //need to understand respawn
            {
                nextRespawnTime = 0;
            }

            //timer for handsignal to stop vehicle
            if (timeUntilNextManualHold > 0)
            {
                timeUntilNextManualHold -= currentTime;
            }

            if (timeUntilNextManualHold < 0)
            {
                timeUntilNextManualHold = 0;
            }

            //check for hand rising
            if (leftHand.transform.lossyScale.y > playerHead.transform.lossyScale.y)
            {
                leftHandRaised = true;
            }
            else if (leftHand.transform.lossyScale.y < playerHead.transform.lossyScale.y)
            {
                leftHandRaised = false;
            }

            if (rightHand.transform.lossyScale.y > playerHead.transform.lossyScale.y)
            {
                rightHandRaised = true;
            }
            else if (rightHand.transform.lossyScale.y < playerHead.transform.lossyScale.y)
            {
                rightHandRaised = false;
            }

            UpdateManualHoldView();

            UpdateGeneralTimerView();
            UpdateBonusTimerView();

        }
        UpdateInfoTexts();
    }

    private void UpdateManualHoldView()
    {
        var timerCircle = (Image)_manualHoldView[0];
        var backGroundCircle = (Image)_manualHoldView[1];
        var text = (Text)_manualHoldView[2];

        var fillAmount = 0.0f;
        var transparency = 0.5f;
        var textContent = "Manual hold\n cooldown";

        if (!autonomous)
        {
            fillAmount = 1 - (timeUntilNextManualHold / ManualHoldCooldown);
            transparency = 1.0f;
            if (fillAmount >= 1)
            {
                textContent = "Manual hold\n ready!";
            }
            else
            {
                textContent = "Manual hold\n charging...";
            }
        }

        var oldColor = timerCircle.color;
        timerCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        timerCircle.fillAmount = fillAmount;

        oldColor = backGroundCircle.color;
        backGroundCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);

        oldColor = text.color;
        text.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        text.text = textContent;
    }

    public void UpdateGeneralTimerView()
    {
        var timerCircle = (Image)_generalTimerView[0];
        var backGroundCircle = (Image)_generalTimerView[1];
        var text = (Text)_generalTimerView[2];

        var fillAmount = 1.0f;
        var transparency = 0.5f;
        var textContent = "General\n timer:\n" + (int)(GeneralTaskTime - elapsedTime) + " sec";

        if (scenarioIsRunning)
        {
            fillAmount = 1 - (elapsedTime / GeneralTaskTime);
            transparency = 1.0f;
            if (fillAmount <= 0)
            {
                fillAmount = 0;
                textContent = "Zeit abgelaufen!";
            }
        }
        else
        {
            textContent = "General\n timer";
        }

        var oldColor = timerCircle.color;
        timerCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        timerCircle.fillAmount = fillAmount;

        oldColor = backGroundCircle.color;
        backGroundCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);

        oldColor = text.color;
        text.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        text.text = textContent;
    }

    public void UpdateBonusTimerView()
    {
        var timerCircle = (Image)_bonusTimerView[0];
        var backGroundCircle = (Image)_bonusTimerView[1];
        var text = (Text)_bonusTimerView[2];

        var fillAmount = 1.0f;
        var transparency = 0.5f;
        var textContent = "" + (int)(UrgentTaskTime - elapsedTime);

        if (scenarioIsRunning)
        {
            if (urgent)
            {
                fillAmount = 1 - (elapsedTime / UrgentTaskTime);
            }
            else
            {
                textContent = "" + (int)(NormalTaskTime - elapsedTime);
                fillAmount = 1 - (elapsedTime / NormalTaskTime);
            }

            transparency = 1.0f;
            if (fillAmount <= 0)
            {
                fillAmount = 0;
                textContent = "";
            }
        }
        else
        {
            textContent = "";
        }
        /*
        var oldColor = timerCircle.color;
        timerCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        timerCircle.fillAmount = fillAmount;

        oldColor = backGroundCircle.color;
        backGroundCircle.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);

        oldColor = text.color;
        text.color = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
        */
        text.text = textContent;
    }

    private void UpdateInfoTexts()
    {
        if (transition)
        {
            var firstText = (Text)_transitionInfoView[0];
            var secondText = (Text)_transitionInfoView[1];
            var policeText = (Text)_transitionInfoView[2];

            if (!readyTransition)
            {
                if (!playerBusted && _firstTransitionTextChanged)
                {
                    firstText.rectTransform.anchoredPosition -= new Vector2(0, 100);
                    secondText.rectTransform.anchoredPosition += new Vector2(0, 110);
                    policeText.gameObject.SetActive(false);
                    _firstTransitionTextChanged = false;
                }

                if (playerIsDead && !gameOver)
                {
                    firstText.color = Color.red;
                    secondText.color = Color.red;
                    if (Language == 0)
                    {
                        firstText.text = "Gestorben! Szenario fehlgeschlagen. Score: " + scenarioScore;
                        secondText.text = "Gehe zurück zum Startpunkt";
                    }
                    else if(Language == 1){
                        firstText.text = "You died! Scenario failed. Score: " + scenarioScore;
                        secondText.text = "Go back to the starting point";
                    }
                }
                else if (timeOver && !gameOver)
                {
                    firstText.color = Color.red;
                    secondText.color = Color.red;
                    if (Language == 0)
                    {
                        firstText.text = "Zeit vorbei! Szenario fehlgeschlagen. Score: " + scenarioScore;
                        secondText.text = "Gehe zurück zum Startpunkt";
                    }
                    else if (Language == 1)
                    {
                        firstText.text = "Time over! Scenario failed. Score: " + scenarioScore;
                        secondText.text = "Go back to the starting point";
                    }
                }
                else if (playerBusted && !gameOver)
                {
                    firstText.color = Color.red;
                    if (!_firstTransitionTextChanged)
                    {
                        firstText.rectTransform.anchoredPosition += new Vector2(0, 100);
                        secondText.rectTransform.anchoredPosition -= new Vector2(0, 110);
                        _firstTransitionTextChanged = true;
                    }

                    secondText.color = Color.red;
                    policeText.gameObject.SetActive(true);

                    if (Language == 0)
                    {
                        firstText.text = "";// Szenario fehlgeschlagen! Score: " + scenarioScore;
                        policeText.text =
                            "Polizei:\n\n" +
                            "\"STEHEN BLEIBEN! Was glauben Sie, was Sie da tun? Sie können die Straße nicht überqueren, wenn es einen fließenden Verkehr" +
                            " gibt. Sie können nur entweder von einem Fußgängerüberweg aus überqueren oder Sie müssen warten, bis es sicher ist," +
                            " die Straße zu überqueren.\"";
                        secondText.text = "Er behält Sie, bis die Zeit abgelaufen ist: " + (int)nextRespawnTime + "s ...";
                    }
                    else if (Language == 1)
                    {
                        firstText.text = "";// Scenario failed! Score: " + scenarioScore;
                        policeText.text =
                    "Police:\n\n"+
                     "\"FREEZE! What do you think you’re doing! You cannot cross the road where there’s a flowing traffic." +
                    " You can only cross either from a pedestrian crossing or you need to wait until it is safe to cross.\""  ;

                    //"FREEZE! Police is keeping you until your time is over: " + (int)nextRespawnTime + "s ...";

                        secondText.text = "He is keeping you until your time is over: " + (int)nextRespawnTime + "s ...";
                    }



                }
                else if (scenarioCompleted && !gameOver)
                {
                    firstText.color = Color.green;
                    secondText.color = Color.green;
                    if (Language == 0)
                    {
                        firstText.text = "Szenario geschafft! Score: " + scenarioScore;
                        secondText.text = "Gehe zurück zum Startpunkt";
                    }
                    else if (Language == 1)
                    {
                        firstText.text = "Scenario completed! Score: " + scenarioScore;
                        secondText.text = "Go back to the starting point";
                    }
                }
                else if (respawnAfterPlayerGotBusted)
                {
                    firstText.color = Color.red;
                    secondText.color = Color.red;
                    if (Language == 0)
                    {
                        firstText.text = "Von Polizei erwischt! Szenario fehlgeschlagen. Score: " + scenarioScore;
                        secondText.text = "Gehe zurück zum Startpunkt";
                    }
                    else if (Language == 1)
                    {
                        firstText.text = "You got busted by the police! Scenario failed. Score: " + scenarioScore;
                        secondText.text = "Go back to the starting point";
                    }
                    
                }

            }
            //Spawn transition
            else
            {
                firstText.color = Color.white;
                secondText.color = Color.white;
                if (Language == 0)
                {
                    firstText.text = "Das Szenario beginnt, sobald der Startpunkt verlassen wird.";
                    secondText.text = "";
                }
                else if (Language == 1)
                {
                    firstText.text = "The scenario begins as soon as you leave the starting point.";
                    secondText.text = "";
                }
            }
            //Gameover Screen
            if (gameOver)
            {
                firstText.color = Color.white;
                secondText.color = Color.white;
                if (Language == 0)
                {
                    firstText.text = "Alle Szenarios abgeschlossen!";
                    secondText.text =
                        "Danke für deine Teilnahme. Wir haben aus deinen erfolgreichen Versuchen eine Auslosung vorgenommen.\nDein Endpunktestand ist " + finalScore + ". Du kannst die VR-Brille jetzt abnehmen.";
                }
                else if (Language == 1)
                {
                    firstText.text = "All scenarios completed!";
                    secondText.text =
                        "Thank you for your participation. We have drawn a lottery from your successful attempts.\nYour final score is " + finalScore + " points. You can now remove the VR Headset.";
                }
            }
        }
        else
        {
            if (scenarioSequence.Count < maxScenarios)
            {
                if (Language == 0)
                {
                    _scenarioCountView.text = "Szenario:\n" + (maxScenarios - scenarioSequence.Count) + "/" + maxScenarios;
                }
                else if (Language == 1)
                {
                    _scenarioCountView.text = "Scenario:\n" + (maxScenarios - scenarioSequence.Count) + "/" + maxScenarios;
                }
            }
            else
            {
                if (Language == 0)
                {
                    _scenarioCountView.text = "Testszenario " + (maxScenarios + maxTestScenarios - scenarioSequence.Count) + "/" + maxTestScenarios;
                }
                else if (Language == 1)
                {
                    _scenarioCountView.text = "Testscenario " + (maxScenarios + maxTestScenarios - scenarioSequence.Count) + "/" + maxTestScenarios;
                }
            }
        }
    }

    public void GenerateScenarioOrder()
    {
        //int ts = 12;
        //List<int> numbers = new List<int> {ts,ts,ts,ts, ts, ts, ts, ts, ts, ts, ts, ts, ts, ts, ts, ts };
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

        scenarioSequence.Add(100);
        scenarioSequence.Add(101);
        scenarioSequence.Add(102);
        scenarioSequence.Add(103);
        scenarioSequence.Add(104);

        int scenarioCount = numbers.Count;

        for (int i = 0; i < scenarioCount; i++)
        {
            int randomIndex = Random.Range(0, numbers.Count);
            int nextNumber = numbers[randomIndex];

            scenarioSequence.Add(nextNumber);
            numbers.Remove(nextNumber);
        }

        /*for(int i =0; i < 15; i++)
        {
            scenarioSequence.Remove(scenarioSequence[0]);
        }*/

    }

    public void SetNextScenario()
    {
        timeUntilNextManualHold = 0;
        usedAutonomousHolds = 0;
        usedManualHolds = 0;

        if (scenarioSequence.Count > 0)
        {
            currentScenario = scenarioSequence[0];
            scenarioSequence.Remove(scenarioSequence[0]);

            //Testscenarios
            if (currentScenario == 100)
            {
                autonomous = false; //false
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false; //false
                NPC_police = false; //false
                bustingEnabled = false;//false
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 101)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 102)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 0.0f;
            }
            else if (currentScenario == 103)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 104)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 1.0f;
            }

            //Study Scenarios
            else if (currentScenario == 1)
            {
                autonomous = true;
                urgent = true;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 2)
            {
                autonomous = true;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 3)
            {
                autonomous = true;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 4)
            {
                autonomous = true;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 5)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 6)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 7)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 8)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 9)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 10)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 11)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 12)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 13)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 14)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 15)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 1.0f;
            }
            else if (currentScenario == 16)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                chanceForHold = 1.0f;
            }

            // new design extra scenarios

            else if (currentScenario == 17)
            {
                autonomous = true;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 1.0f;
            }

            else if (currentScenario == 18)
            {
                autonomous = true;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 1.0f;
            }

            else if (currentScenario == 19)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 1.0f;
            }

            else if (currentScenario == 20)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 1.0f;
            }

            else if (currentScenario == 21)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 22)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 23)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 24)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 25)
            {
                autonomous = false;
                urgent = true;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 26)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = true;
                chanceForHold = 0.0f;
            }

            else if (currentScenario == 27)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = true;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }


            else if (currentScenario == 28)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = false;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }


            else if (currentScenario == 29)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = false;
                NPC_crossingPerson = true;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }


            else if (currentScenario == 30)
            {
                autonomous = false;
                urgent = false;
                NPC_mother = true;
                NPC_crossingPerson = false;
                NPC_police = false;
                bustingEnabled = false;
                chanceForHold = 0.0f;
            }

            traffic.ChanceForManualHold = chanceForHold;

            if (scenarioSequence.Count < maxScenarios + maxTestScenarios)
            {
                traffic.ResetTraffic();

                CrossingPerson.GetComponent<PedestrianAggressiveCrosser>().ResetPosition();
                Mother.GetComponent<ProsocialController>().ResetPedestrians();

                streetTrigger.ResetTimers();
                firstSidewalkTrigger.ResetTimers();
                secondSidewalkTrigger.ResetTimers();
            }

            if (urgent)
            {
                urgencyImage.sprite = urgentSymbol;
                urgencySymbol.GetComponentInChildren<RawImage>().color = new Color(255, 0, 0, 81f / 255f);
            }
            else
            {
                urgencyImage.sprite = nonurgentSymbol;
                urgencySymbol.GetComponentInChildren<RawImage>().color = new Color(0, 255, 0, 81f / 255f);
            }

            if (NPC_crossingPerson)
            {
                CrossingPerson.SetActive(true);
            }
            else
            {
                CrossingPerson.SetActive(false);
            }

            if (NPC_mother)
            {
                Mother.SetActive(true);
            }
            else
            {
                Mother.SetActive(false);
            }

            if (NPC_police)
            {
                Police.SetActive(true);
            }
            else
            {
                Police.SetActive(false);
            }

            HungryNPC01.SetActive(false);
            HungryNPC02.SetActive(false);
            HungryNPC03.SetActive(false);

            var NPCChance = Random.Range(1, 4);

            switch (NPCChance)
            {
                case 1:
                    HungryNPC01.SetActive(true);
                    break;
                case 2:
                    HungryNPC02.SetActive(true);
                    break;
                default:
                    HungryNPC03.SetActive(true);
                    break;
            }
        }
        else
        {
            traffic.ResetTraffic();

            StartCoroutine(gameOverWaiter());
        }

        buttonPressedTime = -1;
        lastCarPassedTime = -1;
        buttonPressedDuration = -1;
    }

    IEnumerator gameOverWaiter()
    {
        yield return new WaitUntil(() => readyTransition);

        scenarioSequence.Clear();
        if (scoreLottery.Count > 0)
        {
            finalScore = scoreLottery[Random.Range(0, scoreLottery.Count)];
        }
        else
        {
            finalScore = 0;
        }
        gameOver = true;
        player.enabled = false;
        transitionScreen.transform.gameObject.SetActive(true);
        transition = true;
        LogFinalScore();
    }

    private string GetNPCType()
    {
        if (NPC_mother)
        {
            return "mother + child";
        }
        else if (NPC_crossingPerson)
        {
            return "crossing person";
        }
        else if (NPC_police)
        {
            return "police officer";
        }
        else
        {
            return "none";
        }
    }

    private void LogData()
    {
        string result = "";
        if (scenarioCompleted)
        {
            result = "nan";
        }
        else if (playerIsDead)
        {
            result = "dead";
        }
        else if (timeOver)
        {
            result = "time elapsed";
        }
        else if (playerBusted || respawnAfterPlayerGotBusted)
        {
            result = "busted";
        }

        int vehicleStops = 0;
        if (autonomous)
        {
            vehicleStops = usedAutonomousHolds;
        }
        else
        {
            vehicleStops = usedManualHolds;
        }

        secondSidewalkTrigger.setEnterTimeToNow();

        float streetCrossingTime = (float)(int)((secondSidewalkTrigger.firstEnterTime - firstSidewalkTrigger.firstExitTime) * 1000) / 1000;//(float)(int)(streetTrigger.playerInAreaTime * 100) / 100;
        //float waitOnSidewalkTime = (float)(int)(firstSidewalkTrigger.playerInAreaTime * 1000) / 1000;
        float timeWhenPlayerReachedStreet = (float)(int)(firstSidewalkTrigger.firstExitTime * 1000) / 1000;
        float roundedElapsedTime = (float)(int)(elapsedTime * 1000) / 1000;

        String vehicleType = autonomous ? "AV" : "CV";
        String urgencyType = urgent ? "urgent" : "nonurgent";
        String bustingType = NPC_police ? (bustingEnabled ? "busting" : "nonbusting") : "nan";
        String yieldType = chanceForHold == 0 ? "nonyielding" : "yielding";
        String buttonPressedTimeString = buttonPressedTime != -1 ? buttonPressedTime.ToString() : "nan";
        String lastCarPassed = lastCarPassedTime != -1 ? lastCarPassedTime.ToString() : "nan";
        //String buttonPressedDurationString = (buttonPressedDuration != -1) ? buttonPressedDuration.ToString() : "nan";
        //String scenarioID = currentScenario

        string NPC_type = GetNPCType();
        /*string content = "Scenario: " + (16 - scenarioSequence.Count) + " (scenario id: " + currentScenario +
                         ", autonomous: " + autonomous + ", urgent: " + urgent +
                         ", NPC: " + NPC_type + ")"
                         + "\nScenario score: " + scenarioScore
                         + "\nScenario result: " + result
                         + "\nTotal duration participant spent on the Task: " + roundedElapsedTime + " sec"
                         + "\nTime when player reached first sidewalk: " + timeWhenPlayerReachedSidewalk + " sec"
                         + "\nDuration participant spent 'waiting' before crossing: " + waitOnSidewalkTime + " sec"
                         + "\nDuration participant spent on street: " + streetCrossingTime + " sec"
                         + "\nTimes participant stopped a vehicle: " + vehicleStops
                         + "\n";*/

        String content = "";
        content = (maxScenarios + maxTestScenarios - scenarioSequence.Count - 1)
            + "," + currentScenario
            + "," + vehicleType
            + "," + NPC_type
            + "," + bustingType
            + "," + yieldType
            + "," + urgencyType
            + "," + timeWhenPlayerReachedStreet
            + "," + streetCrossingTime
            + "," + buttonPressedTimeString
            //+ "," + buttonPressedDurationString
            + "," + lastCarPassed
            + "," + result
            + "," + scenarioScore
            + ",";


        StreamWriter streamWriter = new StreamWriter(_loggingPath, true);
        streamWriter.WriteLine(content);
        streamWriter.Close();
    }

    private void LogFinalScore()
    {
        StreamWriter streamWriter = new StreamWriter(_loggingPath, true);
        streamWriter.WriteLine(",,,,,,,,,,,,," + finalScore);
        streamWriter.Close();
    }

    public void setButtonPressed()
    {
        if (buttonPressedTime == -1 && scenarioIsRunning) buttonPressedTime = elapsedTime;
    }

    public void setButtonReleased()
    {
        if (buttonPressedDuration == -1) buttonPressedDuration = elapsedTime - buttonPressedTime;
    }

    public void logLastCarPassed()
    {
        if (lastCarPassedTime == -1)
        {
            lastCarPassedTime = elapsedTime;
            Debug.Log("last car passed at " + lastCarPassedTime);
            bustingEnabled = false;
        }

    }

    public void RespawnPlayer()
    {
        if (transition)
        {
            if (playerBusted)
            {
                nextRespawnTime = PolicePenaltyTime;
                //player.transform.position = playerSpawnPoint.transform.position;
            }
            else
            {
                nextRespawnTime = RespawnTime;
                //player.transform.position = playerSpawnPoint.transform.position;
            }
        }
    }

    public void startWaitTransition(int seconds)
    {
        //player.enabled = false;
        scenarioIsRunning = false;
        StartCoroutine(WaitTransition(seconds));
        transitionScreen.transform.gameObject.SetActive(true);
        if (playerBusted)
        {
            //elapsedTime = 0;
            UI.gameObject.SetActive(false);
            nextRespawnTime = PolicePenaltyTime;
            traffic.ManualHold(player.gameObject, false);
        }
        transition = true;
    }

    IEnumerator WaitTransition(int seconds)
    {
        if (playerBusted)
        {
            _blackScreen.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

            yield return new WaitForSecondsRealtime(seconds);

        }
        else
        {
            yield return new WaitUntil(() => spawnEntered);
            readyTransition = true;
            urgencySymbol.gameObject.SetActive(true);
            yield return new WaitUntil(() => spawnExited);
            readyTransition = false;
            urgencySymbol.gameObject.SetActive(false);
        }
        if (!gameOver)
        {
            transitionScreen.transform.gameObject.SetActive(false);
            player.enabled = true;
            playerIsDead = false;
            scenarioCompleted = false;
            timeOver = false;
            respawnAfterPlayerGotBusted = false;
            transition = false;

            //second normal transition
            if (playerBusted)
            {
                _blackScreen.color = new Color(0.0f, 0.0f, 0.0f, 200f / 255f);

                playerBusted = false;
                respawnAfterPlayerGotBusted = true;
                UI.gameObject.SetActive(true);
                startWaitTransition(RespawnTime);
                RespawnPlayer();
                SetNextScenario();
            }
        }
    }

    void OnGUI()
    {
        if (!transition)
        {
            _guiStyle.fontSize = 12;
            _guiStyle.normal.textColor = Color.white;
            _guiStyle.alignment = TextAnchor.LowerLeft;
            GUI.Label(new Rect(10, -230, Screen.width, Screen.height), "Current scenario: " + currentScenario,
                _guiStyle);
            GUI.Label(new Rect(10, -210, Screen.width, Screen.height), "Scenario timer: " + elapsedTime + " sec",
                _guiStyle);
            GUI.Label(new Rect(10, -190, Screen.width, Screen.height),
                "Car yield CD: " + timeUntilNextManualHold + " sec", _guiStyle);
            GUI.Label(new Rect(10, -170, Screen.width, Screen.height), "autonomous: " + autonomous, _guiStyle);
            GUI.Label(new Rect(10, -150, Screen.width, Screen.height), "urgent: " + urgent, _guiStyle);
            GUI.Label(new Rect(10, -130, Screen.width, Screen.height), "NPC_mother: " + NPC_mother, _guiStyle);
            GUI.Label(new Rect(10, -110, Screen.width, Screen.height), "NPC_crossingPerson: " + NPC_crossingPerson,
                _guiStyle);
            GUI.Label(new Rect(10, -90, Screen.width, Screen.height), "NPC_police: " + NPC_police, _guiStyle);
        }
    }
}