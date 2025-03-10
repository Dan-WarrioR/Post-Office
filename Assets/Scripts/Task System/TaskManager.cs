using System;
using System.Linq;
using System.Collections.Generic;
using TaskSystem.NoteBook;
using UnityEngine;
using Zenject;
using UnityModification;

namespace TaskSystem
{
	public class TaskManager : DestructiveBehaviour<TaskManager>
	{
		#region Task System

		[field: Header("Tassk System")]

		public static TaskManager Instance { get; private set; }

		public Task CurrentTask { get; private set; } = null;

		public int TaskCount => _tasks.Count;

		#region Actions

		public event Action OnAddedNewTask;

		public event Action OnTaskCompleted;

		public event Action<Task> OnNewCurrentTaskSet;
		public event Action OnCurrentTaskCompleted;

		#endregion

		[SerializeField] private List<TaskData> _taskDatas = new();

		private Dictionary<int, Task> _tasks = new();

		#endregion

		[Inject] private Tablet _tablet;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				EditorDebug.LogWarning($"{this} Instance already exists!");
			}
			
			_tablet.SubcribeOnTaskManager();

			foreach (var taskData in _taskDatas)
			{
				TryAddNewTask(taskData);
			}

			foreach (var task in _tasks)
			{
				task.Value.OnCompleted += CompleteTask;
			}
		}

		private void Start()
		{
			if (CurrentTask == null && _tasks.Count > 0)
				SetNewCurrentTask(0);
		}

		public bool TryGetTask(int id, out Task task)
			=> _tasks.TryGetValue(id, out task);

		public void SetNewCurrentTask(int index)
		{
			if (index < 0 || index >= _tasks.Count)
			{
				EditorDebug.LogWarning($"Can't set task, as current with index[{index}]");

				return;
			}

			Task task = _tasks.Values.ElementAt(index);

			CurrentTask = task;

			OnNewCurrentTaskSet?.Invoke(task);
		}

		public void SetNewCurrentTask(TaskData taskData)
		{
			Task task = new Task(taskData.Task);

			SetNewCurrentTask(task);
		}

		public void SetNewCurrentTask(Task task)
		{
			if (!TryGetTask(task.ID, out Task _))
			{
				EditorDebug.LogWarning($"You are trying to set task({task.Name}) which doesn't exists in task collection therefore we try to add task automatically");

				if (!TryAddNewTask(task))
				{
					EditorDebug.LogWarning($"Couldn't add this task({task.Name}!)");

					return;
				}
			}

			CurrentTask = task;

			OnNewCurrentTaskSet?.Invoke(task);
		}

		public bool TryAddNewTask(TaskData taskData)
		{
			Task task = new Task(taskData.Task);

			return TryAddNewTask(task);
		}

		public bool TryAddNewTask(Task task)
		{
			if (IsContainsTask(task.ID))
			{
				EditorDebug.LogWarning($"You are trying to add task({task.Name}) which already exists in task collection. We can't add him!");

				return false;
			}

			_tasks.Add(task.ID, task);

			task.OnCompleted += CompleteTask;
			
			OnAddedNewTask?.Invoke();

			if (CurrentTask == null)
				SetNewCurrentTask(task);		

			return true;
		}

		private bool IsContainsTask(int taskId)
			=> _tasks.ContainsKey(taskId);	

		private void CompleteTask(Task completedTask)
		{
			_tasks.Remove(completedTask.ID);

			bool isCompleteTaskIsCurrent = CurrentTask.ID == completedTask.ID;

			if (isCompleteTaskIsCurrent)
				OnCurrentTaskCompleted?.Invoke();

			OnTaskCompleted?.Invoke();

			if (TaskCount > 0 && isCompleteTaskIsCurrent)
				SetNewCurrentTask(0);

			EditorDebug.Log($"Task: {completedTask.Name} has been completed");
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			foreach (var task in _tasks)
			{
				task.Value.OnCompleted -= CompleteTask;
			}
		}
	}
}