/********************************************************************
	created:	2022/01/10
	author:		maoqy
	
	purpose:	
*********************************************************************/

using iFramework;

public class Dump
{
    object _o;
    int _depth;

    public Dump(object o, int depth = 3)
    {
        _o = o;
        _depth = depth;
    }

    public override string ToString()
    {
        return Util.String.Dump(_o, _depth);
    }
}