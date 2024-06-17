﻿// <auto-generated />
using System;
using MPM_Betting.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    [DbContext(typeof(MpmDbContext))]
    [Migration("20240610103657_set points to long")]
    partial class setpointstolong
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AchievementMpmUser", b =>
                {
                    b.Property<int>("AchievmentsId")
                        .HasColumnType("int");

                    b.Property<string>("UsersId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AchievmentsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("AchievementMpmUser");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Bet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Completed")
                        .HasColumnType("bit");

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("Hit")
                        .HasColumnType("bit");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<bool>("Processed")
                        .HasColumnType("bit");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("Bets", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.CustomSeasonEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GameId")
                        .HasColumnType("int");

                    b.Property<int>("SeasonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.HasIndex("SeasonId");

                    b.ToTable("CustomSeasonEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BuiltinSeasonId")
                        .HasColumnType("int");

                    b.Property<int>("GameState")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("ReferenceId")
                        .HasColumnType("int");

                    b.Property<int>("SportType")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("BuiltinSeasonId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.ScoreEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.Property<int>("SeasonEntryId")
                        .HasColumnType("int");

                    b.Property<int>("UserGroupEntryId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SeasonEntryId");

                    b.HasIndex("UserGroupEntryId");

                    b.ToTable("ScoreEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Season", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.Property<DateTime>("End")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Sport")
                        .HasColumnType("int");

                    b.Property<DateTime>("Start")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Seasons");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Season");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.SeasonEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<int>("SeasonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("SeasonId");

                    b.ToTable("SeasonEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Rewarding.Achievement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Achievements");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Rewarding.AchievementEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AchievementId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateEarned")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AchievementId");

                    b.HasIndex("UserId");

                    b.ToTable("AchievementEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.FavouriteFootballLeague", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("LeagueId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "LeagueId");

                    b.ToTable("UserFavouriteSeasons");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(5000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RecipientGroupId")
                        .HasColumnType("int");

                    b.Property<string>("RecipientUserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SenderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("RecipientGroupId");

                    b.HasIndex("RecipientUserId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.MpmGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CreatorId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("ProfilePictureUrl")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.MpmUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastRedeemed")
                        .HasColumnType("datetime2");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<long>("Points")
                        .HasColumnType("bigint");

                    b.Property<string>("ProfilePictureUrl")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("TargetId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TargetId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.UserGroupEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<string>("MpmUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("MpmUserId");

                    b.ToTable("UserGroupEntries");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Football.GameBet", b =>
                {
                    b.HasBaseType("MPM_Betting.DataModel.Betting.Bet");

                    b.Property<int>("AwayScore")
                        .HasColumnType("int");

                    b.Property<int>("HomeScore")
                        .HasColumnType("int");

                    b.Property<double>("Quote")
                        .HasColumnType("float");

                    b.Property<bool>("ResultHit")
                        .HasColumnType("bit");

                    b.Property<bool>("ScoreHit")
                        .HasColumnType("bit");

                    b.ToTable("FootballGameBets", (string)null);
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.BuiltinSeason", b =>
                {
                    b.HasBaseType("MPM_Betting.DataModel.Betting.Season");

                    b.Property<int>("ReferenceId")
                        .HasColumnType("int");

                    b.HasDiscriminator().HasValue("BuiltinSeason");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.CustomSeason", b =>
                {
                    b.HasBaseType("MPM_Betting.DataModel.Betting.Season");

                    b.HasDiscriminator().HasValue("CustomSeason");
                });

            modelBuilder.Entity("AchievementMpmUser", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Rewarding.Achievement", null)
                        .WithMany()
                        .HasForeignKey("AchievmentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Bet", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Betting.Game", "Game")
                        .WithMany("Bets")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.MpmGroup", "Group")
                        .WithMany("AllBets")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.CustomSeasonEntry", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Betting.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.Betting.CustomSeason", "Season")
                        .WithMany("Entries")
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Game");

                    b.Navigation("Season");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Game", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Betting.BuiltinSeason", "BuiltinSeason")
                        .WithMany()
                        .HasForeignKey("BuiltinSeasonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BuiltinSeason");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.ScoreEntry", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Betting.SeasonEntry", "SeasonEntry")
                        .WithMany("ScoreEntries")
                        .HasForeignKey("SeasonEntryId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.UserGroupEntry", "UserGroupEntry")
                        .WithMany("ScoreEntries")
                        .HasForeignKey("UserGroupEntryId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("SeasonEntry");

                    b.Navigation("UserGroupEntry");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.SeasonEntry", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmGroup", "Group")
                        .WithMany("Seasons")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.Betting.Season", "Season")
                        .WithMany()
                        .HasForeignKey("SeasonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Season");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Rewarding.AchievementEntry", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Rewarding.Achievement", "Achievement")
                        .WithMany()
                        .HasForeignKey("AchievementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Achievement");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.FavouriteFootballLeague", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "User")
                        .WithMany("FavouriteFootballLeagues")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.Message", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmGroup", "RecipientGroup")
                        .WithMany()
                        .HasForeignKey("RecipientGroupId");

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "RecipientUser")
                        .WithMany()
                        .HasForeignKey("RecipientUserId");

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId");

                    b.Navigation("RecipientGroup");

                    b.Navigation("RecipientUser");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.MpmGroup", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.Notification", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "Target")
                        .WithMany()
                        .HasForeignKey("TargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Target");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.UserGroupEntry", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmGroup", "Group")
                        .WithMany("UserGroupEntries")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", "MpmUser")
                        .WithMany("UserGroupEntries")
                        .HasForeignKey("MpmUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("MpmUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.User.MpmUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Football.GameBet", b =>
                {
                    b.HasOne("MPM_Betting.DataModel.Betting.Bet", null)
                        .WithOne()
                        .HasForeignKey("MPM_Betting.DataModel.Football.GameBet", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.Game", b =>
                {
                    b.Navigation("Bets");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.SeasonEntry", b =>
                {
                    b.Navigation("ScoreEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.MpmGroup", b =>
                {
                    b.Navigation("AllBets");

                    b.Navigation("Seasons");

                    b.Navigation("UserGroupEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.MpmUser", b =>
                {
                    b.Navigation("FavouriteFootballLeagues");

                    b.Navigation("UserGroupEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.User.UserGroupEntry", b =>
                {
                    b.Navigation("ScoreEntries");
                });

            modelBuilder.Entity("MPM_Betting.DataModel.Betting.CustomSeason", b =>
                {
                    b.Navigation("Entries");
                });
#pragma warning restore 612, 618
        }
    }
}
