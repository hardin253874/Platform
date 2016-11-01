ReadiNow.TenantHealth.Test
~~~~~~~~~~~~~~~~~~~~~~~~~~

This test assembly is special.

This assembly is for validating that the contents of a tenant are in good and working order.

It is intended that it will be run against production tenants, and as such the content of the tenant varies.

Tests:
1. Should run against all tenants
2. Should be non-disruptive to the data
3. This assembly must not reference the other test assemblies - because they won't always be present.

Refer to notes at:
http://spwiki.sp.local/display/DEV/2016-07-13+Automated+Upgrade+Verification
