namespace beta.Models
{
    /// <summary>
    /// Class for properties
    /// </summary>
    public class PropertyFilterDescription
    {
        public PropertyFilterDescription(string displayedProperty, string property)
        {
            DisplayedProperty = displayedProperty;
            Property = property;
        }

        public string DisplayedProperty { get; }
        public string Property { get; }
    }
}
