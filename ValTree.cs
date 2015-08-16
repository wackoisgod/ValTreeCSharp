using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication2
{
	public class ValTree
	{
		static int findWhitespace(string s, int start)
		{
			int i = start;
			for (; i < s.Length && !char.IsWhiteSpace(s[i]); i++)
				;
			return i;
		}

		static int findNonWhitespace(string s, int start)
		{
			int i = start;
			for (; i < s.Length && char.IsWhiteSpace(s[i]) && s[i] != '\n' && s[i] != '\r'; i++)
				;
			return i;
		}

		static int findNewline(string s, int start)
		{
			int i = start;
			for (; i < s.Length && s[i] != '\n' && s[i] != '\r'; i++)
				;
			return i;
		}

		static int findAfterNewline(string s, int start)
		{
			int i = findNewline(s, start);
			for (; i < s.Length && (s[i] == '\n' || s[i] == '\r'); i++)
				;
			return skipComment(s, i);
		}

		static int skipComment(string s, int start)
		{
			// so we have found a comment
			if (start < s.Length && s[start] == ';')
			{
				return findAfterNewline(s, start);
			}

			return start;
		}

		private string _key;
		private string _value;
		private int _valInt;
		private double _valFloat;
		private List<ValTree> _children = new List<ValTree>();
		private List<ValTree> _siblings = new List<ValTree>();

		public List<ValTree> Children
		{
			get
			{
				return _children;
			}
		}

		public List<ValTree> Siblings
		{
			get
			{
				return _siblings;
			}
		}

		private void SetValInt()
		{
			int v = 0;
			int.TryParse(_value, out v);
			_valInt = v;
		}
		private void SetValFloat()
		{
			double v = 0;
			double.TryParse(_value, out v);
			_valFloat = v;
		}

		private int GetDepth(string data, int pos)
		{
			for (int i = pos; i < data.Length; i++)
				if (!char.IsWhiteSpace((data[i])))
					return i - pos;
			return -1;
		}

		private bool Parse(string data, ref int pos, bool firstSibling)
		{
			var depth = GetDepth(data, pos);
			if (depth < 0) return false;

			int nextPos = skipComment(data, pos);
			nextPos = findAfterNewline(data, nextPos);
			int childDepth = GetDepth(data, nextPos);

			int startPos = pos + depth;
			pos = findWhitespace(data, pos + depth + 1);


			_key = data.Substring(startPos, pos < data.Length ? pos - startPos : data.Length);

			if (_key.Length > 0)
			{
				pos = findNonWhitespace(data, pos);
				int end = findNewline(data, pos);
				if (pos < data.Length && end > pos)
				{
					_value = data.Substring(pos, end - pos);

					SetValInt();
					SetValFloat();
				}
			}

			pos = nextPos;

			if (childDepth > depth)
			{
				ValTree v = new ValTree();
				if (v.Parse(data, ref pos, true))
					_children.Add(v);
				childDepth = GetDepth(data, pos);
			}

			if (childDepth == depth && firstSibling)
			{
				bool success = true;
				while (success && childDepth == depth)
				{
					ValTree v = new ValTree();
					success = v.Parse(data, ref pos, false);
					if (success)
						_siblings.Add(v);
					childDepth = GetDepth(data, pos);
				}
				return _siblings.Count > 0;
			}

			return !IsNull();
		}

		public ValTree()
		{
			_valInt = 0;
			_valFloat = 0;
		}

		public ValTree(string inKey, string inValue)
		{
			Set(inKey, inValue);
		}

		public void Clear()
		{
			_key = String.Empty;
			_value = String.Empty;
			_valInt = 0;
			_valFloat = 0.0;
			_children.Clear();
			_siblings.Clear();

		}

		public bool IsNull()
		{
			return (_key.Length <= 0 && _value.Length <= 0);
		}

		public string GetKey() { return _key; }
		public string GetValue() { return _value; }
		public int GetInt() { return _valInt; }
		public double GetFloat() { return _valFloat; }

		public void Set(string key, string val)
		{
			_key = key;
			_value = val;
			SetValInt();
			SetValFloat();
		}

		public int Size() { return 0; }

		ValTree GetIndex(int index)
		{
			if (index > 0 && index - 1 < _siblings.Count())
				return _siblings[index - 1];

			return this;
		}

		public bool HasChildren()
		{
			return _children.Count > 0;
		}

		public ValTree GetFirstChild()
		{
			return (HasChildren() ? _children.First() : null);
		}

		public ValTree GetChild(string key)
		{
			return _children.Where(x => x._key == _key).SingleOrDefault();
		}
		public ValTree GetSibling(string key)
		{
			if (key == _key)
				return this;

			return _siblings.Where(x => x._key == key).SingleOrDefault();
		}

		public void AddChild(ref ValTree v)
		{
			_children.Add(v);
		}
		public void AddSibling(ref ValTree v)
		{
			_siblings.Add(v);
		}

		public bool Parse(string filename)
		{
			Clear();

			if (!System.IO.File.Exists(filename))
				return false;

			var data = System.IO.File.ReadAllText(filename);
			if (data.Length <= 0)
				return false;

			int pos = 0;
			Parse(data, ref pos, true);

			return true;
		}

		static void InternalLog(ref StringBuilder sb, ValTree inValue, int depth)
		{
			for (int i = 0; i < depth; i++)
				sb.AppendFormat("\t");

			sb.AppendFormat(inValue.GetKey() + " " + inValue.GetValue());
			if (inValue.HasChildren())
			{
				foreach (var c in inValue._children)
				{
					InternalLog(ref sb, c, depth + 1);
				}
			}

			int y = 0;
			foreach (var s in inValue._siblings)
				if (y++ > 0)
					InternalLog(ref sb, s, depth);

		}

		public bool Save(string filename)
		{
			var sb = new StringBuilder();
			InternalLog(ref sb, this, 0);
			System.IO.File.WriteAllText(filename, sb.ToString());

			return true;
		}

		public void Log()
		{
			var sb = new StringBuilder();
			InternalLog(ref sb, this, 0);
			Console.WriteLine(sb.ToString());
		}
	}
}
