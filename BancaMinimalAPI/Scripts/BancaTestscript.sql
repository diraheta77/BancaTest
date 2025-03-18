/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Configurations]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Configurations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InterestRate] [decimal](18, 2) NOT NULL,
	[MinimumPaymentRate] [decimal](18, 2) NOT NULL,
	[LastUpdated] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Configurations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CreditCards]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditCards](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardNumber] [nvarchar](max) NOT NULL,
	[HolderName] [nvarchar](max) NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CurrentBalance] [decimal](18, 2) NOT NULL,
	[InterestRate] [decimal](18, 2) NOT NULL,
	[MinimumPaymentRate] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_CreditCards] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transactions]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transactions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreditCardId] [int] NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Type] [int] NOT NULL,
 CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_CalculatePaymentAmounts]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_CalculatePaymentAmounts]
    @CreditCardId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar si existe la tarjeta
    IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE Id = @CreditCardId)
    BEGIN
        THROW 50404, 'Tarjeta de crédito no encontrada', 1;
    END

    DECLARE @CurrentBalance decimal(18,2)
    DECLARE @InterestRate decimal(18,2)
    DECLARE @MinimumPaymentRate decimal(18,2)

    SELECT 
        @CurrentBalance = CurrentBalance,
        @InterestRate = InterestRate,
        @MinimumPaymentRate = MinimumPaymentRate
    FROM CreditCards 
    WHERE Id = @CreditCardId;

    SELECT
        @CurrentBalance AS TotalBalance,
        -- Cálculo del interés bonificable (Saldo Total x Porcentaje Interés Configurable)
        CAST((@CurrentBalance * (@InterestRate/100)) AS decimal(18,2)) AS BonusInterest,
        -- Cálculo de la cuota mínima (Saldo Total x Porcentaje Configurable Saldo Mínimo)
        CAST((@CurrentBalance * (@MinimumPaymentRate/100)) AS decimal(18,2)) AS MinimumPayment,
        -- Monto total de contado con intereses (Saldo Total + Interés Bonificable)
        CAST((@CurrentBalance + (@CurrentBalance * (@InterestRate/100))) AS decimal(18,2)) AS TotalAmountWithInterest;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_CreateTransaction]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_CreateTransaction]
    @CreditCardId INT,
    @Date DATETIME,
    @Description NVARCHAR(200),
    @Amount DECIMAL(18,2),
    @Type INT -- 0: Purchase, 1: Payment
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verificar si existe la tarjeta
        IF NOT EXISTS (SELECT 1 FROM CreditCards WHERE Id = @CreditCardId)
        BEGIN
            THROW 50404, 'Tarjeta de crédito no encontrada', 1;
        END

        -- Obtener el saldo actual y límite
        DECLARE @CurrentBalance DECIMAL(18,2)
        DECLARE @CreditLimit DECIMAL(18,2)
        
        SELECT 
            @CurrentBalance = CurrentBalance,
            @CreditLimit = CreditLimit
        FROM CreditCards 
        WHERE Id = @CreditCardId;

        -- Validaciones según el tipo de transacción
        IF @Type = 0 -- Compra
        BEGIN
            -- Validar límite de crédito
            IF (@CurrentBalance + @Amount) > @CreditLimit
            BEGIN
                THROW 50400, 'La compra excede el límite de crédito disponible', 1;
            END
            
            -- Actualizar saldo
            UPDATE CreditCards 
            SET CurrentBalance = CurrentBalance + @Amount
            WHERE Id = @CreditCardId;
        END
        ELSE -- Pago
        BEGIN
            -- Validar que el pago no exceda el saldo
            IF @Amount > @CurrentBalance
            BEGIN
                THROW 50400, 'El pago no puede ser mayor al saldo actual', 1;
            END
            
            -- Actualizar saldo
            UPDATE CreditCards 
            SET CurrentBalance = CurrentBalance - @Amount
            WHERE Id = @CreditCardId;
        END

        -- Insertar la transacción
        INSERT INTO Transactions (CreditCardId, Date, Description, Amount, Type)
        VALUES (@CreditCardId, @Date, @Description, @Amount, @Type);

        -- Obtener el ID de la transacción creada
        SELECT CAST(SCOPE_IDENTITY() AS INT) AS TransactionId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetCreditCardStatement]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_GetCreditCardStatement]
    @CreditCardId INT
AS
BEGIN
    -- Obtener la configuración global
    DECLARE @InterestRate DECIMAL(5,2),
            @MinimumPaymentRate DECIMAL(5,2)
    
    SELECT @InterestRate = InterestRate,
           @MinimumPaymentRate = MinimumPaymentRate
    FROM Configurations
    WHERE Id = 1

    -- Obtener información de la tarjeta y calcular montos
    SELECT 
        cc.Id,
        cc.CardNumber,
        cc.HolderName,
        cc.CreditLimit,
        cc.CurrentBalance AS TotalBalance,
        @InterestRate AS InterestRate,
        @MinimumPaymentRate AS MinimumPaymentRate,
        cc.CreditLimit - cc.CurrentBalance AS AvailableBalance,
        cc.CurrentBalance * (@InterestRate / 100.0) AS BonusInterest,
        cc.CurrentBalance * (@MinimumPaymentRate / 100.0) AS MinimumPayment,
        cc.CurrentBalance AS TotalAmount,
        cc.CurrentBalance + (cc.CurrentBalance * (@InterestRate / 100.0)) AS TotalAmountWithInterest,
        ISNULL((
            SELECT SUM(Amount)
            FROM Transactions
            WHERE CreditCardId = cc.Id 
            AND Type = 0 -- Purchase
            AND MONTH(Date) = MONTH(GETDATE())
            AND YEAR(Date) = YEAR(GETDATE())
        ), 0) AS CurrentMonthPurchases,
        ISNULL((
            SELECT SUM(Amount)
            FROM Transactions
            WHERE CreditCardId = cc.Id 
            AND Type = 0 -- Purchase
            AND MONTH(Date) = MONTH(DATEADD(MONTH, -1, GETDATE()))
            AND YEAR(Date) = YEAR(DATEADD(MONTH, -1, GETDATE()))
        ), 0) AS PreviousMonthPurchases
    FROM CreditCards cc
    WHERE cc.Id = @CreditCardId;

    -- Obtener transacciones
    SELECT 
        Id,
        Date,
        Description,
        Amount,
        Type
    FROM Transactions
    WHERE CreditCardId = @CreditCardId
    ORDER BY Date DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetCurrentMonthTransactions]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_GetCurrentMonthTransactions]
    @CreditCardId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentMonth INT = MONTH(GETDATE());
    DECLARE @CurrentYear INT = YEAR(GETDATE());

    -- Obtener todas las transacciones del mes actual
    SELECT 
        t.Id,
        t.CreditCardId,
        t.Date,
        t.Description,
        CAST(t.Amount AS decimal(18,2)) AS Amount,
        t.Type,
        CASE t.Type 
            WHEN 0 THEN 'Compra'
            WHEN 1 THEN 'Pago'
            ELSE 'Desconocido'
        END AS TypeDescription
    FROM Transactions t
    WHERE t.CreditCardId = @CreditCardId
        AND MONTH(t.Date) = @CurrentMonth
        AND YEAR(t.Date) = @CurrentYear
    ORDER BY t.Date DESC;

    -- Obtener resumen del mes
    SELECT 
        CAST(SUM(CASE WHEN Type = 0 THEN Amount ELSE 0 END) AS decimal(18,2)) AS TotalPurchases,
        CAST(SUM(CASE WHEN Type = 1 THEN Amount ELSE 0 END) AS decimal(18,2)) AS TotalPayments,
        COUNT(CASE WHEN Type = 0 THEN 1 END) AS PurchaseCount,
        COUNT(CASE WHEN Type = 1 THEN 1 END) AS PaymentCount
    FROM Transactions
    WHERE CreditCardId = @CreditCardId
        AND MONTH(Date) = @CurrentMonth
        AND YEAR(Date) = @CurrentYear;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetFullCreditCardStatement]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_GetFullCreditCardStatement]
    @CreditCardId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validación inicial
    IF NOT EXISTS (SELECT 1 FROM CreditCards WITH (NOLOCK) WHERE Id = @CreditCardId)
    BEGIN
        THROW 50404, 'Tarjeta de crédito no encontrada', 1;
        RETURN;
    END;

    -- Variables para fechas
    DECLARE @CurrentDate DATE = GETDATE();
    DECLARE @FirstDayOfMonth DATE = DATEFROMPARTS(YEAR(@CurrentDate), MONTH(@CurrentDate), 1);
    DECLARE @FirstDayOfPrevMonth DATE = DATEADD(MONTH, -1, @FirstDayOfMonth);

    -- Obtener toda la información en una sola consulta
    SELECT 
        c.Id,
        c.CardNumber,
        c.HolderName,
        c.CreditLimit,
        c.CurrentBalance AS TotalBalance,
        c.InterestRate,
        c.MinimumPaymentRate,
        (c.CreditLimit - c.CurrentBalance) AS AvailableBalance,
        CAST((c.CurrentBalance * (c.InterestRate/100)) AS decimal(18,2)) AS BonusInterest,
        CAST((c.CurrentBalance * (c.MinimumPaymentRate/100)) AS decimal(18,2)) AS MinimumPayment,
        c.CurrentBalance AS TotalAmount,
        CAST((c.CurrentBalance + (c.CurrentBalance * (c.InterestRate/100))) AS decimal(18,2)) AS TotalAmountWithInterest,
        COALESCE(
            (SELECT SUM(Amount) 
             FROM Transactions WITH (NOLOCK)
             WHERE CreditCardId = c.Id 
             AND Type = 0 
             AND Date >= @FirstDayOfMonth), 0) AS CurrentMonthPurchases,
        COALESCE(
            (SELECT SUM(Amount)
             FROM Transactions WITH (NOLOCK)
             WHERE CreditCardId = c.Id 
             AND Type = 0 
             AND Date >= @FirstDayOfPrevMonth 
             AND Date < @FirstDayOfMonth), 0) AS PreviousMonthPurchases
    FROM CreditCards c WITH (NOLOCK)
    WHERE c.Id = @CreditCardId;

    -- Obtener transacciones del mes actual
    SELECT 
        t.Id,
        t.Date,
        t.Description,
        t.Amount,
        t.Type
    FROM Transactions t WITH (NOLOCK)
    WHERE t.CreditCardId = @CreditCardId
    AND t.Date >= @FirstDayOfMonth
    ORDER BY t.Date DESC;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetMonthlyBalances]    Script Date: 18/3/2025 09:01:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Optimizar el stored procedure de saldos mensuales
CREATE   PROCEDURE [dbo].[sp_GetMonthlyBalances]
    @CreditCardId INT,
    @MonthsBack INT = 6
AS
BEGIN
    SET NOCOUNT ON;

    -- Validación con índice
    IF NOT EXISTS (SELECT 1 FROM CreditCards WITH (NOLOCK) WHERE Id = @CreditCardId)
    BEGIN
        THROW 50404, 'Tarjeta de crédito no encontrada', 1;
        RETURN;
    END;

    -- Calcular fecha inicial una sola vez
    DECLARE @StartDate DATE = DATEADD(MONTH, -@MonthsBack, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1));

    -- Usar CTE para mejorar legibilidad y rendimiento
    WITH MonthlyTotals AS (
        SELECT 
            DATEFROMPARTS(YEAR(t.Date), MONTH(t.Date), 1) AS MonthYear,
            SUM(CASE WHEN t.Type = 0 THEN t.Amount ELSE 0 END) AS TotalPurchases,
            SUM(CASE WHEN t.Type = 1 THEN t.Amount ELSE 0 END) AS TotalPayments,
            COUNT(CASE WHEN t.Type = 0 THEN 1 END) AS PurchaseCount,
            COUNT(CASE WHEN t.Type = 1 THEN 1 END) AS PaymentCount
        FROM Transactions t WITH (NOLOCK)
        WHERE t.CreditCardId = @CreditCardId
        AND t.Date >= @StartDate
        GROUP BY YEAR(t.Date), MONTH(t.Date)
    )
    SELECT 
        MonthYear,
        CAST(COALESCE(TotalPurchases, 0) AS decimal(18,2)) AS TotalPurchases,
        CAST(COALESCE(TotalPayments, 0) AS decimal(18,2)) AS TotalPayments,
        PurchaseCount,
        PaymentCount,
        CAST((COALESCE(TotalPurchases, 0) - COALESCE(TotalPayments, 0)) AS decimal(18,2)) AS NetBalance
    FROM MonthlyTotals
    ORDER BY MonthYear DESC;
END;

GO
