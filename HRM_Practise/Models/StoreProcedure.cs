namespace HRM_Practise.Models
{
    public static class StoreProcedureQuery
    {
        public const string CreateRole = @"
            CREATE PROCEDURE GetMachineData
            @StartDate DATE = NULL,
            @EndDate DATE = NULL,
            @SearchValue NVARCHAR(255) = NULL,
            @SortColumn NVARCHAR(255) = NULL,
            @SortDirection NVARCHAR(4) = 'ASC',
            @Start INT = 0,
            @Length INT = 10,
            @TotalRecords INT OUTPUT,
            @FilteredRecords INT OUTPUT
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Temporary table to hold filtered results
            CREATE TABLE #MachineData (
                AutoId INT,
                FingerPrintId NVARCHAR(255),
                MachineId NVARCHAR(255),
                Date DATE,
                Time TIME,
                Latitude FLOAT,
                Longitude FLOAT,
                HOALR NVARCHAR(255)
            );

            -- Insert filtered data into temporary table
            INSERT INTO #MachineData (AutoId, FingerPrintId, MachineId, Date, Time, Latitude, Longitude, HOALR)
            SELECT 
                AutoId,
                FingerPrintId,
                MachineId,
                Date,
                Time,
                Latitude,
                Longitude,
                HOALR
            FROM 
                HRM_ATD_MachineData
            WHERE 
                (@StartDate IS NULL OR Date = @StartDate)
                AND (@EndDate IS NULL OR Date <= @EndDate)
                AND (@SearchValue IS NULL OR 
                    (FingerPrintId LIKE '%' + @SearchValue + '%' OR 
                     MachineId LIKE '%' + @SearchValue + '%' OR 
                     HOALR LIKE '%' + @SearchValue + '%'));

            -- Get total count of records
            SELECT @TotalRecords = COUNT(*) FROM #MachineData;

            -- Apply sorting
            DECLARE @SQL NVARCHAR(MAX);
            SET @SQL = 'SELECT * FROM #MachineData';

            IF @SortColumn IS NOT NULL
            BEGIN
                SET @SQL = @SQL + ' ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection;
            END

            -- Apply pagination
            SET @SQL = @SQL + ' OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY';

            -- Execute the dynamic SQL
            EXEC sp_executesql @SQL, 
                N'@Start INT, @Length INT', 
                @Start = @Start, 
                @Length = @Length;

            -- Get filtered count
            SELECT @FilteredRecords = COUNT(*) FROM #MachineData;

            -- Clean up
            DROP TABLE #MachineData;
        END
        ";
        }
}
