/********************************************************************
	created:	2020/09/22
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace iFramework
{
    public enum ColorType
    {
        blue,
        yellow,
        red,
    }
    public class MsgBoxContent : UIData
    {
        public string dailyKey;
        public string title = "LC_UI_Title_Notice";            //可以为null
        public string msg;
        public string yes = "LC_UI_Btn_Confirm";       //取消按钮显示字符，为null或空则没有取消按钮
        public string no = "LC_UI_Btn_Cancel";    //确定按钮显示字符，为null或空则没有确定按钮
        public ColorType colorYes = ColorType.blue;  // 
        public ColorType colorNo = ColorType.yellow;  // 
        public bool closeEnable;
        public Action<string, string> onClickLink; //超链接点击回调
        public bool isYesRight = true;  // 确定是在左边还是右边
    }

    public enum MsgBoxEventID
    {
        YES,
        NO,
        CLOSE,
    }

    public class MessageBox : UIView
    {
        public static IObjectPool Pool { get; set; }
        public static ILocalizationMgr Localization { get; set; }
        internal new static UIMgr UIMgr { get; set; }


        public TextMeshProUGUI txtTitle;
        public TextMeshProUGUI txtMsg;

        public GameObject goTogRoot;
        public Toggle togHint;
        public TextMeshProUGUI txtTog;

        public GameObject goNo;
        public Button btnNoBlue;
        public TextMeshProUGUI txtNoBlue;
        public Button btnNoYellow;
        public TextMeshProUGUI txtNoYellow;


        public GameObject goYes;
        public Button btnYesYellow;
        public TextMeshProUGUI txtYesYellow;
        public Button btnYesRed;
        public TextMeshProUGUI txtYesRed;
        public Button btnYesBlue;
        public TextMeshProUGUI txtYesBlue;

        public GameObject btnClose;

        private Action<MsgBoxEventID> _onComplete;
        private Action<string, string> _onClickLink;

        private static GameObject _prefab = null;
        private MsgBoxContent _content;
        private static List<MessageBox> _messageBoxes = new List<MessageBox>();


        private void Start()
        {
            var btns = goNo.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                b.onClick.AddListener(OnClickNo);
            }

            btns = goYes.GetComponentsInChildren<Button>(true);
            foreach (var b in btns)
            {
                b.onClick.AddListener(OnClickYes);
            }
        }

        internal static MessageBox Show(MsgBoxContent content, Action<MsgBoxEventID> callback)
        {
            if (!string.IsNullOrEmpty(content.dailyKey) && !IsShowDailyTip(content.dailyKey))
            {
                callback?.InvokeSafely(MsgBoxEventID.YES);
                return null;
            }
            if (_prefab == null)
            {
                _prefab = Resources.Load("MsgBox") as GameObject;
            }

            //GameObject go = Pool.Spawn(_prefab, UIMgr.UIRoot.topUICanvas);
            GameObject go = Instantiate(_prefab, UIMgr.UIRoot.topUICanvas);
            MessageBox mb = go.GetComponent<MessageBox>();
            mb.Init(content, callback);
            UIMgr.UIRoot.AddTopUI(mb, true, true);
            _messageBoxes.Add(mb);
            return mb;
        }

        private void Init(MsgBoxContent content, Action<MsgBoxEventID> callback)
        {
            _content = content;
            _onComplete = callback;
            _onClickLink = content.onClickLink;
            btnClose.SetActive(content.closeEnable);
            if (!string.IsNullOrEmpty(content.dailyKey))
            {
                togHint.isOn = false;
                goTogRoot.SetActive(true);
                bool isShowDailyTip = IsShowDailyTip(content.dailyKey);
                if (!isShowDailyTip)
                {
                    HandleEvent(MsgBoxEventID.YES);
                    return;
                }

                txtTog.text = Localization.Get("LC_UI_Common_DontNoticeAgain");
            }
            else
            {
                goTogRoot.SetActive(false);
            }

            txtTitle.text = Localization.Get(content.title);
            txtMsg.text = Localization.Get(content.msg);
            txtMsg.rectTransform.anchoredPosition = new Vector2(0, Mathf.Max(txtMsg.preferredHeight, 360) / -2);

            SetButtonTxt(content.yes, content.colorYes, content.no, content.colorNo, content.isYesRight);



            UIAnimation anim = GetComponent<UIAnimation>();
            anim?.PlayShowAnimations();
        }

        void SetButtonTxt(string txtyes, ColorType coloryes, string txtno, ColorType colorno,
            bool isYesRight)
        {
            if (string.IsNullOrEmpty(txtyes))
            {
                goYes.gameObject.SetActive(false);
            }
            else
            {
                goYes.gameObject.SetActive(true);


                for (int i = 0; i < goYes.transform.childCount; i++)
                {
                    goYes.transform.GetChild(i).gameObject.SetActive(false);
                }

                switch (coloryes)
                {
                    case ColorType.blue:
                        btnYesBlue.gameObject.SetActive(true);
                        txtYesBlue.text = Localization.Get(txtyes);
                        break;
                    case ColorType.yellow:
                        btnYesYellow.gameObject.SetActive(true);
                        txtYesYellow.text = Localization.Get(txtyes);
                        break;
                    case ColorType.red:
                        btnYesRed.gameObject.SetActive(true);
                        txtYesRed.text = Localization.Get(txtyes);
                        break;
                    default:
                        Debug.LogError("not supported " + coloryes);
                        btnYesBlue.gameObject.SetActive(true);
                        txtYesBlue.text = Localization.Get(txtyes);
                        break;
                }
            }

            if (string.IsNullOrEmpty(txtno))
            {
                goNo.gameObject.SetActive(false);
            }
            else
            {
                goNo.gameObject.SetActive(true);

                for (int i = 0; i < goNo.transform.childCount; i++)
                {
                    goNo.transform.GetChild(i).gameObject.SetActive(false);
                }

                switch (colorno)
                {
                    case ColorType.blue:
                        btnNoBlue.gameObject.SetActive(true);
                        txtNoBlue.text = Localization.Get(txtno);
                        break;
                    case ColorType.yellow:
                        btnNoYellow.gameObject.SetActive(true);
                        txtNoYellow.text = Localization.Get(txtno);
                        break;
                    default:
                        Debug.LogError("not supported " + colorno);
                        btnNoBlue.gameObject.SetActive(true);
                        txtNoBlue.text = Localization.Get(txtno);
                        break;
                }
            }


            // 确定是在左边还是右边
            if (isYesRight)
            {
                goYes.transform.SetAsLastSibling();
            }
            else
            {
                goNo.transform.SetAsLastSibling();
            }
        }

        private static bool IsShowDailyTip(string dailyKey)
        {
            if (PlayerPrefs.HasKey(dailyKey))
            {
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long seconds = Convert.ToInt64(ts.TotalSeconds);
                long day = seconds / 86400;//每天86400秒
                //当前day 已经超过 记录时候的 day
                return day > long.Parse(PlayerPrefs.GetString(dailyKey));
            }
            else
            {
                return true;
            }
        }
        private void RecordDailyKey(string dailyKey)
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long seconds = Convert.ToInt64(ts.TotalSeconds);
            long day = seconds / 86400;//每天86400秒
            PlayerPrefs.SetString(dailyKey, day.ToString());
        }
        public void OnClickNo()
        {
            HandleEvent(MsgBoxEventID.NO);
        }

        public void OnClickYes()
        {
            if (!string.IsNullOrEmpty(_content.dailyKey) && togHint.isOn)
            {
                RecordDailyKey(_content.dailyKey);
            }
            HandleEvent(MsgBoxEventID.YES);
        }

        public void OnClickClose()
        {
            HandleEvent(MsgBoxEventID.CLOSE);
        }

        public void OnClickLink(string linkID, string linkText)
        {
            _onClickLink?.Invoke(linkID, linkText);
            _onClickLink = null;
        }

        public sealed override void OnMaskBGClick()
        {
            HandleEvent(MsgBoxEventID.NO);
        }

        private void HandleEvent(MsgBoxEventID id)
        {

            _onComplete?.Invoke(id);
            _onComplete = null;
            Close();
        }

        public void Close()
        {
            UIAnimation anim = GetComponent<UIAnimation>();
            if (anim != null)
            {
                anim.PlayHideAnimations(onComplete:() =>
                {
                    _messageBoxes.Remove(this);
                    UIMgr.UIRoot.RemoveTopUI(this, false);
                    Destroy(gameObject);
                    //Pool.Unspawn(gameObject);
                    UIMgr.TrySetTopUI();
                });
            }
            else
            {
                _messageBoxes.Remove(this);
                UIMgr.UIRoot.RemoveTopUI(this, false);
                //Pool.Unspawn(gameObject);
                Destroy(gameObject);
                UIMgr.TrySetTopUI();
            }
        }

        internal static bool TryCloseTopUI()
        {
            if (_messageBoxes.Count > 0)
            {
                MessageBox mb = _messageBoxes[_messageBoxes.Count - 1];
                mb.OnClickNo();
                return true;
            }
            return false;
        }

        internal static bool TrySetTopUI()
        {
            if (_messageBoxes.Count > 0)
            {
                UIMgr.UIRoot.AddTopUI(_messageBoxes[_messageBoxes.Count - 1], true, true);
                return true;
            }
            return false;

        }
    }
}