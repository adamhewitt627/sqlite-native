namespace SqliteNative
{
    public interface IBindings
    {
        bool Clear();
        bool SetBlob(int index, byte[] value);
        bool SetText(int index, string value);
        bool SetInt64(int index, long value);
        bool SetDouble(int index, double value);
        bool SetNull(int index);

        int Count { get; }
        int IndexOf(string name);
        string NameOf(int index);
    }
}
