namespace RichardSzalay.MockHttp
{
    /// <summary>
    /// Defines the behavior for processing BackendDefinitions when Request Expectations exist
    /// </summary>
    public enum BackendDefinitionBehavior
    {
        /// <summary>
        /// Will not match a Backend Definition if Request Expectations exist
        /// </summary>
        NoExpectations = 0,
        /// <summary>
        /// Will match Backend Definition if the next Request Expectation did not match
        /// </summary>
        Always = 1
    }
}
