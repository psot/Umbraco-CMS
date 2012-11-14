using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.BusinessLogic;
using System.Linq;

namespace Umbraco.Tests.PublishedContent
{
	/// <summary>
	/// Tests the typed extension methods on IPublishedContent using the DefaultPublishedMediaStore
	/// </summary>
	[TestFixture]
	public class PublishedMediaTests : BaseWebTest
	{
		
		public override void Initialize()
		{
			base.Initialize();
			DoInitialization(GetUmbracoContext("/test", 1234));			
		}

		/// <summary>
		/// Shared with PublishMediaStoreTests
		/// </summary>
		/// <param name="umbContext"></param>
		internal static void DoInitialization(UmbracoContext umbContext)
		{
			PropertyEditorValueConvertersResolver.Current = new PropertyEditorValueConvertersResolver(
				new[]
					{
						typeof(DatePickerPropertyEditorValueConverter),
						typeof(TinyMcePropertyEditorValueConverter),
						typeof(YesNoPropertyEditorValueConverter)
					});

			//need to specify a custom callback for unit tests
			PublishedContentHelper.GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
			{
				if (propertyAlias == "content")
				{
					//return the rte type id
					return Guid.Parse("5e9b75ae-face-41c8-b47e-5f4b0fd82f83");
				}
				return Guid.Empty;
			};

			UmbracoSettings.ForceSafeAliases = true;
			UmbracoSettings.UmbracoLibraryCacheDuration = 1800;

			UmbracoContext.Current = umbContext;
			PublishedMediaStoreResolver.Current = new PublishedMediaStoreResolver(new DefaultPublishedMediaStore());

			UmbracoSettings.ForceSafeAliases = true;
		}

		public override void TearDown()
		{
			base.TearDown();

			DoTearDown();
		}

		/// <summary>
		/// Shared with PublishMediaStoreTests
		/// </summary>
		internal static void DoTearDown()
		{
			PropertyEditorValueConvertersResolver.Reset();
			UmbracoContext.Current = null;
			PublishedMediaStoreResolver.Reset();
		}

		/// <summary>
		/// Shared with PublishMediaStoreTests
		/// </summary>
		/// <param name="id"></param>
		/// <param name="umbracoContext"></param>
		/// <returns></returns>
		internal static IPublishedContent GetNode(int id, UmbracoContext umbracoContext)
		{
			var ctx = umbracoContext;
			var mediaStore = new DefaultPublishedMediaStore();
			var doc = mediaStore.GetDocumentById(ctx, id);
			Assert.IsNotNull(doc);
			return doc;
		}

		private IPublishedContent GetNode(int id)
		{
			return GetNode(id, GetUmbracoContext("/test", 1234));
		}

		[Test]
		public void Children_Without_Examine()
		{
			var user = new User(0);
			var mType = global::umbraco.cms.businesslogic.media.MediaType.MakeNew(user, "TestMediaType");
			var mRoot = global::umbraco.cms.businesslogic.media.Media.MakeNew("MediaRoot", mType, user, -1);
			
			var mChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child1", mType, user, mRoot.Id);
			var mChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child2", mType, user, mRoot.Id);
			var mChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("Child3", mType, user, mRoot.Id);

			var mSubChild1 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild1", mType, user, mChild1.Id);
			var mSubChild2 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild2", mType, user, mChild1.Id);
			var mSubChild3 = global::umbraco.cms.businesslogic.media.Media.MakeNew("SubChild3", mType, user, mChild1.Id);

			var publishedMedia = GetNode(mRoot.Id);
			var rootChildren = publishedMedia.Children();
			Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(new[] {mChild1.Id, mChild2.Id, mChild3.Id}));

			var publishedChild1 = GetNode(mChild1.Id);
			var subChildren = publishedChild1.Children();
			Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(new[] { mSubChild1.Id, mSubChild2.Id, mSubChild3.Id }));
		}
	}
}