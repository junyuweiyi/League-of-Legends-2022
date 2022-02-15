/********************************************************************
	created:	2021/06/10
	author:		ZYL

	purpose:    UI动画组件，挂在UI窗口顶层，UI窗口打开和关闭时候会
                由UI框架自动调用。也可挂在别的地方，按需调用。
                支持DOTween和Animator两种方式控制UI动画。
*********************************************************************/

using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace iFramework
{
    public class UIAnimation : MonoBehaviour
    {
        public UIAnim_Dotween dotweenUIAnim;
        public UIAnim_Animator animatorUIAnim;

        private GraphicRaycaster _raycaster;
        protected Coroutine _waitingHideCoroutine;
        protected Coroutine _waitingShowCoroutine;
        private Action _onShowComplete;
        private Action _onHideComplete;

        private void Awake()
        {
            _raycaster = GetComponent<GraphicRaycaster>();
        }

        public virtual void PlayShowAnimations(IUIData initData = null, Action onComplete = null)
        {
            float duration = 0;
            if (dotweenUIAnim.Valid)
            {
                dotweenUIAnim.PlayShow();
                duration = Mathf.Max(duration, dotweenUIAnim.showDuration);
            }
            if (animatorUIAnim.Valid)
            {
                animatorUIAnim.PlayShow();
                duration = Mathf.Max(duration, animatorUIAnim.showDuration);
            }
            if (duration > 0)
                _waitingShowCoroutine = StartCoroutine(WaitingShow(duration, onComplete));
            else
                onComplete?.Invoke();
        }

        public virtual void PlayHideAnimations(IUIData initData = null, Action onComplete = null)
        {
            float duration = 0;
            if (dotweenUIAnim.Valid)
            {
                dotweenUIAnim.PlayHide();
                duration = Mathf.Max(duration, dotweenUIAnim.hideDuration);
            }
            if (animatorUIAnim.Valid)
            {
                animatorUIAnim.PlayHide();
                duration = Mathf.Max(duration, animatorUIAnim.hideDuration);
            }
            if (duration > 0)
                _waitingHideCoroutine = StartCoroutine(WaitingHide(duration, onComplete));
            else
                onComplete?.Invoke();
        }

        protected IEnumerator WaitingShow(float duration, Action onComplete)
        {
            _onShowComplete = onComplete;
            if (_raycaster != null) _raycaster.enabled = true;
            yield return new WaitForSeconds(duration);

            StopShow();
        }

        protected IEnumerator WaitingHide(float duration, Action onComplete)
        {
            _onHideComplete = onComplete;
            if (_raycaster != null) _raycaster.enabled = false;
            yield return new WaitForSeconds(duration);
            if (_raycaster != null) _raycaster.enabled = true;

            StopHide();
        }

        private void StopShow()
        {
            _waitingShowCoroutine = null;
            animatorUIAnim.StopShow();
            dotweenUIAnim.StopShow();

            _onShowComplete?.Invoke();
            _onShowComplete = null;
        }

        private void StopHide()
        {
            _waitingHideCoroutine = null;
            animatorUIAnim.StopHide();
            dotweenUIAnim.StopHide();

            _onHideComplete?.Invoke();
            _onHideComplete = null;
        }

        public void StopPlay()
        {
            if (_waitingShowCoroutine != null)
            {
                StopCoroutine(_waitingShowCoroutine);
                StopShow();
            }

            if (_waitingHideCoroutine != null)
            {
                StopCoroutine(_waitingHideCoroutine);
                StopHide();
            }
        }

        private void OnDisable()
        {
            if (_waitingShowCoroutine != null)
            {
                StopShow();
            }
            if (_waitingHideCoroutine != null)
            {
                StopHide();
            }
        }
    }
}