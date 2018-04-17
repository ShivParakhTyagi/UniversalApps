using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Universal.Edge.Models
{
    public class NodeItem
    {
        public string Text { get; set; }
        public int Index { get; set; }

        public void Close()
        {
            OnClosed(this);
        }

        public event Action<NodeItem> Closed;
        public event Action<NodeItem> Added;

        public NodeItem AddItem()
        {
            var node = new NodeItem()
            {
                Text = Text,
                Index = Index + 1
            };

            Closed += node.OnClosed;
            OnAdded(node);
            return node;
        }
        
        protected virtual void OnAdded(NodeItem obj)
        {
            Added?.Invoke(obj);
        }

        protected virtual void OnClosed(NodeItem obj)
        {
            Closed?.Invoke(obj);
        }
    }
}