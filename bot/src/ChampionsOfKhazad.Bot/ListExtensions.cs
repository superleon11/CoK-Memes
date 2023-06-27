namespace ChampionsOfKhazad.Bot;

public static class ListExtensions
{
    public static T PickRandom<T>(this IList<T> source) => RandomUtils.PickRandom(source);
}
