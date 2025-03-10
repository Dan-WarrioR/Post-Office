using System;
using TaskSystem.NoteBook;
using UnityEngine;
using UnityModification;

namespace TaskSystem
{
	[CreateAssetMenu]
	public class TaskData : ScriptableObject
	{
		[field: SerializeField] public Task Task { get; private set; }
	}

	[Serializable]
	public class Task
	{
		public bool IsCompleted { get; private set; } = false;

		[field: SerializeField] public int ID { get; private set; }

		[field: SerializeField, TextArea(2, 2)] public string Name { get; private set; }

		[field: SerializeField, TextArea(5, 5)] public string Description { get; private set; }

		public event Action<Task> OnCompleted;

		public Task(int id, string name, string description)
		{
			ID = id;

			Name = name;

			Description = description;
		}

		public Task(Task task)
		{
			ID = task.ID;

			Name = task.Name;

			Description = task.Description;
		}

		public void Complete()
		{
			if (IsCompleted)
				EditorDebug.LogWarning($"Task is already completed but you still trying to complete him");

			IsCompleted = true;

			OnCompleted?.Invoke(this);
		}
	}
}