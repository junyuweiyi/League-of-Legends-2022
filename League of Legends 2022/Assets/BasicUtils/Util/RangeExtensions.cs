/********************************************************************
	created:	2020/07/26
	author:		guolu
	
	purpose:	Range 和 RangeInt 的拓展函数
*********************************************************************/

using DG.DemiLib;
using System;

using UnityEngine;


namespace iFramework
{

    public static class RangeExtensions
    {
        /// <summary>
        /// 闭区间
        /// </summary>
        public static bool IsWithInClosedInterval(this RangeInt rangeInt, int intValue)
        {
            return intValue >= rangeInt.start && intValue <= rangeInt.end;
        }
        /// <summary>
        /// 开区间
        /// </summary>
        public static bool IsWithInOpenInterval(this RangeInt rangeInt, int intValue)
        {
            return intValue > rangeInt.start && intValue < rangeInt.end;
        }
    }
}
