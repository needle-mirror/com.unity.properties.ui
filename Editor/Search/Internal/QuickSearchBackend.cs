
#if QUICKSEARCH_3_0_0_OR_NEWER
using Unity.Search;
#elif QUICKSEARCH_2_1_0_OR_NEWER
using Unity.QuickSearch;
#endif

#if QUICKSEARCH_2_1_0_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Internal;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Full implementation of the query engine. This relies on the QuickSearch package.
    /// </summary>
    class QuickSearchBackend<TData> : SearchBackend<TData>
    {
        class SearchQuery : ISearchQuery<TData>
        {
            readonly Query<TData> m_Query;
            
            public string SearchString { get; }
            
            public ICollection<string> Tokens => m_Query.tokens;

            public SearchQuery(string searchString, Query<TData> query)
            {
                SearchString = searchString;
                m_Query = query;
            }

            public IEnumerable<TData> Apply(IEnumerable<TData> data)
            {
                return m_Query?.Apply(data) ?? data;
            }
        }
        
        class FilterVisitor : IPropertyBagVisitor, IPropertyVisitor
        {
            class CollectionFilterVisitor : ICollectionPropertyBagVisitor
            {
                public PropertyPath Path;
                public QueryEngine<TData> QueryEngine;
                public string Token;
                
                void ICollectionPropertyBagVisitor.Visit<TCollection, TElement>(ICollectionPropertyBag<TCollection, TElement> properties, ref TCollection container)
                {
                    var path = Path;
                    
                    // This query engine filter is provided with two delegates:
                    //
                    // parameterTransformer: This function is responsible for transforming the input string into a strong generic type.
                    //                       this is done via the `TypeConversion` API in properties.
                    //
                    // filterResolver:       This function takes the data for each element being filtered, along with the original token, and the 
                    //                       transformed input. The function then applies the filter and returns true or false. In this particular case
                    //                       we want to test equality with each collection element and return true if ANY match.
                    //
                    QueryEngine.AddFilter<TElement, TElement>(Token, (data, _, token, transformedInput) =>
                    {
                        if (!PropertyContainer.TryGetValue<TData, TCollection>(ref data, path, out var collection))
                            return false;

                        if (RuntimeTypeInfoCache<TCollection>.CanBeNull && null == collection)
                            return false;

                        return collection.Any(e => FilterOperator.ApplyOperator(token, e, transformedInput, QueryEngine.globalStringComparison));
                    }, TypeConversion.Convert<string, TElement>, FilterOperator.GetSupportedOperators<TElement>());
                }
            }

            readonly CollectionFilterVisitor m_CollectionFilterVisitor = new CollectionFilterVisitor();
            
            public int PathIndex;
            public PropertyPath Path;
            public VisitErrorCode ErrorCode;
            public QueryEngine<TData> QueryEngine;
            public string Token;
            public string[] SupportedOperatorTypes;
            
            void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
            {
                var part = Path[PathIndex++];

                IProperty<TContainer> property;

                switch (part.Type)
                {
                    case PropertyPath.PartType.Name:
                    {
                        if (properties is IPropertyNameable<TContainer> keyable && keyable.TryGetProperty(ref container, part.Name, out property))
                            ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                        else
                            ErrorCode = VisitErrorCode.InvalidPath;
                    }
                    break;

                    case PropertyPath.PartType.Index:
                    {
                        if (properties is IPropertyIndexable<TContainer> indexable && indexable.TryGetProperty(ref container, part.Index, out property))
                            ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                        else
                            ErrorCode = VisitErrorCode.InvalidPath;
                    }
                        break;

                    case PropertyPath.PartType.Key:
                    {
                        if (properties is IPropertyKeyable<TContainer, object> keyable && keyable.TryGetProperty(ref container, part.Key, out property))
                            ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                        else
                            ErrorCode = VisitErrorCode.InvalidPath;
                    }
                        break;

                    default:
                        ErrorCode = VisitErrorCode.InvalidPath;
                        break;
                }
            }

            void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
            {
                var value = default(TValue);
                
                if (PathIndex >= Path.PartsCount)
                {
                    if (PropertyBagStore.GetPropertyBag<TValue>() is ICollectionPropertyBagAccept<TValue> collectionPropertyBagAccept)
                    {
                        m_CollectionFilterVisitor.Path = Path;
                        m_CollectionFilterVisitor.QueryEngine = QueryEngine;
                        m_CollectionFilterVisitor.Token = Token;
                        
                        // The final result of the path has resolved to an array type.
                        // In this case we assume the user wants to filter based on any element of the collection.
                        // But first we need to resolve the generic element type. We use a property visitor for this.
                        collectionPropertyBagAccept.Accept(m_CollectionFilterVisitor, ref value);
                        return;
                    }
                    
                    var path = Path;
                    var token = Token;
                    var supportedOperatorTypes = SupportedOperatorTypes;
                    QueryEngine.AddFilter(token, data => PropertyContainer.TryGetValue<TData, TValue>(ref data, path, out var v) ? v : default, supportedOperatorTypes);
                    return;
                }

                if (RuntimeTypeInfoCache<TValue>.IsAbstractOrInterface || typeof(TValue) == typeof(object))
                {
                    throw new InvalidOperationException($"Failed to register filter with Token=[{Token}] at Path=[{Path}]. Unable to bind to polymorphic fields.");
                }
                
                var propertyBag = PropertyBagStore.GetPropertyBag<TValue>();

                if (null == propertyBag)
                {
                    ErrorCode = VisitErrorCode.InvalidPath;
                    return;
                }

                PropertyBag.AcceptWithSpecializedVisitor(propertyBag, this, ref value);
            }
        }

        readonly QueryEngine<TData> m_QueryEngine = new QueryEngine<TData>();
        readonly FilterVisitor m_FilterVisitor = new FilterVisitor();

        public QuickSearchBackend()
        {
            m_QueryEngine.SetSearchDataCallback(GetSearchData);
            m_QueryEngine.validateFilters = false;
        }

        public override ISearchQuery<TData> Parse(string text)
        {
            return new SearchQuery(text, !string.IsNullOrEmpty(text) ? m_QueryEngine.Parse(text) : null);
        }

        public override void AddSearchFilterProperty(string token, PropertyPath path, string[] supportedOperatorTypes = null)
        {
            m_FilterVisitor.PathIndex = 0;
            m_FilterVisitor.Path = path;
            m_FilterVisitor.ErrorCode = VisitErrorCode.Ok;
            m_FilterVisitor.QueryEngine = m_QueryEngine;
            m_FilterVisitor.Token = token;
            m_FilterVisitor.SupportedOperatorTypes = supportedOperatorTypes;

            var container = default(TData);
            
            var properties = PropertyBagStore.GetPropertyBag<TData>();
                
            if (null == properties)
            {
                throw new MissingPropertyBagException(typeof(TData));
            }
                
            properties.Accept(m_FilterVisitor, ref container);

            switch (m_FilterVisitor.ErrorCode)
            {
                case VisitErrorCode.Ok:
                    break;
                case VisitErrorCode.InvalidPath:
                    throw new InvalidPathException($"SearchElement Failed to AddSearchFilter for Type=[{typeof(TData)}] Token=[{token}] could not resolve Path=[{path}]");
                default:
                    throw new InvalidBindingException($"SearchElement Failed to AddSearchFilter for Type=[{typeof(TData)}] Token=[{token}] Path=[{path}] VisitErrorCode=[{m_FilterVisitor.ErrorCode}]");
            }
        }

        public override void AddSearchFilterCallback<TFilter>(string token, Func<TData, TFilter> getFilterDataFunc, string[] supportedOperatorType = null)
        {
            m_QueryEngine.AddFilter(token, getFilterDataFunc, supportedOperatorType);
        }
    }
}
#endif