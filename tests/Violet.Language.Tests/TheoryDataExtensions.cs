namespace Xunit;

public static class TheoryDataExtensions
{
    public static TheoryData<T1, T2> ToTheoryData<T1, T2>(this IEnumerable<(T1, T2)> tuples)
    {
        var data = new TheoryData<T1, T2>();
        foreach (var (t1, t2) in tuples)
        {
            data.Add(t1, t2);
        }

        return data;
    }

    public static TheoryData<T1, T2, T3> ToTheoryData<T1, T2, T3>(this IEnumerable<(T1, T2, T3)> tuples)
    {
        var data = new TheoryData<T1, T2, T3>();
        foreach (var (t1, t2, t3) in tuples)
        {
            data.Add(t1, t2, t3);
        }

        return data;
    }

    public static TheoryData<T1, T2, T3, T4> ToTheoryData<T1, T2, T3, T4>(this IEnumerable<(T1, T2, T3, T4)> tuples)
    {
        var data = new TheoryData<T1, T2, T3, T4>();
        foreach (var (t1, t2, t3, t4) in tuples)
        {
            data.Add(t1, t2, t3, t4);
        }

        return data;
    }

    public static TheoryData<T1, T2, T3, T4, T5> ToTheoryData<T1, T2, T3, T4, T5>(this IEnumerable<(T1, T2, T3, T4, T5)> tuples)
    {
        var data = new TheoryData<T1, T2, T3, T4, T5>();
        foreach (var (t1, t2, t3, t4, t5) in tuples)
        {
            data.Add(t1, t2, t3, t4, t5);
        }

        return data;
    }

    public static TheoryData<T1, T2, T3, T4, T5, T6> ToTheoryData<T1, T2, T3, T4, T5, T6>(this IEnumerable<(T1, T2, T3, T4, T5, T6)> tuples)
    {
        var data = new TheoryData<T1, T2, T3, T4, T5, T6>();
        foreach (var (t1, t2, t3, t4, t5, t6) in tuples)
        {
            data.Add(t1, t2, t3, t4, t5, t6);
        }

        return data;
    }
}
