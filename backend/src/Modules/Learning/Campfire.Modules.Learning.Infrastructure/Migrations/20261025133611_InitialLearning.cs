using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Campfire.Modules.Learning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialLearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "learning");

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HskLevel = table.Column<int>(type: "integer", nullable: false),
                    ContentVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vocabularies",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Chinese = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Pinyin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MeaningVi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HskLevel = table.Column<int>(type: "integer", nullable: false),
                    ExampleChinese = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExamplePinyin = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ExampleTranslationVi = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabularies", x => x.Id);
                    table.CheckConstraint("CK_vocabularies_HskLevel", "\"HskLevel\" BETWEEN 1 AND 6");
                });

            migrationBuilder.CreateTable(
                name: "course_units",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_course_units_courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "learning",
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_courses",
                schema: "learning",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalXp = table.Column<int>(type: "integer", nullable: false),
                    EnrolledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_courses", x => new { x.UserId, x.CourseId });
                    table.CheckConstraint("CK_user_courses_TotalXp", "\"TotalXp\" >= 0");
                    table.ForeignKey(
                        name: "FK_user_courses_courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "learning",
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_relations",
                schema: "learning",
                columns: table => new
                {
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_relations", x => new { x.SourceId, x.TargetId, x.RelationType });
                    table.CheckConstraint("CK_vocabulary_relations_Type", "\"RelationType\" IN ('Compound','Semantic')");
                    table.ForeignKey(
                        name: "FK_vocabulary_relations_vocabularies_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "learning",
                        principalTable: "vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vocabulary_relations_vocabularies_TargetId",
                        column: x => x.TargetId,
                        principalSchema: "learning",
                        principalTable: "vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "learning_days",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TitleChinese = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    IsReview = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_days", x => x.Id);
                    table.CheckConstraint("CK_learning_days_DayNumber", "\"DayNumber\" > 0");
                    table.CheckConstraint("CK_learning_days_XpReward", "\"XpReward\" >= 0");
                    table.ForeignKey(
                        name: "FK_learning_days_course_units_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "learning",
                        principalTable: "course_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_learning_days_courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "learning",
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_activities",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    ContentJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_activities", x => x.Id);
                    table.CheckConstraint("CK_lesson_activities_Type", "\"Type\" IN ('VOCABULARY','SENTENCE','GRAMMAR','EXERCISE')");
                    table.ForeignKey(
                        name: "FK_lesson_activities_learning_days_LearningDayId",
                        column: x => x.LearningDayId,
                        principalSchema: "learning",
                        principalTable: "learning_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_day_progress",
                schema: "learning",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    XpEarned = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_day_progress", x => new { x.UserId, x.LearningDayId });
                    table.CheckConstraint("CK_user_day_progress_Percent", "\"ProgressPercent\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_user_day_progress_Status", "\"Status\" IN ('Available','InProgress','Completed')");
                    table.ForeignKey(
                        name: "FK_user_day_progress_learning_days_LearningDayId",
                        column: x => x.LearningDayId,
                        principalSchema: "learning",
                        principalTable: "learning_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_activity_progress",
                schema: "learning",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_activity_progress", x => new { x.UserId, x.ActivityId });
                    table.CheckConstraint("CK_user_activity_progress_Score", "\"Score\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_user_activity_progress_lesson_activities_ActivityId",
                        column: x => x.ActivityId,
                        principalSchema: "learning",
                        principalTable: "lesson_activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "courses",
                columns: new[] { "Id", "ContentVersion", "Description", "HskLevel", "IsPublished", "Name", "Slug" },
                values: new object[] { new Guid("a1000000-0000-0000-0000-000000000001"), "1.0", "Lộ trình nền tảng cho người mới bắt đầu.", 1, true, "HSK 1 – Tiếng Trung cơ bản", "hsk1" });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "vocabularies",
                columns: new[] { "Id", "Chinese", "ExampleChinese", "ExamplePinyin", "ExampleTranslationVi", "HskLevel", "MeaningVi", "Pinyin" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000001001"), "学生", "我是学生。", "Wǒ shì xuésheng.", "Tôi là học sinh.", 1, "học sinh / sinh viên", "xuésheng" },
                    { new Guid("b1000000-0000-0000-0000-000000001002"), "大学生", "她是大学生。", "Tā shì dàxuéshēng.", "Cô ấy là sinh viên đại học.", 1, "sinh viên đại học", "dàxuéshēng" },
                    { new Guid("b1000000-0000-0000-0000-000000001003"), "中学生", "他是中学生。", "Tā shì zhōngxuéshēng.", "Cậu ấy là học sinh trung học.", 1, "học sinh trung học", "zhōngxuéshēng" },
                    { new Guid("b1000000-0000-0000-0000-000000001004"), "留学生", "我是越南留学生。", "Wǒ shì Yuènán liúxuéshēng.", "Tôi là du học sinh Việt Nam.", 1, "du học sinh", "liúxuéshēng" },
                    { new Guid("b1000000-0000-0000-0000-000000001005"), "老师", "她是汉语老师。", "Tā shì Hànyǔ lǎoshī.", "Cô ấy là giáo viên tiếng Trung.", 1, "giáo viên", "lǎoshī" },
                    { new Guid("b1000000-0000-0000-0000-000000001006"), "同学", "他是我的同学。", "Tā shì wǒ de tóngxué.", "Cậu ấy là bạn học của tôi.", 1, "bạn học", "tóngxué" }
                });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "course_units",
                columns: new[] { "Id", "CourseId", "OrderIndex", "Title" },
                values: new object[] { new Guid("a1000000-0000-0000-0000-000000000101"), new Guid("a1000000-0000-0000-0000-000000000001"), 1, "Unit 01 — Làm quen tiếng Trung" });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "vocabulary_relations",
                columns: new[] { "RelationType", "SourceId", "TargetId" },
                values: new object[,]
                {
                    { "Compound", new Guid("b1000000-0000-0000-0000-000000001001"), new Guid("b1000000-0000-0000-0000-000000001002") },
                    { "Compound", new Guid("b1000000-0000-0000-0000-000000001001"), new Guid("b1000000-0000-0000-0000-000000001003") },
                    { "Compound", new Guid("b1000000-0000-0000-0000-000000001001"), new Guid("b1000000-0000-0000-0000-000000001004") },
                    { "Semantic", new Guid("b1000000-0000-0000-0000-000000001001"), new Guid("b1000000-0000-0000-0000-000000001005") },
                    { "Semantic", new Guid("b1000000-0000-0000-0000-000000001001"), new Guid("b1000000-0000-0000-0000-000000001006") }
                });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "learning_days",
                columns: new[] { "Id", "CourseId", "DayNumber", "Description", "EstimatedMinutes", "IsReview", "Title", "TitleChinese", "UnitId", "XpReward" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000001001"), new Guid("a1000000-0000-0000-0000-000000000001"), 1, "Chào hỏi, hỏi thăm và nói lời cảm ơn.", 18, false, "Xin chào", "你好！", new Guid("a1000000-0000-0000-0000-000000000101"), 100 },
                    { new Guid("a1000000-0000-0000-0000-000000001002"), new Guid("a1000000-0000-0000-0000-000000000001"), 2, "Giới thiệu tên và quốc tịch.", 20, false, "Tôi tên là…", "我叫…", new Guid("a1000000-0000-0000-0000-000000000101"), 100 }
                });

            migrationBuilder.InsertData(
                schema: "learning",
                table: "lesson_activities",
                columns: new[] { "Id", "ContentJson", "LearningDayId", "OrderIndex", "Required", "Title", "Type" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000010001"), "{\"items\":[{\"chinese\":\"你好\",\"pinyin\":\"nǐ hǎo\",\"meaningVi\":\"xin chào\",\"exampleChinese\":\"你好！\",\"examplePinyin\":\"Nǐ hǎo!\",\"translationVi\":\"Xin chào!\"},{\"chinese\":\"谢谢\",\"pinyin\":\"xièxie\",\"meaningVi\":\"cảm ơn\",\"exampleChinese\":\"谢谢你。\",\"examplePinyin\":\"Xièxie nǐ.\",\"translationVi\":\"Cảm ơn bạn.\"},{\"chinese\":\"再见\",\"pinyin\":\"zàijiàn\",\"meaningVi\":\"tạm biệt\",\"exampleChinese\":\"老师，再见！\",\"examplePinyin\":\"Lǎoshī, zàijiàn!\",\"translationVi\":\"Tạm biệt thầy/cô!\"}]}", new Guid("a1000000-0000-0000-0000-000000001001"), 1, true, "Từ vựng chào hỏi", "VOCABULARY" },
                    { new Guid("a1000000-0000-0000-0000-000000010002"), "{\"items\":[{\"chinese\":\"你好吗？\",\"pinyin\":\"Nǐ hǎo ma?\",\"translationVi\":\"Bạn khỏe không?\"},{\"chinese\":\"我很好。\",\"pinyin\":\"Wǒ hěn hǎo.\",\"translationVi\":\"Tôi rất khỏe.\"}]}", new Guid("a1000000-0000-0000-0000-000000001001"), 2, true, "Mẫu câu cơ bản", "SENTENCE" },
                    { new Guid("a1000000-0000-0000-0000-000000010003"), "{\"name\":\"A + 很 + adjective\",\"explanationVi\":\"Dùng 很 trước tính từ; thêm 吗 cuối câu để tạo câu hỏi có/không.\",\"examples\":[\"我很好。\",\"你好吗？\"]}", new Guid("a1000000-0000-0000-0000-000000001001"), 3, true, "Cấu trúc 很 và 吗", "GRAMMAR" },
                    { new Guid("a1000000-0000-0000-0000-000000010004"), "{\"questions\":[{\"id\":\"q1\",\"type\":\"MULTIPLE_CHOICE\",\"question\":\"你好 có nghĩa là gì?\",\"options\":[\"Xin chào\",\"Cảm ơn\",\"Tạm biệt\"],\"correctAnswer\":\"Xin chào\"},{\"id\":\"q2\",\"type\":\"PINYIN\",\"question\":\"Pinyin đúng của 谢谢 là gì?\",\"options\":[\"xièxie\",\"zàijiàn\",\"nǐhǎo\"],\"correctAnswer\":\"xièxie\"},{\"id\":\"q3\",\"type\":\"SENTENCE_ORDER\",\"question\":\"Sắp xếp: [很] [我] [好]\",\"options\":[],\"correctAnswer\":\"我很好\"}]}", new Guid("a1000000-0000-0000-0000-000000001001"), 4, true, "Luyện tập ngày 1", "EXERCISE" },
                    { new Guid("a1000000-0000-0000-0000-000000020001"), "{\"items\":[{\"chinese\":\"叫\",\"pinyin\":\"jiào\",\"meaningVi\":\"tên là / gọi\"},{\"chinese\":\"名字\",\"pinyin\":\"míngzi\",\"meaningVi\":\"tên\"},{\"chinese\":\"越南\",\"pinyin\":\"Yuènán\",\"meaningVi\":\"Việt Nam\"}]}", new Guid("a1000000-0000-0000-0000-000000001002"), 1, true, "Tên và quốc tịch", "VOCABULARY" },
                    { new Guid("a1000000-0000-0000-0000-000000020002"), "{\"items\":[{\"chinese\":\"我叫小明。\",\"pinyin\":\"Wǒ jiào Xiǎomíng.\",\"translationVi\":\"Tôi tên là Tiểu Minh.\"},{\"chinese\":\"我是越南人。\",\"pinyin\":\"Wǒ shì Yuènán rén.\",\"translationVi\":\"Tôi là người Việt Nam.\"}]}", new Guid("a1000000-0000-0000-0000-000000001002"), 2, true, "Tự giới thiệu", "SENTENCE" },
                    { new Guid("a1000000-0000-0000-0000-000000020003"), "{\"questions\":[{\"id\":\"q1\",\"type\":\"FILL_BLANK\",\"question\":\"我___小明。\",\"options\":[\"叫\",\"是\",\"吗\"],\"correctAnswer\":\"叫\"},{\"id\":\"q2\",\"type\":\"MULTIPLE_CHOICE\",\"question\":\"越南 nghĩa là gì?\",\"options\":[\"Trung Quốc\",\"Việt Nam\",\"Nhật Bản\"],\"correctAnswer\":\"Việt Nam\"}]}", new Guid("a1000000-0000-0000-0000-000000001002"), 3, true, "Luyện tập ngày 2", "EXERCISE" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_units_CourseId_OrderIndex",
                schema: "learning",
                table: "course_units",
                columns: new[] { "CourseId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_Slug",
                schema: "learning",
                table: "courses",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_days_CourseId_DayNumber",
                schema: "learning",
                table: "learning_days",
                columns: new[] { "CourseId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_days_UnitId",
                schema: "learning",
                table: "learning_days",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_activities_LearningDayId_OrderIndex",
                schema: "learning",
                table: "lesson_activities",
                columns: new[] { "LearningDayId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_activity_progress_ActivityId",
                schema: "learning",
                table: "user_activity_progress",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_user_courses_CourseId",
                schema: "learning",
                table: "user_courses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_user_day_progress_LearningDayId",
                schema: "learning",
                table: "user_day_progress",
                column: "LearningDayId");

            migrationBuilder.CreateIndex(
                name: "IX_user_day_progress_UserId_Status",
                schema: "learning",
                table: "user_day_progress",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_vocabularies_Chinese_HskLevel",
                schema: "learning",
                table: "vocabularies",
                columns: new[] { "Chinese", "HskLevel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_relations_SourceId",
                schema: "learning",
                table: "vocabulary_relations",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_relations_TargetId",
                schema: "learning",
                table: "vocabulary_relations",
                column: "TargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_activity_progress",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "user_courses",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "user_day_progress",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "vocabulary_relations",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "lesson_activities",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "vocabularies",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "learning_days",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "course_units",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "learning");
        }
    }
}
