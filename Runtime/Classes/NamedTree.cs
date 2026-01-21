using System;
using System.Collections.Generic;

namespace _UTIL_
{
    public sealed class NamedTree<T>
    {
        public readonly Dictionary<string, T> _leaves = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, NamedTree<T>> _branches = new(StringComparer.OrdinalIgnoreCase);

        //--------------------------------------------------------------------------------------------------------------

        public NamedTree<T> GetOrCreateBranch(string branch) => GetOrCreateBranch(new string[] { branch });
        public NamedTree<T> GetOrCreateBranch(in IEnumerable<string> path, in bool remove_empties = false)
        {
            var node = this;
            foreach (var key in path)
            {
                if (remove_empties)
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                if (!node._branches.TryGetValue(key, out var child))
                {
                    child = new NamedTree<T>();
                    node._branches.Add(key, child);
                }
                node = child;
            }
            return node;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Clear()
        {
            _leaves.Clear();
            _branches.Clear();
        }
    }
}