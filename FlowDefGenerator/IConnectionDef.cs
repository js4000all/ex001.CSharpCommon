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
            => By(s.Split(new string[] { delim }, StringSplitOptions.RemoveEmptyEntries));

        public static NodeKey By(string[] ss) => By(ss, ss.Length);
        static NodeKey By(string[] ss, int len)
        {
            if(ss.Length == 0) { throw new ArgumentException(); }
            if (len > 1)
            {
                return new NodeKey(By(ss, len-1), ss[len-1]);
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

    /// <summary>
    /// 以下の関係図のノード。
    /// ・親子による縦の関係
    /// ・先行・後続による横の関係
    /// </summary>
    public class Node
    {
        readonly IDictionary<NodeKey, Node> nodeCache;
        readonly List<Node> subNodes = new List<Node>();
        readonly List<Node> prevNodes = new List<Node>();
        readonly List<Node> nextNodes = new List<Node>();
        
        public NodeKey Key { get; }
        public Option<Node> Parent { get; }
        public IEnumerable<Node> SubNodes
        {
            get => subNodes;
        }

        public Node(NodeKey key, IDictionary<NodeKey, Node> nodeCache)
        {
            Key = key;
            this.nodeCache = nodeCache;
            nodeCache[key] = this;
            Parent = key.ParentKey
                .Map(parentKey => nodeCache.Find(parentKey, k => new Node(k, nodeCache)))
                .Do(parent => parent.subNodes.Add(this));
        }

        public uint Depth
        {
            get => (prevNodes.Count() == 0) ? 0 : prevNodes.Select(node => node.Depth).Max() + 1;
        }
        /// <summary>
        /// このノードを先頭とする後続ノードの高さ
        /// </summary>
        public uint FollowTreeHeight
        {
            get
            {
                // TODO: 配置位置をずらす規則があったはず。この計算だと上から詰める形になる
                var nextFamilyNode = nextNodes.Where(node => node.Parent == Parent);
                return (nextFamilyNode.Count() == 0) ? 1 : (uint)nextFamilyNode.Select(node => node.FollowTreeHeight).Sum(v => v);
            }
        }
    }

}
