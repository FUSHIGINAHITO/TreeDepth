using System.Collections.Generic;

public class Graph<T>
{
    public int VertexNum => vertices.Count;

    private LinkedList<Vertex> vertices = new();
    private Queue<Vertex> queue = new();

    public class Vertex
    {
        private LinkedList<Vertex> neighbors = new();
        public T value;
        public bool visited; // 仅外部用
        public bool _visited; // 仅内部用

        public IEnumerable<LinkedListNode<Vertex>> Neighbors
        {
            get
            {
                LinkedListNode<Vertex> cur = neighbors.First;
                while (cur != null)
                {
                    LinkedListNode<Vertex> next = cur.Next;
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
            return neighbors.Contains(u);
        }

        public void AddNeighbor(Vertex u)
        {
            u.neighbors.AddLast(this);
            neighbors.AddLast(u);
        }

        public void DelNeighbor(Vertex u)
        {
            u.neighbors.Remove(this);
            neighbors.Remove(u);
        }

        public void DelNeighbor(LinkedListNode<Vertex> u)
        {
            u.Value.neighbors.Remove(this);
            neighbors.Remove(u);
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

        foreach (var v in u.Neighbors)
        {
            u.DelNeighbor(v);
        }
    }

    public void DelVertex(LinkedListNode<Vertex> u)
    {
        vertices.Remove(u);

        foreach (var v in u.Value.Neighbors)
        {
            u.Value.DelNeighbor(v);
        }
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

            foreach (var v in u.Neighbors)
            {
                if (!v.Value._visited)
                {
                    queue.Enqueue(v.Value);
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
}