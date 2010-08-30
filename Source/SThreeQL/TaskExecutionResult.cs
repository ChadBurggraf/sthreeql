//-----------------------------------------------------------------------
// <copyright file="TaskExecutionResult.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using SThreeQL.Configuration;

    /// <summary>
    /// Represents the result of a <see cref="Task"/> execution.
    /// </summary>
    public class TaskExecutionResult
    {
        /// <summary>
        /// Initializes a new instance of the TaskExecutionResult class.
        /// </summary>
        public TaskExecutionResult()
        {
            this.Success = true;
        }

        /// <summary>
        /// Gets or sets the exception thrown to cause the task to fail, if applicable.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the task was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the task's target.
        /// </summary>
        public DatabaseTargetConfigurationElement Target { get; set; }
    }
}
