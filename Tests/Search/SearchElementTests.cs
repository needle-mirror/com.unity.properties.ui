using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    sealed class SearchElementTests
    {
        EditorWindow m_Window;

        SearchElement m_SearchElement;
        PropertyElement m_PropertyElement;

        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            m_Window = EditorWindow.CreateInstance<EditorWindow>();
            m_Window.Show();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            m_Window.Close();
        }

        [SetUp]
        public void SetUp()
        {
            m_SearchElement = new SearchElement {SearchDelay = 0};
            m_PropertyElement = new PropertyElement();
            m_Window.rootVisualElement.Add(m_SearchElement);
            m_Window.rootVisualElement.Add(m_PropertyElement);
        }

        [TearDown]
        public void Teardown()
        {
            m_Window.rootVisualElement.Clear();
        }

        class DataContainerInspector : Inspector<TestDataContainerWithCustomInspector>
        {
            public override VisualElement Build()
            {
                var root = new VisualElement();
                new Internal.UITemplate("test-data-container").Clone(root);
                return root;
            }
        }

        class TestDataContainerWithCustomInspector
        {
            [CreateProperty] public TestData[] SourceData;
            [CreateProperty] public List<TestData> DestinationData;
        }

        class TestDataContainer
        {
            public static readonly PropertyPath ValidSourceDataPath = new PropertyPath(nameof(ValidSourceData));
            public static readonly PropertyPath ValidDestinationDataPath = new PropertyPath(nameof(ValidDestinationData));
            public static readonly PropertyPath ReadOnlyDestinationDataPath = new PropertyPath(nameof(ReadOnlyDestinationData));
            public static readonly PropertyPath NonCollectionSourceDataPath = new PropertyPath(nameof(NonCollectionSourceData));
            public static readonly PropertyPath NonCollectionDestinationDataPath = new PropertyPath(nameof(NonCollectionDestinationData));
            
#pragma warning disable 649
            [CreateProperty] public TestData[] ValidSourceData;
            [CreateProperty] public List<TestData> ValidDestinationData;
            [CreateProperty] public TestData[] ReadOnlyDestinationData;
            [CreateProperty] public TestData NonCollectionSourceData;
            [CreateProperty] public TestData NonCollectionDestinationData;
#pragma warning restore 649
        }
        
        [Test]
        public void Search_WithNoSearchDataProperties_DoesNotReturnAnyResults()
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            m_SearchElement.value = "Mesh";
            
            Assert.That(filteredData.Length, Is.EqualTo(0));
        }

        [Test]
        [TestCase("Mesh", "Name", 25)]
        [TestCase("1", "Id",19)]
        [TestCase("nested0", "Nested.Value",1)]
        public void Search_WithSearchDataProperties_ReturnsFilteredResults(string searchString, string searchDataProperties, int expectedCount)
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            
            foreach (var path in searchDataProperties.Split(' '))
                m_SearchElement.AddSearchDataProperty(new PropertyPath(path));

            m_SearchElement.value = searchString;
            
            Assert.That(filteredData.Length, Is.EqualTo(expectedCount));
        }

        [Test]
        [TestCase("id<10", "id:Id", 10)]
        [TestCase("x<50", "x:Position.x", 5)]
        [TestRequires_QUICKSEARCH_2_0_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        public void Search_WithSearchFilterProperties_ReturnsFilteredResults(string searchString, string searchFilterProperties, int expectedCount)
        {
            var originalData = Generate(100);
            var filteredData = default(TestData[]);

            foreach (var searchFilter in searchFilterProperties.Split(' '))
            {
                var tokenAndPath = searchFilter.Split(':');
                
                var token = tokenAndPath[0];
                var path = tokenAndPath[1];
                
                m_SearchElement.AddSearchFilterProperty(token, new PropertyPath(path));
            }

            m_SearchElement.RegisterSearchQueryHandler<TestData>(search => { filteredData = search.Apply(originalData).ToArray(); });
            m_SearchElement.value = searchString;
            
            Assert.That(filteredData.Length, Is.EqualTo(expectedCount));
        }
        
        [Test]
        public void RegisterHandler_WithInvalidSourceDataPath_Throw()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, new PropertyPath("SomeUnknownPath"), TestDataContainer.ValidDestinationDataPath);
            });
        }

        [Test]
        public void RegisterHandler_WithInvalidDestinationDataPath_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, new PropertyPath("SomeUnknownPath"));
            });
        }
        
        [Test]
        public void RegisterHandler_WithInvalidSourceDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.NonCollectionSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithInvalidDestinationDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            });

            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.NonCollectionDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithReadOnlyDestinationDataType_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer());

            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ReadOnlyDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithNullSourceData_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = null,
                ValidDestinationData = new List<TestData>()
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }
        
        [Test]
        public void RegisterHandler_WithNullDestinationData_Throws()
        {
            m_PropertyElement.SetTarget(new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = null
            });
            
            Assert.Throws<InvalidBindingException>(() =>
            {
                m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            });
        }

        [Test]
        public void Search_WithValidBindings_ResultsAreWrittenToDestinationData()
        {
            var container = new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            };
            
            m_PropertyElement.SetTarget(container);
            m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            m_SearchElement.AddSearchDataProperty(new PropertyPath("Name"));
            
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(0));

            m_SearchElement.Search("");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(container.ValidSourceData.Length));

            m_SearchElement.Search("Mesh");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(25));
        }
        
        [Test]
        public void Search_WithCollectionSearchData_ResultsAreWrittenToDestinationData()
        {
            var container = new TestDataContainer
            {
                ValidSourceData = Generate(100),
                ValidDestinationData = new List<TestData>()
            };
            
            container.ValidSourceData[0].StringArray = new[]
            {
                "one", "two", "three", "four"
            };
            
            container.ValidSourceData[1].StringArray = new[]
            {
                "two", "three", "four"
            };
            
            container.ValidSourceData[2].StringArray = new[]
            {
                "three", "four"
            };
            
            container.ValidSourceData[3].StringArray = new[]
            {
                "four"
            };
            
            m_PropertyElement.SetTarget(container);
            m_SearchElement.RegisterSearchQueryHandler(m_PropertyElement, TestDataContainer.ValidSourceDataPath, TestDataContainer.ValidDestinationDataPath);
            m_SearchElement.AddSearchDataProperty(new PropertyPath("StringArray"));
            
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(0));

            m_SearchElement.Search("two");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(2));
            
            m_SearchElement.Search("three");
            Assert.That(container.ValidDestinationData.Count, Is.EqualTo(3));
        }
        
        [Test]
        public void Search_CustomInspectorWithDataBindings_ReturnsFilteredResults()
        {
            var container = new TestDataContainerWithCustomInspector
            {
                SourceData = Generate(100),
                DestinationData = new List<TestData>()
            };
            
            m_PropertyElement.SetTarget(container);

            var searchElement = m_PropertyElement.Q<SearchElement>("filter");
            var searchHandler = searchElement.GetUxmlSearchHandler();

            /* The UXML declaration of the search element is.
             * 
             * <pui:SearchElement
             *      name="filter"
             *      search-delay="100"
             *      handler-type="sync"
             *      search-data="Id Name"
             *      source-data="SourceData"
             *      filtered-data="DestinationData"
             *      search-filters="a:StringArray"/>
             */
            
            Assert.That(searchElement.SearchDelay, Is.EqualTo(100));
            Assert.That(searchHandler.Mode, Is.EqualTo(SearchHandlerType.sync));
            Assert.That(searchHandler.SearchDataType, Is.EqualTo(typeof(TestData)));

            searchElement.Search("Mesh");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(25));
        }
        
        [Test]
        [TestRequires_QUICKSEARCH_2_0_0_OR_NEWER("Filtering is only supported using the com.unity.quicksearch package.")]
        public void Search_CustomInspectorWithCollectionFilter_ReturnsFilteredResults()
        {
            var container = new TestDataContainerWithCustomInspector
            {
                SourceData = Generate(100),
                DestinationData = new List<TestData>()
            };
            
            container.SourceData[0].StringArray = new[]
            {
                "a", "b"
            };
            
            container.SourceData[1].StringArray = new[]
            {
                "b", "c"
            };
            
            container.SourceData[2].StringArray = new[]
            {
                "c", "d", "e"
            };
            
            m_PropertyElement.SetTarget(container);

            var searchElement = m_PropertyElement.Q<SearchElement>("filter");
            
            searchElement.Search("a:e");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(1));
            
            searchElement.Search("a:b");
            
            Assert.That(container.DestinationData.Count, Is.EqualTo(2));
        }

        class TestData
        {
            public struct NestedStruct
            {
                public string Value;
            }
            
            [CreateProperty] public int Id { get; set; }
            [CreateProperty] public string Name { get; set; }
            [CreateProperty] public Vector2 Position { get; set; }
            [CreateProperty] public bool Active { get; set; }
            [CreateProperty] public NestedStruct Nested { get; set; }
            [CreateProperty] public string[] StringArray { get; set; }
        }

        static TestData[] Generate(int size)
        {
            var data = new TestData[size];
            
            for (var i = 0; i < size; ++i)
            {
                var posX = i * 10;
                var posY = i * -25;

                string name;

                switch (i % 4)
                {
                    case 0:
                        name = $"Material {i}";
                        break;
                    case 1:
                        name = $"Mesh {i}";
                        break;
                    case 2:
                        name = $"Camera {i}";
                        break;
                    default:
                        name = $"Object {i}";
                        break;
                }

                data[i] = new TestData {Id = i, Name = name, Position = new Vector2(posX, posY), Active = i % 2 == 0, Nested = new TestData.NestedStruct { Value = $"nested{i}"}};
            }

            return data;
        }
    }
}