/********************************************************************
	created:	2021/01/22
	author:		maoqy
	
	purpose:	框架内要用到的事件
*********************************************************************/

namespace iFramework
{
    public class WndOpeningEventArgs : EventArgs
    {
        public static int EventId { get; } = typeof(WndOpeningEventArgs).GetHashCode();
    }

    public class PopupOpeningEventArgs : EventArgs
    {
        public static int EventId { get; } = typeof(PopupOpeningEventArgs).GetHashCode();
    }
}