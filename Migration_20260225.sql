-- ============================================================
-- AirtimeDistributionCloud - Migration Script
-- Generated: 2026-02-25
-- Database: AirtimeDistributionCloud
-- Purpose: Add AssetManager role, reorganize permission keys,
--          seed default permissions for new role
-- ============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

-- ============================================================
-- STEP 1: Add AssetManager role (if not exists)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [NormalizedName] = 'ASSETMANAGER')
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'AssetManager', 'ASSETMANAGER', NEWID());
    PRINT '>> Added AssetManager role.';
END
ELSE
    PRINT '>> AssetManager role already exists.';

-- ============================================================
-- STEP 2: Update permission keys (rename old keys to new ones)
-- ============================================================

-- Rename admin.reports -> reports.reports
UPDATE [PagePermissions]
SET [PageKey] = 'reports.reports'
WHERE [PageKey] = 'admin.reports';

-- Rename admin.analytics -> reports.analytics
UPDATE [PagePermissions]
SET [PageKey] = 'reports.analytics'
WHERE [PageKey] = 'admin.analytics';

-- Rename admin.assets -> assets.registry
UPDATE [PagePermissions]
SET [PageKey] = 'assets.registry'
WHERE [PageKey] = 'admin.assets';

-- Rename admin.asset-categories -> assets.categories
UPDATE [PagePermissions]
SET [PageKey] = 'assets.categories'
WHERE [PageKey] = 'admin.asset-categories';

PRINT '>> Updated permission keys (reports.*, assets.*).';

-- ============================================================
-- STEP 3: Seed default AssetManager permissions
-- ============================================================

-- AssetManager -> assets.registry
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'AssetManager' AND [PageKey] = 'assets.registry'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('AssetManager', 'assets.registry', 1);
END

-- AssetManager -> assets.categories
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'AssetManager' AND [PageKey] = 'assets.categories'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('AssetManager', 'assets.categories', 1);
END

PRINT '>> Seeded AssetManager default permissions.';

-- ============================================================
-- STEP 4: Ensure SuperAdministrator has new key permissions
-- ============================================================

-- SuperAdministrator -> reports.reports (may already exist from rename)
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'SuperAdministrator' AND [PageKey] = 'reports.reports'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('SuperAdministrator', 'reports.reports', 1);
END

-- SuperAdministrator -> reports.analytics
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'SuperAdministrator' AND [PageKey] = 'reports.analytics'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('SuperAdministrator', 'reports.analytics', 1);
END

-- SuperAdministrator -> assets.registry
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'SuperAdministrator' AND [PageKey] = 'assets.registry'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('SuperAdministrator', 'assets.registry', 1);
END

-- SuperAdministrator -> assets.categories
IF NOT EXISTS (
    SELECT 1 FROM [PagePermissions]
    WHERE [RoleName] = 'SuperAdministrator' AND [PageKey] = 'assets.categories'
)
BEGIN
    INSERT INTO [PagePermissions] ([RoleName], [PageKey], [IsAllowed])
    VALUES ('SuperAdministrator', 'assets.categories', 1);
END

PRINT '>> Verified SuperAdministrator permissions for new keys.';

-- ============================================================
-- STEP 5: Remove deprecated Arabic company settings (if exist)
-- ============================================================

DELETE FROM [SystemSettings]
WHERE [Key] IN ('CompanyNameAr', 'CompanyAbbreviationAr', 'CompanyAddressAr');

PRINT '>> Removed deprecated Arabic company settings (if any existed).';

-- ============================================================
-- DONE
-- ============================================================

COMMIT TRANSACTION;
PRINT '>> Migration script completed successfully.';
