using System;
using System.Configuration;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents the configuration for a target's on-complete execution action.
    /// </summary>
    public class OnCompleteConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the arguments to pass to the executable, if applicable.
        /// </summary>
        [ConfigurationProperty("arguments", IsRequired = false)]
        public string Arguments
        {
            get { return (string)this["arguments"]; }
        }

        /// <summary>
        /// Gets the executable or command to run.
        /// </summary>
        [ConfigurationProperty("executable", IsRequired = true)]
        public string Executable
        {
            get { return (string)this["executable"]; }
        }

        /// <summary>
        /// Gets the working directory to execute the action in, if applicable.
        /// </summary>
        [ConfigurationProperty("workingDirectory", IsRequired = false)]
        public string WorkingDirectory
        {
            get { return (string)this["workingDirectory"]; }
        }
    }
}
