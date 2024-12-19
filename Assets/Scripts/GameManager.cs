using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("--------------UI----------------")]
    public GameObject LosePanel;
    public GameObject SettingsPanel;
    public TextMeshProUGUI ScoreText, ReplayScoreText;
    public Image SoundBtn, MusicBtn;
    public Sprite soundOn, soundOff, musicOn, musicOff;
    public Image NextFruitUI;
    public Sprite[] fruitsUI;

    [SerializeField] private Fruit.FruitType[] fruitTypes = null;

    [Header("------------------------------")]



    public Transform AimLine;
    public float YSpawnPosition, TimeBetweenSpawnes;



    public Fruit[] fruits;

    public AudioSource SoundAudioSource, MusicAudioSource;
    public AudioClip loseSound, SmallMergeSound, BigMergeSound, ReleaseSound;
    public int Score;
    public int currentFriutIndex, NextFuitIndex;
    Camera maincamera;
    Vector3 SpawnLoc;
    Fruit currentFruit;
    float lastSpawnTime, LastWink;
    [HideInInspector] public bool IsGameOver;
    private bool stopTouch = false;


    void Start()
    {
        //SexyGirlUI.Instance.OnActive(false);
        maincamera = Camera.main;
        currentFriutIndex = UnityEngine.Random.Range(0, 5);
        NextFuitIndex = UnityEngine.Random.Range(0, 5);

        NextFruitUI.sprite = fruitsUI[NextFuitIndex];
        SetAimLineAndCurentFruit(new Vector3(0, AimLine.position.y, 0));
        SpawnFruit(new Vector3(0, YSpawnPosition, 0));


        if (PlayerPrefs.GetInt("CanPlayMusic", 1) == 0)
        {
            MusicBtn.sprite = musicOff;
            MusicAudioSource.volume = 0;
        }
        if (PlayerPrefs.GetInt("CanPlaySounds", 1) == 0)
        {
            SoundBtn.sprite = soundOff;

        }

        //AdmobController.Instance.ShowBannerAd(0.1f);
    }
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void Update()
    {
        GameOver();
        if (Time.time > LastWink + 2)
        {
            LastWink = Time.time;
            Fruit[] Winkfruits = FindObjectsOfType<Fruit>();
            Winkfruits[UnityEngine.Random.Range(0, Winkfruits.Length)].DoWink();

        }

        if (IsPointerOverUIObject()) return;


        if (Time.time > TimeBetweenSpawnes + lastSpawnTime && currentFruit == null)
        {
            lastSpawnTime = Time.time;
            SetAimLineAndCurentFruit(new Vector3(AimLine.position.x, AimLine.position.y, 0));
            SpawnLoc = new Vector2(AimLine.position.x, YSpawnPosition);
            SpawnFruit(SpawnLoc);

        }

        FruiSpawner();
    }

    void FruiSpawner()
    {
        if (!stopTouch)
        {
            if (Input.GetMouseButton(0) && !IsPointerOverUIObject())
            {
                Vector3 mousePositionWorld = maincamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));

                SetAimLineAndCurentFruit(mousePositionWorld);


            }
            if (!IsPointerOverUIObject() && Input.GetMouseButtonUp(0) && !SettingsPanel.activeSelf)
            {
                if (currentFruit)
                {
                    lastSpawnTime = Time.time;

                    currentFruit.Release();
                    PlayClip(ReleaseSound);
                    currentFruit = null;
                    AimLine.gameObject.SetActive(false);

                }
            }
        }
    }
    void GameOver()
    {
        if (IsGameOver && !LosePanel.activeSelf)
        {
            Fruit[] Losefruits = FindObjectsOfType<Fruit>();
            for (var i = 0; i < Losefruits.Length; i++)
            {
                Losefruits[i].GameOver();
            }
            Invoke("ShowLosePanel", 1.5f);
        }

    }
    public void SetAimLineAndCurentFruit(Vector2 NewLoc)
    {
        AimLine.gameObject.SetActive(true);
        float Xpos = NewLoc.x;
        if (Xpos > 3.0f - fruits[currentFriutIndex].raduis)//0.25 >> fruit raduis
        {
            Xpos = 3.0f - fruits[currentFriutIndex].raduis;
        }
        else if (Xpos < -3.0f + fruits[currentFriutIndex].raduis)
        {
            Xpos = -3.0f + fruits[currentFriutIndex].raduis;
        }
        AimLine.position = new Vector3(Xpos, 2.56f, 0);
        if (currentFruit) currentFruit.transform.position = new Vector3(Xpos, YSpawnPosition, 0);
    }
    private void SetScore(int ScoreIncrement)
    {
        Score += SumNumbers(ScoreIncrement);
        ScoreText.text = "" + Score;
    }
    void SpawnFruit(Vector3 SpawnLoc)
    {
        currentFriutIndex = NextFuitIndex;
        NextFuitIndex = UnityEngine.Random.Range(0, 5);
        NextFruitUI.sprite = fruitsUI[NextFuitIndex];
        currentFruit = Instantiate(fruits[currentFriutIndex], SpawnLoc, quaternion.identity);
        currentFruit.Initialize();
    }
    public void MergeFruit(Fruit F1, Fruit F2)
    {
        if (F1 && F2)
        {
            int n = (int)F1.MyType + 1;
            if (n < fruits.Length)
            {
                if (n < 5) { PlayClip(SmallMergeSound); } else { PlayClip(BigMergeSound); }
                SetScore(n);
                Vector3 position = (F1.transform.position + F2.transform.position) / 2;
                Destroy(F1.gameObject);
                Destroy(F2.gameObject);
                Fruit NewFruit = Instantiate(fruits[n], position, quaternion.identity);
                NewFruit.Release();

                foreach(Fruit.FruitType fruitType in fruitTypes) 
                {
                    if (NewFruit.MyType == fruitType)
                    {
                        //if (AdmobController.Instance.IsRewardedAdReady())
                        //{
                        //    stopTouch = true;
                        //    Invoke(nameof(ShowWatermelonPanel), 0.5f);
                        //}
                        //else
                        //{
                            break;
                        //}
                    }
                }
            }
        }
    }

    private void ShowWatermelonPanel()
    {
        //SexyGirlUI.Instance.OnActive(true);
        //SexyGirlUI.Instance.OnShow();
    }


    public void EnableTouch(bool isAd)
    {
	if(isAd)
	{
	    //AdmobController.Instance.ShowInterstitialAd(0.2f);
	}
        StartCoroutine(CREnableTouch());
    }

    private IEnumerator CREnableTouch()
    {
        float timer = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        stopTouch = false;
    }

    int SumNumbers(int n)
    {
        int sum = 0;
        for (int i = 1; i <= n; i++)
        {
            sum += i;
        }
        return sum;
    }




    public void PlayClip(AudioClip clip)
    {
        if (PlayerPrefs.GetInt("CanPlaySounds", 1) == 1) SoundAudioSource.PlayOneShot(clip);
    }
    void ShowLosePanel()
    {
        if (!LosePanel.activeSelf)
        {
            ReplayScoreText.text = Score + "";
            PlayClip(loseSound);
            LosePanel.SetActive(true);
            //AdmobController.Instance.ShowInterstitialAd(0.2f);
        }
    }
    public void ReplayLevelBtn()
    {
        SceneManager.LoadScene(0);
    }
    public void ShowSettingsPanel()
    {
        SettingsPanel.SetActive(true);
        //AdmobController.Instance.ShowInterstitialAd(0.2f);
    }
    public void HideSettingsPanel()
    {
        Invoke("DelayedHideSettingsPanel", 0.2f);
    }
    void DelayedHideSettingsPanel()
    {
        SettingsPanel.SetActive(false);
    }
    public void SoundButton()
    {
        if (SoundBtn.sprite == soundOn)
        {
            SoundBtn.sprite = soundOff;
            PlayerPrefs.SetInt("CanPlaySounds", 0);
        }
        else
        {
            SoundBtn.sprite = soundOn;
            PlayerPrefs.SetInt("CanPlaySounds", 1);
        }
    }
    public void MusicButton()
    {
        if (MusicBtn.sprite == musicOn)
        {
            MusicBtn.sprite = musicOff;
            PlayerPrefs.SetInt("CanPlayMusic", 0);
            MusicAudioSource.volume = 0;
        }
        else
        {
            MusicBtn.sprite = musicOn;
            PlayerPrefs.SetInt("CanPlayMusic", 1);
            MusicAudioSource.volume = 0.4f;
        }
    }
}
