/********************************************************************
	created:	2020/09/17
	author:		xp
	
	purpose:	scrollrect 的扩展函数
*********************************************************************/

using System;

using UnityEngine;
using UnityEngine.UI;

namespace iFramework
{
    public static class ScrollRectExtensions
    {
        static Vector2 GetRectSize(RectTransform rect)
        {
            return rect.rect.size;
        }
        // 调用之前可能需要调用 UITools.RebuildAllLayoutInChildren
        // normalizedPosition 0-1
        // return x; 然后调用scroll.verticalNormalizedPosition = x or  scroll.horizontalNormalizedPosition = x
        public static float CalcChildToNormalizedPosition(this ScrollRect scroll, Transform child, float normalizedPosition)
        {
            if (child.parent != scroll.content.transform)
            {
                Debug.LogError("not child");
                return 0;
            }

            if (child.gameObject.activeSelf == false)
            {
                Debug.LogError("not active");
                return 0;
            }

            if (scroll.horizontal)
            {
                return CalcChildToNormalizedPositionH(scroll, child, normalizedPosition);
            }
            else
            {
                return CalcChildToNormalizedPositionV(scroll, child, normalizedPosition);
            }
        }


        public static float CalcChildToNormalizedPositionV(ScrollRect thescroll, Transform child, float normalizedPosition)
        {
            Vector2 sizeview = GetRectSize(thescroll.viewport.GetComponent<RectTransform>());
            Vector2 sizecontent = GetRectSize(thescroll.content.GetComponent<RectTransform>());

            float relativepos = 0;
            Transform thep = child;
            int max = 4;
            do
            {
                relativepos += thep.localPosition.y;
                thep = thep.parent;
                if (max-- == 0)
                {
                    Debug.LogError("thetr is not the child of thescroll");
                    break;
                }
            } while (thep.GetInstanceID() != thescroll.content.GetInstanceID());


            float tarpos = Mathf.Abs(relativepos);

            float x1 = sizeview.y * normalizedPosition;
            float y1 = 1;
            float x2 = sizecontent.y - sizeview.y * normalizedPosition;
            float y2 = 0;

            float k = (y2 - y1) / (x2 - x1);
            float a = y1 - k * x1;

            float ratio = k * Mathf.Abs(tarpos) + a;
            ratio = Mathf.Clamp01(ratio);


            return ratio;
        }

        public static float CalcChildToNormalizedPositionH(ScrollRect thescroll, Transform child, float normalizedPosition)
        {
            Vector2 sizeview = GetRectSize(thescroll.viewport.GetComponent<RectTransform>());
            Vector2 sizecontent = GetRectSize(thescroll.content.GetComponent<RectTransform>());

            float relativepos = 0;
            Transform thep = child;
            int max = 4;
            do
            {
                relativepos += thep.localPosition.x;
                thep = thep.parent;
                if (max-- == 0)
                {
                    Debug.LogError("thetr is not the child of thescroll");
                    break;
                }
            } while (thep.GetInstanceID() != thescroll.content.GetInstanceID());


            float tarpos = Mathf.Abs(relativepos);

            float x1 = sizeview.x * normalizedPosition;
            float y1 = 0;
            float x2 = sizecontent.x - sizeview.x * normalizedPosition;
            float y2 = 1;

            float k = (y2 - y1) / (x2 - x1);
            float a = y1 - k * x1;

            float ratio = k * Mathf.Abs(tarpos) + a;
            ratio = Mathf.Clamp01(ratio);


            return ratio;
        }
    }
}