using System;

namespace FlowDef
{
    using System.Collections.Generic;
    using System.Linq;
    using JsCommon;


    public class NodeKey
    {
        const string DELIM = ":";

        public static NodeKey Parse(string s, string delim = DELIM)
            => GenKey(s.Split(new string[] { delim }, StringSplitOptions.RemoveEmptyEntries));

        public static NodeKey GenKey(string[] ss)
        {
            if(ss.Length == 0) { throw new ArgumentException(); }
            if (ss.Length > 1)
            {
                return new NodeKey(GenKey(ss.Take(ss.Length - 1).ToArray()), ss.Last());
            }
            return new NodeKey(ss[0]);
        }


        public Option<NodeKey> ParentKey { get; }
        public string Key { get; }
        public string Name { get; }

        public NodeKey(NodeKey parentKey, string name)
        {
            ParentKey = parentKey.AsSome();
            Name = name;
            Key = parentKey.Key + DELIM + name;
        }
        public NodeKey(string name)
        {
            ParentKey = Option.None<NodeKey>();
            Name = name;
            Key = name;
        }

        public override int GetHashCode() => Key.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (obj == this) { return true; }
            if (!(obj is NodeKey)) { return false; }
            return ((NodeKey)obj).Key.Equals(Key);
        }
    }

    public interface IConnectionDef
    {
        NodeKey Key { get; }
        Option<NodeKey> PrevNode { get; }
    }

    public class Node
    {
        readonly IDictionary<NodeKey, Node> nodeCache;
        readonly List<Node> subNodes = new List<Node>();
        
        public NodeKey Key { get; }
        public Option<Node> Parent
        {
            get => Key.ParentKey.Map(parentKey => nodeCache[parentKey]);
        }
        public IEnumerable<Node> SubNodes
        {
            get => subNodes;
        }

        public Node(NodeKey key, IDictionary<NodeKey, Node> nodeCache)
        {
            Key = key;
            this.nodeCache = nodeCache;
            nodeCache[key] = this;
        }

    }

}
