using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

partial class Util
{
    public static IEnumerator<float> EBuildNavMesh(this NavMeshSurface surf)
    {
        NavMeshData data = surf.navMeshData;

        if (data == null)
        {
            data = new(surf.agentTypeID)
            {
                name = $"RuntimeNavMesh_{surf.name}"
            };
            surf.navMeshData = data;
            surf.AddData();
        }

        AsyncOperation op = surf.UpdateNavMesh(data);

        while (!op.isDone)
            yield return op.progress;
    }
}