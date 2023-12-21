using System;
using System.Collections.Generic;

public class Graph<T, S>
{
    public int VertexNum => vertices.Count;
    public bool inPool = true;

    private LinkedList<Vertex> vertices = new();
    private Queue<Vertex> queue = new();

    public class Vertex
    {
        public class Edge
        {
            public Vertex origin;
            public Vertex target;
            public S value;

            private Edge inverseEdge;
            private LinkedListNode<Edge> node;

            public static void Create(Vertex _origin, Vertex _target, S v)
            {
                Edge e = new();
                e.value = v;
                e.origin = _origin;
                e.target = _target;
                e.node = _origin.neighbors.AddLast(e);

                e.inverseEdge = new();
                e.inverseEdge.origin = _target;
                e.inverseEdge.target = _origin;
                e.inverseEdge.inverseEdge = e;
                e.inverseEdge.value = v;
                e.inverseEdge.node = _target.neighbors.AddLast(e.inverseEdge);
            }

            public static void Delete(Edge e)
            {
                e.Delete();
                e.inverseEdge.Delete();
            }

            private void Delete()
            {
                origin.neighbors.Remove(node);
            }
        }


        private LinkedList<Edge> neighbors = new();
        public T value;
        public bool visited; // 仅外部用
        public bool _visited; // 仅内部用

        public IEnumerable<LinkedListNode<Edge>> Neighbors
        {
            get
            {
                var cur = neighbors.First;
                while (cur != null)
                {
                    var next = cur.Next;
                    yield return cur;
                    cur = next;
                }
            }
        }

        public Vertex(T v)
        {
            value = v;
        }

        public bool Adj(Vertex u)
        {
            foreach (var cur in Neighbors)
            {
                if (cur.Value.target == u)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddNeighbor(Vertex u, S edgeValue)
        {
            Edge.Create(this, u, edgeValue);
        }

        public void DelNeighbor(Vertex u)
        {
            foreach (var cur in Neighbors)
            {
                if (cur.Value.target == u)
                {
                    Edge.Delete(cur.Value);
                    break;
                }
            }
        }

        internal void Clear()
        {
            foreach (var cur in Neighbors)
            {
                Edge.Delete(cur.Value);
            }
        }
    }

    public LinkedListNode<Vertex> GetAtIndex(int index)
    {
        int count = 0;
        LinkedListNode<Vertex> cur = vertices.First;
        while (cur != null)
        {
            if (count == index)
            {
                return cur;
            }

            LinkedListNode<Vertex> next = cur.Next;
            count++;
            cur = next;
        }

        return null;
    }

    public void AddVertex(Vertex vertex)
    {
        vertices.AddLast(vertex);
    }

    public void DelVertex(Vertex u)
    {
        vertices.Remove(u);
        u.Clear();
    }

    public void DelVertex(LinkedListNode<Vertex> u)
    {
        vertices.Remove(u);
        u.Value.Clear();
    }

    public IEnumerable<Vertex> BfsEnumerator(Vertex start)
    {
        queue.Clear();
        foreach (var v in vertices)
        {
            v._visited = false;
        }

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vertex u = queue.Dequeue();
            u._visited = true;
            yield return u;

            foreach (var e in u.Neighbors)
            {
                if (!e.Value.target._visited)
                {
                    queue.Enqueue(e.Value.target);
                }
            }
        }
    }

    public void ClearTag()
    {
        foreach (var v in vertices)
        {
            v.visited = false;
        }
    }

    public IEnumerable<LinkedListNode<Vertex>> Vertices
    {
        get
        {
            LinkedListNode<Vertex> cur = vertices.First;
            while (cur != null)
            {
                LinkedListNode<Vertex> next = cur.Next;
                yield return cur;
                cur = next;
            }
        }
    }

    public void Clear()
    {
        vertices.Clear();
        queue.Clear();
    }
}


public class GraphPool<T, S>
{
    private List<Graph<T, S>> graphs = new();
    
    public Graph<T, S> Require()
    {
        foreach (var graph in graphs)
        {
            if (graph.inPool)
            {
                graph.inPool = false;
                return graph;
            }
        }

        Graph<T, S> newGraph = new();
        newGraph.inPool = false;
        graphs.Add(newGraph);
        return newGraph;
    }

    public void Return(Graph<T, S> graph)
    {
        graph.inPool = true;
        graph.Clear();
    }
}