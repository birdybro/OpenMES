using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OpenMES.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Revision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    OperationCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ResourceCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false),
                    IsObsolete = table.Column<bool>(type: "boolean", nullable: false),
                    UrlOrPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "material_lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Supplier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_lots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scan_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RawValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ParsedType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ParsedKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParsedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    JobId = table.Column<int>(type: "integer", nullable: true),
                    ResourceId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scan_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_links",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_links_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "part_revisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartId = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    ReleasedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_part_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_part_revisions_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PartRevisionId = table.Column<int>(type: "integer", nullable: false),
                    ResourceId = table.Column<int>(type: "integer", nullable: true),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityGood = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityScrap = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_jobs_part_revisions_PartRevisionId",
                        column: x => x.PartRevisionId,
                        principalTable: "part_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_jobs_resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "resources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartRevisionId = table.Column<int>(type: "integer", nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PreferredResourceCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StandardRunTimeMinutes = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    StandardSetupTimeMinutes = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_operations_part_revisions_PartRevisionId",
                        column: x => x.PartRevisionId,
                        principalTable: "part_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_material_issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    MaterialLotId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IssuedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_material_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_job_material_issues_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_material_issues_material_lots_MaterialLotId",
                        column: x => x.MaterialLotId,
                        principalTable: "material_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_schedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResourceId = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_schedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resource_schedule_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_schedule_resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "production_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: true),
                    ResourceId = table.Column<int>(type: "integer", nullable: true),
                    OperationId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawScanValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_events_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_production_events_operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "operations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_production_events_resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "resources",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_production_events_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "quality_checks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OperationId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CheckType = table.Column<int>(type: "integer", nullable: false),
                    MinValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quality_checks_operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QualityCheckId = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    NumericValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TextValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Pass = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quality_results_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_results_quality_checks_QualityCheckId",
                        column: x => x.QualityCheckId,
                        principalTable: "quality_checks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_links_DocumentId",
                table: "document_links",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_document_links_Scope_ScopeKey",
                table: "document_links",
                columns: new[] { "Scope", "ScopeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_documents_IsObsolete",
                table: "documents",
                column: "IsObsolete");

            migrationBuilder.CreateIndex(
                name: "IX_documents_PartNumber_Revision",
                table: "documents",
                columns: new[] { "PartNumber", "Revision" });

            migrationBuilder.CreateIndex(
                name: "IX_job_material_issues_JobId",
                table: "job_material_issues",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_job_material_issues_MaterialLotId",
                table: "job_material_issues",
                column: "MaterialLotId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_DueUtc",
                table: "jobs",
                column: "DueUtc");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_JobNumber",
                table: "jobs",
                column: "JobNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jobs_PartRevisionId",
                table: "jobs",
                column: "PartRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_ResourceId",
                table: "jobs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_Status",
                table: "jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_material_lots_LotCode",
                table: "material_lots",
                column: "LotCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_lots_PartNumber",
                table: "material_lots",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_operations_PartRevisionId_OperationCode",
                table: "operations",
                columns: new[] { "PartRevisionId", "OperationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_part_revisions_PartId_Revision",
                table: "part_revisions",
                columns: new[] { "PartId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parts_PartNumber",
                table: "parts",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_events_EventType_TimestampUtc",
                table: "production_events",
                columns: new[] { "EventType", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_production_events_JobId_TimestampUtc",
                table: "production_events",
                columns: new[] { "JobId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_production_events_OperationId",
                table: "production_events",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_production_events_ResourceId",
                table: "production_events",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_production_events_UserId",
                table: "production_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_quality_checks_OperationId",
                table: "quality_checks",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_quality_results_JobId",
                table: "quality_results",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_quality_results_QualityCheckId",
                table: "quality_results",
                column: "QualityCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_schedule_JobId",
                table: "resource_schedule",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_schedule_ResourceId_PlannedStartUtc",
                table: "resource_schedule",
                columns: new[] { "ResourceId", "PlannedStartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_resources_Code",
                table: "resources",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scan_events_TimestampUtc",
                table: "scan_events",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_users_Code",
                table: "users",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_links");

            migrationBuilder.DropTable(
                name: "job_material_issues");

            migrationBuilder.DropTable(
                name: "production_events");

            migrationBuilder.DropTable(
                name: "quality_results");

            migrationBuilder.DropTable(
                name: "resource_schedule");

            migrationBuilder.DropTable(
                name: "scan_events");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "material_lots");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "quality_checks");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "operations");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "part_revisions");

            migrationBuilder.DropTable(
                name: "parts");
        }
    }
}
