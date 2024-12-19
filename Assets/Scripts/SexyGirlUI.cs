using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SexyGirlUI : MonoBehaviour
{
    public static SexyGirlUI Instance { private set; get; }


    [SerializeField] private Image mainImage = null;
    [SerializeField] private Transform canvasTrans = null;
    [SerializeField] private GameObject closeButton1 = null;
    [SerializeField] private GameObject closeButton2 = null;
    [SerializeField] private Sprite[] imageSprites = null;

    private const string SAVED_INDEX = "SAVED_INDEX";
    private Sprite originalSprite = null;
    private bool isAds = false;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance = null;
            DestroyImmediate(gameObject);
        }
    }


    public void OnActive(bool active)
    {
        canvasTrans.gameObject.SetActive(active);
    }


    public void OnShow()
    {
        if (!PlayerPrefs.HasKey(SAVED_INDEX)) { PlayerPrefs.SetInt(SAVED_INDEX, 0); }
	isAds = true;
        closeButton1.SetActive(false);
        closeButton2.SetActive(false);
        originalSprite = mainImage.sprite;
        StartCoroutine(ShowCloseButton());
    }


    public void WatchAdButton()
    {
	isAds = false;
        StopAllCoroutines();
        closeButton2.SetActive(false);
        AdmobController.Instance.ShowRewardedAd(0.2f);
    }

    private IEnumerator ShowCloseButton()
    {
        float timeCount = 3f;
        while(timeCount > 0) 
        {
            timeCount -= Time.deltaTime;
            yield return null;
        }
        closeButton2.SetActive(true);
    }

    public void OnRewardedAdClosed()
    {
        int currentIndex = PlayerPrefs.GetInt(SAVED_INDEX);
        mainImage.sprite = imageSprites[currentIndex];
        PlayerPrefs.SetInt(SAVED_INDEX, currentIndex == imageSprites.Length - 1 ? 0 : currentIndex + 1); 
        closeButton1.SetActive(true);
    }


    public void CloseButton()
    {
        mainImage.sprite = originalSprite;
        FindObjectOfType<GameManager>().EnableTouch(isAds);
        OnActive(false);
    }
}
