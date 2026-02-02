using System;
using System.Collections.Generic;
using UnityEngine;

public static class LoadingProgress
{
    private class TaskState
    {
        public float Weight;
        public float Progress;
    }

    private static readonly Dictionary<int, TaskState> Tasks = new Dictionary<int, TaskState>();
    private static int _nextId = 1;
    private static bool _active;
    private static float _progress;

    public static event Action<float, bool> ProgressChanged;

    public static bool IsActive => _active;
    public static float Progress => _progress;

    public static void BeginSession()
    {
        _active = true;
        Tasks.Clear();
        _progress = 0f;
        RaiseChanged();
    }

    public static void EndSession()
    {
        if (!_active)
        {
            return;
        }

        _active = false;
        _progress = 1f;
        RaiseChanged();
    }

    public static int BeginTask(float weight = 1f)
    {
        if (!_active)
        {
            BeginSession();
        }

        int id = _nextId++;
        Tasks[id] = new TaskState() { Weight = Mathf.Max(0.0001f, weight), Progress = 0f };
        Recalculate();
        return id;
    }

    public static void Report(int taskId, float progress)
    {
        if (!Tasks.TryGetValue(taskId, out TaskState task))
        {
            return;
        }

        task.Progress = Mathf.Clamp01(progress);
        Recalculate();
    }

    public static void Complete(int taskId)
    {
        if (!Tasks.TryGetValue(taskId, out TaskState task))
        {
            return;
        }

        task.Progress = 1f;
        Recalculate();
    }

    public static void Clear()
    {
        Tasks.Clear();
        _progress = 0f;
        RaiseChanged();
    }

    private static void Recalculate()
    {
        if (Tasks.Count == 0)
        {
            _progress = 0f;
            RaiseChanged();
            return;
        }

        float totalWeight = 0f;
        float weighted = 0f;
        foreach (TaskState task in Tasks.Values)
        {
            totalWeight += task.Weight;
            weighted += task.Weight * task.Progress;
        }

        _progress = totalWeight > 0f ? weighted / totalWeight : 0f;

        if (AllTasksComplete())
        {
            EndSession();
            return;
        }

        RaiseChanged();
    }

    private static bool AllTasksComplete()
    {
        if (Tasks.Count == 0)
        {
            return false;
        }

        foreach (TaskState task in Tasks.Values)
        {
            if (task.Progress < 1f)
            {
                return false;
            }
        }

        return true;
    }

    private static void RaiseChanged()
    {
        ProgressChanged?.Invoke(_progress, _active);
    }
}
