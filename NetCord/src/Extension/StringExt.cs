namespace src.Extension;

public static class StringExtension
{
    extension(string str)
    {
        public string GetFileName()
            => Path.GetFileName(str);

        public u64 ParseU64()
            => u64.Parse(str);
        public i64 ParseI64()
            => i64.Parse(str);
        public u32 ParseU32()
            => u32.Parse(str);

        public bool TryParseU64(out u64 value)
            => u64.TryParse(str, out value);
        public bool TryParseI64(out i64 value)
            => i64.TryParse(str, out value);
        public bool TryParseU32(out u32 value)
            => u32.TryParse(str, out value);
    }
}
