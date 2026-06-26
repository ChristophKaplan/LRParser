using System.Collections.Generic;

namespace LRParser.Parser
{
    public abstract class ArenaTree<T>
    {
        // Arena storage (struct-of-arrays style)
        protected readonly List<T> _data = new();
        protected readonly List<int> _childStart = new();
        protected readonly List<int> _childCount = new();
        protected readonly List<int> _children = new();

        protected int AddNode(T symbol)
        {
            var index = _data.Count;
            _data.Add(symbol);
            _childStart.Add(_children.Count);
            _childCount.Add(0);
            return index;
        }
        
        public T GetSymbol(int nodeId)
        {
            return _data[nodeId];
        }
        
        public void AddChild(int parentId, int childId)
        {
            int insertAt = _childStart[parentId];
            _children.Insert(insertAt, childId);
            _childCount[parentId]++;
            for (int i = parentId + 1; i < _childStart.Count; i++)
            {
                _childStart[i]++;
            }
        }

        public void EvaluateTree(int nodeId)
        {
            var start = _childStart[nodeId];
            var count = _childCount[nodeId];

            for (var i = 0; i < count; i++)
            {
                EvaluateTree(_children[start + i]);
            }

            // Always invoke Semantic, even for childless nodes: an epsilon
            // production reduces to a node with no children but still carries a
            // semantic action that must run. Leaf nodes are filtered out by the
            // concrete implementation (they have no action).
            Semantic(nodeId, start, count);
        }
        
        protected abstract void Semantic(int nodeId, int start, int count);
    }
}