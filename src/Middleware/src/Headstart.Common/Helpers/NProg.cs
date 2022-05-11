using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;

	namespace NProg
	{
		public class Tracker
		{
			private long _startTime = DateTime.UtcNow.Ticks;
			private long _endTime;
			private int _total;
			private int _started;
			private int _succeeded;
			private int _failed;

			private List<ProgressAction> _actions = new List<ProgressAction>();
			private List<Task> _tasks = new List<Task>();
			private List<TimerAction> _timers = new List<TimerAction>();

			public Tracker() { }
			public Tracker(int itemCount) => _total = itemCount;

			public void Start()
			{
				_startTime = DateTime.UtcNow.Ticks;
				_timers.ForEach(t => t.Start(this));
			}

			public void Stop()
			{
				_endTime = DateTime.UtcNow.Ticks;
				_timers.ForEach(t => t.Stop());
			}

			public void Every(Trigger trigger, Action<Progress> action) => _actions.Add(new ProgressAction { Trigger = trigger, Action = action, Recurring = true });
			public void On(Trigger trigger, Action<Progress> action) => _actions.Add(new ProgressAction { Trigger = trigger, Action = action, Recurring = false });
			public void Every(TimeSpan interval, Action<Progress> action) => _timers.Add(new TimerAction { Interval = interval, Action = action });
			public void OnComplete(Action<Progress> action) => On(_total.ItemsDone(), action);
			public void Now(Action<Progress> action) => action(GetProgress());

			public void Every(Trigger trigger, Func<Progress, Task> action) => _actions.Add(new ProgressAction { Trigger = trigger, AsyncAction = action, Recurring = true });
			public void On(Trigger trigger, Func<Progress, Task> action) => _actions.Add(new ProgressAction { Trigger = trigger, AsyncAction = action, Recurring = false });
			public void Every(TimeSpan interval, Func<Progress, Task> action) => _timers.Add(new TimerAction { Interval = interval, AsyncAction = action });
			public void OnComplete(Func<Progress, Task> action) => On(_total.ItemsDone(), action);
			public void Now(Func<Progress, Task> action) => action(GetProgress());

			private readonly object _lock = new object();

			public void ItemStarted() => ProcessTriggers(ref _started);
			public void ItemSucceeded() => ProcessTriggers(ref _succeeded);
			public void ItemFailed() => ProcessTriggers(ref _failed);

			public void ItemsDiscovered(int count)
			{
				Interlocked.Add(ref _total, count);
			}

			/// <summary>
			/// Returns a completion task that should be awaited if any async actions were triggered.
			/// Async actions are NOT awaited inline.
			/// </summary>
			public Task CompleteAsync() => Task.WhenAll(_tasks);

			public Progress GetProgress() => new Progress(_startTime, _endTime, _total, _started, _succeeded, _failed);

			private void ProcessTriggers(ref int fieldToIncrement)
			{
				var fired = new List<ProgressAction>();
				Progress prog;

				lock (_lock)
				{
					fieldToIncrement++;
					prog = GetProgress();
					fired = _actions.Where(act => act.Trigger.IsFired(prog)).ToList();

					foreach (var act in fired)
					{
						if (!act.Recurring)
						{
							_actions.Remove(act);
						}
					}

					_tasks.RemoveAll(t => t.IsCompleted);
				}

				fired.ForEach(act => act.Invoke(prog, _tasks));
			}

			private abstract class ActionBase
			{
				public Action<Progress> Action { get; set; }
				public Func<Progress, Task> AsyncAction { get; set; }

				public void Invoke(Progress prog, IList<Task> tasks)
				{
					var task = AsyncAction?.Invoke(prog);
					Action?.Invoke(prog);
					if (task?.IsCompleted == false)
						tasks.Add(task);
				}
			}

			private class ProgressAction : ActionBase
			{
				public Trigger Trigger { get; set; }
				public bool Recurring { get; set; }
			}

			private class TimerAction : ActionBase
			{
				private Timer _timer;

				public TimeSpan Interval { get; set; }

				public void Start(Tracker tracker)
				{
					_timer = new Timer(_ => Invoke(tracker.GetProgress(), tracker._tasks), null, Interval, Interval);
				}

				public void Stop()
				{
					_timer.Dispose();
				}
			}
		}

		public class Trigger
		{
			private readonly int _targetNumber;
			private readonly Func<Progress, int> _getCurrentNumber;
			private int _nextNumber;

			public Trigger(int targetNumber, Func<Progress, int> getCurrentNumber)
			{
				_nextNumber = _targetNumber = targetNumber;
				_getCurrentNumber = getCurrentNumber;
			}

			public bool IsFired(Progress prog)
			{
				if (_getCurrentNumber(prog) >= _nextNumber)
				{
					_nextNumber += _targetNumber;
					return true;
				}
				return false;
			}
		}

		public static class TriggerBuilderExtensions
		{
			public static Trigger ItemsStarted(this int i) => new Trigger(i, p => p.ItemsStarted);
			public static Trigger ItemsDone(this int i) => new Trigger(i, p => p.ItemsDone);
			public static Trigger ItemsSucceeded(this int i) => new Trigger(i, p => p.ItemsSucceeded);
			public static Trigger ItemsFailed(this int i) => new Trigger(i, p => p.ItemsFailed);

			public static Trigger PercentStarted(this int i) => new Trigger(i, p => p.PercentStarted);
			public static Trigger PercentDone(this int i) => new Trigger(i, p => p.PercentDone);
			public static Trigger PercentSucceeded(this int i) => new Trigger(i, p => p.PercentSucceeded);
			public static Trigger PercentFailed(this int i) => new Trigger(i, p => p.PercentFailed);

			public static TimeSpan Seconds(this int i) => TimeSpan.FromSeconds(i);
			public static TimeSpan Minutes(this int i) => TimeSpan.FromMinutes(i);
			public static TimeSpan Hours(this int i) => TimeSpan.FromHours(i);
		}

		public class Progress
		{
			private readonly long _startTime;
			private readonly long _endTime;

			public int TotalItems { get; }
			public int ItemsStarted { get; }
			public int ItemsSucceeded { get; }
			public int ItemsFailed { get; }

			public int ItemsDone => ItemsSucceeded + ItemsFailed;
			public int ItemsInProgress => ItemsStarted - ItemsDone;
			public int ItemsRemaining => TotalItems - ItemsStarted;

			public int PercentStarted => SafeDivide(100 * ItemsStarted, TotalItems);
			public int PercentDone => SafeDivide(100 * ItemsDone, TotalItems);
			public int PercentSucceeded => SafeDivide(100 * ItemsSucceeded, TotalItems);
			public int PercentFailed => SafeDivide(100 * ItemsFailed, TotalItems);
			public int PercentInProgress => SafeDivide(100 * ItemsInProgress, TotalItems);
			public int PercentRemaining => SafeDivide(100 * ItemsRemaining, TotalItems);

			public double PercentStartedExact => SafeDivideExact(100 * ItemsStarted, TotalItems);
			public double PercentDoneExact => SafeDivideExact(100 * ItemsDone, TotalItems);
			public double PercentSucceededExact => SafeDivideExact(100 * ItemsSucceeded, TotalItems);
			public double PercentFailedExact => SafeDivideExact(100 * ItemsFailed, TotalItems);
			public double PercentInProgressExact => SafeDivideExact(100 * ItemsInProgress, TotalItems);
			public double PercentRemainingExact => SafeDivideExact(100 * ItemsRemaining, TotalItems);

			private long ElapsedTicks => (_endTime == 0 ? DateTime.UtcNow.Ticks : _endTime) - _startTime;

			public TimeSpan ElapsedTime => TimeSpan.FromTicks(ElapsedTicks);
			public int ElapsedSeconds => (int)ElapsedTime.TotalSeconds;
			public int ElapsedMinutes => (int)ElapsedTime.TotalMinutes;
			public int ElapsedHours => (int)ElapsedTime.TotalHours;

			public TimeSpan EstTotalTime => TimeSpan.FromTicks(SafeDivide(ElapsedTicks * TotalItems, ItemsDone));
			public TimeSpan EstTimeRemaining => EstTotalTime - ElapsedTime;
			public DateTime EstEndTimeUtc => DateTime.UtcNow + EstTimeRemaining;
			public DateTime EstEndTimeLocal => DateTime.Now + EstTimeRemaining;

			public bool IsDone => ItemsDone == TotalItems;

			public Progress(long startTime, long endTime, int total, int started, int succeeded, int failed)
			{
				_startTime = startTime;
				_endTime = endTime;
				TotalItems = total;
				ItemsStarted = started;
				ItemsSucceeded = succeeded;
				ItemsFailed = failed;
			}

			private int SafeDivide(int x, int y) => y == 0 ? 0 : x / y;
			private long SafeDivide(long x, long y) => y == 0 ? 0 : x / y;
			private double SafeDivideExact(long x, long y) => y == 0 ? 0 : (double)x / y;
		}
	}
}
