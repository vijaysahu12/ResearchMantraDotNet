CREATE TABLE EmailTemplates (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Subject VARCHAR(255) NOT NULL,
    Body TEXT NOT NULL,
    CreatedOn DATETIME,
    UpdatedOn DATETIME,
	ModifiedBy varchar(50) 
);