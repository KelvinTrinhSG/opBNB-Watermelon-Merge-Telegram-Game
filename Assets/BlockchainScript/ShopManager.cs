using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using TMPro;
using UnityEngine.UI;
using System.Numerics;
using System;
using System.Data;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Collections.LowLevel.Unsafe;

public class ShopManager : MonoBehaviour
{
    public string Address { get; private set; }
    public static BigInteger ChainId = 204;

    public UnityEngine.UI.Button playButton;
    public UnityEngine.UI.Button claimTokenButton;
    public UnityEngine.UI.Button getPassButton;

    public TMP_Text tokenBoughtText;
    public TMP_Text buyingStatusText;

    string customSmartContractAddress = "0xb48e9fF5066BE95763D5A06f99Fa372926c310D7";
    string abiString = "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"newTotal\",\"type\":\"uint256\"}],\"name\":\"KeyPassAdded\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"addKeyPass\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getPlayerKeyPass\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"}]";

    int tokenAmount = 10;

    string notEnoughToken = " BNB";

    void Start()
    {
        playButton.gameObject.SetActive(false);


        if (PlayerPrefs.GetInt("IsFirstTime", 1) == 1)        {
            PlayerPrefs.SetInt("Token", 1);
            PlayerPrefs.SetInt("IsFirstTime", 0);
            PlayerPrefs.Save();
            Debug.Log("First time playing! Token set to 1.");
        }
        else
        {
            Debug.Log("Not the first time playing.");
        }

        int tokenOwned = PlayerPrefs.GetInt("Token", 0);
        tokenBoughtText.text = "Total Tokens: " + tokenOwned.ToString();

        buyingStatusText.gameObject.SetActive (false);
    }

    private void HideAllButtons()
    {
        playButton.interactable = false;
        claimTokenButton.interactable = false;
        getPassButton.interactable = false;
    }

    private void ShowAllButtons()
    {
        playButton.interactable = true;
        claimTokenButton.interactable = true;
        getPassButton.interactable = true;
    }

    private void UpdateStatus(string messageShow)
    {
        buyingStatusText.text = messageShow;
        buyingStatusText.gameObject.SetActive(true);
    }

    private void BoughtSuccessFully()
    {
        //Lấy token từ bộ nhớ máy local
        //cộng với số token mới claim
        //thiết lập lại token lưu trong máy local với số token mới
        //lưu lại
        //Debug ra để theo dõi
        //cập nhật ra ngoài ui cho người chơi biết
        int currentToken = PlayerPrefs.GetInt("Token", 0);
        int newTokenAmount = currentToken + tokenAmount;
        PlayerPrefs.SetInt("Token", newTokenAmount);
        PlayerPrefs.Save();
        Debug.Log($"Token updated. Previous: {currentToken}, Added: {tokenAmount}, New: {newTokenAmount}");
        UpdateStatus("Got 10 Tokens");
        int tokenOwned = PlayerPrefs.GetInt("Token", 0);
        tokenBoughtText.text = "Total Tokens: " + tokenOwned.ToString();
    }
    IEnumerator WaitAndExecute()
    {
        Debug.Log("Coroutine started, waiting for 3 seconds...");
        yield return new WaitForSeconds(3f); // Chờ 3 giây
        Debug.Log("3 seconds have passed!");
        BoughtSuccessFully();
        ShowAllButtons();
    }

    public async void Claim10Tokens()
    {
        var wallet = ThirdwebManager.Instance.GetActiveWallet();
        var contract = await ThirdwebManager.Instance.GetContract(
           customSmartContractAddress,
           ChainId,
           abiString
       );
        var address = await wallet.GetAddress();

        // Gọi hàm `submitScore` trong hợp đồng với điểm số (score)
        await ThirdwebContract.Write(wallet, contract, "addKeyPass", 0, address, 1);

        var result = ThirdwebContract.Read<int>(contract, "getPlayerKeyPass", address);
        Debug.Log("result: " + result);
    }

    public async void GetTokens()
    {
        //Che đi toàn bộ nút
        //Cập nhật status ra UI cho người chơi biết
        //lấy balance hiện tại của ví
        //nếu không có token nào thì thông báo không đủ token cho người biết
        //Hiện lại toàn bộ nút
        //chạy song hành với claim token luôn chức năng credit token cho người chơi
        //rồi nó sẽ chạy try hàm claim 10 token để tương tác với blockchain.
        // nếu có lỗi thì nó sẽ báo ra.

        HideAllButtons();
        UpdateStatus("Getting 10 Tokens...");
        var wallet = ThirdwebManager.Instance.GetActiveWallet();
        var balance = await wallet.GetBalance(chainId: ChainId);
        var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 4, addCommas: true);
        Debug.Log("balanceEth1: " + balanceEth);
        if (float.Parse(balanceEth) <= 0f)
        {
            UpdateStatus("Not Enough" + notEnoughToken);
            ShowAllButtons();
            return;
        }
        //Bắt đầu Coroutine
        StartCoroutine(WaitAndExecute());
        try
        {
            Claim10Tokens();
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred during the transfer: {ex.Message}");
        }
    }

    public void SpendTokenToPlayGame()
    {
        int currentToken = PlayerPrefs.GetInt("Token", 0);
        if (currentToken <= 0)
        {
            Debug.Log("Not enough tokens!");
            return;
        }
        currentToken -= 1;
        PlayerPrefs.SetInt("Token", currentToken);
        PlayerPrefs.Save();
        Debug.Log("1 token has been deducted. Remaining tokens: " + currentToken);

        int tokenOwned = PlayerPrefs.GetInt("Token", 0);
        tokenBoughtText.text = "Total Tokens: " + tokenOwned.ToString();
        playButton.gameObject.SetActive(true);
        getPassButton.gameObject.SetActive(false);
    }

    public void ChangeToScenePlay()
    {
        SceneManager.LoadScene("GameScene");
    }
}
