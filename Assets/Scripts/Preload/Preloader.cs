using System;
using System.Collections;
using System.Collections.Generic;
using Tayx.Graphy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tayx.Graphy.Utils.NumString;
using Unity.Entities;

public class Preloader : SingletonPersistance<Preloader>
{
    
    public string TittleOne => "-battery-" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
    public string TittleTwo => "-performance-" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
    public Button normalSpeedBtn, fastSpeedBtn, loadBtn,recordBtn, recordBtn2, stopRecordBtn;
    public Text loadText, currentBatteryLevel;
    public InputField numberOfAntInput, mapInput, inveterInput, batteryLevelInput;

    public GraphyManager graphyManager;

    public Dropdown dropdown;

    public int speed;

    public Coroutine recordCoroutine;

    public string selection;

    protected override void Awake()
    {
        base.Awake();
        if (_isInit)
        {
            Application.targetFrameRate = 60;
            StartCoroutine(DoActionOnAwake(null));
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "DOP", "OOP" });
        dropdown.onValueChanged.AddListener((value) =>
        {
            if (value == 0)
            {
                selection= "DOPSimulation";
            }
            else
            {
                selection = "OOPSimulation";
            }
        });
        selection = "DOPSimulation";    
    }


    private void Update()
    {
        currentBatteryLevel.text = "Current battery: "+ SystemInfo.batteryLevel.ToString();
    }

    public override IEnumerator DoActionOnAwake(Action onCompleted = null)
    {
        normalSpeedBtn.onClick.AddListener(() => { speed = 1; Time.timeScale = speed; });
        fastSpeedBtn.onClick.AddListener(() => { speed = 5; Time.timeScale = speed; });

        loadBtn.onClick.AddListener(() =>
        {
            if (CurrentActiveScene == "Preloader")
            {
                LoadSimulation(selection);
            }
            else
            {
                UnLoadSimulation();
            }
        });

        stopRecordBtn.onClick.AddListener(() =>
        {
            StopRecord();
        });
        recordBtn.onClick.AddListener(() =>
        {
            stopRecordBtn.gameObject.SetActive(true);
            if (recordCoroutine == null)
            {
                recordCoroutine = StartCoroutine(RecordOneSetting());
            }
            else
            {
                StopCoroutine(recordCoroutine);
                recordCoroutine = null;
            }
        });

        recordBtn2.onClick.AddListener(() =>
        {
            stopRecordBtn.gameObject.SetActive(true);
            if (recordCoroutine == null)
            {
                recordCoroutine = StartCoroutine(RecordTwoSetting());
            }
            else
            {
                StopCoroutine(recordCoroutine);
                recordCoroutine = null;
            }
        });

        numberOfAntInput.text = "100";
        mapInput.text = "128";
        loadBtn.gameObject.SetActive(true);
        loadText.text = "Go to simulation";
        stopRecordBtn.gameObject.SetActive(false);


       
        yield return null;


    }
    private IEnumerator RecordOneSetting()
    {
        int interval = 1;
        int time = 0;
        string tittle = selection + TittleOne;
        var writer = new SimpleCSVFileWriter(tittle, 
            "Time", 
            "NumberOfAnt", 
            "MapSize", 
            "ReservedMem", 
            "AllocatedMem", 
            "MonoMem", 
            "FPS", 
            "FrameTime", 
            "Battery");
        float batteryLevel = 0.2f;

        if(float.TryParse(batteryLevelInput.text, out var newBatteryLevel))
        {
            batteryLevel= Mathf.Clamp(newBatteryLevel, 0.1f, 1);
        }
        

        while (SystemInfo.batteryLevel > batteryLevel)
        {
            yield return new WaitForSeconds(interval);
            time += interval;
            var data = 
                $"{time.ToString().Replace(",", ".")}," +
                $"{numberOfAntInput.text}," +
                $"{mapInput.text}," +
                $"{graphyManager.ReservedRam.ToString().Replace(",", ".")}," +
                $"{graphyManager.AllocatedRam.ToString().Replace(",", ".")}," +
                $"{graphyManager.MonoRam.ToString().Replace(",", ".")}, " +
                $"{graphyManager.CurrentFPS.ToString().Replace(",", ".")}," +
                $"{(1000 / graphyManager.CurrentFPS).ToString().Replace(",",".")}," +
                $"{SystemInfo.batteryLevel.ToString().Replace(",", ".")}";
            writer.AddNewLine(data);
        }
        StopRecord();
    }

    private IEnumerator RecordTwoSetting()
    {
        var interval = 500;
        if (int.TryParse(inveterInput.text, out var number))
        {
            interval = number;
        }
        else
        {
            inveterInput.text = interval.ToString();
        }
        string tittle = selection + TittleTwo;

        var writer = new SimpleCSVFileWriter(tittle, 
            "NumberOfAnt", 
            "MapSize", 
            "ReservedMem", 
            "AllocatedMem", 
            "MonoMem", 
            "FPS", 
            "FrameTime", 
            "Battery");
        int maxFrameWait = 60;
        while (true)
        {
            var currentFrame = 0;
            float frameCount = 0;


            LoadSimulation(selection);
            yield return new WaitForSeconds(1);
            while (currentFrame < maxFrameWait)
            {
                var framePerSec = 1 / Time.deltaTime;
                frameCount += framePerSec;
                currentFrame++;
                yield return null;
            }
            var average = frameCount / currentFrame;


            var data = $"{numberOfAntInput.text}," +
                $"{mapInput.text}," +
                $"{graphyManager.ReservedRam.ToString().Replace(",",".")}," +
                $"{graphyManager.AllocatedRam.ToString().Replace(",", ".")}," +
                $"{graphyManager.MonoRam.ToString().Replace(",", ".")}, " +
                $"{average.ToString().Replace(",", ".")}," +
                $"{(1000 / average).ToString().Replace(",", ".")}," +
                $"{SystemInfo.batteryLevel.ToString().Replace(",", ".")}";
            writer.AddNewLine(data);
            yield return new WaitForSeconds(1);
            UnLoadSimulation();


            if (!int.TryParse(numberOfAntInput.text, out number))
            {
                number = 1000;
            }

            numberOfAntInput.text = number + interval + "";
            yield return new WaitForSeconds(1);




        }
        recordCoroutine = null;
    }

    private void StopRecord()
    {
        if(recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
            recordCoroutine = null;
        }
        stopRecordBtn.gameObject.SetActive(false);
    }

    public void LoadSimulation(string sceneName)
    {
        LoadScene(sceneName, null);
        loadText.text = "Back";


    }

    public void UnLoadSimulation()
    {
        if(CurrentActiveScene == "DOPSimulation")
        {
            var entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            Unity.Entities.World.DisposeAllWorlds();
            DefaultWorldInitialization.Initialize("Default World", false);
        }
       
        LoadScene("Preloader", null);
        loadText.text = "Go to simulation";
    }



    public string CurrentActiveScene => SceneManager.GetActiveScene().name;

    public string SceneName => SceneManager.GetActiveScene().name;

    public void LoadScene(string level, System.Action onLoadSceneCompleted)
    {
        StartCoroutine(LoadSceneRoutineCoroutine(level, onLoadSceneCompleted));
    }
    private static IEnumerator LoadSceneRoutineCoroutine(string level, System.Action onLoadSceneCompleted = null)
    {
        string name = level;
        Application.backgroundLoadingPriority = ThreadPriority.High;
        var asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;
        onLoadSceneCompleted?.Invoke();
    }

    public void UnLoadScene(System.Action onLoadSceneCompleted)
    {
        StartCoroutine(UnLoadSceneRoutineCoroutine(onLoadSceneCompleted));
    }
    private static IEnumerator UnLoadSceneRoutineCoroutine(System.Action onLoadSceneCompleted = null)
    {
        string name = SceneManager.GetActiveScene().name;
        Application.backgroundLoadingPriority = ThreadPriority.High;
        var asyncLoad = SceneManager.UnloadSceneAsync(name);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;
        onLoadSceneCompleted?.Invoke();
    }
}
