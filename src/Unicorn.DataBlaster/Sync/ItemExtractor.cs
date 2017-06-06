using System;
using System.Collections.Generic;
using System.Linq;
using Rainbow.Model;
using Rainbow.Storage;
using Sitecore.DataBlaster.Load;
using Unicorn.Configuration;
using Unicorn.Data;
using Unicorn.Evaluators;
using Unicorn.Predicates;

namespace Unicorn.DataBlaster.Sync
{
	/// <summary>
	/// Extracts items from Unicorn.
	/// </summary>
	public class ItemExtractor
	{
		protected virtual ItemMapper ItemMapper { get; }

		public ItemExtractor(ItemMapper itemMapper = null)
		{
			ItemMapper = itemMapper ?? new ItemMapper();
		}

		public virtual IEnumerable<BulkLoadItem> ExtractBulkItems(BulkLoadContext context, IConfiguration[] configurations, string database)
		{
			var uniqueItems = new HashSet<Guid>();

			return GetTreeRoots(configurations, database)
				.SelectMany(tr =>
				{
					var action = GetBulkLoadAction(tr.Item1, tr.Item2);
					var dataStore = tr.Item1.Resolve<ITargetDataStore>();

					return dataStore.GetByPath(tr.Item2.Path, database)
						.SelectMany(i => GetSelfAndAllDescendants(dataStore, i))
						// For example '/sitecore/layout/Layouts/User Defined' can occur more than once 
						// because it has children from different configurations. 
						// Make sure we add the item itself only once.
						.Where(item => uniqueItems.Add(item.Id))
						.Select(y => ItemMapper.ToBulkLoadItem(y, context, action));
				});
		}

		protected virtual IEnumerable<string> GetDatabaseNames(IEnumerable<IConfiguration> configurations)
		{
			return configurations
				.SelectMany(c => c.Resolve<PredicateRootPathResolver>().GetRootPaths().Select(rp => rp.DatabaseName))
				.Distinct();
		}

		protected virtual IEnumerable<Tuple<IConfiguration, PresetTreeRoot>> GetTreeRoots(IEnumerable<IConfiguration> configurations, string db)
		{
			return configurations
				.Select(x => new
				{
					Configuration = x,
					TreeRoots = x.Resolve<PredicateRootPathResolver>()
						.GetRootPaths()
						.Where(rp => rp.DatabaseName.Equals(db, StringComparison.OrdinalIgnoreCase))
						.Select(tr => tr as PresetTreeRoot)
				})
				.SelectMany(x => x.TreeRoots.Select(tr => new Tuple<IConfiguration, PresetTreeRoot>(x.Configuration, tr)));
		}

		protected virtual BulkLoadAction GetBulkLoadAction(IConfiguration configuration, PresetTreeRoot treeRoot)
		{
			var evaluator = configuration.Resolve<IEvaluator>();

			if (evaluator is SerializedAsMasterEvaluator)
			{
				// Only revert the tree when there are no exclusions for this tree root.
				return treeRoot.Exclusions == null || treeRoot.Exclusions.Count == 0
					? BulkLoadAction.RevertTree
					: BulkLoadAction.Revert;
			}

			if (evaluator is NewItemOnlyEvaluator)
			{
				return BulkLoadAction.AddItemOnly;
			}

			if (evaluator is AddOnlyEvaluator)
			{
				return BulkLoadAction.AddOnly;
			}

			throw new ArgumentException($"Unknown evaluator type: '{evaluator.GetType().Name}'");
		}

		private IEnumerable<IItemData> GetSelfAndAllDescendants(IDataStore targetDataStore, IItemData parentItem)
		{
			var queue = new Queue<IItemData>();
			queue.Enqueue(parentItem);
			while (queue.Count > 0)
			{
				var item = queue.Dequeue();
				foreach (var child in targetDataStore.GetChildren(item))
				{
					queue.Enqueue(child);
				}
				yield return item;
			}
		}

	}
}