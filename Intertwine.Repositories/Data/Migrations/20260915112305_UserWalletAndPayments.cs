using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserWalletAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UserWallets",
                columns: table => new
                {
                    UserWalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    CreditBalance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWallets", x => x.UserWalletId);
                    table.CheckConstraint("CK_UserWallets_CreditBalance_NonNegative", "[CreditBalance] >= 0");
                    table.ForeignKey(
                        name: "FK_UserWallets_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditPackages",
                columns: table => new
                {
                    CreditPackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credits = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditPackages", x => x.CreditPackageId);
                    table.CheckConstraint("CK_CreditPackages_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_CreditPackages_Credits_Positive", "[Credits] > 0");
                    table.ForeignKey(
                        name: "FK_CreditPackages_Currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPayments",
                columns: table => new
                {
                    UserPaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(3)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditsPurchased = table.Column<long>(type: "bigint", nullable: false),
                    PaymentProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPayments", x => x.UserPaymentId);
                    table.ForeignKey(
                        name: "FK_UserPayments_Currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPayments_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    FinancialTransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserWalletId = table.Column<int>(type: "int", nullable: false),
                    CreditAmount = table.Column<long>(type: "bigint", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    UserPaymentId = table.Column<long>(type: "bigint", nullable: true),
                    BalanceAfterTransaction = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.FinancialTransactionId);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_UserPayments_UserPaymentId",
                        column: x => x.UserPaymentId,
                        principalTable: "UserPayments",
                        principalColumn: "UserPaymentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_UserWallets_UserWalletId",
                        column: x => x.UserWalletId,
                        principalTable: "UserWallets",
                        principalColumn: "UserWalletId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditPackages_CurrencyCode_Amount",
                table: "CreditPackages",
                columns: new[] { "CurrencyCode", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_UserPaymentId",
                table: "FinancialTransactions",
                column: "UserPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_UserWalletId_DateCreated",
                table: "FinancialTransactions",
                columns: new[] { "UserWalletId", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_CurrencyCode",
                table: "UserPayments",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_ProviderTransactionId",
                table: "UserPayments",
                column: "ProviderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPayments_UserProfileId_DateCreated",
                table: "UserPayments",
                columns: new[] { "UserProfileId", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_UserWallets_UserProfileId",
                table: "UserWallets",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditPackages");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "UserPayments");

            migrationBuilder.DropTable(
                name: "UserWallets");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
