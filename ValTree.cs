using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Security.Policy;
using System.Text;

namespace TinyArms
{
    public class ValTree : IEnumerable
    {
        private const int KMaxFileSize = 8*1024*1024;
        private const string KCommentStartTag = "//";
        private static ValTree blank = new ValTree();

        static bool IsWhiteSpace(char c)
        {
            return (c == ' ' || c == '\t' || c == '\n' || c == '\r');
        }

        static int FindWhitespace(string s, int start)
        {
            int i = start;
            for (; i < s.Length && !IsWhiteSpace(s[i]); i++)
                ;
            return i;
        }

        static int FindNonWhitespace(string s, int start)
        {
            int i = start;
            for (; i < s.Length && IsWhiteSpace(s[i]) && s[i] != '\n' && s[i] != '\r'; i++)
                ;
            return i;
        }

        static int FindNewline(string s, int start)
        {
            int i = start;
            for (; i < s.Length && s[i] != '\n' && s[i] != '\r'; i++)
                ;
            return i;
        }

        static int FindAfterNewline(string s, int start)
        {
            int i = FindNewline(s, start);
            for (; i < s.Length && (s[i] == '\n' || s[i] == '\r'); i++)
                ;

            return i;
        }

        static int FindCommentOrNewline(string s, int start)
        {
            int i = start;
            int nl = FindNewline(s, start);
            int commentIndex = 0;
            var commentTagSize = KCommentStartTag.Length;

            while (i < nl)
            {
                var ss = s[i++];
                var yy = KCommentStartTag[commentIndex++];

                if (ss == yy)
                {
                    if (commentIndex == commentTagSize)
                        return i - commentTagSize;
                }
                else
                    commentIndex = 0;
            }

            return i;
        }

        private string _key;
        private string _value;
        private int _valInt;
        private double _valFloat;
        private readonly List<ValTree> _children = new List<ValTree>();
     
        public List<ValTree> Children => _children;

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
                if (!IsWhiteSpace((data[i])))
                    return i - pos;
            return -1;
        }

        private bool Parse(string data, ref int pos, int lastDepth)
        {
            int nextLineStart = FindAfterNewline(data, pos);
            if (nextLineStart > KMaxFileSize || pos == nextLineStart)
                return false;

            if (FindCommentOrNewline(data, pos) <= FindNonWhitespace(data, pos))
            {
                pos = nextLineStart;
                return Parse(data, ref pos, lastDepth);
            }

            var depth = GetDepth(data, pos);
            if (depth == lastDepth)
            {
                var dataSize = data.Length;
                int startPos = pos + depth;
                if (startPos < nextLineStart)
                {
                    pos = FindWhitespace(data, pos + depth + 1);
                    _key = data.Substring(startPos, pos < dataSize ? pos - startPos : -1);
                }

                if (_key.Length > 0)
                {
                    pos = FindNonWhitespace(data, pos);
                    int end = FindCommentOrNewline(data, pos);
                    if (pos < dataSize && end > pos)
                    {
                        _value = data.Substring(pos, end - pos);
                        SetValInt();
                        SetValFloat();
                    }
                }

                pos = nextLineStart;
                depth = GetDepth(data, pos);
            }

            // parse children
            if (depth > lastDepth)
            {
                bool success = true;
                lastDepth = depth;
                do
                {
                    _children.Add(new ValTree());
                    success = _children.Last().Parse(data, ref pos, depth);
                    if (!success)
                        _children.RemoveAt(_children.Count - 1);

                    depth = GetDepth(data, pos);
                } while (success && depth == lastDepth);
            }

            return !IsNull();
        }

        public ValTree()
        {
            _valInt = 0;
            _valFloat = 0;

            Clear();
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
        }

        public bool IsNull()
        {
            return (_key.Length <= 0 && _value.Length <= 0);
        }

        public string GetKey()
        {
            return _key;
        }

        public string GetValue()
        {
            return _value;
        }

        public int GetInt()
        {
            return _valInt;
        }

        public double GetFloat()
        {
            return _valFloat;
        }

        public void Set(string key, string val)
        {
            _key = key;
            _value = val;
            SetValInt();
            SetValFloat();
        }

        public int Size()
        {
            return _children.Count;
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
            return _children.SingleOrDefault(x => x._key == key);
        }

        public ValTree GetIndex(int index)
        {
            return (index < _children.Count ? _children[index] : blank);
        }

        public void AddChild(ValTree v)
        {
            _children.Add(v);
        }

        public ValTree Query(string query)
        {
            int x = query.IndexOf(".", StringComparison.Ordinal);
            if (x == -1)
            {
                return GetChild(query);
            }

            string k = query.Substring(0, x);
            string v = query.Substring(x + 1);
            return k.Length > 0 ? GetChild(k).Query(v) : Query(v);
        }

        public void AddChild(string query, string value)
        {
            var pos = query.IndexOf(".", StringComparison.Ordinal);
            if (pos == -1)
            {
                ValTree yy = new ValTree(query, value);
                AddChild(yy);
                return;
            }

            string k = query.Substring(0, pos);
            string v = query.Substring(pos + 1);
            if (k.Length <= 0) return;

            ValTree child = GetChild(k);

            if (child == null)
            {
                ValTree xx = new ValTree(k, string.Empty);
                xx.AddChild(v, value);
                AddChild(xx);
            }
            else
            {
                child.AddChild(v, value);
            }
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
            Parse(data, ref pos, -1);

            return true;
        }

        static void InternalLog(ref StringBuilder sb, ValTree inValue, int depth)
        {
            if (inValue.GetKey().Length > 0 || inValue.GetValue().Length > 0)
            {
                for (int i = 0; i < depth; i++)
                {
                    sb.Append("\t");
                }

                sb.AppendLine(inValue.GetKey() + " " + inValue.GetValue());
            }

            for (int i = 0; i < inValue.Size(); i++)
            {
                var child = inValue.GetIndex(i);
                InternalLog(ref sb, child, depth + 1);
            }
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

        public IEnumerator GetEnumerator()
        {
            foreach (ValTree child in _children)
            {
                yield return child;
            }
        }
    }
}
