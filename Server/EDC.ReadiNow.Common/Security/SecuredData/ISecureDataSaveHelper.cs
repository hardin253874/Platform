using EDC.ReadiNow.Model;
using System;

namespace EDC.ReadiNow.Security.SecuredData
{
    /// <summary>
    /// Interface for helping saving of SecureData
    /// </summary>
    public interface ISecuredDataSaveHelper
    {
        /// <summary>
        ///    This takes the value in the data field and pushed into secureData
        /// </summary>
        void UpgradeField(string securedDataContext, EntityType entityType, EntityRef dataField, EntityRef secureIdField, Func<string, string> decrytor);

        /// <summary>
        ///     Move all the passwords into secure storage, replacing the data field with the default value.
        /// </summary>
        void OnBeforeSave(string securedDataContext, EntityRef dataField, EntityRef secureIdField, IEntity entity);
    }
}