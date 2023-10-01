using System;

namespace RichardSzalay.MockHttp
{
    /// <summary>
    /// Exception thrown by the ThrowMatchSummary extension method, indicating that the request could
    /// not be matched against any of the mocked requests
    /// </summary>
    public class MockHttpMatchException : Exception
    {
        /// <summary>
        /// Creates a new instance of MockHttpMatchException
        /// </summary>
        public MockHttpMatchException(string message)
            : base(message)
        {
                
        }
    }
}
