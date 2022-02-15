/********************************************************************
	created:	2021/12/14
	author:		ZYL
	
	purpose:	用Animator控制UI动画，支持多个animator，UI打开和关闭
                时自动调用
*********************************************************************/
using UnityEngine;


namespace iFramework
{
    [System.Serializable]
    public class UIAnim_Animator
    {
        public Animator[] animators;

        [HideInInspector] public float showDuration;
        [HideInInspector] public float hideDuration;
        private bool _inited;

        public bool Valid => animators.Length > 0;

        private void Init()
        {
            if (_inited) return;
            _inited = true;

            foreach (var animator in animators)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                Debug.Assert(clips.Length > 0);
                foreach (var clip in clips)
                {
                    if (clip.name.Contains("show"))
                    {
                        showDuration = Mathf.Max(showDuration, clip.length);
                    }
                    else if (clip.name.Contains("hide"))
                    {
                        hideDuration = Mathf.Max(hideDuration, clip.length);
                    }
                }
            }
        }

        public void PlayShow(string animName = "Show")
        {
            Init();

            foreach (var animator in animators)
            {
                animator.enabled = true;
                animator.Play(animName);
            }
        }

        public void PlayHide(string animName = "Hide")
        {
            Init();

            foreach (var animator in animators)
            {
                animator.enabled = true;
                animator.Play(animName);
            }
        }

        public void StopShow()
        {
            Stop();
        }

        public void StopHide()
        {
            Stop();
        }

        void Stop()
        {
            foreach (var animator in animators)
            {
                animator.Update(Mathf.Max(showDuration, hideDuration));
                animator.enabled = false;
            }
        }
    }
}

