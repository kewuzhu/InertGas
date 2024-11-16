namespace InertGas.Common.Utility
{
    public class QueueWithRangeSupport<T> : Queue<T>
    {
        public IEnumerable<T> DequeueMultiple(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Count > 0)
                {
                    yield return Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        public void EnqueueMultiple(params T[] elements)
        {
            foreach (var element in elements)
            {
                Enqueue(element);
            }
        }

    }
}
