using System.Collections.Generic;

namespace KoBeLU.Schema
{
    /// <summary>
    /// A workflow contains all information for a contextual learning scenario
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// Gets or sets the ID of this workflow
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL of a preview image
        /// </summary>
        public string ThumbailUrl { get; set; }

        /// <summary>
        /// Gets or sets the steps
        /// </summary>
        public IList<WorkingStep> Steps { get; set; }

    }


}
