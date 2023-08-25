using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace SolidSpace.Profiling.Editor.Tests
{
    internal class ProfilingManagerTests
    {
        private ProfilingManager _manager;
        private ProfilingConfig _config;
        private List<ProfilingNode> _nodes;
        private int _totalNodeCount;

        [SetUp]
        public void SetUp()
        {
            _nodes = new List<ProfilingNode>();
            _config = new ProfilingConfig(true, true, 2, 2);
            _manager = new ProfilingManager(_config);
            _totalNodeCount = 0;
            _manager.Initialize();
        }
        
        [Test]
        public void GetHandle_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.GetHandle(null));
        }

        [Test]
        public void OnBeginSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.OnBeginSample(null));
        }

        [Test]
        public void OnEndSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.OnEndSample(null));
        }

        [Test]
        public void OnUpdate_WithoutSamples_HasRootOnly()
        {
            UpdateManager_ReadResults();
            Assert.That(_totalNodeCount, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootDeepIsZero()
        {
            UpdateManager_ReadResults();
            Assert.That(_nodes[0].deep, Is.EqualTo(0));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootNameIsCorrect()
        {
            UpdateManager_ReadResults(); 
            Assert.That(_nodes[0].name, Is.EqualTo("_root"));
        }

        [Test]
        public void OnUpdate_WithDummy_HasRootAndDummyOnly()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_totalNodeCount, Is.EqualTo(2));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyDeepIsOne()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_nodes[1].deep, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyNameIsCorrect()
        {
            BeginEndDummySample_UpdateManager_ReadResults();
            Assert.That(_nodes[1].name, Is.EqualTo("TestSample"));
        }

        [Test]
        public void OnBeginSample_WithRecordLimit_ThrowsException()
        {
            for (var i = 0; i < _config.MaxRecordCount; i++)
            {
                _manager.OnBeginSample("TestSample");
            }
            Assert.Throws<OutOfMemoryException>(() => _manager.OnBeginSample("TestSample"));
        }

        [Test]
        public void OnEndSample_WithRecordLimit_ThrowsException()
        {
            for (var i = 0; i < _config.MaxRecordCount; i++)
            {
                _manager.OnEndSample("TestSample");
            }
            Assert.Throws<OutOfMemoryException>(() => _manager.OnEndSample("TestSample"));
        }

        [Test]
        public void OnBeginSample_WithoutOnEndSample_WhenUpdate_ThrowsException_WithDummyPath()
        {
            _manager.OnBeginSample("TestSample");
            var exception = Assert.Throws<InvalidOperationException>(() => _manager.Update());
            Assert.That(exception.Message, Does.Contain("_root/TestSample"));
        }

        [Test]
        public void OnBeginSample_WithOverflow_WhenUpdate_ThrowsException_WithPath()
        {
            _manager.OnBeginSample("SampleA");
            _manager.OnBeginSample("SampleB");
            var exception = Assert.Throws<StackOverflowException>(() => _manager.Update());
            Assert.That(exception.Message, Does.Contain("_root/SampleA/SampleB"));
        }

        [Test]
        public void OnEndSample_WithoutOnBeginSample_WhenUpdate_ThrowsException_WithPath()
        {
            _manager.OnEndSample("TestSample");
            var exception = Assert.Throws<InvalidOperationException>(() => _manager.Update());
            Assert.That(exception.Message, Does.Contain("_root/TestSample"));
        }

        [Test]
        public void OnBeginSample_OnEndSample_WithDifferentNames_WhenUpdate_ThrowsException_WithPaths()
        {
            _manager.OnBeginSample("SampleA");
            _manager.OnEndSample("SampleB");
            var exception = Assert.Throws<InvalidOperationException>(() => _manager.Update());
            Assert.That(exception.Message, Does.Contain("_root/SampleA").And.Contain("_root/SampleB"));
        }

        [Test]
        public void OnBeginSample_OnEndSample_WithTimeDelay_TimeIsCorrect()
        {
            var timer = new Stopwatch();
            
            timer.Start();
            _manager.OnBeginSample("TestSample");
            Thread.SpinWait((int)(Stopwatch.Frequency * 0.001f));
            _manager.OnEndSample("TestSample");
            timer.Stop();
            
            _manager.Update();
            _manager.Reader.Read(0, 2, _nodes, out _totalNodeCount);

            var actualTime = _nodes[1].time;
            var expectedTime = timer.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            Assert.That(actualTime, Is.EqualTo(expectedTime).Within(actualTime * 0.01f));
        }

        private void BeginEndDummySample_UpdateManager_ReadResults()
        {
            _manager.OnBeginSample("TestSample");
            _manager.OnEndSample("TestSample");
            _manager.Update();

            _manager.Reader.Read(0, 2, _nodes, out _totalNodeCount);
        }

        private void UpdateManager_ReadResults()
        {
            _manager.Update();
            
            _manager.Reader.Read(0, 1, _nodes, out _totalNodeCount);
        }

        [TearDown]
        public void TearDown()
        {
            _manager.FinalizeObject();
        }
    }
}