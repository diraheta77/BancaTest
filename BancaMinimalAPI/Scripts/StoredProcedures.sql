-- Procedimiento para obtener el estado de cuenta completo
CREATE OR ALTER PROCEDURE sp_GetCreditCardStatement
    @CreditCardId INT
AS
BEGIN
    -- Obtener información de la tarjeta
    SELECT 
        cc.Id,
        cc.CardNumber,
        cc.HolderName,
        cc.CreditLimit,
        cc.CurrentBalance,
        cc.InterestRate,
        cc.MinimumPaymentRate,
        (cc.CreditLimit - cc.CurrentBalance) AS AvailableBalance,
        (cc.CurrentBalance * (cc.InterestRate/100)) AS BonusInterest,
        (cc.CurrentBalance * (cc.MinimumPaymentRate/100)) AS MinimumPayment,
        (cc.CurrentBalance + (cc.CurrentBalance * (cc.InterestRate/100))) AS TotalPaymentWithInterest
    FROM CreditCards cc
    WHERE cc.Id = @CreditCardId;
END
GO



-- Procedimiento para obtener transacciones del mes actual
CREATE OR ALTER PROCEDURE sp_GetCurrentMonthTransactions
    @CreditCardId INT
AS
BEGIN
    DECLARE @CurrentMonth INT = MONTH(GETDATE())
    DECLARE @CurrentYear INT = YEAR(GETDATE())

    SELECT 
        t.Id,
        t.Date,
        t.Description,
        t.Amount,
        t.Type
    FROM Transactions t
    WHERE t.CreditCardId = @CreditCardId
        AND MONTH(t.Date) = @CurrentMonth
        AND YEAR(t.Date) = @CurrentYear
    ORDER BY t.Date DESC
END
GO

-- Procedimiento para crear una nueva transacción
CREATE OR ALTER PROCEDURE sp_CreateTransaction
    @CreditCardId INT,
    @Date DATETIME,
    @Description NVARCHAR(200),
    @Amount DECIMAL(18,2),
    @Type INT
AS
BEGIN
    BEGIN TRANSACTION
    
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
        WHERE Id = @CreditCardId

        -- Validar límite de crédito para compras
        IF @Type = 0 -- Compra
        BEGIN
            IF (@CurrentBalance + @Amount) > @CreditLimit
            BEGIN
                THROW 50400, 'La compra excede el límite de crédito disponible', 1;
            END
            
            -- Actualizar saldo
            UPDATE CreditCards 
            SET CurrentBalance = CurrentBalance + @Amount
            WHERE Id = @CreditCardId
        END
        ELSE -- Pago
        BEGIN
            IF @Amount > @CurrentBalance
            BEGIN
                THROW 50400, 'El pago no puede ser mayor al saldo actual', 1;
            END
            
            -- Actualizar saldo
            UPDATE CreditCards 
            SET CurrentBalance = CurrentBalance - @Amount
            WHERE Id = @CreditCardId
        END

        -- Insertar la transacción
        INSERT INTO Transactions (CreditCardId, Date, Description, Amount, Type)
        VALUES (@CreditCardId, @Date, @Description, @Amount, @Type)

        COMMIT TRANSACTION

        -- Retornar el ID de la transacción creada
        SELECT SCOPE_IDENTITY() AS TransactionId
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW;
    END CATCH
END
GO