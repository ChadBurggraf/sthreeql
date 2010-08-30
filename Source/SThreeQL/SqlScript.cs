//-----------------------------------------------------------------------
// <copyright file="SqlScript.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Represents the contents of an embedded SQL script.
    /// </summary>
    public class SqlScript
    {
        private string text;

        /// <summary>
        /// Initializes a new instance of the SqlScript class.
        /// </summary>
        /// <param name="name">The name of the script.</param>
        public SqlScript(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the script.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the script's text contents.
        /// </summary>
        public string Text
        {
            get
            {
                if (this.text == null)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(String.Concat("SThreeQL.Sql.", this.Name)))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            this.text = reader.ReadToEnd();
                        }
                    }
                }

                return this.text;
            }
        }
    }
}
