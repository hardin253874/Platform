// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Services
{
    /// <summary>
    /// Implements services related to account management in CAST related systems.
    /// </summary>
    public class AccountService : IAccountService
    {
        private IEntityRepository EntityRepository { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public AccountService()
        {
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
        }

        /// <summary>
        /// Verifies that a new customer is valid and allows account creation to continue.
        /// </summary>
        /// <param name="token">The verification token.</param>
        /// <returns>The verified customer.</returns>
        public ManagedCustomer Verify(string token)
        {
            return default(ManagedCustomer);
            //var link = EntityModel.GetByName<VerificationLink>(token).FirstOrDefault();
            //if (link == null)
            //{
            //    throw new EntityNotFoundException(string.Format("Verification Link [token={0}]", token));
            //}

            //var customer = link.ManagedCustomer.AsWritable<ManagedCustomer>();
            //if (customer == null)
            //{
            //    throw new EntityNotFoundException(string.Format("Managed Customer [link={0}]", link.Id));
            //}

            //var tasks = EntityModel.GetInstancesOfType<DisplayFormUserTask>().Where(t => t.Name == "Approve New Customer" && t.RecordToPresent.Id == customer.Id).ToList();

            //foreach (var task in tasks)
            //{
            //    var writeableTask = task.AsWritable<DisplayFormUserTask>();
            //    if (writeableTask == null)
            //    {
            //        continue;
            //    }

            //    var transitions = writeableTask.AvailableTransitions.ToList();
            //    if (transitions.Count == 0)
            //    {
            //        continue;
            //    }

            //    writeableTask.UserResponse = transitions.OrderBy(t => t.FromExitPoint.ExitPointOrdinal).First();
            //    writeableTask.UserTaskCompletedOn = DateTime.UtcNow;
            //    writeableTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
            //    //writeableTask.UserTaskIsComplete = true;
            //    writeableTask.Save();
            //}

            //customer.Verified = true;
            //customer.VerifiedBy = link.MailedTo;
            //customer.Save();

            //return customer;
        }

        /// <summary>
        /// Rejects the creation of a new account from customer registration details.
        /// </summary>
        /// <param name="token">The verification token.</param>
        public void Reject(string token)
        {
            //var link = EntityModel.GetByName<VerificationLink>(token).FirstOrDefault();
            //if (link == null)
            //{
            //    throw new EntityNotFoundException(string.Format("Verification Link [token={0}]", token));
            //}
        }

        private void EncryptCustomerPassword()
        {
            //var managedCustomerId = Entity.GetId("cast:managedCustomer");
            //var managedCustomerPasswordId = Entity.GetId("cast:customerPasswordField");
            //var managedCustomers = entities.Where(e => e.TypeIds.Contains(managedCustomerId)).ToList();
            //var cryptoProvider = new EncodingCryptoProvider();

            //foreach (var managedCustomer in managedCustomers)
            //{
            //    var managedCustomerInternal = managedCustomer as IEntityInternal;
            //    if (managedCustomerInternal == null)
            //    {
            //        continue;
            //    }

            //    // Always set the "name" to be the email or user name
            //    var email = managedCustomer.GetField("cast:customerEmailField");
            //    managedCustomer.SetField("core:name", email);

            //    // Encrypt the password if necessary
            //    var field = managedCustomer.GetField(managedCustomerPasswordId);
            //    if (field == null)
            //    {
            //        continue;
            //    }

            //    var newPassword = field as string;
            //    if (newPassword == null)
            //    {
            //        continue;
            //    }

            //    var newEncryptedPassword = cryptoProvider.EncryptAndEncode(newPassword);

            //    if (!managedCustomerInternal.IsTemporaryId)
            //    {
            //        var savedCredential = Entity.Get(managedCustomer.Id);
            //        if (savedCredential != null)
            //        {
            //            var oldEncryptedPassword = savedCredential.GetField(managedCustomerPasswordId) as string;

            //            // has the password actually changed?
            //            if (newPassword == oldEncryptedPassword)
            //            {
            //                continue;
            //            }
            //        }
            //    }

            //    // set the newly encrypted password back into the entity
            //    managedCustomer.SetField(managedCustomerPasswordId, newEncryptedPassword);
            //}
        }

        private void UpdateVerifyLink()
        {
            //foreach (var link in from entity in entities where entity.Is<VerificationLink>() select entity.AsWritable<VerificationLink>())
            //{
            //    var linkInternal = link as IEntityInternal;
            //    if (linkInternal == null)
            //    {
            //        continue;
            //    }

            //    if (linkInternal.IsTemporaryId)
            //    {
            //        var baseAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;

            //        var r = new Random((int)DateTime.Now.Ticks);
            //        var bytes = new byte[8];
            //        r.NextBytes(bytes);

            //        var token = HttpServerUtility.UrlTokenEncode(bytes);

            //        link.Name = token;
            //        link.URI = string.Format(@"https://{0}/cast/v1/accounts/{1}/verify", baseAddress, token);
            //    }
            //}
        }
    }
}
