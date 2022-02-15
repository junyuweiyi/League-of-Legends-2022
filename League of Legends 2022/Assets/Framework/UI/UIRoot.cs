using System;
using iFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRoot : MonoBehaviour
{
    public Transform normalUICanvas;
    public Transform topUICanvas;
    public Camera uiCamera;
    public GameObject blurImg;

    private UIView _topUI;
    private IGussianBlurSwitch _blurSwitch;

    private void Start()
    {
        InitCamera();
    }

    private void InitCamera()
    {
        uiCamera.orthographicSize = Screen.height / 2f;
        uiCamera.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }

    public void AddTopUI(UIView ui, bool showMask, bool moveTopBack)
    {
        if (showMask)
        {
            if (_topUI != null) RemoveTopUI(_topUI, moveTopBack);
            blurImg.SetActive(true);
            _blurSwitch?.TurnOn();
        }
        _topUI = ui;

        AddTopUI(ui);
    }

    public void AddTopUI(UIView ui)
    {
        var trans = ui.transform as RectTransform;
        if (trans.parent != topUICanvas)
            trans.SetParent(topUICanvas);
    }

#if UNITY_EDITOR
    private float _lastwidth = 0f;
    private float _lastheight = 0f;
#endif

    private void Update()
    {
#if UNITY_EDITOR
        if (Math.Abs(_lastwidth - Screen.width) > 0.001f || Math.Abs(_lastheight - Screen.height) > 0.001f)
        {
            InitCamera();
        }
#endif
        if (!blurImg.activeSelf && _blurSwitch != null)
        {
            _blurSwitch.ResetPasses();
        }
    }

    public void TurnBlur(bool turnOn)
    {
        if (blurImg.activeSelf == turnOn) return;

        if (turnOn)
        {
            blurImg.SetActive(true);
            _blurSwitch?.TurnOn();
        }
        else
        {
            blurImg.SetActive(false);
            _blurSwitch?.TurnOff();
        }
    }

    public void RemoveTopUI(UIView ui, bool moveBack)
    {
        if (ui == _topUI)
        {
            _topUI = null;
            if (moveBack) AddNormalUI(ui);
            blurImg.SetActive(false);
            _blurSwitch?.TurnOff();
        }
    }

    public void AddNormalUI(UIView ui)
    {
        if (ui.transform.parent != normalUICanvas)
            ui.transform.SetParent(normalUICanvas);
    }

    public void OnMaskBGClick(PointerEventData _)
    {
        _topUI?.OnMaskBGClick();
    }

    internal void SetGussianBlurSwitch(IGussianBlurSwitch blurSwitch)
    {
        _blurSwitch = blurSwitch;
        var img = blurImg.GetComponent<Image>();
        if (_blurSwitch != null)
        {
            img.material = new Material(Shader.Find("SLG/UI/BlurGlass"));
            if (blurImg.activeSelf)
                _blurSwitch.TurnOn();
            else
                _blurSwitch.TurnOff();
        }
        else
        {
            img.material = null;
        }
    }
}
