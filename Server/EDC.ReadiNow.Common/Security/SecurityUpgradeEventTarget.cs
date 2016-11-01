// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security
{
    public class SecurityUpgradeEventTarget : IEntityEventUpgrade
    {
        /// <summary>
        /// Called after upgrade.
        /// </summary>
        public void OnAfterUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            string[] entitiesToDelete =
            {
                "core:adminFullAuthorization",
                "core:testUserAccount",
                "core:reportTestPageType"
            };

            using (new SecurityBypassContext())
            {
                foreach (string entityToDelete in entitiesToDelete)
                {
                    try
                    {
                        if (Entity.Exists(entityToDelete))
                        {
                            Entity.Delete(entityToDelete);
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteWarning(
                            "Delete entity {0} during upgrade failed: {1}",
                            entityToDelete,
                            ex.Message
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Called before upgrade.
        /// </summary>
        public bool OnBeforeUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // Do not cancel upgrade.
            return false;
        }
    }
}
