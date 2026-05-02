IF OBJECT_ID('dbo.Beneficiary', 'U') IS NULL
CREATE TABLE Beneficiary (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES [User](Id),
    Name NVARCHAR(200) NOT NULL,
    Iban VARCHAR(34) NOT NULL,
    BankName NVARCHAR(200) NULL,
    LastTransferDate DATETIME2 NULL,
    TotalAmountSent DECIMAL(18,2) NOT NULL DEFAULT 0,
    TransferCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT UQ_Beneficiary_UserId_Iban UNIQUE (UserId, Iban),
    CONSTRAINT CK_Beneficiary_TotalAmountSent_NonNegative CHECK (TotalAmountSent >= 0),
    CONSTRAINT CK_Beneficiary_TransferCount_NonNegative CHECK (TransferCount >= 0)
);
GO