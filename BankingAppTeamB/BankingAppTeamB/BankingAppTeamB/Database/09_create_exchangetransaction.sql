IF NOT EXISTS (
   SELECT * FROM sys.tables
        WHERE name = 'ExchangeTransaction'
)
BEGIN
    CREATE TABLE ExchangeTransaction (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        
        UserId INT NOT NULL,
        SourceAccountId INT NOT NULL,
        TargetAccountId INT NOT NULL,
                        
        TransactionId INT NULL,
                      
        SourceCurrency VARCHAR(3) NOT NULL,
        TargetCurrency VARCHAR(3) NOT NULL,
                      
        SourceAmount DECIMAL(18,2) NOT NULL,
        TargetAmount DECIMAL(18,2) NOT NULL,
                      
        ExchangeRate DECIMAL(18,6) NOT NULL,
        Commission DECIMAL(18,2) NOT NULL,
        
        RateLockedAt DATETIME2 NULL,
        
        Status VARCHAR(20) NOT NULL,
                      
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
   