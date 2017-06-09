﻿using System;
using System.Diagnostics;
using System.Linq;
using Sitecore.Configuration;
using Sitecore.DataBlaster.Load;
using Sitecore.DataBlaster.Load.Processors;
using Sitecore.DataBlaster.Util;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Unicorn.Configuration;
using Unicorn.DataBlaster.Logging;
using Unicorn.Loader;
using Unicorn.Pipelines.UnicornSyncComplete;
using Unicorn.Pipelines.UnicornSyncEnd;
using Unicorn.Pipelines.UnicornSyncStart;
using Unicorn.Predicates;
using Unicorn.Publishing;
using ILogger = Unicorn.Logging.ILogger;

namespace Unicorn.DataBlaster.Sync
{
	public class UnicornDataBlaster : IUnicornSyncStartProcessor
	{
		public const string DisableDataBlasterSettingName = "Unicorn.DisableDataBlaster";
		public const string PipelineArgsParametersKey = "DataBlaster.Parameters";

		private bool? _isUnicornPublishEnabled;

		protected virtual bool IsUnicornPublishEnabled
		{
			get
			{
				if (_isUnicornPublishEnabled == null)
				{
					_isUnicornPublishEnabled = Factory.GetConfigNode("//sitecore/pipelines/unicornSyncEnd/processor")?
						.Attributes?["type"]?.Value
						.StartsWith("Unicorn.Pipelines.UnicornSyncEnd.TriggerAutoPublishSyncedItems") ?? false;
				}

				return _isUnicornPublishEnabled.Value;
			}
		}

		private BulkLoader BulkLoader { get; }

		private ItemExtractor ItemExtractor { get; }

		/// <summary>
		/// Whether to skip updating the history engine.
		/// </summary>
		/// <remarks>Skipped by default.</remarks>
		public bool SkipHistoryEngine { get; set; } = true;

		/// <summary>
		/// Whether to skip updating the publish queue for incremental publishing.
		/// </summary>
		/// <remarks>Skipped by default.</remarks>
		public bool SkipPublishQueue { get; set; } = true;

		/// <summary>
		/// Whether to skip updating the link database.
		/// </summary>
		/// <remarks>If not skipped and at least one config nneds to update the link database, it's updated for all configs.</remarks>
		public bool SkipLinkDatabase { get; set; } = false;

		/// <summary>
		/// Whether to skip updating the indexes.
		/// </summary>
		/// <remarks>If not skipped and at least one config nneds to update the indexes, it's updated for all configs.</remarks>
		public bool SkipIndexes { get; set; } = false;

		public UnicornDataBlaster(BulkLoader bulkLoader = null, ItemExtractor itemExtractor = null)
		{
			BulkLoader = bulkLoader ?? new BulkLoader();
			ItemExtractor = itemExtractor ?? new ItemExtractor();
		}

		public virtual void Process(UnicornSyncStartPipelineArgs args)
		{
			// Find optional data blaster parameters in custom data of arguments.
			object parms;
			args.CustomData.TryGetValue(PipelineArgsParametersKey, out parms);
			var parameters = parms as DataBlasterParameters;
			if (parameters == null)
			{
				// Is DataBlaster disabled through config?
				if (Settings.GetBoolSetting(DisableDataBlasterSettingName, false)) return;

				// Use default parameters.
				parameters = new DataBlasterParameters();
			}

			// Is DataBlaster disabled through parameters?
			if (parameters.DisableDataBlaster) return;

			var logger = args.Logger;
			try
			{
				logger.Info($"Start Bulk Unicorn Sync for configurations: '{string.Join("', '", args.Configurations.Select(x => x.Name))}'.");

				var watch = Stopwatch.StartNew();
				var startTimestamp = DateTime.Now;
				var configs = args.Configurations;

				LoadItems(configs, parameters, args.Logger);
				logger.Info($"Extracted and loaded items ({(int)watch.Elapsed.TotalMilliseconds}ms)");

				watch.Restart();
				ClearCaches();
				logger.Info($"Caches cleared ({(int)watch.Elapsed.TotalMilliseconds}ms)");

				// All configurations have been synced, run complete pipelines to support post-processing, e.g. users and roles.
				watch.Restart();
				foreach (var config in configs)
				{
					CorePipeline.Run("unicornSyncComplete", new UnicornSyncCompletePipelineArgs(config, startTimestamp));
				}
				logger.Info($"Ran sync complete pipelines ({(int)watch.Elapsed.TotalMilliseconds}ms)");

				// When we tell Unicorn that sync is handled, end pipeline is not called anymore.
				CorePipeline.Run("unicornSyncEnd", new UnicornSyncEndPipelineArgs(args.Logger, true, configs));
				logger.Info($"Ran sync end pipeline ({(int)watch.Elapsed.TotalMilliseconds}ms)");
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw;
			}
			finally
			{
				// This will signal that we handled the sync for all configurations, 
				// and no further handling should be done.
				args.SyncIsHandled = true;
			}
		}

		protected virtual void LoadItems(IConfiguration[] configurations, DataBlasterParameters parameters, ILogger logger)
		{
			var databaseNames = configurations
				.SelectMany(c => c.Resolve<PredicateRootPathResolver>().GetRootPaths().Select(rp => rp.DatabaseName))
				.Distinct();

			foreach (var databaseName in databaseNames)
			{
				logger.Info($"Syncing database '{databaseName}'...");

				var context = CreateBulkLoadContext(BulkLoader, databaseName, configurations, parameters, logger);
				var bulkItems = ItemExtractor.ExtractBulkItems(context, configurations, databaseName);
				BulkLoader.LoadItems(context, bulkItems);

				if (context.AnyStageFailed)
					throw new Exception($"Stage failed during bulkload of database '{databaseName}': {context.FailureMessage}");

				// Support publishing after sync.
				if (!IsUnicornPublishEnabled && !databaseName.Equals("core", StringComparison.OrdinalIgnoreCase))
				{
					foreach (var itemChange in context.ItemChanges)
					{
						ManualPublishQueueHandler.AddItemToPublish(itemChange.ItemId);
					}
				}
			}
		}

		protected virtual void ClearCaches()
		{
			var cacheUtil = new CacheUtil();

			Factory.GetDatabases()
				.ForEach(x =>
				{
					x.Engines.TemplateEngine.Reset();
					cacheUtil.ClearLanguageCache(x);
				});
			Sitecore.Caching.CacheManager.ClearAllCaches();

			// Slow as hell, most people don't use it.
			//Translate.ResetCache();
		}

		protected virtual BulkLoadContext CreateBulkLoadContext(BulkLoader bulkLoader, string databaseName,
			IConfiguration[] configurations, DataBlasterParameters parameters, ILogger logger)
		{
			var context = bulkLoader.NewBulkLoadContext(databaseName);

			context.Log = new SitecoreAndUnicornLog(LoggerFactory.GetLogger(GetType()), logger);

			context.AllowTemplateChanges = true;
			context.StageDataWithoutProcessing = parameters.StageDataWithoutProcessing;

			// Use the shotgun, removing items one by one is too slow for full deserialize.
			context.RemoveItemsFromCaches = false;

			context.UpdateHistory = !SkipHistoryEngine;
			context.UpdatePublishQueue = !SkipPublishQueue;
			context.UpdateLinkDatabase = !SkipLinkDatabase &&
				configurations.Any(x => x.Resolve<ISyncConfiguration>().UpdateLinkDatabase);
			context.UpdateIndexes = !SkipIndexes &&
				configurations.Any(x => x.Resolve<ISyncConfiguration>().UpdateSearchIndex);

			return context;
		}
	}
}
