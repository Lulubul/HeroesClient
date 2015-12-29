using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathFinding
{
    public static class PathFind
    {
        public static Path<Node> FindPath<Node>(
            Node start,
            Node destination,
            Func<Node, Node, double> distance,
            Func<Node, double> estimate)
            where Node : IHasNeighbours<Node>
        {
            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));

            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();

                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;

                closed.Add(path.LastStep);
                foreach (var n in path.LastStep.Neighbours)
                {
                    var d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }

            return null;
        }

        public static List<List<Node>> MovementRange<Node>(Node start, int movement)
            where Node : IHasNeighbours<Node>
        {
            var visited = new List<Node>();
            var fringes = new List<List<Node>> {new List<Node>()};
            fringes[0].Add(start);
            visited.Add(start);

            for (var k = 1; k <= movement; k++)
            {
                fringes.Add(new List<Node>());
                fringes[k] = new List<Node>();
                foreach (var hex in fringes[k - 1])
                {
                    foreach (var neighbor in hex.Neighbours)
                    {
                        if (visited.Contains(neighbor)) continue;
                        visited.Add(neighbor);
                        fringes[k].Add(neighbor);
                    }
                }
            }

            return fringes;
        }

    }
}
