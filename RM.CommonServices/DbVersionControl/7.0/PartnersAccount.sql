-- Step 1: Alter column type
ALTER TABLE PartnerAccountDetails
ALTER COLUMN PartnerWith BIT;

GO 


-- Drop 'IsVerified' column if it exists
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PartnerAccounts'
      AND COLUMN_NAME = 'IsVerified'
)
BEGIN
    ALTER TABLE PartnerAccounts
    DROP COLUMN IsVerified;
END

-- Drop 'PartnerWithAccount' column if it exists
IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PartnerAccounts'
      AND COLUMN_NAME = 'PartnerWithAccount'
)
BEGIN
    ALTER TABLE PartnerAccounts
    DROP COLUMN PartnerWithAccount;
END


GO 

