using System.Collections.Generic;

namespace Interactions
{
    public static class InteractObjectFinderManager
    {
        private static readonly List<InteractObjectFinder> _finders = new List<InteractObjectFinder>();

        public static void Add(InteractObjectFinder objectFinder)
        {
            _finders.Add(objectFinder);
        }

        public static void Remove(InteractObjectFinder objectFinder)
        {
            _finders.Remove(objectFinder);
        }

        public static IReadOnlyList<InteractObjectFinder> GetObjectFinders()
        {
            return _finders;
        }
    }
}
