-- ============================================================
-- AirtimeDistributionCloud - Clear All Tables Script
-- Generated: 2026-02-24
-- Database: AirtimeDistributionCloud
-- Purpose: Clear all data from all tables (no sample data)
-- ============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

-- ============================================================
-- STEP 1: CLEAR DATA (respecting FK order - children first)
-- ============================================================

-- Asset module
DELETE FROM [AssetMaintenanceRecords];
DELETE FROM [AssetAssignments];
DELETE FROM [AssetCategories];
DELETE FROM [Assets];

-- Transfer & deposit module
DELETE FROM [AirtimeTransfers];
DELETE FROM [AirtimeDeposits];
DELETE FROM [CashDeposits];

-- Dealer module
DELETE FROM [DealerProducts];
DELETE FROM [Dealers];

-- Other modules
DELETE FROM [Expenses];
DELETE FROM [Products];
DELETE FROM [AuditLogs];

-- Branch module (after all FK dependents are cleared)
DELETE FROM [Branches];

PRINT '>> All tables cleared.';

-- ============================================================
-- STEP 2: RESET IDENTITY SEEDS
-- ============================================================

DBCC CHECKIDENT ('AssetMaintenanceRecords', RESEED, 0);
DBCC CHECKIDENT ('AssetAssignments', RESEED, 0);
DBCC CHECKIDENT ('AssetCategories', RESEED, 0);
DBCC CHECKIDENT ('Assets', RESEED, 0);
DBCC CHECKIDENT ('AirtimeTransfers', RESEED, 0);
DBCC CHECKIDENT ('AirtimeDeposits', RESEED, 0);
DBCC CHECKIDENT ('CashDeposits', RESEED, 0);
DBCC CHECKIDENT ('DealerProducts', RESEED, 0);
DBCC CHECKIDENT ('Dealers', RESEED, 0);
DBCC CHECKIDENT ('Expenses', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('AuditLogs', RESEED, 0);
DBCC CHECKIDENT ('Branches', RESEED, 0);

PRINT '>> Identity seeds reset.';

-- ============================================================
-- DONE
-- ============================================================

COMMIT TRANSACTION;
PRINT '>> Script completed successfully. All tables are empty.';
