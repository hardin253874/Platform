// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using EDC.ReadiNow.CAST.Contracts;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Services.ApplicationManager;
using Autofac;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Processing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.CAST.Test
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class PlatformServiceTests
    {
        private static bool? _installed;

        [Test]
        public void TestGetDatabaseId()
        {
            // Arrange
            var ps = new PlatformService();

            // Act
            var result = ps.GetDatabaseId();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void TestGetPlatformsByDatabaseId()
        {
            var mqQueryHelper = new Mock<ICastEntityHelper>();
            mqQueryHelper.Setup(m => m.GetEntityByField<ManagedPlatform>(It.IsAny<EntityRef>(), It.IsAny<string>()));
            
            using (var scope = Factory.Current.BeginLifetimeScope(b =>
            {
                b.RegisterInstance(mqQueryHelper.Object).As<ICastEntityHelper>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                var p = ps.GetPlatformByDatabaseId(TestPlatformId);
                p.Should().BeNull();

                // Assert
                mqQueryHelper.Verify(m => m.GetEntityByField<ManagedPlatform>(It.IsAny<EntityRef>(), It.IsAny<string>()), Times.Once);

                Action a1 = () => ps.GetPlatformByDatabaseId(null);
                a1.ShouldThrow<ArgumentNullException>();
            }
        }

        [Test]
        public void TestCreateOrUpdate()
        {
            ManagedPlatform p = null;
            PlatformFrontEnd fe = null;
            PlatformDatabase db = null;

            try
            {
                // Arrange
                var ps = new PlatformService();
                var pi = new RemotePlatformInfo
                {
                    Id = TestPlatformId,
                    FrontEndHost = "test",
                    FrontEndDomain = "platform.co",
                    Database = "db",
                    DatabaseServer = "ds",
                    Apps = new List<AvailableApplication>(),
                    Tenants = new TenantList()
                };
                var t = DateTime.UtcNow;

                // Act
                p = (ManagedPlatform)ps.CreateOrUpdate(pi);

                // Assert
                p.Should().NotBeNull();
                p.DatabaseId.Should().Be(pi.Id);
                p.Name.Should().Be(pi.Id.ToLowerInvariant());
                p.LastContact.Should().BeBefore(DateTime.UtcNow).And.BeAfter(t);
                p.ContainsTenants.Should().NotBeNull().And.BeEmpty();
                p.AvailableAppVersions.Should().NotBeNull().And.BeEmpty();
                p.FrontEndHistory.Should().NotBeNullOrEmpty();
                p.FrontEndHistory.Count.Should().Be(1);

                fe = p.FrontEndHistory.First();
                fe.Name.Should().Be("test.platform.co");
                fe.Host.Should().Be("test");
                fe.Domain.Should().Be("platform.co");
                fe.LastContact.Should().BeBefore(DateTime.UtcNow).And.BeAfter(t);

                p.DatabaseHistory.Should().NotBeNullOrEmpty();
                p.DatabaseHistory.Count.Should().Be(1);

                db = p.DatabaseHistory.First();
                db.Name.Should().Be("ds (db)");
                db.Catalog.Should().Be("db");
                db.Server.Should().Be("ds");
                db.LastContact.Should().BeBefore(DateTime.UtcNow).And.BeAfter(t);
            }
            finally
            {
                if (db != null)
                    Entity.Delete(db);
                if (fe != null)
                    Entity.Delete(fe);
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestCreateOrUpdateThrows()
        {
            ManagedPlatform p = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                Action f1 = () => { p = (ManagedPlatform)ps.CreateOrUpdate(null); };

                // Assert
                f1.ShouldThrow<ArgumentException>().WithMessage("Platform information was invalid.");

                Action f2 = () => { p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo()); };
                f2.ShouldThrow<ArgumentException>().WithMessage("Platform information was invalid.");

                Action f3 = () => { p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo { Id = TestPlatformId }); };
                f3.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: host");

                Action f4 = () => { p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo { Id = TestPlatformId, FrontEndHost = "HOST" }); };
                f4.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: catalog");
            }
            finally
            {
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestCreateOrUpdateTenant()
        {
            ManagedTenant t = null;

            try
            {
                // Arrange
                var ps = new PlatformService();
                var ti = new RemoteTenantInfo
                {
                    Name = TestTenantName
                };

                // Act
                t = (ManagedTenant)ps.CreateOrUpdateTenant(TestPlatformId, ti);
                
                // Assert
                t.Should().NotBeNull();
                t.Name.Should().Be(TestTenantName);
                t.RemoteId.Should().Be("0");
                t.Disabled.Should().BeFalse();
                t.Platform.Should().BeNull();
            }
            finally
            {
                if (t != null)
                    Entity.Delete(t);
            }
        }

        [Test]
        public void TestCreateOrUpdateTenantThrows()
        {
            ManagedTenant t = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                Action f1 = () => { t = (ManagedTenant)ps.CreateOrUpdateTenant(null, null); };

                // Assert
                f1.ShouldThrow<ArgumentException>().WithMessage("Tenant information was invalid.");

                Action f2 = () => { t = (ManagedTenant)ps.CreateOrUpdateTenant(null, new RemoteTenantInfo()); };
                f2.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: id");

                Action f3 = () => { t = (ManagedTenant)ps.CreateOrUpdateTenant(TestPlatformId, new RemoteTenantInfo()); };
                f3.ShouldNotThrow();
            }
            finally
            {
                if (t != null)
                    Entity.Delete(t);
            }
        }

        [Test]
        public void TestCreateOrUpdateUser()
        {
            ManagedPlatform p = null;
            ManagedTenant t = null;
            ManagedUser u = null;

            try
            {
                // Arrange
                var ps = new PlatformService();
                var ui = new RemoteUserInfo
                {
                    Name = TestUserName
                };

                // Act
                u = (ManagedUser)ps.CreateOrUpdateUser(TestPlatformId, TestTenantName, ui);

                // Assert
                u.Should().BeNull();
                
                p = CreateTestPlatform();
                t = CreateTestTenant(p);
                p.Save();

                u = (ManagedUser)ps.CreateOrUpdateUser(TestPlatformId, TestTenantName, ui);
                u.Should().BeNull();

                ui.RemoteId = TestUserRemoteId;

                u = (ManagedUser)ps.CreateOrUpdateUser(TestPlatformId, TestTenantName, ui);
                u.Should().NotBeNull();
                u.Name.Should().Be(TestUserName);
                u.RemoteId.Should().Be(TestUserRemoteId.ToString());
                u.Status_Enum.Should().Be(ManagedUserStatusEnumeration.Unknown);
                u.Tenant.Name.Should().Be(TestTenantName);
                u.Tenant.Platform.DatabaseId.Should().Be(TestPlatformId);

                var uid = u.Id;

                u = (ManagedUser)ps.CreateOrUpdateUser(TestPlatformId, TestTenantName, new RemoteUserInfo
                {
                    RemoteId = TestUserRemoteId,
                    Name = "Another Name",
                    Status = UserStatus.Expired
                });

                u.Should().NotBeNull();
                u.Id.Should().Be(uid);
                u.Name.Should().Be("Another Name");
                u.RemoteId.Should().Be(TestUserRemoteId.ToString());
                u.Status_Enum.Should().Be(ManagedUserStatusEnumeration.Expired);
                u.Tenant.Name.Should().Be(TestTenantName);
                u.Tenant.Platform.DatabaseId.Should().Be(TestPlatformId);
            }
            finally
            {
                if (u != null)
                    Entity.Delete(u);
                if (t != null)
                    Entity.Delete(t);
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestCreateOrUpdateUserThrows()
        {
            ManagedUser u = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                Action f1 = () => { u = (ManagedUser)ps.CreateOrUpdateUser(null, null, null); };

                // Assert
                f1.ShouldThrow<ArgumentException>().WithMessage("User information was invalid.");

                Action f2 = () => { u = (ManagedUser)ps.CreateOrUpdateUser(null, null, new RemoteUserInfo()); };
                f2.ShouldThrow<ArgumentException>().WithMessage("Value cannot be null.\r\nParameter name: id");

                Action f3 = () => { u = (ManagedUser)ps.CreateOrUpdateUser(TestPlatformId, null, new RemoteUserInfo()); };
                f3.ShouldNotThrow();
            }
            finally
            {
                if (u != null)
                    Entity.Delete(u);
            }
        }
        
        [Test]
        public void TestUpdateInstalledApplications()
        {
            ManagedPlatform p = null;
            ManagedTenant t = null;
            ManagedAppVersion v = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                t = (ManagedTenant)ps.UpdateInstalledApplications(TestPlatformId, TestTenantName, new List<InstalledApplication>());

                // Assert
                t.Should().BeNull();

                p = CreateTestPlatform();
                t = CreateTestTenant(p);
                p.Save();

                t = (ManagedTenant)ps.UpdateInstalledApplications(TestPlatformId, TestTenantName, new List<InstalledApplication>());
                t.Should().NotBeNull();
                t.Name.Should().Be(TestTenantName);
                t.Platform.DatabaseId.Should().Be(TestPlatformId);
                t.HasAppsInstalled.Count.Should().Be(0);

                var tid = t.Id;

                t = (ManagedTenant)ps.UpdateInstalledApplications(TestPlatformId, TestTenantName, new List<InstalledApplication>
                {
                    new InstalledApplication { ApplicationVersionId = TestAppVersionId }
                });

                t.Should().NotBeNull();
                t.Id.Should().Be(tid);
                t.Name.Should().Be(TestTenantName);
                t.Platform.DatabaseId.Should().Be(TestPlatformId);
                t.HasAppsInstalled.Count.Should().Be(1);

                v = t.HasAppsInstalled.First();
                v.Name.Should().BeNull();
                v.PublishDate.Should().NotHaveValue();
                v.Version.Should().BeNull();
                v.VersionId.Should().Be(TestAppVersionId);
                v.Application.Should().BeNull();
                v.RequiredApps.Should().BeEmpty();
                v.RequiredAppVersions.Should().BeEmpty();

                var vid = v.Id;

                t = (ManagedTenant)ps.UpdateInstalledApplications(TestPlatformId, TestTenantName, new List<InstalledApplication>
                {
                    new InstalledApplication
                    {
                        ApplicationVersionId = TestAppVersionId,
                        Name = "Test App Version",
                        ReleaseDate = new DateTime(2000, 10, 24),
                        PackageVersion = "2.3.3.0",
                        SolutionVersion = "1.7.7.0"
                    }
                });

                t.Should().NotBeNull();
                t.Id.Should().Be(tid);
                t.Name.Should().Be(TestTenantName);
                t.Platform.DatabaseId.Should().Be(TestPlatformId);
                t.HasAppsInstalled.Count.Should().Be(1);

                v = t.HasAppsInstalled.First();
                v.Id.Should().Be(vid);
                v.Name.Should().Be("Test App Version");
                v.PublishDate.Should().HaveValue().And.Be(new DateTime(2000, 10, 24));
                v.Version.Should().Be("1.7.7.0");
                v.VersionId.Should().Be(TestAppVersionId);
                v.Application.Should().BeNull();
                v.RequiredApps.Should().BeEmpty();
                v.RequiredAppVersions.Should().BeEmpty();
            }
            finally
            {
                if (v != null)
                    Entity.Delete(v);
                if (t != null)
                    Entity.Delete(t);
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestDeleteTenant()
        {
            ManagedPlatform p = null;
            ManagedTenant t = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                p = CreateTestPlatform();
                t = CreateTestTenant(p);
                p.Save();

                p.Should().NotBeNull();
                t.Should().NotBeNull();

                var pid = p.Id;
                var tid = t.Id;

                // Act
                ps.DeleteTenant(TestPlatformId, TestTenantName);

                // Assert
                var e1 = Entity.Get(pid);
                e1.Should().NotBeNull();

                var e2 = Entity.Get(tid);
                e2.Should().BeNull();
            }
            finally
            {
                if (t != null)
                    Entity.Delete(t);
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestDeleteUser()
        {
            ManagedPlatform p = null;
            ManagedTenant t = null;
            ManagedUser u = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                p = CreateTestPlatform();
                t = CreateTestTenant(p);
                u = CreateTestUser(t);
                p.Save();

                p.Should().NotBeNull();
                t.Should().NotBeNull();
                u.Should().NotBeNull();

                var pid = p.Id;
                var tid = t.Id;
                var uid = u.Id;

                // Act
                ps.DeleteUser(TestPlatformId, TestTenantName, TestUserName);

                // Assert
                var e1 = Entity.Get(pid);
                e1.Should().NotBeNull();

                var e2 = Entity.Get(tid);
                e2.Should().NotBeNull();

                var e3 = Entity.Get(uid);
                e3.Should().BeNull();
            }
            finally
            {
                if (u != null)
                    Entity.Delete(u);
                if (t != null)
                    Entity.Delete(t);
                if (p != null)
                    Entity.Delete(p);
            }
        }

        [Test]
        public void TestGetApp()
        {
            ManagedApp a = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                a = (ManagedApp)ps.GetApp(Guid.Empty);

                // Assert
                a.Should().BeNull();

                a = CreateTestApp();
                a.Save();
                
                var app = ps.GetApp(TestAppId);
                app.Should().NotBeNull();
                app.Id.Should().Be(a.Id);
                app.ApplicationId.Should().Be(TestAppId);
            }
            finally
            {
                if (a != null)
                    Entity.Delete(a);
            }
        }

        [Test]
        public void TestGetAppVersion()
        {
            ManagedAppVersion v = null;

            try
            {
                // Arrange
                var ps = new PlatformService();

                // Act
                v = (ManagedAppVersion)ps.GetAppVersion(Guid.Empty);

                // Assert
                v.Should().BeNull();

                v = CreateTestAppVersion();
                v.Save();

                var version = ps.GetAppVersion(TestAppVersionId);
                version.Should().NotBeNull();
                version.Id.Should().Be(v.Id);
                version.VersionId.Should().Be(TestAppVersionId);
            }
            finally
            {
                if (v != null)
                    Entity.Delete(v);
            }
        }

        [Test]
        [RunWithTransaction]
        public void TestUpdateApps()
        {
            // Arrange
            var g1 = new Guid("E94426B3-BD3C-4D74-99F8-6D9B8C753366");
            var g2 = new Guid("36B6ABDA-5259-46D2-A8F4-A98B2042D015");
            var a1 = new Guid("B198B5A1-EC72-4F03-B55B-899083180B1B");
            
            var p = CreateTestPlatform();

            var app1 = new ManagedApp
            {
                Name = "MyApp1",
                ApplicationId = a1
            };

            app1.Save();

            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppNullVersion",
                Application = app1,
                VersionId = null
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppEmptyVersion",
                Application = app1,
                VersionId = Guid.Empty
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppMissingVersion",
                Application = app1,
                VersionId = new Guid("B4DBC866-0BCF-40C7-8EB3-7406137C1588")
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppMatchedVersion",
                Application = app1,
                VersionId = g2
            });

            p.Save();
            p.AvailableAppVersions.Count.Should().Be(4);

            var pid = p.Id;

            var ps = new PlatformService();

            // Act
            p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo
            {
                Id = TestPlatformId,
                FrontEndHost = "test",
                FrontEndDomain = "platform.co",
                Database = "db",
                DatabaseServer = "ds",
                Apps = new List<AvailableApplication>
                {
                    new AvailableApplication
                    {
                        Name = "AppIncomingEmptyVersion",
                        ApplicationId = a1,
                        ApplicationVersionId = Guid.Empty
                    },
                    new AvailableApplication
                    {
                        Name = "AppIncomingNewVersion",
                        ApplicationId = a1,
                        ApplicationVersionId = g1
                    },
                    new AvailableApplication
                    {
                        Name = "AppIncomingExistingVersion",
                        ApplicationId = a1,
                        ApplicationVersionId = g2
                    }
                }
            });
            
            // Assert
            p.Should().NotBeNull();
            p.Id.Should().Be(pid);

            var result = p.AvailableAppVersions.ToList();
            result.Count.Should().Be(2);

            var v1 = p.AvailableAppVersions.FirstOrDefault(v => v.Name == "AppIncomingNewVersion");
            v1.Should().NotBeNull();
            v1.Application.Should().NotBeNull();
            v1.Application.Id.Should().Be(app1.Id);
            v1.Application.Name.Should().Be(app1.Name);
            v1.VersionId.Should().Be(g1);

            var v2 = p.AvailableAppVersions.FirstOrDefault(v => v.Name == "AppIncomingExistingVersion");
            v2.Should().NotBeNull();
            v2.Application.Should().NotBeNull();
            v2.Application.Id.Should().Be(app1.Id);
            v2.Application.Name.Should().Be(app1.Name);
            v2.VersionId.Should().Be(g2);
        }

        [Test]
        [RunWithTransaction]
        public void TestUpdateAppsWithVersionsOnly()
        {
            // Arrange
            var g = new Guid("36B6ABDA-5259-46D2-A8F4-A98B2042D015");

            var p = CreateTestPlatform();
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppNullVersion",
                VersionId = null
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppEmptyVersion",
                VersionId = Guid.Empty
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppMissingVersion",
                VersionId = new Guid("B4DBC866-0BCF-40C7-8EB3-7406137C1588")
            });
            p.AvailableAppVersions.Add(new ManagedAppVersion
            {
                Name = "AppMatchedVersion",
                VersionId = g
            });

            p.Save();
            p.AvailableAppVersions.Count.Should().Be(4);

            var pid = p.Id;
            var v1 = p.AvailableAppVersions.FirstOrDefault(v => v.Name == "AppMatchedVersion");

            var ps = new PlatformService();

            // Act
            p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo
            {
                Id = TestPlatformId,
                FrontEndHost = "test",
                FrontEndDomain = "platform.co",
                Database = "db",
                DatabaseServer = "ds",
                Apps = new List<AvailableApplication>
                {
                    new AvailableApplication
                    {
                        Name = "AppIncomingEmptyVersion",
                        ApplicationVersionId = Guid.Empty
                    },
                    new AvailableApplication
                    {
                        Name = "AppIncomingNewVersion",
                        ApplicationVersionId = new Guid("E94426B3-BD3C-4D74-99F8-6D9B8C753366")
                    },
                    new AvailableApplication
                    {
                        Name = "AppIncomingExistingVersion",
                        ApplicationVersionId = g
                    }
                }
            });

            // Assert
            p.Should().NotBeNull();
            p.Id.Should().Be(pid);

            var result = p.AvailableAppVersions.ToList();
            result.Count.Should().Be(2);

            var v2 = p.AvailableAppVersions.FirstOrDefault(v => v.Name == "AppIncomingNewVersion");
            v2.Should().NotBeNull();

            var v3 = p.AvailableAppVersions.FirstOrDefault(v => v.Name == "AppIncomingExistingVersion");
            v3.Should().NotBeNull();
            v3.Id.Should().Be(v1.Id);
        }
        
        [Test]
        [RunWithTransaction]
        public void TestUpdateTenants()
        {
            // Arrange
            var p = CreateTestPlatform();
            var t1 = new ManagedTenant { Name = "ContainsTenant", RemoteId = "1", Platform = p };
            var t2 = new ManagedTenant { Name = "OldTenant", RemoteId = "2", Platform = p };
            p.ContainsTenants.Add(t1);
            p.ContainsTenants.Add(t2);
            p.Save();

            var pid = p.Id;

            var ps = new PlatformService();

            // Act
            p = (ManagedPlatform)ps.CreateOrUpdate(new RemotePlatformInfo
            {
                Id = TestPlatformId,
                FrontEndHost = "test",
                FrontEndDomain = "platform.co",
                Database = "db",
                DatabaseServer = "ds",
                Tenants = new TenantList
                {
                    new RemoteTenantInfo { Name = "NewTenant", RemoteId = 3 },
                    new RemoteTenantInfo { Name = "", RemoteId = 8 },
                    new RemoteTenantInfo { Name = "RenamedTenant", RemoteId = 2 },
                    new RemoteTenantInfo { Name = null, RemoteId = 9 },
                    new RemoteTenantInfo { Name = "ContainsTenant", RemoteId = 1 }
                }
            });

            // Assert
            p.Should().NotBeNull();
            p.Id.Should().Be(pid);

            var tenants = p.ContainsTenants.ToList();
            tenants.Should().NotBeNull().And.NotBeEmpty();
            tenants.Count.Should().Be(5);

            var v1 = p.ContainsTenants.FirstOrDefault(v => v.RemoteId == "1");
            v1.Name.Should().Be("ContainsTenant");
            v1.Should().NotBeNull();

            var v2 = p.ContainsTenants.FirstOrDefault(v => v.RemoteId == "2");
            v2.Name.Should().Be("RenamedTenant");
            v2.Should().NotBeNull();

            var v3 = p.ContainsTenants.FirstOrDefault(v => v.RemoteId == "3");
            v3.Name.Should().Be("NewTenant");
            v3.Should().NotBeNull();

            var v4 = p.ContainsTenants.FirstOrDefault(v => v.RemoteId == "8");
            v4.Name.Should().BeEmpty();
            v4.Should().NotBeNull();

            var v5 = p.ContainsTenants.FirstOrDefault(v => v.RemoteId == "9");
            v5.Name.Should().BeNull();
            v5.Should().NotBeNull();
        }

        [Test]
        [RunWithTransaction]
        public void TestCastCreateOrUpdate()
        {
            // Arrange
            var knownAppId = Guid.NewGuid();
            var knownAppVersionId = Guid.NewGuid();
            var availableVersion = "1.0";
            var installedVersion = "2.0";
            var ps = new PlatformService();
            var pi = new RemotePlatformInfo
            {
                Id = TestPlatformId,
                FrontEndHost = "test",
                FrontEndDomain = "platform.co",
                Database = "db",
                DatabaseServer = "ds",
                Apps = new List<AvailableApplication>
                {
                    new AvailableApplication
                    {
                        Name = "testapp",
                        ApplicationId = Guid.NewGuid(),
                        ApplicationVersionId = Guid.NewGuid()
                    },
                    new AvailableApplication
                    {
                        Name = "knownapp",
                        ApplicationId = knownAppId,
                        ApplicationVersionId = knownAppVersionId,
                        PackageVersion = availableVersion
                    }
                },
                Tenants = new TenantList
                {
                    new RemoteTenantInfo
                    {
                        Name = "ttt",
                        Apps = new List<InstalledApplication>
                        {
                            new InstalledApplication
                            {
                                Name = "testinstalled",
                                ApplicationId = Guid.NewGuid(),
                                ApplicationVersionId = Guid.NewGuid()
                            },
                            new InstalledApplication
                            {
                                Name = "knowninstalled",
                                ApplicationId = knownAppId,
                                ApplicationVersionId = knownAppVersionId,
                                PackageVersion = availableVersion,
                                SolutionVersion = installedVersion
                            }
                        },
                        Users = new UserList
                        {
                            new RemoteUserInfo
                            {
                                Name = "alex",
                                RemoteId = 1
                            }
                        }
                    }
                }
            };

            // Act
            var p = ps.CreateOrUpdate(pi);

            // Assert
            p.Should().NotBeNull();
            p.DatabaseId.Should().Be(TestPlatformId);

            p.FrontEndHistory.Should().NotBeNullOrEmpty().And.NotContainNulls();
            var fe = p.FrontEndHistory.FirstOrDefault(f => f.Host == "test" && f.Domain == "platform.co");
            fe.Should().NotBeNull();

            p.DatabaseHistory.Should().NotBeNullOrEmpty().And.NotContainNulls();
            var db = p.DatabaseHistory.FirstOrDefault(f => f.Catalog == "db" && f.Server == "ds");
            db.Should().NotBeNull();

            p.ContainsTenants.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p.ContainsTenants.Count.Should().Be(1);
            var tn = p.ContainsTenants.FirstOrDefault(t => t.Name == "ttt");
            tn.Should().NotBeNull();

            tn.HasAppsInstalled.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tn.HasAppsInstalled.Count.Should().Be(2);
            var ia = tn.HasAppsInstalled.FirstOrDefault(i => i.Name == "testinstalled");
            ia.Should().NotBeNull();
            ia = tn.HasAppsInstalled.FirstOrDefault(i => i.Name == "knowninstalled");
            ia.Should().NotBeNull();
            ia.Version.Should().Be("2.0");

            tn.Users.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tn.Users.Count.Should().Be(1);
            var us = tn.Users.FirstOrDefault(u => u.Name == "alex");
            us.Should().NotBeNull();

            p.AvailableAppVersions.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p.AvailableAppVersions.Count.Should().Be(2);
            var ap = p.AvailableAppVersions.FirstOrDefault(a => a.Name == "testapp");
            ap.Should().NotBeNull();
            ap = p.AvailableAppVersions.FirstOrDefault(a => a.Name == "knownapp");
            ap.Should().NotBeNull();
            ap.Version.Should().Be("1.0");

            var apps = Entity.GetInstancesOfType<ManagedApp>().ToList();
            apps.Where(a => a.Name == "testapp").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "testinstalled").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "knownapp").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "knowninstalled").Select(a => a).Count().Should().Be(0);

            var pu = new RemotePlatformInfo
            {
                Id = TestPlatformId,
                FrontEndHost = "test2",
                FrontEndDomain = "platform.co",
                Database = "db",
                DatabaseServer = "ds",
                Apps = new List<AvailableApplication>
                {
                    new AvailableApplication
                    {
                        Name = "testapp2",
                        ApplicationId = Guid.NewGuid(),
                        ApplicationVersionId = Guid.NewGuid()
                    },
                    new AvailableApplication
                    {
                        Name = "knownapp",
                        ApplicationId = knownAppId,
                        ApplicationVersionId = knownAppVersionId,
                        PackageVersion = availableVersion
                    }
                },
                Tenants = new TenantList
                {
                    new RemoteTenantInfo
                    {
                        Name = "ttt",
                        Apps = new List<InstalledApplication>
                        {
                            new InstalledApplication
                            {
                                Name = "testinstalled2",
                                ApplicationId = Guid.NewGuid(),
                                ApplicationVersionId = Guid.NewGuid()
                            },
                            new InstalledApplication
                            {
                                Name = "knowninstalled",
                                ApplicationId = knownAppId,
                                ApplicationVersionId = knownAppVersionId,
                                PackageVersion = availableVersion,
                                SolutionVersion = installedVersion
                            }
                        },
                        Users = new UserList
                        {
                            new RemoteUserInfo { Name = "alex", RemoteId = 1 },
                            new RemoteUserInfo { Name = "gary", RemoteId = 2 }
                        }
                    }
                }
            };

            var p2 = ps.CreateOrUpdate(pu);

            p2.Should().NotBeNull();
            p2.DatabaseId.Should().Be(TestPlatformId);

            p2.FrontEndHistory.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p2.FrontEndHistory.Count.Should().Be(2);
            var fe2 = p2.FrontEndHistory.FirstOrDefault(f => f.Host == "test2" && f.Domain == "platform.co");
            fe2.Should().NotBeNull();
            fe2 = p2.FrontEndHistory.FirstOrDefault(f => f.Host == "test" && f.Domain == "platform.co");
            fe2.Should().NotBeNull();

            p2.DatabaseHistory.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p2.DatabaseHistory.Count.Should().Be(1);
            db = p2.DatabaseHistory.FirstOrDefault(f => f.Catalog == "db" && f.Server == "ds");
            db.Should().NotBeNull();

            p2.ContainsTenants.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p2.ContainsTenants.Count.Should().Be(1);
            tn = p2.ContainsTenants.FirstOrDefault(t => t.Name == "ttt");
            tn.Should().NotBeNull();

            tn.HasAppsInstalled.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tn.HasAppsInstalled.Count.Should().Be(2);
            ia = tn.HasAppsInstalled.FirstOrDefault(i => i.Name == "testinstalled2");
            ia.Should().NotBeNull();
            ia = tn.HasAppsInstalled.FirstOrDefault(i => i.Name == "knowninstalled");
            ia.Should().NotBeNull();
            ia.Version.Should().Be("2.0");

            tn.Users.Should().NotBeNullOrEmpty().And.NotContainNulls();
            tn.Users.Count.Should().Be(2);
            us = tn.Users.FirstOrDefault(u => u.Name == "gary");
            us.Should().NotBeNull();
            us = tn.Users.FirstOrDefault(u => u.Name == "alex");
            us.Should().NotBeNull();

            p.AvailableAppVersions.Should().NotBeNullOrEmpty().And.NotContainNulls();
            p.AvailableAppVersions.Count.Should().Be(2);
            ap = p.AvailableAppVersions.FirstOrDefault(a => a.Name == "testapp2");
            ap.Should().NotBeNull();
            ap = p.AvailableAppVersions.FirstOrDefault(a => a.Name == "knownapp");
            ap.Should().NotBeNull();
            ap.Version.Should().Be("1.0");

            apps = Entity.GetInstancesOfType<ManagedApp>().ToList();
            apps.Where(a => a.Name == "testapp").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "testinstalled").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "testapp2").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "testinstalled2").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "knownapp").Select(a => a).Count().Should().Be(1);
            apps.Where(a => a.Name == "knowninstalled").Select(a => a).Count().Should().Be(0);
        }

        [Test]
        [RunWithTransaction]
        public void TestPlatformResourceKey()
        {
            var myPlatformKey = "myPlatformKey";

            // first
            var managedPlatform = Entity.Create<ManagedPlatform>();

            managedPlatform.Name = myPlatformKey;
            managedPlatform.DatabaseId = myPlatformKey;
            managedPlatform.LastContact = DateTime.UtcNow;

            managedPlatform.Save();

            Entity.GetInstancesOfType<ManagedPlatform>().Count().Should().Be(1);

            // second
            var managedPlatform2 = Entity.Create<ManagedPlatform>();

            managedPlatform2.Name = myPlatformKey;
            managedPlatform2.DatabaseId = myPlatformKey;
            managedPlatform2.LastContact = DateTime.UtcNow;

            managedPlatform2.Save();

            Entity.GetInstancesOfType<ManagedPlatform>().Count().Should().Be(1);

            // third
            var managedPlatform3 = Entity.Create<ManagedPlatform>();

            managedPlatform3.Name = "Some other name";
            managedPlatform3.DatabaseId = myPlatformKey;
            managedPlatform3.LastContact = DateTime.UtcNow;

            managedPlatform3.Save();

            Entity.GetInstancesOfType<ManagedPlatform>().Count().Should().Be(1);

            // change the key
            var managedPlatform4 = Entity.Create<ManagedPlatform>();

            managedPlatform4.Name = myPlatformKey;
            managedPlatform4.DatabaseId = "Some other key";
            managedPlatform4.LastContact = DateTime.UtcNow;

            managedPlatform4.Save();

            Entity.GetInstancesOfType<ManagedPlatform>().Count().Should().Be(2);
        }

        #region Private Helper

        private static ManagedPlatform CreateTestPlatform()
        {
            var platform = Entity.Create<ManagedPlatform>();
            platform.DatabaseId = TestPlatformId;
            return platform;
        }

        private static ManagedTenant CreateTestTenant(IManagedPlatform platform)
        {
            var tenant = Entity.Create<ManagedTenant>();
            tenant.Name = TestTenantName;

            platform.ContainsTenants.Add(tenant);
            tenant.Platform = platform;

            return tenant;
        }

        private static ManagedUser CreateTestUser(IManagedTenant tenant)
        {
            var user = Entity.Create<ManagedUser>();
            user.RemoteId = TestUserRemoteId.ToString();
            user.Name = TestUserName;

            tenant.Users.Add(user);
            user.Tenant = tenant;

            return user;
        }

        private static ManagedApp CreateTestApp()
        {
            var app = Entity.Create<ManagedApp>();
            app.ApplicationId = TestAppId;
            return app;
        }

        private static ManagedAppVersion CreateTestAppVersion()
        {
            var version = Entity.Create<ManagedAppVersion>();
            version.VersionId = TestAppVersionId;
            return version;
        }

        private static void DeleteTestPlatform()
        {
            var entities = Entity.GetByField<ManagedPlatform>(TestPlatformId, ManagedPlatformSchema.DatabaseIdField);
            foreach (var entity in entities)
            {
                Entity.Delete(entity);
            }
        }

        private static void DeleteTestTenant()
        {
            var entities = Entity.GetByName<ManagedTenant>(TestUserName);
            foreach (var entity in entities)
            {
                Entity.Delete(entity);
            }
        }

        private static void DeleteTestUser()
        {
            var entities = Entity.GetByField<ManagedUser>(TestUserRemoteId.ToString(), ManagedUserSchema.RemoteIdField);
            foreach (var entity in entities)
            {
                Entity.Delete(entity);
            }
        }

        private static void DeleteTestApp()
        {
            var entities = Entity.GetByField<ManagedApp>(TestAppId.ToString(), ManagedAppSchema.AppIdField);
            foreach (var entity in entities)
            {
                Entity.Delete(entity);
            }
        }

        private static void DeleteTestAppVersion()
        {
            var entities = Entity.GetByField<ManagedAppVersion>(TestAppVersionId.ToString(), ManagedAppVersionSchema.AppVersionIdField);
            foreach (var entity in entities)
            {
                Entity.Delete(entity);
            }
        }

        [SetUp]
        public void TestSetUp()
        {
            using (new DeferredChannelMessageContext())
            {
                DeleteTestAppVersion();
                DeleteTestApp();
                DeleteTestUser();
                DeleteTestTenant();
                DeleteTestPlatform();
            }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            using (new DeferredChannelMessageContext())
            {
                _castWasInstalled = IsCastInstalled();
                if (_castWasInstalled)
                {
                    return;
                }

                var resource = Assembly.GetExecutingAssembly().GetName().Name + "." + CastApp;
                var output = GetCastAppLocation();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    if (stream == null)
                        throw new Exception("failed to extract lastest cast app");

                    var doc = new XmlDocument();
                    using (var fileStream = new FileStream(output, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);

                        try
                        {
                            fileStream.Position = 0;

                            doc.Load(fileStream);

                            // TODO : Consider making IDataSource public and using FileManager.CreateDataSource to get metadata

                            // Note: can't get xpath to work with default namespace
                            XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
                            ns.AddNamespace( "c", "core" );

                            string packageIdText;
                            string packageVerText;
                            string version = doc.DocumentElement.SelectSingleNode( "/c:xml/@version", ns ).Value;

                            switch ( version )
                            {
                                case "1.0":
                                    packageIdText = doc.DocumentElement.SelectSingleNode( "/c:xml/c:application/c:package/@id", ns )?.Value;
                                    packageVerText = doc.DocumentElement.SelectSingleNode( "/c:xml/c:application/c:package/c:version", ns )?.Value;
                                    break;
                                case "2.0":
                                    packageIdText = doc.DocumentElement.SelectSingleNode( "/c:xml/c:metadata/c:package/@id", ns )?.Value;
                                    packageVerText = doc.DocumentElement.SelectSingleNode( "/c:xml/c:metadata/c:package/c:version", ns )?.InnerText;
                                    break;
                                default:
                                    throw new Exception($"Unknown version {version}");
                            }

                            _castAppVersionId = new Guid( packageIdText );
                            _castAppVersion = packageVerText;
                        }
                        finally
                        {
                            fileStream.Close();
                        }
                    }
                }

                AppManager.ImportAppPackage(output);
                AppManager.DeployApp(RunAsDefaultTenant.DefaultTenantName, _castAppId.ToString("B"), _castAppVersion);
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (_castWasInstalled)
            {
                return;
            }

            using (new DeferredChannelMessageContext())
            {
                try
                {
                    AppManager.RemoveApp(RunAsDefaultTenant.DefaultTenantName, _castAppId.ToString("B"));
                }
                catch (Exception)
                {
                    // may or may not have been installed
                }

                AppManager.DeleteApp(_castAppVersionId);

                var output = GetCastAppLocation();
                if (File.Exists(output))
                {
                    try
                    {
                        File.Delete(output);
                    }
                    catch (Exception)
                    {
                        // in use. can't do much
                        Console.WriteLine(@"failed to clean up " + output);
                    }
                }
            }            
        }

        private static bool IsCastInstalled()
        {
            if (_installed.HasValue)
            {
                return _installed.Value;
            }

            try
            {
                using (var ctx = DatabaseContext.GetContext())
                {
                    const string query = @"SELECT TOP 1 [TenantId] FROM [Data_Alias] WITH (NOLOCK) WHERE [Data] = 'castApp' AND [Namespace] = 'cast' AND [TenantId] > 0";

                    using (var cmd = ctx.CreateCommand(query))
                    {
                        var result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            long t;
                            if (long.TryParse(result.ToString(), out t))
                            {
                                _installed = (t >= 0);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Let's not fail here. Just let the tests fail.
            }

            if (!_installed.HasValue)
            {
                _installed = false;
            }

            return _installed.Value;
        }

        private static string GetCastAppLocation()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(dir))
                throw new Exception("failed to derive output path for cast app");

            return Path.Combine(dir, CastApp);
        }

        private static Guid _castAppId = new Guid("{325cb03f-4bd8-4ffa-a07f-0f8ecfcd1db4}");

        private static Guid _castAppVersionId = Guid.Empty;

        private static string _castAppVersion = "";

        private static bool _castWasInstalled = false;

        private const string CastApp = "CAST.xml";

        private const string TestPlatformId = "foo";

        private const string TestTenantName = "Abc";

        private const string TestUserName = "AAA";

        private const long TestUserRemoteId = 99999;

        private static readonly Guid TestAppId = new Guid("{FE373999-663D-4592-9E36-8B6ABE5F2074}");

        private static readonly Guid TestAppVersionId = new Guid("{43A83AA3-9200-47A9-B8C5-D3082C208134}");
        
        #endregion
    }
}
