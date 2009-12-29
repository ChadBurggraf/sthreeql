using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SThreeQL
{
    /// <summary>
    /// Represents the contents of an embedded SQL script.
    /// </summary>
    public class SqlScript
    {
        private string text;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the script.</param>
        public SqlScript(string name)
        {
            Name = name;
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
                if (text == null)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(String.Concat("SThreeQL.Sql.", Name)))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            text = reader.ReadToEnd();
                        }
                    }
                }

                return text;
            }
        }
    }
}
