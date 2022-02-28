#pragma warning disable 0219
using System;
using System.Text;
using System.IO;
using iFramework;
namespace iFramework
{
	public sealed partial class I18NText : DataUnit
	{
		public int ID;
		public string key;
		public string value;
		private static byte[] _buffer = new byte[4];
		private static byte[] _strBuffer = new byte[1024];
		public override int Key()
		{
			return ID;
		}
		public override void Decode(MemoryStream s)
		{
			var sb = _buffer;
			{
				s.Read(sb,0,4);
				ID = BitConverter.ToInt32(sb, 0);
			}
			{
				s.Read(sb,0,2);
				short strlen = BitConverter.ToInt16(sb, 0);
				byte[] strbuf = strlen <= 512 ? _strBuffer : new byte[strlen * 2];
				s.Read(strbuf, 0, strlen * 2);
				key = Encoding.Unicode.GetString(strbuf, 0, strlen * 2);
			}
			{
				s.Read(sb,0,2);
				short strlen = BitConverter.ToInt16(sb, 0);
				byte[] strbuf = strlen <= 512 ? _strBuffer : new byte[strlen * 2];
				s.Read(strbuf, 0, strlen * 2);
				value = Encoding.Unicode.GetString(strbuf, 0, strlen * 2);
			}
		}
	}
}