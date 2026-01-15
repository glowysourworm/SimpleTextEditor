namespace SimpleTextEditor.Extension
{
    public static class CollectionExtension
    {
        /// <summary>
        /// Searches collection for indices of item instances, based on Equality via Equals
        /// </summary>
        public static IEnumerable<int> Search<T>(this IEnumerable<T> collection, T searchItem)
        {
            var result = new List<int>();
            var index = 0;

            foreach (var item in collection)
            {
                if (item.Equals(searchItem))
                    result.Add(index);

                index++;
            }

            return result;
        }
    }
}
