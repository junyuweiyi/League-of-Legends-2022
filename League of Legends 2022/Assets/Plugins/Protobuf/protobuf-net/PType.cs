using System;
using System.Collections.Generic;

namespace ProtoBuf
{
    public class PType
    {
        public delegate Type DelegateFunctionGetRealType(object o);
        public delegate object DelegateFunctionCreateInstance(string typeName);

        static PType Current
        {
            get
            {
                if (m_Current == null)
                {
                    m_Current = new PType();
                }
                return m_Current;
            }
        }

        private PType() { }

        public static void RegisterType(string metaName, Type type)
        {
            Current.RegisterTypeInternal(metaName, type);
            if (!type.IsEnum)
            {
                var name = type.Name;
                var hash = BKDRHash(name);
#if UNITY_EDITOR
                if (MsgIDType.ContainsKey(hash) && MsgIDType[hash].Namespace != type.Namespace)
                {
                    UnityEngine.Debug.LogError($"Same hash: {MsgIDType[hash]} and {type}");
                }
#endif
                MsgNameID[name] = hash;
                MsgIDName[hash] = name;
                MsgIDType[hash] = type;
            }
        }

        public static bool MsgIDExist(uint id)
        {
            return MsgIDName.ContainsKey(id);
        }

        public static string MsgID2Name(uint id)
        {
#if DEBUG
            UnityEngine.Debug.AssertFormat(MsgIDName.ContainsKey(id), "MsgIDName no id {0}", id);
#endif
            return MsgIDName[id];
        }

        public static Type MsgID2Type(uint id)
        {
#if DEBUG
            UnityEngine.Debug.AssertFormat(MsgIDType.ContainsKey(id), "MsgIDType no id {0}", id);
#endif
            return MsgIDType[id];
        }

        public static uint MsgName2ID(string name)
        {
#if DEBUG
            UnityEngine.Debug.AssertFormat(MsgNameID.ContainsKey(name), "MsgNameID no name {0}", name);
#endif
            return MsgNameID[name];
        }

        public static void RegisterFunctionCreateInstance(DelegateFunctionCreateInstance func)
        {
            CreateInstanceFunc = func;
        }

        public static void RegisterFunctionGetRealType(DelegateFunctionGetRealType func)
        {
            GetRealTypeFunc = func;
        }

        public static Type FindType(string metaName)
        {
            return Current.FindTypeInternal(metaName);
        }

        public static object CreateInstance(Type type)
        {
            if (Type.GetType(type.FullName) == null)
            {
                if (CreateInstanceFunc != null)
                {
                    return CreateInstanceFunc.Invoke(type.FullName);
                }
            }
            return Activator.CreateInstance(type
#if !(CF || SILVERLIGHT || WINRT || PORTABLE || NETSTANDARD1_3 || NETSTANDARD1_4)
                , nonPublic: true
#endif
            );
        }

        public static Type GetPType(object o)
        {
            if (GetRealTypeFunc != null)
            {
                return GetRealTypeFunc.Invoke(o);
            }
            return o.GetType();
        }

        static uint BKDRHash(string s)
        {
            uint seed = 131;
            uint hash = 0;

            for (int i = 0; i < s.Length; i++)
            {
                hash = hash * seed + s[i];
            }
            return hash;
        }

        void RegisterTypeInternal(string metaName, Type type)
        {
            m_Types[metaName] = type;
            //if (!m_Types.ContainsKey(metaName))
            //     {
            //m_Types.Add(metaName,type);
            //     }
            //     else
            //throw new SystemException(string.Format("PropertyMeta : {0} is registered!",metaName));
        }

        Type FindTypeInternal(string metaName)
        {
            Type type = null;
            if (!m_Types.TryGetValue(metaName, out type))
            {
                throw new SystemException(string.Format("PropertyMeta : {0} is not registered!", metaName));
            }
            return type;
        }

        static PType m_Current;
        static DelegateFunctionGetRealType GetRealTypeFunc;
        static DelegateFunctionCreateInstance CreateInstanceFunc;
        Dictionary<string, Type> m_Types = new Dictionary<string, Type>();
        static Dictionary<uint, Type> MsgIDType = new Dictionary<uint, Type>();
        static Dictionary<string, uint> MsgNameID = new Dictionary<string, uint>();
        static Dictionary<uint, string> MsgIDName = new Dictionary<uint, string>();
    }
}
