using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BluePrint.Compiler.Parser;

public ref struct TextStream(ReadOnlySpan<char> buffer)
{
    private ref char _current = ref MemoryMarshal.GetReference(buffer);
    private readonly ref char _end = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), buffer.Length);

    public char? Peek(int offset = 0)
    {
        ref var c = ref Unsafe.Add(ref _current, offset);
        if (Unsafe.IsAddressGreaterThanOrEqualTo(ref c, ref _end))
            return null;
        return c;
    }

    public void Advance(int n = 1)
        => _current = ref Unsafe.Add(ref _current, n);

    public bool TryAdvance(char c)
    {
        if (Peek() != c)
            return false;

        Advance(n: 1);
        return true;
    }

    public char? Pop()
    {
        var c = Peek();
        if (c != null)
            Advance(n: 1);
        return c;
    }
}
