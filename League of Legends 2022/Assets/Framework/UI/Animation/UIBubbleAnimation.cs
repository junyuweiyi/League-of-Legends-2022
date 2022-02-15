/********************************************************************
    created:    2021/12/27
    author:     PHT

    purpose:    
*********************************************************************/

using System;
using UnityEngine;

namespace iFramework
{
    public class UIBubbleAnimation : UIAnimation
    {
        public enum AnimDir
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        public AnimDir Dir { get; set; }

        public sealed override void PlayShowAnimations(IUIData initData = null, Action onComplete = null)
        {
            float duration = 0;
            if (dotweenUIAnim.Valid)
            {
                dotweenUIAnim.PlayShow();
                duration = Mathf.Max(duration, dotweenUIAnim.showDuration);
            }
            if (animatorUIAnim.Valid)
            {
                var animName = Dir == AnimDir.None ? "Show" : "Show_" + Dir.ToString().ToLower();
                animatorUIAnim.PlayShow(animName);
                duration = Mathf.Max(duration, animatorUIAnim.showDuration);
            }
            if (duration > 0)
                _waitingShowCoroutine = StartCoroutine(WaitingShow(duration, onComplete));
            else
                onComplete?.Invoke();
        }

        public sealed override  void PlayHideAnimations(IUIData initData = null, Action onComplete = null)
        {
            float duration = 0;
            if (dotweenUIAnim.Valid)
            {
                dotweenUIAnim.PlayHide();
                duration = Mathf.Max(duration, dotweenUIAnim.hideDuration);
            }
            if (animatorUIAnim.Valid)
            {
                var animName = Dir == AnimDir.None ? "Hide" : "Hide_" + Dir.ToString().ToLower();
                animatorUIAnim.PlayHide(animName);
                duration = Mathf.Max(duration, animatorUIAnim.hideDuration);
            }
            if (duration > 0)
                _waitingHideCoroutine = StartCoroutine(WaitingHide(duration, onComplete));
            else
                onComplete?.Invoke();
        }
    }
}
