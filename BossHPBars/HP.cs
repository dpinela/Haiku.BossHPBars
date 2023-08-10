namespace Haiku.BossHPBars
{
    internal class HP
    {
        public Func<int> Current;
        public int Max;

        public HP(Func<int> current, int max)
        {
            Current = current;
            Max = max;
        }
    }
}