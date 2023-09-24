using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class RelativeCollection<T> : IEnumerable<T> where T : class
    {
        private Dictionary<T, RelativeCollectionNode<T>> Items = new Dictionary<T, RelativeCollectionNode<T>>();
        private RelativeCollectionNode<T> Top;
        private RelativeCollectionNode<T> Bottom;


        public bool TryGetNext(T source, out T item)
        {
            RelativeCollectionNode<T> node;
            if (Items.TryGetValue(source, out node) && node.Next is not null)
            {
                item = node.Next.Value;
                return true;
            }
            item = null;
            return false;
        }

        public bool TryGetPrevious(T source, out T item)
        {
            RelativeCollectionNode<T> node;
            if (Items.TryGetValue(source, out node) && node.Previous is not null)
            {
                item = node.Previous.Value;
                return true;
            }
            item = null;
            return false;
        }

        public void PlaceTop(T item)
        {
            if (Items.ContainsKey(item)) Remove(item);

            var node = new RelativeCollectionNode<T>(item);
            if (Top is null)
            {
                Top = node;
                if (Bottom is null)
                {
                    Bottom = node;
                }
            }
            else
            {
                Top.Next = node;
                node.Previous = Top;
                Top = node;
            }
            Items.Add(item, node);
        }

        public void PlaceBottom(T item)
        {
            if (Items.ContainsKey(item)) Remove(item);

            var node = new RelativeCollectionNode<T>(item);
            if (Bottom is null)
            {
                Bottom = node;
                if (Top is null)
                {
                    Top = node;
                }
            }
            else
            {
                Bottom.Previous = node;
                node.Next = Bottom;
                Bottom = node;
            }
            Items.Add(item, node);
        }

        public void PlaceAbove(T item, T source)
        {
            if (!Items.ContainsKey(source)) throw new ArgumentException("Cannot place above not present item");
            if (Items.ContainsKey(item))
            {
                if (Items[item].Previous == Items[source])
                    return;
                Remove(item);
            }
            var node = new RelativeCollectionNode<T>(item);
            var sourceNode = Items[source];
            if (sourceNode == Top)
            {
                Top = node;
            }
            else
            {
                var topNode = sourceNode.Next;
                node.Next = topNode;
                topNode.Previous = node;
            }
            sourceNode.Next = node;
            node.Previous = sourceNode;
            Items.Add(item, node);
        }

        public void PlaceBelow(T item, T source)
        {
            if (!Items.ContainsKey(source)) throw new ArgumentException("Cannot place below not present item");
            if (Items.ContainsKey(item))
            {
                if (Items[item].Next == Items[source])
                    return;
                Remove(item);
            }
            var node = new RelativeCollectionNode<T>(item);
            var sourceNode = Items[source];
            if (sourceNode == Bottom)
            {
                Bottom = node;
            }
            else
            {
                var bottomNode = sourceNode.Previous;
                node.Previous = bottomNode;
                bottomNode.Next = node;
            }
            sourceNode.Previous = node;
            node.Next = sourceNode;
            Items.Add(item, node);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Items.Count == 0) yield break;
            var node = Bottom;
            while (node is not null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Remove(T item)
        {
            if (!Items.ContainsKey(item)) return;
            var node = Items[item];
            if (node.Next is not null) node.Next.Previous = node.Previous;
            else Top = node.Previous;
            if (node.Previous is not null) node.Previous.Next = node.Next;
            else Bottom = node.Next;
            Items.Remove(item);
        }

        public int Length => Items.Count;

        public IEnumerable<T> Reversed
        {
            get
            {
                if (Items.Count == 0) yield break;
                var node = Top;
                while (node is not null)
                {
                    yield return node.Value;
                    node = node.Previous;
                }
            }
        }

        public bool Contains(T item) => Items.ContainsKey(item);
    }

    public class RelativeCollectionNode<T>
    {
        public readonly T Value;
        public RelativeCollectionNode<T> Previous;
        public RelativeCollectionNode<T> Next;

        public RelativeCollectionNode(T item)
        {
            Value = item;
        }
    }
}
