namespace System.Security
{
    /// <summary>
    /// Primitive SecureString implementation for Windows Phone and WinRT platforms for compatibility reasons.
    /// </summary>
    public sealed class SecureString : IDisposable
    {
        private string str;
        private bool isReadOnly;

        public SecureString()
            : this(string.Empty) {}

        public SecureString(string value)
        {
            str = value ?? string.Empty;
        }

        public int Length
        {
            get { return str.Length; }
        }

        public void AppendChar(char c)
        {
            CheckReadOnly();
            str += c;
        }

        public void Clear()
        {
            CheckReadOnly();
            str = string.Empty;
        }

        public SecureString Copy()
        {
            return new SecureString(str);
        }

        public void InsertAt(int index, char c)
        {
            CheckReadOnly();
            if (index < 0 || index > str.Length)
            {
                throw new ArgumentOutOfRangeException("index", "< 0 || > length");
            }

            str = str.Insert(index, c.ToString());
        }

        public bool IsReadOnly()
        {
            return isReadOnly;
        }

        public void MakeReadOnly()
        {
            isReadOnly = true;
        }

        public void RemoveAt(int index)
        {
            CheckReadOnly();
            if (index < 0 || index >= str.Length)
            {
                throw new ArgumentOutOfRangeException("index", "< 0 || > length");
            }

            str = str.Remove(index, 1);
        }

        public void SetAt(int index, char c)
        {
            CheckReadOnly();
            if (index < 0 || index >= str.Length)
            {
                throw new ArgumentOutOfRangeException("index", "< 0 || > length");
            }

            string temp = str.Remove(index, 1);
            str = temp.Insert(index, c.ToString());
        }

        public void Dispose()
        {
            // intentionally left blank
        }

        private void CheckReadOnly()
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException("SecureString is read-only.");
            }
        }
    }
}