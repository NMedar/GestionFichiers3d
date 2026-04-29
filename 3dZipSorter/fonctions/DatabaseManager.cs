using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Collections.Generic;

namespace _3dZipSorter.Database
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private const int CurrentSchemaVersion = 2;

        // ─── Models ───────────────────────────────────────────────────────────

        public class ArchiveSortingRule
        {
            public int Id { get; set; }
            public string Extension { get; set; } = string.Empty;
            public string DestinationFile { get; set; } = string.Empty;
        }

        public class FileSortingRule
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Categorie { get; set; } = string.Empty;
            public string Cible { get; set; } = string.Empty;
        }

        public class Tag
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = "#607D8B";
        }

        public class Software
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = "#607D8B";
        }

        public class Author
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<AuthorLink> Links { get; set; } = new();
        }

        public class AuthorLink
        {
            public int Id { get; set; }
            public int AuthorId { get; set; }
            public string Label { get; set; } = string.Empty; // ex: "Blendermarket", "Twitter"
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Type d'asset : Model3D | Texture | Other
        /// </summary>
        public enum AssetType { Model3D, Texture, Other }

        /// <summary>
        /// État de l'asset
        /// </summary>
        public enum AssetStatus { NonTeste, Fonctionnel, Defectueux, EnCoursDeModification }

        /// <summary>
        /// Modèle de licence
        /// </summary>
        public enum AssetLicense { Free, OneTime, Subscription, Unknown }

        public class Asset
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? AuthorId { get; set; }
            public AssetType Type { get; set; } = AssetType.Other;
            public AssetStatus Status { get; set; } = AssetStatus.NonTeste;
            public AssetLicense License { get; set; } = AssetLicense.Unknown;
            public string? SiteUrl { get; set; }       // lien page produit / téléchargement
            public string? LocalPath { get; set; }     // dossier ou fichier local
            public string? ImagePath { get; set; }     // miniature
            public string? Description { get; set; }
            public bool IsFavorite { get; set; } = false;
            public int? Rating { get; set; }           // 1-10, nullable = non noté
            public DateTime DateAdded { get; set; } = DateTime.UtcNow;
            public DateTime DateModified { get; set; } = DateTime.UtcNow;

            public string Version { get; set; } = string.Empty;

            // Chargés via le DAL
            public Author? Author { get; set; }
            public List<Tag> Tags { get; set; } = new();
            public List<Software> Softwares { get; set; } = new();
        }

        // ─── Constructor ──────────────────────────────────────────────────────

        public DatabaseManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            int version = GetSchemaVersion(connection);

            if (version == 0)
                InitialiseSchema(connection);

            MigrateSchema(connection, version);
        }

        // ─── Schema versioning ────────────────────────────────────────────────

        private static int GetSchemaVersion(SqliteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA user_version;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void SetSchemaVersion(SqliteConnection connection, int version)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA user_version = {version};";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Schéma v1 : tables existantes (ArchiveSortingRules + FileSortingRules)
        /// </summary>
        private static void InitialiseSchema(SqliteConnection connection)
        {
            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS ArchiveSortingRules (
                    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    Extension     TEXT NOT NULL UNIQUE,
                    DestinationFile TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS FileSortingRules (
                    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nom      TEXT NOT NULL UNIQUE,
                    Categorie TEXT NOT NULL,
                    Cible    TEXT NOT NULL
                );
            ");

            SetSchemaVersion(connection, 1);
        }

        /// <summary>
        /// Applique les migrations manquantes dans l'ordre
        /// </summary>
        private static void MigrateSchema(SqliteConnection connection, int fromVersion)
        {
            if (fromVersion < 2)
                MigrateV2(connection);

            // Ajouter ici : if (fromVersion < 3) MigrateV3(connection);
        }

        /// <summary>
        /// Migration v2 : Tags, Softwares, Authors, AuthorLinks, Assets, tables de liaison
        /// </summary>
        private static void MigrateV2(SqliteConnection connection)
        {
            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Tags (
                    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name  TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    Color TEXT NOT NULL DEFAULT '#607D8B'
                );

                CREATE TABLE IF NOT EXISTS Softwares (
                    Id    INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name  TEXT NOT NULL UNIQUE COLLATE NOCASE,
                    Color TEXT NOT NULL DEFAULT '#607D8B'
                );

                CREATE TABLE IF NOT EXISTS Authors (
                    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE COLLATE NOCASE
                );

                CREATE TABLE IF NOT EXISTS AuthorLinks (
                    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                    AuthorId INTEGER NOT NULL REFERENCES Authors(Id) ON DELETE CASCADE,
                    Label    TEXT NOT NULL,
                    Url      TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Assets (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name         TEXT NOT NULL,
                    AuthorId     INTEGER REFERENCES Authors(Id) ON DELETE SET NULL,
                    Type         TEXT NOT NULL DEFAULT 'Other',
                    Status       TEXT NOT NULL DEFAULT 'NonTeste',
                    License      TEXT NOT NULL DEFAULT 'Unknown',
                    SiteUrl      TEXT,
                    LocalPath    TEXT,
                    ImagePath    TEXT,
                    Description  TEXT,
                    IsFavorite   INTEGER NOT NULL DEFAULT 0,
                    Rating       INTEGER CHECK(Rating IS NULL OR (Rating >= 1 AND Rating <= 10)),
                    DateAdded    TEXT NOT NULL DEFAULT (datetime('now')),
                    DateModified TEXT NOT NULL DEFAULT (datetime('now')),
                    Version TEXT NOT NULL DEFAULT '1.0'
                );

                CREATE TABLE IF NOT EXISTS AssetTags (
                    AssetId INTEGER NOT NULL REFERENCES Assets(Id) ON DELETE CASCADE,
                    TagId   INTEGER NOT NULL REFERENCES Tags(Id)   ON DELETE CASCADE,
                    PRIMARY KEY (AssetId, TagId)
                );

                CREATE TABLE IF NOT EXISTS AssetSoftwares (
                    AssetId    INTEGER NOT NULL REFERENCES Assets(Id)    ON DELETE CASCADE,
                    SoftwareId INTEGER NOT NULL REFERENCES Softwares(Id) ON DELETE CASCADE,
                    PRIMARY KEY (AssetId, SoftwareId)
                );
            ");

            // Données prédéfinies - Tags
            var defaultTags = new[]
            {
                ("stylized",   "#E91E63"),
                ("realistic",  "#2196F3"),
                ("lowpoly",    "#4CAF50"),
                ("highpoly",   "#FF5722"),
                ("pbr",        "#9C27B0"),
                ("rigged",     "#00BCD4"),
                ("animated",   "#FF9800"),
                ("scifi",      "#3F51B5"),
                ("fantasy",    "#8BC34A"),
                ("nature",     "#795548"),
                ("architecture","#607D8B"),
                ("character",  "#F44336"),
                ("vehicle",    "#009688"),
                ("prop",       "#CDDC39"),
                ("environment","#673AB7"),
            };

            foreach (var (name, color) in defaultTags)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO Tags (Name, Color) VALUES ($name, $color);";
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$color", color);
                cmd.ExecuteNonQuery();
            }

            // Données prédéfinies - Softwares
            var defaultSoftwares = new[]
            {
                ("Blender",           "#E87D0D"),
                ("Maya",              "#0696D7"),
                ("3ds Max",           "#0696D7"),
                ("Cinema 4D",        "#011A6A"),
                ("Houdini",           "#FF4713"),
                ("ZBrush",            "#CC2222"),
                ("Substance Painter", "#FFAA00"),
                ("Substance Designer","#FFAA00"),
                ("Marmoset Toolbag",  "#4A90D9"),
                ("Unreal Engine",     "#0E1128"),
                ("Unity",             "#222C37"),
            };

            foreach (var (name, color) in defaultSoftwares)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO Softwares (Name, Color) VALUES ($name, $color);";
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$color", color);
                cmd.ExecuteNonQuery();
            }

            SetSchemaVersion(connection, 2);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void Execute(SqliteConnection connection, string sql)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        // ─── ArchiveSortingRules CRUD (inchangé) ──────────────────────────────

        public int InsertOrUpdateArchiveSortingRules(string extension, string destinationFile)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ArchiveSortingRules (Extension, DestinationFile)
                VALUES ($ext, $dest)
                ON CONFLICT(Extension) DO UPDATE SET DestinationFile = $dest;
            ";
            cmd.Parameters.AddWithValue("$ext", extension);
            cmd.Parameters.AddWithValue("$dest", destinationFile);
            cmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT Id FROM ArchiveSortingRules WHERE Extension = $ext";
            idCmd.Parameters.AddWithValue("$ext", extension);
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void UpdateArchiveSortingRulesById(int id, string extension, string destinationFile)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE ArchiveSortingRules
                SET Extension = $ext, DestinationFile = $dest
                WHERE Id = $id;
            ";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$ext", extension);
            cmd.Parameters.AddWithValue("$dest", destinationFile);
            cmd.ExecuteNonQuery();
        }

        public void DeleteArchiveSortingRuleById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM ArchiveSortingRules WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public string GetArchiveSortingRuleByExtension(string extension)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT DestinationFile FROM ArchiveSortingRules WHERE Extension = $ext";
            cmd.Parameters.AddWithValue("$ext", extension);
            return cmd.ExecuteScalar()?.ToString() ?? string.Empty;
        }

        public List<ArchiveSortingRule> GetAllArchiveSortingRules()
        {
            var rules = new List<ArchiveSortingRule>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Extension, DestinationFile FROM ArchiveSortingRules";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                rules.Add(new ArchiveSortingRule
                {
                    Id = reader.GetInt32(0),
                    Extension = reader.GetString(1),
                    DestinationFile = reader.GetString(2)
                });
            return rules;
        }

        // ─── Tags CRUD ────────────────────────────────────────────────────────

        public List<Tag> GetAllTags()
        {
            var list = new List<Tag>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Color FROM Tags ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Tag { Id = reader.GetInt32(0), Name = reader.GetString(1), Color = reader.GetString(2) });
            return list;
        }

        /// <summary>Retourne l'Id (insert si absent, grâce à UNIQUE COLLATE NOCASE)</summary>
        public int InsertOrIgnoreTag(string name, string color = "#607D8B")
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Tags (Name, Color) VALUES ($name, $color);";
            cmd.Parameters.AddWithValue("$name", name.Trim());
            cmd.Parameters.AddWithValue("$color", color);
            cmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT Id FROM Tags WHERE Name = $name COLLATE NOCASE;";
            idCmd.Parameters.AddWithValue("$name", name.Trim());
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void UpdateTag(int id, string name, string color)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Tags SET Name = $name, Color = $color WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$name", name.Trim());
            cmd.Parameters.AddWithValue("$color", color);
            cmd.ExecuteNonQuery();
        }

        public void DeleteTag(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Tags WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ─── Softwares CRUD ───────────────────────────────────────────────────

        public List<Software> GetAllSoftwares()
        {
            var list = new List<Software>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Color FROM Softwares ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Software { Id = reader.GetInt32(0), Name = reader.GetString(1), Color = reader.GetString(2) });
            return list;
        }

        public int InsertOrIgnoreSoftware(string name, string color = "#607D8B")
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Softwares (Name, Color) VALUES ($name, $color);";
            cmd.Parameters.AddWithValue("$name", name.Trim());
            cmd.Parameters.AddWithValue("$color", color);
            cmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT Id FROM Softwares WHERE Name = $name COLLATE NOCASE;";
            idCmd.Parameters.AddWithValue("$name", name.Trim());
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void UpdateSoftware(int id, string name, string color)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Softwares SET Name = $name, Color = $color WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$name", name.Trim());
            cmd.Parameters.AddWithValue("$color", color);
            cmd.ExecuteNonQuery();
        }

        public void DeleteSoftware(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Softwares WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ─── Authors CRUD ─────────────────────────────────────────────────────

        public List<Author> GetAllAuthors()
        {
            var list = new List<Author>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Authors ORDER BY Name;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Author { Id = reader.GetInt32(0), Name = reader.GetString(1) });

            foreach (var author in list)
                author.Links = GetAuthorLinks(connection, author.Id);

            return list;
        }

        private static List<AuthorLink> GetAuthorLinks(SqliteConnection connection, int authorId)
        {
            var links = new List<AuthorLink>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, AuthorId, Label, Url FROM AuthorLinks WHERE AuthorId = $id;";
            cmd.Parameters.AddWithValue("$id", authorId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                links.Add(new AuthorLink
                {
                    Id = reader.GetInt32(0),
                    AuthorId = reader.GetInt32(1),
                    Label = reader.GetString(2),
                    Url = reader.GetString(3)
                });
            return links;
        }

        public int InsertOrGetAuthor(string name)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO Authors (Name) VALUES ($name);";
            cmd.Parameters.AddWithValue("$name", name.Trim());
            cmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT Id FROM Authors WHERE Name = $name COLLATE NOCASE;";
            idCmd.Parameters.AddWithValue("$name", name.Trim());
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void AddAuthorLink(int authorId, string label, string url)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO AuthorLinks (AuthorId, Label, Url) VALUES ($authorId, $label, $url);";
            cmd.Parameters.AddWithValue("$authorId", authorId);
            cmd.Parameters.AddWithValue("$label", label);
            cmd.Parameters.AddWithValue("$url", url);
            cmd.ExecuteNonQuery();
        }

        public void DeleteAuthorLink(int linkId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM AuthorLinks WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", linkId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteAuthor(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Authors WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ─── Assets CRUD ──────────────────────────────────────────────────────

        public int InsertAsset(Asset asset)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Assets (Name, AuthorId, Type, Status, License, SiteUrl, LocalPath, ImagePath, Description, IsFavorite, Rating, DateAdded, DateModified)
                VALUES ($name, $authorId, $type, $status, $license, $siteUrl, $localPath, $imagePath, $desc, $fav, $rating, $dateAdded, $dateMod);
            ";
            cmd.Parameters.AddWithValue("$name", asset.Name);
            cmd.Parameters.AddWithValue("$authorId", asset.AuthorId.HasValue ? (object)asset.AuthorId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$type", asset.Type.ToString());
            cmd.Parameters.AddWithValue("$status", asset.Status.ToString());
            cmd.Parameters.AddWithValue("$license", asset.License.ToString());
            cmd.Parameters.AddWithValue("$siteUrl", asset.SiteUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$localPath", asset.LocalPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$imagePath", asset.ImagePath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$desc", asset.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$fav", asset.IsFavorite ? 1 : 0);
            cmd.Parameters.AddWithValue("$rating", asset.Rating.HasValue ? (object)asset.Rating.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$dateAdded", asset.DateAdded.ToString("o"));
            cmd.Parameters.AddWithValue("$dateMod", asset.DateModified.ToString("o"));
            cmd.ExecuteNonQuery();

            var idCmd = connection.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            int newId = Convert.ToInt32(idCmd.ExecuteScalar());

            SetAssetTags(connection, newId, asset.Tags);
            SetAssetSoftwares(connection, newId, asset.Softwares);

            return newId;
        }

        public void UpdateAsset(Asset asset)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                UPDATE Assets SET
                    Name = $name, AuthorId = $authorId, Type = $type, Status = $status,
                    License = $license, SiteUrl = $siteUrl, LocalPath = $localPath,
                    ImagePath = $imagePath, Description = $desc, IsFavorite = $fav,
                    Rating = $rating, DateModified = $dateMod
                WHERE Id = $id;
            ";
            cmd.Parameters.AddWithValue("$id", asset.Id);
            cmd.Parameters.AddWithValue("$name", asset.Name);
            cmd.Parameters.AddWithValue("$authorId", asset.AuthorId.HasValue ? (object)asset.AuthorId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$type", asset.Type.ToString());
            cmd.Parameters.AddWithValue("$status", asset.Status.ToString());
            cmd.Parameters.AddWithValue("$license", asset.License.ToString());
            cmd.Parameters.AddWithValue("$siteUrl", asset.SiteUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$localPath", asset.LocalPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$imagePath", asset.ImagePath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$desc", asset.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$fav", asset.IsFavorite ? 1 : 0);
            cmd.Parameters.AddWithValue("$rating", asset.Rating.HasValue ? (object)asset.Rating.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("$dateMod", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();

            SetAssetTags(connection, asset.Id, asset.Tags);
            SetAssetSoftwares(connection, asset.Id, asset.Softwares);
        }

        public void DeleteAsset(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Assets WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public List<Asset> GetAllAssets()
        {
            var list = new List<Asset>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, AuthorId, Type, Status, License, SiteUrl, LocalPath, ImagePath, Description, IsFavorite, Rating, DateAdded, DateModified FROM Assets;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(ReadAsset(reader));

            foreach (var asset in list)
            {
                asset.Tags = GetAssetTags(connection, asset.Id);
                asset.Softwares = GetAssetSoftwares(connection, asset.Id);
                if (asset.AuthorId.HasValue)
                    asset.Author = GetAuthorById(connection, asset.AuthorId.Value);
            }
            return list;
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private static Asset ReadAsset(SqliteDataReader r) => new Asset
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            AuthorId = r.IsDBNull(2) ? null : r.GetInt32(2),
            Type = Enum.Parse<AssetType>(r.GetString(3)),
            Status = Enum.Parse<AssetStatus>(r.GetString(4)),
            License = Enum.Parse<AssetLicense>(r.GetString(5)),
            SiteUrl = r.IsDBNull(6) ? null : r.GetString(6),
            LocalPath = r.IsDBNull(7) ? null : r.GetString(7),
            ImagePath = r.IsDBNull(8) ? null : r.GetString(8),
            Description = r.IsDBNull(9) ? null : r.GetString(9),
            IsFavorite = r.GetInt32(10) == 1,
            Rating = r.IsDBNull(11) ? null : r.GetInt32(11),
            DateAdded = DateTime.Parse(r.GetString(12)),
            DateModified = DateTime.Parse(r.GetString(13)),
        };

        private static Author? GetAuthorById(SqliteConnection connection, int id)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Authors WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            var author = new Author { Id = reader.GetInt32(0), Name = reader.GetString(1) };
            author.Links = GetAuthorLinks(connection, author.Id);
            return author;
        }

        private static List<Tag> GetAssetTags(SqliteConnection connection, int assetId)
        {
            var list = new List<Tag>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT t.Id, t.Name, t.Color FROM Tags t
                INNER JOIN AssetTags at ON t.Id = at.TagId
                WHERE at.AssetId = $assetId;";
            cmd.Parameters.AddWithValue("$assetId", assetId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Tag { Id = reader.GetInt32(0), Name = reader.GetString(1), Color = reader.GetString(2) });
            return list;
        }

        private static List<Software> GetAssetSoftwares(SqliteConnection connection, int assetId)
        {
            var list = new List<Software>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT s.Id, s.Name, s.Color FROM Softwares s
                INNER JOIN AssetSoftwares asw ON s.Id = asw.SoftwareId
                WHERE asw.AssetId = $assetId;";
            cmd.Parameters.AddWithValue("$assetId", assetId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Software { Id = reader.GetInt32(0), Name = reader.GetString(1), Color = reader.GetString(2) });
            return list;
        }

        private static void SetAssetTags(SqliteConnection connection, int assetId, List<Tag> tags)
        {
            var del = connection.CreateCommand();
            del.CommandText = "DELETE FROM AssetTags WHERE AssetId = $assetId;";
            del.Parameters.AddWithValue("$assetId", assetId);
            del.ExecuteNonQuery();

            foreach (var tag in tags)
            {
                var ins = connection.CreateCommand();
                ins.CommandText = "INSERT OR IGNORE INTO AssetTags (AssetId, TagId) VALUES ($assetId, $tagId);";
                ins.Parameters.AddWithValue("$assetId", assetId);
                ins.Parameters.AddWithValue("$tagId", tag.Id);
                ins.ExecuteNonQuery();
            }
        }

        private static void SetAssetSoftwares(SqliteConnection connection, int assetId, List<Software> softwares)
        {
            var del = connection.CreateCommand();
            del.CommandText = "DELETE FROM AssetSoftwares WHERE AssetId = $assetId;";
            del.Parameters.AddWithValue("$assetId", assetId);
            del.ExecuteNonQuery();

            foreach (var sw in softwares)
            {
                var ins = connection.CreateCommand();
                ins.CommandText = "INSERT OR IGNORE INTO AssetSoftwares (AssetId, SoftwareId) VALUES ($assetId, $swId);";
                ins.Parameters.AddWithValue("$assetId", assetId);
                ins.Parameters.AddWithValue("$swId", sw.Id);
                ins.ExecuteNonQuery();
            }
        }
    }
}