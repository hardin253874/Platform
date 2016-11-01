// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Services.ReportTemplate;
using EDC.ReadiNow.Model;
using Model = EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Services.Test.Reporting
{
    [TestFixture]
    public class ReportTemplateInterfaceTests
    {
        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void VerifyResourceTypes_TemplateAppliesToNoType( )
        {
            var template = new Model.ReportTemplate( );
            
            var settings = new ReportTemplateSettings( );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [ExpectedException]
        public void VerifyResourceTypes_NullResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:person" );
            
            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = null;

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [ExpectedException]
        public void VerifyResourceTypes_NoResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:person" );

            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = new List<EntityRef>( );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [ExpectedException]
        public void VerifyResourceTypes_WrongResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:person" );

            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = new List<EntityRef>( );
            settings.SelectedResources.Add( new EntityRef( "test:aaCoke" ) );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void VerifyResourceTypes_RightExactResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:drink" );

            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = new List<EntityRef>( );
            settings.SelectedResources.Add( new EntityRef( "test:aaCoke" ) );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void VerifyResourceTypes_RightDerivedResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:person" );

            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = new List<EntityRef>( );
            settings.SelectedResources.Add( new EntityRef( "test:aaronWitte" ) );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [ExpectedException]
        public void VerifyResourceTypes_AncestorResourcesProvided( )
        {
            var template = new Model.ReportTemplate( );
            template.ReportTemplateAppliesToType = Entity.Get<EntityType>( "test:manager" );

            var settings = new ReportTemplateSettings( );
            settings.SelectedResources = new List<EntityRef>( );
            settings.SelectedResources.Add( new EntityRef( "test:aaDavidQuint" ) );

            ReportTemplateInterface.VerifyResourceTypes( settings, template );
        }
    }
}