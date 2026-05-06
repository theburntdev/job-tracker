using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Infrastructure.Migrations;

/// <inheritdoc />
public partial class StoreJobApplicationTimestampsAsUnixMilliseconds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE "ef_temp_JobApplications" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_JobApplications" PRIMARY KEY,
                "AppliedAt" INTEGER NULL,
                "Company" TEXT NOT NULL,
                "CreatedAt" INTEGER NOT NULL,
                "Description" TEXT NULL,
                "Location" TEXT NULL,
                "PostedAt" INTEGER NULL,
                "Stage" INTEGER NOT NULL,
                "Title" TEXT NOT NULL,
                "UpdatedAt" INTEGER NOT NULL,
                "Url" TEXT NULL
            );

            INSERT INTO "ef_temp_JobApplications" ("Id", "AppliedAt", "Company", "CreatedAt", "Description", "Location", "PostedAt", "Stage", "Title", "UpdatedAt", "Url")
            SELECT
                "Id",
                CASE
                    WHEN "AppliedAt" IS NULL OR "AppliedAt" = '' THEN NULL
                    WHEN "AppliedAt" NOT GLOB '*[!0-9]*' AND LENGTH("AppliedAt") >= 12 THEN CAST("AppliedAt" AS INTEGER)
                    ELSE CAST(ROUND((julianday(REPLACE(TRIM(SUBSTR("AppliedAt", 1,
                        CASE WHEN INSTR("AppliedAt", '+') > 0 THEN INSTR("AppliedAt", '+') - 1
                             WHEN INSTR("AppliedAt", 'Z') > 0 THEN INSTR("AppliedAt", 'Z') - 1
                             ELSE LENGTH("AppliedAt") END)), 'T', ' ')) - 2440587.5) * 86400000) AS INTEGER)
                END,
                "Company",
                CASE
                    WHEN "CreatedAt" NOT GLOB '*[!0-9]*' AND LENGTH("CreatedAt") >= 12 THEN CAST("CreatedAt" AS INTEGER)
                    ELSE CAST(ROUND((julianday(REPLACE(TRIM(SUBSTR("CreatedAt", 1,
                        CASE WHEN INSTR("CreatedAt", '+') > 0 THEN INSTR("CreatedAt", '+') - 1
                             WHEN INSTR("CreatedAt", 'Z') > 0 THEN INSTR("CreatedAt", 'Z') - 1
                             ELSE LENGTH("CreatedAt") END)), 'T', ' ')) - 2440587.5) * 86400000) AS INTEGER)
                END,
                "Description",
                "Location",
                CASE
                    WHEN "PostedAt" IS NULL OR "PostedAt" = '' THEN NULL
                    WHEN "PostedAt" NOT GLOB '*[!0-9]*' AND LENGTH("PostedAt") >= 12 THEN CAST("PostedAt" AS INTEGER)
                    ELSE CAST(ROUND((julianday(REPLACE(TRIM(SUBSTR("PostedAt", 1,
                        CASE WHEN INSTR("PostedAt", '+') > 0 THEN INSTR("PostedAt", '+') - 1
                             WHEN INSTR("PostedAt", 'Z') > 0 THEN INSTR("PostedAt", 'Z') - 1
                             ELSE LENGTH("PostedAt") END)), 'T', ' ')) - 2440587.5) * 86400000) AS INTEGER)
                END,
                "Stage",
                "Title",
                CASE
                    WHEN "UpdatedAt" NOT GLOB '*[!0-9]*' AND LENGTH("UpdatedAt") >= 12 THEN CAST("UpdatedAt" AS INTEGER)
                    ELSE CAST(ROUND((julianday(REPLACE(TRIM(SUBSTR("UpdatedAt", 1,
                        CASE WHEN INSTR("UpdatedAt", '+') > 0 THEN INSTR("UpdatedAt", '+') - 1
                             WHEN INSTR("UpdatedAt", 'Z') > 0 THEN INSTR("UpdatedAt", 'Z') - 1
                             ELSE LENGTH("UpdatedAt") END)), 'T', ' ')) - 2440587.5) * 86400000) AS INTEGER)
                END,
                "Url"
            FROM "JobApplications";

            PRAGMA foreign_keys = 0;

            DROP TABLE "JobApplications";

            ALTER TABLE "ef_temp_JobApplications" RENAME TO "JobApplications";

            PRAGMA foreign_keys = 1;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => throw new NotSupportedException(
            "Reverting Unix millisecond timestamps to TEXT DateTimeOffset values is not supported.");
}
