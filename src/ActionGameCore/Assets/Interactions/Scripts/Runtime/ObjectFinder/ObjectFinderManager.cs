using System.Collections.Generic;

namespace Interactions
{
    public static class ObjectFinderManager
    {
        private static readonly List<ObjectFinder> _finders = new List<ObjectFinder>();

        public static void Add(ObjectFinder objectFinder)
        {
            _finders.Add(objectFinder);
        }

        public static void Remove(ObjectFinder objectFinder)
        {
            _finders.Remove(objectFinder);
        }

        public static IReadOnlyList<ObjectFinder> GetObjectFinders()
        {
            return _finders;
        }
    }
}
