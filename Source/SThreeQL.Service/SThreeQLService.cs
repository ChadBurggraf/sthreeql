//-----------------------------------------------------------------------
// <copyright file="SThreeQLService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Service
{
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Provides service-based execution of SThreeQL tasks.
    /// </summary>
    public sealed class SThreeQLService
    {
        /// <summary>
        /// Prevents a default instance of the SThreeQLService class from being created.
        /// </summary>
        private SThreeQLService()
        {
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main()
        {
            ServiceBase.Run(new ServiceBase[] { new SThreeQLProcessor() });
        }
    }
}
