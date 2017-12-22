using Simplicity.dotNet.Common.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simplicity.dotNet.Common;
using System.Diagnostics;
using Simplicity.dotNet.Core.Configuration;

namespace Simplicity.dotNet.Core.Logic {
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Simplicity.dotNet.Common.Logic.IPerfCounters" />
	public class PerfCounters : IPerfCounters {
		/// <summary>
		/// The category name
		/// </summary>
		private const string CategoryName = "Simplicity";

		/// <summary>
		/// The total jni exceptions
		/// </summary>
		private const string TotalJniExceptions = "Total Jni Exceptions";

		/// <summary>
		/// The total success execution
		/// </summary>
		private const string TotalSuccessExecution = "Total Successful Execution";

		/// <summary>
		/// The total requests
		/// </summary>
		private const string TotalRequests = "Total Requests";

		/// <summary>
		/// The cached jni method call
		/// </summary>
		private const string CachedJniMethodCall = "Jni Method Call (Cached)";

		/// <summary>
		/// The proxy generation
		/// </summary>
		private const string ProxyGeneration = "Dynamic Generated Proxy";

		/// <summary>
		/// The database jni method call
		/// </summary>
		private const string DbJniMethodCall = "Jni Method Call (DB)";

		/// <summary>
		/// The is disposed
		/// </summary>
		private bool _isDisposed;

		/// <summary>
		/// The configuration reader
		/// </summary>
		private IConfigurationReader _configurationReader;

		/// <summary>
		/// Gets the <see cref="PerformanceCounter"/> with the specified counter name.
		/// </summary>
		/// <value>
		/// The <see cref="PerformanceCounter"/>.
		/// </value>
		/// <param name="counterName">Name of the counter.</param>
		/// <returns></returns>
		public PerformanceCounter this[Enums.PerfCounter counterName] {
			get {
				return CountersAvailable.ContainsKey(counterName) ? CountersAvailable[counterName] : null;
			}
		}

		/// <summary>
		/// Gets the created on.
		/// </summary>
		/// <value>
		/// The created on.
		/// </value>
		public DateTime CreatedOn {
			get; private set;
		}

		/// <summary>
		/// Gets the instance identifier.
		/// </summary>
		/// <value>
		/// The instance identifier.
		/// </value>
		public Guid InstanceId {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the counters available.
		/// </summary>
		/// <value>
		/// The counters available.
		/// </value>
		protected Dictionary<Enums.PerfCounter, PerformanceCounter> CountersAvailable {
			get; set;
		}

		/// <summary>
		/// Gets the counter data collection.
		/// </summary>
		/// <value>
		/// The counter data collection.
		/// </value>
		protected CounterCreationDataCollection CounterDataCollection {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerfCounters" /> class.
		/// </summary>
		/// <param name="config">The configuration.</param>
		public PerfCounters(IConfigurationReader config) {
			CreatedOn = DateTime.Now;
			InstanceId = Guid.NewGuid();
			_configurationReader = config;
			CounterDataCollection = new CounterCreationDataCollection();
			CountersAvailable = new Dictionary<Enums.PerfCounter, PerformanceCounter>();
			CreateOrInitializePerfCounters();
		}

		/// <summary>
		/// Increments the counter.
		/// </summary>
		/// <param name="counterName">Name of the counter.</param>
		public void IncrementCounter(Enums.PerfCounter counterName) {
			var config = (CustomConfigReader)_configurationReader.Configuration;

			// Should we enable perfcounters? (as per config)
			if (config.dotNetOptions.enablePerfCounters)
				this[counterName].Increment();
		}

		/// <summary>
		/// Creates the or initialize perf counters.
		/// </summary>
		private void CreateOrInitializePerfCounters() {
			if (!PerformanceCounterCategory.Exists(CategoryName)) {
				CounterDataCollection.AddRange(new[] {
													new CounterCreationData(TotalJniExceptions, string.Empty, PerformanceCounterType.NumberOfItems64),
													new CounterCreationData(TotalRequests, string.Empty, PerformanceCounterType.NumberOfItems64),
													new CounterCreationData(TotalSuccessExecution, string.Empty, PerformanceCounterType.NumberOfItems64),
													new CounterCreationData(CachedJniMethodCall, string.Empty, PerformanceCounterType.NumberOfItems64),
													new CounterCreationData(ProxyGeneration, string.Empty, PerformanceCounterType.NumberOfItems64),
													new CounterCreationData(DbJniMethodCall, string.Empty, PerformanceCounterType.NumberOfItems64),
												   });

				PerformanceCounterCategory.Create(CategoryName, CategoryName, PerformanceCounterCategoryType.MultiInstance, CounterDataCollection);
			}
			AddCountersToInstance();
		}

		/// <summary>
		/// Adds the counters to instance.
		/// </summary>
		private void AddCountersToInstance() {
			if (PerformanceCounterCategory.Exists(CategoryName)) {
				CountersAvailable.Add(Enums.PerfCounter.JniException, new PerformanceCounter(CategoryName, TotalJniExceptions, CategoryName, false));
				CountersAvailable.Add(Enums.PerfCounter.TotalRequests, new PerformanceCounter(CategoryName, TotalRequests, CategoryName, false));
				CountersAvailable.Add(Enums.PerfCounter.SuccessfulExecution, new PerformanceCounter(CategoryName, TotalSuccessExecution, CategoryName, false));
				CountersAvailable.Add(Enums.PerfCounter.CachedJniMethodCall, new PerformanceCounter(CategoryName, CachedJniMethodCall, CategoryName, false));
				CountersAvailable.Add(Enums.PerfCounter.ProxyGeneration, new PerformanceCounter(CategoryName, ProxyGeneration, CategoryName, false));
				CountersAvailable.Add(Enums.PerfCounter.DbJniMethodCall, new PerformanceCounter(CategoryName, DbJniMethodCall, CategoryName, false));
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool isDisposing) {
			if (_isDisposed)
				return;

			if (isDisposing) {
				var counters = CountersAvailable.ToList();

				counters.ForEach(_ => {
					_.Value?.Close();
					_.Value?.Dispose();
				});
			}
			_isDisposed = true;
		}
	}
}
