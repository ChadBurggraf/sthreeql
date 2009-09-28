using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SThreeQL
{
    /// <summary>
    /// Represents the result of an <see cref="ITask"/> execution.
    /// </summary>
    public class TaskExecutionResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TaskExecutionResult()
        {
            Success = true;
        }

        /// <summary>
        /// The exception thrown to cause the task to fail, if applicable.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// A value indicating whether the task was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
