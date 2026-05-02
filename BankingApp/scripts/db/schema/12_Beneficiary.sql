IF OBJECT_ID('dbo.Beneficiaries', 'U') IS NULL
CREATE TABLE Beneficiary(
      Id                  INT             NOT NULL IDENTITY(1,1),
      UserId              INT             NOT NULL,
      Name                NVARCHAR(200)   NOT NULL,
      Iban                NVARCHAR(50)    NOT NULL,
      BankName            NVARCHAR(200)   NULL,
      LastTransferDate    DATETIME2       NULL,
      TotalAmountSent     DECIMAL(18,2)   NOT NULL    DEFAULT 0,
      TransferCount       INT             NOT NULL    DEFAULT 0,
      CreatedAt           DATETIME2       NOT NULL    DEFAULT GETUTCDATE(),

      CONSTRAINT PK_Beneficiary                 PRIMARY KEY (Id),
      CONSTRAINT UQ_Beneficiary_UserIBAN        UNIQUE      (UserId, Iban),
      CONSTRAINT CK_Beneficiary_TotalAmount     CHECK       (TotalAmountSent >= 0),
      CONSTRAINT CK_Beneficiary_TransferCount   CHECK       (TransferCount >= 0)
);
GO

