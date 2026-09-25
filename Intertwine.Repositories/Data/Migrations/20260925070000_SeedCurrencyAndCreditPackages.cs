using Intertwine.Repositories.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations;

/// <summary>
/// Seeds the initial supported currencies and purchasable Spark packages.
/// </summary>
[DbContext(typeof(IntertwineDbContext))]
[Migration("20260925070000_SeedCurrencyAndCreditPackages")]
public partial class SeedCurrencyAndCreditPackages : Migration
{
    private static readonly DateTime SeedDate =
        new(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Currencies",
            columns:
            [
                "Code",
                "Symbol",
                "Name",
                "DecimalPlaces",
                "DateCreated",
                "CreatedBy",
                "IsActive"
            ],
            columnTypes:
            [
                "varchar(3)",
                "nvarchar(10)",
                "nvarchar(100)",
                "int",
                "datetime2",
                "nvarchar(max)",
                "bit"
            ],
            values: new object[,]
            {
                { "PHP", "₱", "Philippine Peso", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "USD", "$", "United States Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "EUR", "€", "Euro", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "GBP", "£", "Pound Sterling", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "JPY", "¥", "Japanese Yen", 0, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "CNY", "¥", "Chinese Yuan", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "KRW", "₩", "South Korean Won", 0, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "SGD", "S$", "Singapore Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "AUD", "A$", "Australian Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "CAD", "C$", "Canadian Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "NZD", "NZ$", "New Zealand Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "HKD", "HK$", "Hong Kong Dollar", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "INR", "₹", "Indian Rupee", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "IDR", "Rp", "Indonesian Rupiah", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "MYR", "RM", "Malaysian Ringgit", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "THB", "฿", "Thai Baht", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "VND", "₫", "Vietnamese Dong", 0, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "CHF", "CHF", "Swiss Franc", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "AED", "د.إ", "UAE Dirham", 2, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { "SAR", "ر.س", "Saudi Riyal", 2, SeedDate, "SeedCurrencyAndCreditPackages", true }
            });

        migrationBuilder.InsertData(
            table: "CreditPackages",
            columns:
            [
                "CreditPackageId",
                "CurrencyCode",
                "Amount",
                "Credits",
                "DateCreated",
                "CreatedBy",
                "IsActive"
            ],
            columnTypes:
            [
                "int",
                "varchar(3)",
                "decimal(18,2)",
                "bigint",
                "datetime2",
                "nvarchar(max)",
                "bit"
            ],
            values: new object[,]
            {
                { 1, "PHP", 100.00m, 200L, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { 2, "PHP", 250.00m, 550L, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { 3, "PHP", 500.00m, 1200L, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { 4, "USD", 1.99m, 200L, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { 5, "USD", 4.99m, 550L, SeedDate, "SeedCurrencyAndCreditPackages", true },
                { 6, "USD", 9.99m, 1200L, SeedDate, "SeedCurrencyAndCreditPackages", true }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "CreditPackages",
            keyColumn: "CreditPackageId",
            keyColumnType: "int",
            keyValues: [1, 2, 3, 4, 5, 6]);

        migrationBuilder.DeleteData(
            table: "Currencies",
            keyColumn: "Code",
            keyColumnType: "varchar(3)",
            keyValues:
            [
                "PHP",
                "USD",
                "EUR",
                "GBP",
                "JPY",
                "CNY",
                "KRW",
                "SGD",
                "AUD",
                "CAD",
                "NZD",
                "HKD",
                "INR",
                "IDR",
                "MYR",
                "THB",
                "VND",
                "CHF",
                "AED",
                "SAR"
            ]);
    }
}
