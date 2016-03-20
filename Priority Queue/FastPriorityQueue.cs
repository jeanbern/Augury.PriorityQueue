using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Priority_Queue
{
    /// <summary>
    /// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    /// See https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started for more information
    /// </summary>
    internal sealed class FastPriorityQueue : IEnumerable<FastPriorityQueueNode>
    {
        private int _numNodes;
        private readonly FastPriorityQueueNode[] _nodes;

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
        public FastPriorityQueue(int maxNodes)
        {
            _nodes = new FastPriorityQueueNode[maxNodes + 1];
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// If the queue is full, the result is undefined.
        /// If the node is already enqueued, the result is undefined.
        /// O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(string word, double priority)
        {
            var node = new FastPriorityQueueNode { Priority = priority, Word = word };
            _numNodes++;
            _nodes[_numNodes] = node;
            node.QueueIndex = _numNodes;
            CascadeUp(_nodes[_numNodes]);
        }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// </summary>
        public FastPriorityQueueNode Dequeue()
        {
            var node = _nodes[1];
            //If the node is already the last node, we can remove it immediately
            if (node.QueueIndex == _numNodes)
            {
                _nodes[_numNodes] = null;
                _numNodes--;
                return node;
            }

            //Swap the node with the last node
            var formerLastNode = _nodes[_numNodes];
            Swap(node, formerLastNode);
            _nodes[_numNodes] = null;
            _numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            var parentIndex = formerLastNode.QueueIndex / 2;
            var parentNode = _nodes[parentIndex];

            if (parentIndex > 0 && formerLastNode.Priority < parentNode.Priority)
            {
                CascadeUp(formerLastNode);
            }
            else
            {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                CascadeDown(formerLastNode);
            }

            return node;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// If the queue is empty, behavior is undefined.
        /// O(1)
        /// </summary>
        public FastPriorityQueueNode First
        {
            get
            {
                return _nodes[1];
            }
        }

        public IEnumerator<FastPriorityQueueNode> GetEnumerator()
        {
            //Don't just return the underlying array, some slots might be empty.
            for (var i = 1; i <= _numNodes; i++)
            {
                yield return _nodes[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(FastPriorityQueueNode node1, FastPriorityQueueNode node2)
        {
            //Swap the nodes
            _nodes[node1.QueueIndex] = node2;
            _nodes[node2.QueueIndex] = node1;

            //Swap their indicies
            var temp = node1.QueueIndex;
            node1.QueueIndex = node2.QueueIndex;
            node2.QueueIndex = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        private void CascadeUp(FastPriorityQueueNode node)
        {
            //aka Heapify-up
            var parent = node.QueueIndex / 2;
            while (parent >= 1)
            {
                var parentNode = _nodes[parent];
                if (parentNode.Priority < node.Priority)
                {
                    break;
                }

                //Node has lower priority value, so move it up the heap
                Swap(node, parentNode); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

                parent = node.QueueIndex / 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeDown(FastPriorityQueueNode node)
        {
            //aka Heapify-down
            var finalQueueIndex = node.QueueIndex;
            while (true)
            {
                var newParent = node;
                var childLeftIndex = 2 * finalQueueIndex;

                //Check if the left-child is higher-priority than the current node
                if (childLeftIndex > _numNodes)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != node twice
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }

                var childLeft = _nodes[childLeftIndex];
                if (childLeft.Priority < newParent.Priority)
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current node or the left child
                var childRightIndex = childLeftIndex + 1;
                if (childRightIndex <= _numNodes)
                {
                    var childRight = _nodes[childRightIndex];
                    if (childRight.Priority < newParent.Priority)
                    {
                        newParent = childRight;
                    }
                }

                //If either of the children has higher (smaller) priority, swap and continue cascading
                if (newParent != node)
                {
                    //Move new parent to its new index.  node will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    _nodes[finalQueueIndex] = newParent;

                    var temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    //See note above
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }
        #endregion private methods
    }
}