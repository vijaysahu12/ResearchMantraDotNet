
ALTER TABLE Stocks
DROP COLUMN IsDelete, PublicKey, CreatedOn, CreatedBy, ModifiedBy;
ALTER TABLE Stocks DROP CONSTRAINT DF__Stocks__CreatedO__3AD6B8E2


GO
ALTER TABLE Stocks
ADD 
    Symbol NVARCHAR(50) NULL, -- To store symbol Name of the stock
    logo NVARCHAR(250) NULL, -- To store the URL or path of the icon image
    Exchange NVARCHAR(250) NULL

GO
   UPDATE Stocks set symbol = name
GO

-- Create ScreenerCategoryM Table
CREATE TABLE ScreenerCategoryM (
    Id INT IDENTITY(1,1) PRIMARY KEY,         -- Auto-incremented unique ID
    CategoryName NVARCHAR(100) NOT NULL,      -- Name of the category
    Description NVARCHAR(255) NULL,           -- Optional description
    IsActive BIT DEFAULT 1,                   -- Indicates if the category is active
    CreatedDate DATETIME DEFAULT GETDATE(),   -- Record creation timestamp
    ModifiedDate DATETIME NULL                -- Record modification timestamp
);

-- Create Screener Table
CREATE TABLE ScreenerM (
    Id INT IDENTITY(1,1) PRIMARY KEY,         -- Auto-incremented unique ID
    CategoryId INT NOT NULL,                  -- Foreign key to ScreenerCategoryM
    ScreenerName NVARCHAR(100) NOT NULL,      -- Name of the screener
    Description NVARCHAR(255) NULL,           -- Optional description of the screener
    IsActive BIT DEFAULT 1,                   -- Indicates if the screener is active
    CreatedDate DATETIME DEFAULT GETDATE(),   -- Record creation timestamp
    ModifiedDate DATETIME NULL,               -- Record modification timestamp
    FOREIGN KEY (CategoryId) REFERENCES ScreenerCategoryM(Id)
);

CREATE TABLE ScreenerStockDataM (
     Id BIGINT PRIMARY KEY IDENTITY(1,1), -- Auto-increment primary key
     ScreenerId NVARCHAR(50) NOT NULL,    -- Unique identifier for the screener
     SymbolId NVARCHAR(50) NOT NULL,      -- Identifier for the stock symbol
     TriggerPrice DECIMAL(18, 2) NOT NULL,   -- Last price with precision
     ModifiedOn DATETIME NULL             -- Record modification timestamp
);

GO 

-- Insert Screener Categories
INSERT INTO ScreenerCategoryM (CategoryName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    ('Breakout', 'Screeners for stocks breaking out of key levels', 1, GETDATE(), NULL),
    ('Volume Buzzer', 'Screeners for unusual volume activity', 1, GETDATE(), NULL),
    ('Indicator Based', 'Screeners based on technical indicators', 1, GETDATE(), NULL),
    ('Gainer & Losers', 'Screeners for top gainers and losers', 1, GETDATE(), NULL),
    ('Exclusive On Kingresearch', 'Custom strategies exclusive to Kingresearch', 1, GETDATE(), NULL);

-- Insert into Screener for 'Volume Buzzer'
DECLARE @CategoryId INT;

SET @CategoryId = (SELECT Id FROM ScreenerCategoryM WHERE CategoryName = 'Volume Buzzer');

INSERT INTO ScreenerM (CategoryId, ScreenerName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    (@CategoryId, 'Volume Buzzer', 'Highlights stocks with unusual volume activity', 1, GETDATE(), NULL);

-- Insert into Screener for 'Breakout'
SET @CategoryId = (SELECT Id FROM ScreenerCategoryM WHERE CategoryName = 'Breakout');

INSERT INTO ScreenerM (CategoryId, ScreenerName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    (@CategoryId, '52-Week High', 'Identifies stocks at their 52-week high levels', 1, GETDATE(), NULL),
    (@CategoryId, '52-Week Low', 'Identifies stocks at their 52-week low levels', 1, GETDATE(), NULL),
    (@CategoryId, 'Short Term Breakout Stocks', 'Stocks with short-term breakout patterns', 1, GETDATE(), NULL),
    (@CategoryId, 'Previous Week Break', 'PREVIOUS WEEK BREAK', 1, GETDATE(), NULL),
    (@CategoryId, 'All Time High Breakout Scanner', 'Identifies stocks breaking all-time high levels', 1, GETDATE(), NULL);

-- Insert into Screener for 'Gainer & Losers'
SET @CategoryId = (SELECT Id FROM ScreenerCategoryM WHERE CategoryName = 'Gainer & Losers');

INSERT INTO ScreenerM (CategoryId, ScreenerName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    (@CategoryId, 'Top Gainers', 'Highlights the top gaining stocks in the market', 1, GETDATE(), NULL),
    (@CategoryId, 'Top Losers', 'Highlights the top losing stocks in the market', 1, GETDATE(), NULL);

-- Insert into Screener for 'Exclusive On Kingresearch'
SET @CategoryId = (SELECT Id FROM ScreenerCategoryM WHERE CategoryName = 'Exclusive On Kingresearch');

INSERT INTO ScreenerM (CategoryId, ScreenerName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    (@CategoryId, 'Darvas Breakout Stocks', 'Identifies stocks based on Darvas breakout strategy', 1, GETDATE(), NULL),
    (@CategoryId, 'Momentum Stocks', 'Highlights stocks with strong momentum', 1, GETDATE(), NULL),
    (@CategoryId, 'Operator Foot print', 'Identifies Operator Foot print- Intraday Setup', 1, GETDATE(), NULL);
-- Insert into Screener for 'Indicator Based'
SET @CategoryId = (SELECT Id FROM ScreenerCategoryM WHERE CategoryName = 'Indicator Based');

INSERT INTO ScreenerM (CategoryId, ScreenerName, Description, IsActive, CreatedDate, ModifiedDate)
VALUES 
    (@CategoryId, 'EMA20', 'Exponential Moving Average (20-day)', 1, GETDATE(), NULL),
    (@CategoryId, 'EMA50', 'Exponential Moving Average (50-day)', 1, GETDATE(), NULL),
    (@CategoryId, 'VWAP', 'Volume Weighted Average Price', 1, GETDATE(), NULL);



-- Add ScreenerCode column to Screener table
ALTER TABLE ScreenerM
ADD Code VARCHAR(100) NULL;


EXEC sp_rename 'ScreenerM.ScreenerName', 'Name', 'COLUMN';



-- Check updated column name
if exists (SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ScreenerM' and COLUMN_NAME = 'NAME')
BEGIN
    select 'Rename Column ScreenerName is successfull'
END 
ELSE BEGIN
    select 'Rename Column ScreenerName is Failed'
END



ALTER TABLE ScreenerCategoryM
ADD 
    [Image] NVARCHAR(255) NULL, -- To store the URL or path of the icon image
    BackgroundColor NVARCHAR(50) NULL -- To store the background color (e.g., HEX or RGB)

    
ALTER TABLE ScreenerM
ADD 
    [Icon] NVARCHAR(255) NULL, -- To store the URL or path of the icon image
    BackgroundColor NVARCHAR(50) NULL -- To store the background color (e.g., HEX or RGB)

UPDATE ScreenerM
SET Code = 
  CASE 
    WHEN Name = 'Weekly Breakout' THEN 'WBE'
    WHEN Name = 'EMA20' THEN 'EMA20'
    WHEN Name = 'EMA50' THEN 'EMA50'
    WHEN Name = 'VWAP' THEN 'VWAP'
    WHEN Name = 'Volume Buzzer' THEN 'VB'
    WHEN Name = '52-Week High' THEN '52WH'
    WHEN Name = '52-Week Low' THEN '52WL'
    WHEN Name = 'Short Term Breakout Stocks' THEN 'STBS'
    WHEN Name = 'All Time High Breakout Scanner' THEN 'ATHBS'
    WHEN Name = 'Top Gainers' THEN 'TG'
    WHEN Name = 'Top Losers' THEN 'TL'
    WHEN Name = 'Darvas Breakout Stocks' THEN 'DBS'
    WHEN Name = 'Momentum Stocks' THEN 'MS'
    ELSE Code -- Keep the original code if not matched
  END
WHERE Name IN (
  'Weekly Breakout', 'EMA20', 'EMA50', 'VWAP', 'Volume Buzzer',
  '52-Week High', '52-Week Low', 'Short Term Breakout Stocks',
  'All Time High Breakout Scanner', 'Top Gainers', 'Top Losers',
  'Darvas Breakout Stocks', 'Momentum Stocks'
);

GO
update screenerM set Code = 'VWAP' where Code ='VWAP'
update screenerM set Code = 'VolumeBuzzer' where Code ='VB'
update screenerM set Code = '52WeekBreakout' where Code ='52WH'
update screenerM set Code = 'ShortTermBreakouts' where Code ='STBS'
update screenerM set Code = 'AllTimeHighBreakout' where Code ='ATHBS'
update screenerM set Code = 'TopGainers' where Code ='TG'
update screenerM set Code = 'TopLosers' where Code ='TL'
update screenerM set Code = '52WeekBreadown' where Code ='52WL'
update screenerM set Code = 'EMA20' where Code ='EMA20'
update screenerM set Code = 'EMA50' where Code ='EMA50'
update screenerM set Code = 'EMA50' where Code ='EMA50'
update screenerM set Code = 'PWB' where Name ='Previous Week Break'

--update screener set Code = '' where Code ='WBE'
--update screener set Code = '' where Code ='DBS'
--update screener set Code = '' where Code ='MS'
GO

GO



GO  
--exec GetScreenerDetails        
CREATE PROCEDURE GetScreenerDetails        
 @IsPaging bit = 1,              
 @PageSize int =25,              
 @PageNumber int =1,           
 @SortExpression varchar(50) = '',           
 @SortOrder varchar(50) = '',           
 @RequestedBy varchar(50)= null,           
 @SearchText varchar(100)= null,           
 @FromDate DateTime= null,           
 @ToDate DateTime= null,           
 @StrategyKey varchar(50) = null,           
 @TotalCount INT = 0 OUTPUT           
AS  
BEGIN  
    -- Fetch all screener details along with their category information  
    SELECT   
        S.Id AS ScreenerId,  
        S.Name AS ScreenerName,  
        S.Code,  
        S.Description AS ScreenerDescription,  
        S.IsActive AS ScreenerIsActive,   
		s.BackgroundColor,
		S.Icon as ScreenerIcon,
        SC.Id AS CategoryId,  
        SC.CategoryName,  
        SC.Description AS CategoryDescription,  
        SC.Image as CategoryImage,  
        SC.BackgroundColor as CategoryBackGroundColor,  
        SC.IsActive AS CategoryIsActive,  
        cast( 1 as bit) as SubscriptionStatus  
          
    FROM ScreenerM S  
    INNER JOIN ScreenerCategorym SC ON S.CategoryId = SC.Id  
    WHERE S.IsActive = 1 AND SC.IsActive = 1 -- Fetch only active screeners and categories  
    ORDER BY SC.CategoryName, S.Name; -- Sort by category and screener name  
END;   
  
   


 update ScreenerCategoryM set MOdifiedDate = GETDATE() , BackgroundColor = '#3a813a' 
 SELECT 'upload image for screener' as Task1 , 'Upload Correct Background color' as Task2


GO
	CREATE TYPE TVP_ScreenerStock AS TABLE (
		Symbol NVARCHAR(50),
		Exchange NVARCHAR(10),
		TriggerPrice DECIMAL(18, 2)
	);
 GO 
 

 GO
   ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--drop TYPE TVP_ScreenerStock 
--drop PROCEDURE ManageScreenerStocksData
 GO
 CREATE PROCEDURE ManageScreenerStocksData
     @ScreenerStocks as [dbo].[TVP_ScreenerStock] READONLY, -- Table-Valued Parameter
	 @ScreenerCategoryName VARCHAR(100) 
 AS
 BEGIN

	INSERT INTO Logs VALUES (@ScreenerCategoryName,'ManageScreenerStocksData',GETDATE())
 
	DECLARE @ScreenerCategoryId int = 99999 
	--truncate TABLE ScreenerStockDataM
	--INSERT INTO ScreenerStockDataM 
	  
	MERGE INTO Stocks AS Target
	USING  (SELECT * fROM @ScreenerStocks AS SS WHERE LEN(SS.Symbol) >0) AS Source
		ON Target.Name = Source.Symbol  
     WHEN NOT MATCHED BY TARGET THEN
         INSERT (Name, Description, LotSize,Exchange, modifiedOn)
         VALUES (Source.symbol , '' , 1, (ISNULL(Source.Exchange,'NSE')) ,GETDATE());
		
		
     SELECT  @ScreenerCategoryId = Id From ScreenerM where Code = @ScreenerCategoryName
	 
	 IF(@ScreenerCategoryId != 99999)
	 BEGIN
	   -- Insert or update existing stocks
     MERGE INTO ScreenerStockDataM AS Target
     USING (

         SELECT sstemp.symbol, s.Id as SymbolId, sstemp.TriggerPrice 
		 FROM @ScreenerStocks as sstemp 
         INNER JOIN Stocks as s 
			on sstemp.Symbol = s.Name

     ) AS Source
     ON Target.SymbolId = Source.SymbolId AND @ScreenerCategoryId = TARGET.ScreenerId
     WHEN MATCHED THEN
         UPDATE SET  Target.TriggerPrice  = Source.TriggerPrice  , modifiedOn = GETDATE()
     WHEN NOT MATCHED BY TARGET THEN
         INSERT (ScreenerId, SymbolId, TriggerPrice, ModifiedOn)
         VALUES (@ScreenerCategoryId , Source.SymbolId, Source.TriggerPrice ,GETDATE());
	 
	 END
   
     SELECT @@ROWCOUNT AS Total
     -- Optional: Delete logic for removed records can go here
 END 



GO
UPDATE Screener
SET backgroundColor = 
    '#' + 
    FORMAT(ABS(CHECKSUM(NEWID())) % 256, 'X2') + 
    FORMAT(ABS(CHECKSUM(NEWID())) % 256, 'X2') + 
    FORMAT(ABS(CHECKSUM(NEWID())) % 256, 'X2')