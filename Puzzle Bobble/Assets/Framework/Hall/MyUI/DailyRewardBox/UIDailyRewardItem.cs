using UnityEngine;
using UnityEngine.UI;

public class UIDailyRewardItem : MonoBehaviour
{
    public Text dayText;
    public Transform rewardRoot;
    public GameObject nextLogo;
    public GameObject receivedLogo;

    DailyReward _data;

    public void SetData(DailyReward dailyReward)
    {
        _data = dailyReward;
        dayText.text = _data.day.ToString();
        nextLogo.SetActive(dailyReward.canReceive);
        receivedLogo.SetActive(dailyReward.received);
        for (int i = 0; i < rewardRoot.childCount; i++)
        {
            rewardRoot.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < _data.rewards.Count; i++)
        {
            if (rewardRoot.childCount <= i) Instantiate(rewardRoot.GetChild(0), rewardRoot);
            rewardRoot.GetChild(i).gameObject.SetActive(true);
            rewardRoot.GetChild(i).GetComponent<UIItem>().SetData(_data.rewards[i]);
        }
    }
}
