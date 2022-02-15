/********************************************************************
	created:	2021/12/14
	author:		ZYL
	
	purpose:	用Dotween控制UI动画
*********************************************************************/
using UnityEngine;
using DG.Tweening;

namespace iFramework
{
    [System.Serializable]
    public class UIAnim_Dotween
    {
        public DOTweenAnimation[] showAnimations;
        public DOTweenAnimation[] hideAnimations;
        [HideInInspector] public float showDuration;
        [HideInInspector] public float hideDuration;

        private bool _inited;

        private void Init()
        {
            if (_inited) return;
            _inited = true;
            foreach (var anim in showAnimations)
            {
                showDuration = Mathf.Max(showDuration, anim.delay + anim.duration);
            }

            foreach (var anim in hideAnimations)
            {
                hideDuration = Mathf.Max(hideDuration, anim.delay + anim.duration);
            }

            if (showAnimations.Length == 0)
            {
                showDuration = hideDuration;
                foreach (var anim in hideAnimations)
                {
                    anim.DOPlay();
                    anim.DOComplete();
                }
            }
            else if (hideAnimations.Length == 0)
                hideDuration = showDuration;
        }

        public bool Valid => showAnimations.Length > 0 || hideAnimations.Length > 0;

        public void PlayShow()
        {
            Init();
            if (showAnimations.Length > 0)
            {
                foreach (var anim in showAnimations)
                {
                    anim.DOPlayForwardById("show");
                }
            }
            else
            {
                foreach (var anim in hideAnimations)
                {
                    anim.DOPlayBackwardsById("hide");
                }
            }
        }

        public void PlayHide()
        {
            Init();
            if (hideAnimations.Length == 0)
            {
                foreach (var anim in showAnimations)
                {
                    anim.DOPlayBackwardsById("show");
                }
            }
            else
            {
                foreach (var anim in hideAnimations)
                {
                    anim.DOPlayForwardById("hide");
                }
            }
        }

        public void StopShow()
        {
            if (showAnimations.Length > 0)
            {
                foreach (var anim in showAnimations)
                    anim.DOComplete();
            }
            else
            {
                foreach (var anmi in hideAnimations)
                    anmi.DOComplete();
            }
        }

        public void StopHide()
        {
            if (hideAnimations.Length > 0)
            {
                foreach (var anmi in hideAnimations)
                    anmi.DOComplete();
            }
            else
            {
                foreach (var anim in showAnimations)
                    anim.DOComplete();
            }
        }
    }
}

