﻿namespace Serilog.Exceptions.Test.Filters
{
    using System;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Filters;
    using Xunit;

    public class CompositeExceptionPropertyFilterTest
    {
        [Fact]
        public void CreationOfCompositeFilter_ForNullFilters_Throws()
        {
            // Arrange
            Action act = () => new CompositeExceptionPropertyFilter(null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void CreationOfCompositeFilter_ForEmptyFilters_Throws()
        {
            // Arrange
            Action act = () => new CompositeExceptionPropertyFilter(new IExceptionPropertyFilter[] { });

            // Act & Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void CreationOfCompositeFilter_ForOneNullFilter_Throws()
        {
            // Arrange
            Action act = () => new CompositeExceptionPropertyFilter(new IExceptionPropertyFilter[] { null });

            // Act
            var ex = Assert.Throws<ArgumentException>(act);

            // Assert
            Assert.Contains("index 0", ex.Message);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenFirstFilterFilters_Filters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "B",
                1);

            // Assert
            Assert.True(shouldFilter);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenSecondFilterFilters_Filters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "A",
                1);

            // Assert
            Assert.True(shouldFilter);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenNoFilterFilters_NotFilters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "C",
                1);

            // Assert
            Assert.False(shouldFilter);
        }
    }
}
