using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.Layout
{
    /// <summary>
    /// Commit graph layout working on snapshots.
    /// Uses edges to infer parent-child relationships.
    /// </summary>
    internal sealed class CommitGraphLayout : ILayoutEngine
    {
        private const float RootRadius = 0f;
        private const float BranchRadius = 200f;
        private const float CommitOffset = 40f;
        private const float FileOffset = 20f;

        private ISceneSnapshot? _snapshot;

        public void SetSceneGraph(ISceneGraph sceneGraph)
        {
            // not used in snapshot-based layout
        }

        public void SetSnapshot(ISceneSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public void Step(IReadOnlyDictionary<string, SceneNode> nodes2, float deltaSeconds)
        {
            if (_snapshot == null || _snapshot.Nodes.Count == 0)
                return;

          //  var nodes = _snapshot.Nodes;
            var nodes = nodes2.Values.ToList();
            var rootNodes = nodes.Where(n => n.Kind == NodeKind.Root).ToList();
            var branchNodes = nodes.Where(n => n.Kind == NodeKind.Branch).ToList();
            var fileNodes = nodes.Where(n => n.Kind == NodeKind.File).ToList();

            // --- 1. Root w centrum ---
            foreach (var root in rootNodes)
            {
                root.Position = new Vec2(RootRadius, RootRadius);
            }

            // --- 2. Branchy wokół root ---
            LayoutBranches(branchNodes, rootNodes);

            // --- 3. Commity według parentów z edge’ów ---
            LayoutCommits(fileNodes, branchNodes.Concat(rootNodes).ToList());
        }

        private void LayoutBranches(List<SceneNode> branches, List<SceneNode> roots)
        {
            if (branches.Count == 0 || roots.Count == 0)
                return;

            int sectorCount = branches.Count;
            float angleStep = MathF.Tau / sectorCount;
            float angle = 0f;

            foreach (var branch in branches.OrderBy(b => b.Id))
            {
                var root = roots[Random.Shared.Next(roots.Count)];

                branch.Position = new Vec2(
                    root.Position.X + MathF.Cos(angle) * BranchRadius + RandomOffset(20f),
                    root.Position.Y + MathF.Sin(angle) * BranchRadius + RandomOffset(20f)
                );

                angle += angleStep;
            }
        }

        private void LayoutCommits(List<SceneNode> commits, List<SceneNode> potentialParents)
        {
            if (_snapshot == null || potentialParents.Count == 0)
                return;

            var parentMap = new Dictionary<string, List<Vec2>>();

            foreach (var edge in _snapshot.EdgesOfKind(EdgeKind.Commit))
            {
                if (!potentialParents.Any(p => p.Id == edge.FromId))
                    continue;

                if (!parentMap.TryGetValue(edge.ToId, out var list))
                {
                    list = new List<Vec2>();
                    parentMap[edge.ToId] = list;
                }

                var parent = potentialParents.FirstOrDefault(p => p.Id == edge.FromId);
                if (parent != null)
                    list.Add(parent.Position);
            }

            foreach (var commit in commits)
            {
                Vec2 parentPos;

                if (!parentMap.TryGetValue(commit.Id, out var parents) || parents.Count == 0)
                {
                    var fallback = potentialParents[Random.Shared.Next(potentialParents.Count)];
                    parentPos = fallback.Position;
                }
                else
                {
                    parentPos = new Vec2(
                        parents.Average(p => p.X),
                        parents.Average(p => p.Y)
                    );
                }

                commit.Position = parentPos + new Vec2(
                    RandomOffset(CommitOffset),
                    RandomOffset(CommitOffset)
                );
            }
        }

        private static float RandomOffset(float magnitude)
        {
            return ((float)Random.Shared.NextDouble() - 0.5f) * magnitude;
        }
    }
}