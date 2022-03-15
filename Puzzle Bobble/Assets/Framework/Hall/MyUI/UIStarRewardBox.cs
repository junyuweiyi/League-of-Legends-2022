using UnityEngine;
using UnityEngine.UI;

public class UIStarRewardBox : MonoBehaviour
{
    public Text starProgressText;
    public Slider starProgressSlider;

    private void OnEnable()
    {
        StarRewardMgr.I.UpdateCurrentStar();
        UpdateContent();
        StarRewardMgr.I.OnStarRewardDataChanged += OnStarRewardDataChanged;
    }

    private void OnDisable()
    {
        StarRewardMgr.I.OnStarRewardDataChanged -= OnStarRewardDataChanged;
    }

    void UpdateContent()
    {
        starProgressText.text = StarRewardMgr.I.CurrentStar + "/" + StarRewardMgr.I.MaxStar;
        starProgressSlider.minValue = 0;
        starProgressSlider.maxValue = StarRewardMgr.I.MaxStar;
        starProgressSlider.value = StarRewardMgr.I.CurrentStar;
    }

    void OnStarRewardDataChanged()
    {
        UpdateContent();
    }
}
