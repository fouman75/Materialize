#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace SimpleLightProbePlacer
{
    [RequireComponent(typeof(LightProbeGroup))]
    [AddComponentMenu("Rendering/Light Probe Group Control")]
    public class LightProbeGroupControl : MonoBehaviour
    {
        public float MergeDistance = 0.5f;
        public float PointLightRange = 1;
        public bool UsePointLights;
        public int MergedProbes => _mergedProbes;
        private int _mergedProbes;
#if UNITY_EDITOR
        public LightProbeGroup LightProbeGroup
        {
            get
            {
                if (_lightProbeGroup != null) return _lightProbeGroup;
                return _lightProbeGroup = GetComponent<LightProbeGroup>();
            }
        }


        private LightProbeGroup _lightProbeGroup;

        public void DeleteAll()
        {
            LightProbeGroup.probePositions = null;
            _mergedProbes = 0;
        }

        public void Create()
        {
            DeleteAll();

            var positions = CreatePositions();
            positions.AddRange(CreateAroundPointLights(PointLightRange));
            positions = MergeClosestPositions(positions, MergeDistance, out _mergedProbes);

            ApplyPositions(positions);
        }

        public void Merge()
        {
            if (LightProbeGroup.probePositions == null) return;

            var positions = MergeClosestPositions(LightProbeGroup.probePositions.ToList(), MergeDistance,
                out _mergedProbes);
            positions = positions.Select(x => transform.TransformPoint(x)).ToList();

            ApplyPositions(positions);
        }

        private void ApplyPositions(IEnumerable<Vector3> positions)
        {
            LightProbeGroup.probePositions = positions.Select(x => transform.InverseTransformPoint(x)).ToArray();
        }

        private static List<Vector3> CreatePositions()
        {
            var lightProbeVolumes = FindObjectsOfType<LightProbeVolume>();

            if (lightProbeVolumes.Length == 0) return new List<Vector3>();

            var probes = new List<Vector3>();

            foreach (var t in lightProbeVolumes) probes.AddRange(t.CreatePositions());

            return probes;
        }

        private static IEnumerable<Vector3> CreateAroundPointLights(float range)
        {
            var lights = FindObjectsOfType<Light>().Where(x => x.type == LightType.Point).ToList();

            if (lights.Count == 0) return new List<Vector3>();

            var probes = new List<Vector3>();

            foreach (var t in lights) probes.AddRange(CreatePositionsAround(t.transform, range));

            return probes;
        }

        private static List<Vector3> MergeClosestPositions(List<Vector3> positions, float distance, out int mergedCount)
        {
            if (positions == null)
            {
                mergedCount = 0;
                return new List<Vector3>();
            }

            var exist = positions.Count;
            var done = false;

            while (!done)
            {
                var closest = new Dictionary<Vector3, List<Vector3>>();

                foreach (var t in positions)
                {
                    var points = positions.Where(x => (x - t).magnitude < distance).ToList();
                    if (points.Count > 0 && !closest.ContainsKey(t)) closest.Add(t, points);
                }

                positions.Clear();
                var keys = closest.Keys.ToList();

                foreach (var t in keys)
                {
                    var center = closest[t].Aggregate(Vector3.zero, (result, target) => result + target) /
                                 closest[t].Count;
                    if (!positions.Exists(x => x == center)) positions.Add(center);
                }

                done = positions.Select(x => positions.Where(y => y != x && (y - x).magnitude < distance))
                    .All(x => !x.Any());
            }

            mergedCount = exist - positions.Count;
            return positions;
        }

        public static List<Vector3> CreatePositionsAround(Transform transform, float range)
        {
            Vector3[] corners =
            {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            };

            return corners.Select(x => transform.TransformPoint(x * range)).ToList();
        }
#endif
    }
}