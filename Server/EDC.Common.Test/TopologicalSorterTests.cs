// Copyright 2011-2016 Global Software Innovation Pty Ltd
using FluentAssertions;
using NUnit.Framework;

namespace EDC.Test
{
    /// <summary>
    /// Tests for the topologics sorter used mainly for sorting apps into their correct install order.
    /// </summary>
    [TestFixture]
    public class TopologicalSorterTests
    {
        [Test]
        public void TestMethod1()
        {
            // Arrange
            int[] idx = { 0, 1, 2, 3, 4, 5 };
            var sorter = new TopologicalSorter(idx.Length);
            sorter.AddVertex(0);
            sorter.AddVertex(1);
            sorter.AddVertex(2);
            sorter.AddVertex(3);
            sorter.AddVertex(4);
            sorter.AddVertex(5);
            sorter.AddEdge(5, 2);
            sorter.AddEdge(5, 0);
            sorter.AddEdge(4, 0);
            sorter.AddEdge(4, 1);
            sorter.AddEdge(2, 3);
            sorter.AddEdge(3, 1);

            // Act
            var result = sorter.Sort();

            // Assert
            result.Length.Should().Be(6);
            result.Should().ContainInOrder(new[] {5, 4, 2, 3, 1, 0});
        }
    }
}
