using System;
using System.Threading.Tasks;
using Xunit;
using static ErrorHandling.ExceptionUtilities;

namespace UnitTests.ErrorHandling
{
    public sealed class ExceptionUtilitiesTests
    {
        private readonly Exception _generalException = new Exception("first level", new Exception("second level", new Exception("third level")));
        private readonly Exception _taskCancellation1 = new Exception("first level", new TaskCanceledException("second level", new Exception("third level")));
        private readonly Exception _taskCancellation2 = new Exception("first level", new Exception("second level", new TaskCanceledException("third level")));

        [Fact]
        public void HasException_AsExpected()
        {
            Assert.False(HasException<TaskCanceledException>(_generalException));
            Assert.True(HasException<TaskCanceledException>(_taskCancellation1));
            Assert.True(HasException<TaskCanceledException>(_taskCancellation2));
        }

        [Fact]
        public void GetInnermostException_AsExpected()
        {
            Assert.Equal("third level", GetInnermostException(_generalException).Message);
            Assert.Equal("third level", GetInnermostException(_taskCancellation1).Message);
            Assert.Equal("third level", GetInnermostException(_taskCancellation2).Message);
        }
    }
}