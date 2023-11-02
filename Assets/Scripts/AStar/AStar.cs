using System.Collections.Generic;
using UnityEngine;

// @Author Patrik Bergsten
public class Comparator<T> : IComparer<Node> {
    private readonly Dictionary<Node, float> _fCost;

    public Comparator(Dictionary<Node, float> fCost) {
        _fCost = fCost;
    }

    public int Compare(Node x, Node y) {
        float xCost = _fCost[x];
        float yCost = _fCost[y];

        return xCost.CompareTo(yCost);
    }
}

public class AStar {

    /// <summary>
    /// A* Pathfinding.
    /// </summary>
    /// <param name="start">Start Node.</param>
    /// <param name="goal">Goal Node.</param>
    /// <returns>Stack of Nodes. Null if no path exists.</returns>
    /// <remarks>Uses Euclidean Distance heuristic by default.</remarks>>
    public Stack<Node> Path(Node start, Node goal) => Path(start, goal, new Euclidean());
    
    /// <summary>
    /// A* Pathfinding.
    /// </summary>
    /// <param name="start">Start Node.</param>
    /// <param name="goal">Goal Node.</param>
    /// <param name="heuristic">IHeuristic for the cost function between nodes.</param>
    /// <returns>Stack of Nodes. Null if no path exists.</returns>
    public static Stack<Node> Path(Node start, Node goal,  IHeuristic<float> heuristic) {
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>(100);
        Dictionary<Node, float> gScore = new Dictionary<Node, float>(100);
        Dictionary<Node, float> fScore = new Dictionary<Node, float>(100);
        
        Comparator<float> comparator = new Comparator<float>(fScore);
        Heap<Node> openSet = new Heap<Node>(comparator, 100);
        
        gScore.Add(start, 0);
        fScore.Add(start, heuristic.CostFunction(start, goal));
        openSet.Insert(start);
        
        while (!openSet.Empty()) {
            Node current = openSet.DeleteMin(); 
            
            if (current == goal)
                return Path(cameFrom, current);

            int numNeighbours = current.neighbours.Length;

            for (int n = 0; n < numNeighbours; n++) {
                Node neighbour = current.neighbours[n];
                float tentativeGScore = gScore[current] + heuristic.CostFunction(current, neighbour);

                if (!gScore.ContainsKey(neighbour) || tentativeGScore < gScore[neighbour]) {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + heuristic.CostFunction(neighbour, goal);

                    openSet.Insert(neighbour);
                }
            }
        }

        return null;
    }

    private static Stack<Node> Path(Dictionary<Node, Node> cameFrom, Node current) {
        Stack<Node> totalPath = new Stack<Node>();
        totalPath.Push(current);
        
        while (cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            totalPath.Push(current);
        }

        return totalPath;
    }
}

public interface IHeuristic <T>{
    public abstract T CostFunction(Node from, Node to);
}

/// <summary>
/// Shortest Distance.
/// </summary>
public class Euclidean : IHeuristic<float> {
    public float CostFunction(Node from, Node to) {
        return Vector3.Distance(from.position, to.position);
    }
}

/// <summary>
/// Manhattan Distance. Z up.
/// </summary>
public class ManhattanXY : IHeuristic<int> {
    public int CostFunction(Node from, Node to) {
        return Mathf.RoundToInt(Mathf.Abs(to.position.x - from.position.x) + Mathf.Abs(to.position.y - from.position.y));
    }
}

/// <summary>
/// Manhattan Distance. Y up.
/// </summary>
public class ManhattanXZ : IHeuristic<int> {
    public int CostFunction(Node from, Node to) {
        return Mathf.RoundToInt(Mathf.Abs(to.position.x - from.position.x) + Mathf.Abs(to.position.z - from.position.z));
    }
}

/// <summary>
/// Manhattan Distance. 3 Dimensional.
/// </summary>
public class ManhattanXYZ : IHeuristic<int> {
    public int CostFunction(Node from, Node to) {
        return Mathf.RoundToInt(Mathf.Abs(to.position.x - from.position.x) + Mathf.Abs(to.position.y - from.position.y) + Mathf.Abs(to.position.z - from.position.z));
    }
}
