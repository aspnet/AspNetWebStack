// No ICloneable interface in .NET Standard 1.3.

namespace System
{
    internal interface ICloneable
    {
        object Clone();
    }
}
