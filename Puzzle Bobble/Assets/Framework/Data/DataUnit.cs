/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System.IO;

namespace iFramework
{
    public abstract class DataUnit
    {
        public abstract void Decode(MemoryStream stream);
        public abstract int Key();

        DataUnitExtension _extension;
        public T Extension<T>() where T : DataUnitExtension
        {
            if (_extension is T)
                return _extension as T;

            _extension = (T)System.Activator.CreateInstance(typeof(T), this);
            return (T)_extension;
        }
    }

    public abstract class DataUnitExtension
    {
        public DataUnitExtension(DataUnit data) { }
    }

    public abstract class DataUnitExtension<T> : DataUnitExtension where T : DataUnit
    {
        protected T _data;
        public DataUnitExtension(T data) : base(data)
        {
            _data = data;
        }
    }
}