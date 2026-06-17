using System;

namespace src.Extension;

public static class RandomExt
{
    extension(Random random)
    {
        public u32 NextRgb()
            => (u32)(((u8)random.Next() << 16) | ((u8)random.Next() << 8) | (u8)random.Next());
    }
}
