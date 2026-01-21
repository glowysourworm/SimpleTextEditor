using SimpleTextEditor.Model;

namespace SimpleTextEditor.Collection
{
    public class RopeNode
    {
        // This parameter is a major performance constraint! Play with this once the tree is built
        // to see optimum setting. Probably want to split before iterating a string takes too much
        // time! Also, would want to be able to concat characters while typing before worrying about
        // array handling! (see TextString)
        //
        public const int SplitSize = 10;      // Smaller strings are not split

        RopeNode? _left;
        RopeNode? _right;

        TextEditorString _content;

        public RopeNode? Left { get { return _left; } }
        public RopeNode? Right { get { return _right; } }
        public int Height
        {
            // This is the "Height" of the current node. The left or right may extend downward. So, this
            // is a recursive call to get the height of this node.
            //
            get
            {
                return 1 + Math.Max(_left?.Height ?? 0, _right?.Height ?? 0);
            }
        }

        public TextEditorString Content { get { return _content; } }

        public int Balance
        {
            get { return _right?.Height ?? 0 - _left?.Height ?? 0; }
        }

        public RopeNode(TextEditorString content)
        {
            if (content.Length == 0)
                throw new ArgumentException("RopeNode must have content of non-zero length");

            ReInitialize(content);
            ReBalance();
        }

        private void ReInitialize(TextEditorString content)
        {
            _content = content;

            // Procedure
            //
            // 0) Locate white space(s)
            // 1) Split on white space closest to the middle (only split if minimum size is reached)
            //

            // >= Split Size
            //
            if (content.Length >= SplitSize)
            {
                var whiteSpaces = content.GetWhiteSpaces(true);
                var middleSpace = whiteSpaces.MinBy(x => Math.Abs(x.Index - (content.Length / 2.0)));
                var middleIndex = -1;

                // No White Space
                if (middleSpace == null)
                {
                    middleIndex = (int)Math.Floor(content.Length / 2.0);
                }
                else
                {
                    middleIndex = middleSpace.Index;
                }

                if (middleIndex == -1)
                    throw new Exception("Invalid content for RopeNode. No middle index found for the string " + content);

                var contentParts = content.Split(middleIndex, true);

                if (_left != null)
                {
                    // Dispose, or whatever needs to "unload node" (should NOT be anything here)
                }
                if (_right != null)
                {
                    // Dispose, or whatever needs to "unload node" (should NOT be anything here)
                }

                // This will re-create the left / right nodes. The content text is split at this level; and
                // all sub-nodes will re-initialize recursively.
                //
                _left = new RopeNode(new TextEditorString(contentParts[0]));
                _right = new RopeNode(new TextEditorString(contentParts[1]));
            }

            // < Split Size
            //
            else
            {
                _left = null;
                _right = null;

                // Nothing to do
            }
        }

        /// <summary>
        /// Sets the left node, adjusts content, but does NOT RE-INITIALIZE THE NODE!
        /// </summary>
        protected void SetLeft(RopeNode left)
        {
            _left = left;
            _content = _right != null ? TextEditorString.From(left.Content, _right.Content) : TextEditorString.From(left.Content);
        }

        /// <summary>
        /// Sets the right node, adjusts content, but does NOT RE-INITIALIZE THE NODE!
        /// </summary>
        protected void SetRight(RopeNode right)
        {
            _right = right;
            _content = _left != null ? TextEditorString.From(_left.Content, right.Content) : TextEditorString.From(right.Content);
        }

        private void ReBalance()
        {
            // Leaf Nodes are always balanced / Non-Split nodes aren't needed to balance
            //
            if (this.Height <= 1)
                return;

            // AVL Balancing:  There may be multiple levels that have developed since we last balanced the tree. So, this must be
            // a while loop.
            //
            while (Math.Abs(this.Balance) > 1)
            {
                if (this.Balance > 1)
                {
                    if (_right.Balance < 0)
                    {
                        _right.RotateRight();
                    }

                    this.RotateLeft();

                    // If this node was unbalanced by more than 2, we've shifted some of
                    // the weight to the left node; so rebalance the left node
                    //
                    _left.ReBalance();
                }

                else if (this.Balance < -1)
                {
                    if (_left.Balance > 0)
                    {
                        _left.RotateLeft();
                    }

                    this.RotateRight();

                    // If this node was unbalanced by more than 2, we've shifted some of the
                    // weight to the right node; so rebalance the right node.
                    _right.ReBalance();
                }
            }
        }

        void RotateLeft()
        {
            if (_left == null || _right == null)
                throw new Exception("Trying to rotate a tree with improper configuration");

            /* Rotate tree to the left
			 * 
			 *       this               this
			 *       /  \               /  \
			 *      A   right   ===>  left  C
			 *           / \          / \
			 *          B   C        A   B
			 */

            RopeNode? a = _left;
            RopeNode? b = _right?.Left;
            RopeNode? c = _right?.Right;

            if (a == null || b == null || c == null)
                throw new Exception("Trying to rotate a tree with improper configuration");

            // This may leave the nodes in a state that needs to be re-initialized (merged, or split)
            _left.SetLeft(a);
            _left.SetRight(b);
            _right = c;

            // CONTENT! (These are set in the Set methods above, but the nodes have to be re-balanced based on string lengths)
            _left.ReInitialize(_left.Content);
            _right.ReInitialize(_right.Content);

            ReInitialize(TextEditorString.From(_left.Content, _right.Content));
        }

        void RotateRight()
        {
            if (_left == null || _right == null)
                throw new Exception("Trying to rotate a tree with improper configuration");

            /* Rotate tree to the right
			 * 
			 *       this             this
			 *       /  \             /  \
			 *     left  C   ===>    A  right
			 *     / \                   /  \
			 *    A   B                 B    C
			 */

            RopeNode? a = _left.Left;
            RopeNode? b = _left.Right;
            RopeNode? c = _right;

            if (a == null || b == null || c == null)
                throw new Exception("Trying to rotate a tree with improper configuration");

            // This may leave the nodes in a state that needs to be re-initialized (merged, or split)
            _right.SetLeft(b);
            _right.SetRight(c);
            _left = a;

            // CONTENT! (These are set in the Set methods above, but the nodes have to be re-balanced based on string lengths)
            _left.ReInitialize(_left.Content);
            _right.ReInitialize(_right.Content);

            ReInitialize(TextEditorString.From(_left.Content, _right.Content));
        }

        /// <summary>
        /// Appends text to the right-most node of this sub-tree
        /// </summary>
        public void Append(TextEditorString content)
        {
            // Procedure
            // 
            // 1) Recursively append to right-most node(s)
            // 2) Append text:
            //      - Node splits at the SplitSize, otherwise it remains a leaf
            //

            if (content.Length == 0)
                throw new ArgumentException("RopeNode must have non-zero length");

            // -> Right
            if (_right != null)
            {
                // Adjust index for right branch
                _right.Append(content);
                _content.Concat(content.Get());                     // Also, append it on our current node
            }

            // Leaf
            else
            {
                // >= Split Size
                if (_content.Length + content.Length >= SplitSize)
                {
                    // Append (then pass into re-initialize)
                    _content.Concat(content.Get());

                    // -> Rebalance() (resets root index)
                    ReInitialize(_content);
                }

                // < Split Size
                else
                {
                    // Nothing to change for this root index
                    _content.Concat(content.Get());
                }
            }

            // -> Height Check -> (Yes / No) -> ReBalance
            ReBalance();
        }

        /// <summary>
        /// Inserts text in the node sub-tree
        /// </summary>
        public void Insert(TextEditorString content, int offset)
        {
            // Procedure
            // 
            // 1) Recursively locate the node for insertion
            //      - Leaf Nodes:  The offset will / must fall within our index range
            //      - Upper Nodes: The offset will be adjusted down the tree to the leaf
            //
            // 2) Insert text:
            //      - Node splits at the SplitSize, otherwise it remains a leaf
            //

            if (content.Length == 0)
                throw new ArgumentException("RopeNode must have non-zero length");

            if (offset < 0 || offset >= _content.Length)
                throw new ArgumentOutOfRangeException();

            // -> Left
            if (_left != null && offset < _left.Content.Length)
            {
                _left.Insert(content, offset);
                _content.Insert(content.Get(), offset);                                                         // Also, insert it on our current node
            }

            // -> Right
            else if (_right != null && offset < _right.Content.Length)
            {
                // Adjust index for right branch
                _right.Insert(content, offset - _left.Content.Length);
                _content.Insert(content.Get(), offset);                                                         // Also, insert it on our current node
            }

            // Leaf
            else
            {
                // >= Split Size
                if (_content.Length + content.Length >= SplitSize)
                {
                    // Insert (then pass into re-initialize)
                    _content.Insert(content.Get(), offset);

                    // -> Rebalance() (resets root index)
                    ReInitialize(_content);
                }

                // < Split Size
                else
                {
                    // Nothing to change for this root index
                    _content.Insert(content.Get(), offset);
                }
            }

            // -> Height Check -> (Yes / No) -> ReBalance
            ReBalance();
        }

        /// <summary>
        /// Removes range of characters from this node, which manages sub-nodes for removing portions of the range. Returns
        /// true if the node needs to be removed. This would indicate that the node is a leaf node and that it has been
        /// reduced to zero size.
        /// </summary>
        public bool Remove(int offset, int count)
        {
            if (count == 0)
                throw new ArgumentException("Must remove at least 1 character using RopeNode.Remove");

            // Procedure:  Going down, perform removal, then adjust indices for branch removals
            //
            // 0) Get Overlaps (using the IndexRange)
            // 1) Navigate Left / Right (recursively)
            // 2) Find Overlapping Indices
            // 3) Rebalance
            //

            // Get Overlap (for this node)
            if (offset < 0 || offset >= _content.Length)
                throw new ArgumentOutOfRangeException("RopeNode remove offset is out of range");

            var range = IndexRange.FromStartCount(offset, count);
            var leftRange = _left == null ? null : IndexRange.FromStartCount(0, _left.Content.Length);
            var rightRange = _right == null ? null : IndexRange.FromStartCount(0, _right.Content.Length);           // Right Index Space

            var leftOverlap = leftRange == null ? null : range.GetOverlap(leftRange);
            var rightOverlap = rightRange == null ? null : range.GetOverlap(rightRange.Add(_left.Content.Length));  // Adjusted Index Space

            var reinitializeRequired = false;

            // -> Left
            if (leftOverlap != null)
            {
                // Remove Left Portion
                reinitializeRequired = _left.Remove(leftOverlap.StartIndex, leftOverlap.Length);
            }

            // -> Right (Add index offset for the right to match the input range)
            if (rightOverlap != null)
            {
                var adjustedOverlap = rightOverlap.Subtract(_left.Content.Length);

                reinitializeRequired = _right.Remove(adjustedOverlap.StartIndex, adjustedOverlap.Length);
            }

            // Leaf:  This remove will happen regardless. The left or right will be tallied here to see
            //        whether this node should be re-initialized

            // Split will be removed
            reinitializeRequired |= (_left?.Content?.Length + _right?.Content?.Length < SplitSize);

            // Finally, remove from our current node
            _content.Remove(offset, count);

            // Re-Initialize
            if (reinitializeRequired)
                ReInitialize(_content);

            ReBalance();

            return _content.Length == 0;
        }

        public override string ToString()
        {
            return string.Format("Balance={0} Height={1} Content={2} Left={3} Right={4}",
                                  this.Balance, this.Height, this.Content, this.Left.Content, this.Right.Content);
        }
    }
}
