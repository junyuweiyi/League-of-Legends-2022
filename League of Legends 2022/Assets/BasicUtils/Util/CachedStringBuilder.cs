/********************************************************************
	created:	2021/11/26
	author:		maoqy
	
	purpose:	
*********************************************************************/

using System.Text;
using System.Collections.Generic;

public static class CachedStringBuilder
{
    static Stack<StringBuilder> _cachedStringBuilders = new Stack<StringBuilder>();
    public static StringBuilder Get()
    {
        if (_cachedStringBuilders.Count > 0)
            return _cachedStringBuilders.Pop();
        return new StringBuilder();
    }

    public static void Release(this StringBuilder sb)
    {
        sb.Clear();
        _cachedStringBuilders.Push(sb);
    }
}