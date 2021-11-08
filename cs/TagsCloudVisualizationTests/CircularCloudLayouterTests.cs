using System;
using System.Drawing;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TagsCloudVisualization;

namespace TagsCloudVisualizationTests
{
    public class CircularCloudLayouterTests
    {
        private readonly TagsPainter painter = new();
        private CircularCloudLayouter systemUnderTest;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!Directory.Exists("FailedTests"))
                Directory.CreateDirectory("FailedTests");
        }

        [SetUp]
        public void SetUp()
        {
            systemUnderTest = new CircularCloudLayouter(new Point(0, 0));
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed) return;

            var filepath = $"{Environment.CurrentDirectory}\\FailedTests\\{TestContext.CurrentContext.Test.Name}.png";
            painter.SaveToFile(filepath, systemUnderTest.GetLayout());
            Console.WriteLine($"Tag cloud visualization saved to file {filepath}");
        }

        [Test]
        public void LayoutIsEmpty_BeforePutNextRectangle()
        {
            systemUnderTest.GetLayout().Should().BeEmpty();
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(5000)]
        public void PutNextRectangle_PutRectanglesWithoutIntersections(int layoutLength)
        {
            systemUnderTest.GenerateRandomLayout(layoutLength);
            var layout = systemUnderTest.GetLayout();

            var hasIntersections = layout.Any(x => layout.Any(y => x.IntersectsWith(y) && x != y));
            hasIntersections.Should().BeFalse();
        }

        [Test]
        public void PutNextRectangle_PutFirstRectangleOnCenter()
        {
            var size = new Size(100, 100);
            var expectedLocation = systemUnderTest.Center - size / 2;

            var rectangle = systemUnderTest.PutNextRectangle(size);

            rectangle.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Test]
        public void PutNexRectangle_ShouldThrowArgumentException_WhenRectangleSizeLessThanZero()
        {
            var invalidSize = new Size(-1, -1);

            Action action = () => systemUnderTest.PutNextRectangle(invalidSize);

            action.Should().Throw<ArgumentException>().WithMessage("Rectangle sizes must be greater than zero");
        }

        [Test]
        public void PutNextRectangle_ShouldCreateTightLayout()
        {
            systemUnderTest.GenerateRandomLayout(1000);
            var radius = systemUnderTest.CalculateLayoutRadius();

            var circleArea = Math.PI * radius * radius;
            var rectanglesArea = systemUnderTest.GetLayout()
                .Aggregate(0.0, (current, rectangle) => current + rectangle.Height * rectangle.Width);
            rectanglesArea.Should().BeInRange(0.8 * circleArea, circleArea);
        }

        [Test]
        public void PutNextRectangle_ShouldCreateCircleLikeLayout()
        {
            const double expectedRadius = 58.309519;
            const double epsilon = 0.000001;

            systemUnderTest.GenerateLayoutOfSquares(100);

            var actualRadius = systemUnderTest.CalculateLayoutRadius();
            actualRadius.Should().BeInRange(expectedRadius - epsilon, expectedRadius + epsilon);
        }
    }
}