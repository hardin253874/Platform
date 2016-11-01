using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Marks up classes generated as part of the entity model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ModelClassAttribute : Attribute
    {
		/// <summary>
		/// Creates a new QueryEngineAttribute.
		/// </summary>
		/// <param name="typeAlias">The type alias.</param>
        public ModelClassAttribute(string typeAlias)
        {
            TypeAlias = typeAlias;
        }

        /// <summary>
        /// The alias for this type.
        /// </summary>
        public string TypeAlias { get; set; }
    }
}
