using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    abstract class WindowTestsFixtureBase
    {
        protected PropertyElement Element => m_Window?.Element;

        TestWindow m_Window;

        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            m_Window = TestWindow.NewInstance();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            m_Window.Close();
        }

        [SetUp]
        public void SetUp()
        {
            m_Window.SetRoot(new PropertyElement());
            OnSetupReady();
        }

        protected virtual void OnSetupReady()
        {
        }

        [TearDown]
        public void Teardown()
        {
            m_Window.RemoveRoot();
        }

        protected PropertyPath GetPath(string path)
        {
            return new PropertyPath(path);
        }

        protected PropertyPath GetListPath(string path, int index)
        {
            return PropertyPath.AppendIndex(new PropertyPath(path), index);
        }

        protected PropertyPath GetDictionaryPath<TKey>(string path, TKey key)
        {
            var p = PropertyPath.AppendKey(new PropertyPath(path), key);
            p = PropertyPath.AppendName(p, "Value");
            return p;
        }

        protected void UpdateBindables()
        {
            Element.Query<BindableElement>().ForEach(e => e.binding?.Update());
        }

        class TestWindow : EditorWindow
        {
            public PropertyElement Element { get; private set; }

            public void SetRoot(PropertyElement root)
            {
                Assert.That(root, Is.Not.Null);
                RemoveRoot();
                rootVisualElement.Add(root);
                Element = root;
            }

            public void RemoveRoot()
            {
                Element?.ClearTarget();
                Element?.RemoveFromHierarchy();
            }

            public static TestWindow NewInstance()
            {
                var wnd = CreateInstance<TestWindow>();
                wnd.Show();
                return wnd;
            }
        }
    }
}